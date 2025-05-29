namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.AcumularPuntos;

/// <summary>
/// Handler para acumular puntos en el programa de fidelización
/// Gestiona cálculo de puntos, aplicación de multiplicadores y registro de transacciones
/// </summary>
public class AcumularPuntosHandler : IRequestHandler<AcumularPuntosCommand, Result<AcumulacionPuntosDto>>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly ITransaccionPuntosRepository _transaccionRepository;
    private readonly IPromocionRepository _promocionRepository;
    private readonly ICalculadoraPuntosService _calculadoraPuntos;
    private readonly IServicioFidelizacionService _servicioFidelizacion;
    private readonly IMapper _mapper;
    private readonly ILogger<AcumularPuntosHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public AcumularPuntosHandler(
        IClienteRepository clienteRepository,
        ITarjetaFidelizacionRepository tarjetaRepository,
        ITransaccionPuntosRepository transaccionRepository,
        IPromocionRepository promocionRepository,
        ICalculadoraPuntosService calculadoraPuntos,
        IServicioFidelizacionService servicioFidelizacion,
        IMapper mapper,
        ILogger<AcumularPuntosHandler> logger,
        ICurrentUserService currentUser)
    {
        _clienteRepository = clienteRepository;
        _tarjetaRepository = tarjetaRepository;
        _transaccionRepository = transaccionRepository;
        _promocionRepository = promocionRepository;
        _calculadoraPuntos = calculadoraPuntos;
        _servicioFidelizacion = servicioFidelizacion;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<AcumulacionPuntosDto>> Handle(AcumularPuntosCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando acumulación de puntos para cliente {ClienteId}: {MontoCompra:C}, Tipo: {TipoTransaccion}", 
            request.ClienteId, request.MontoCompra, request.TipoTransaccion);

        try
        {
            // 1. Verificar y obtener cliente
            var cliente = await _clienteRepository.ObtenerPorIdAsync(request.ClienteId);
            if (cliente == null)
            {
                _logger.LogWarning("Cliente no encontrado: {ClienteId}", request.ClienteId);
                return Result<AcumulacionPuntosDto>.Failure("El cliente especificado no existe");
            }

            // 2. Verificar que el cliente esté activo y elegible para puntos
            if (!cliente.Activo || !cliente.ElegibleParaPuntos)
            {
                _logger.LogWarning("Cliente {ClienteId} no elegible para acumulación de puntos. Activo: {Activo}, Elegible: {Elegible}", 
                    request.ClienteId, cliente.Activo, cliente.ElegibleParaPuntos);
                return Result<AcumulacionPuntosDto>.Failure("El cliente no está elegible para acumular puntos");
            }

            // 3. Obtener o verificar tarjeta de fidelización
            var tarjeta = await ObtenerTarjetaFidelizacion(request, cliente);
            if (tarjeta == null)
            {
                return Result<AcumulacionPuntosDto>.Failure("Error al obtener la tarjeta de fidelización del cliente");
            }

            // 4. Validar promoción si se especifica
            Promocion? promocion = null;
            if (!string.IsNullOrEmpty(request.CodigoPromocion))
            {
                var resultadoPromocion = await ValidarPromocion(request.CodigoPromocion, request.TipoTransaccion);
                if (!resultadoPromocion.Succeeded)
                {
                    return Result<AcumulacionPuntosDto>.Failure(resultadoPromocion.ErrorMessage);
                }
                promocion = resultadoPromocion.Value;
            }

            // 5. Calcular puntos base y aplicar multiplicadores
            var calculoResultado = await CalcularPuntosTransaccion(request, cliente, tarjeta, promocion);
            if (!calculoResultado.Succeeded)
            {
                return Result<AcumulacionPuntosDto>.Failure(calculoResultado.ErrorMessage);
            }

            var puntosPorOtorgar = calculoResultado.Value;

            // 6. Verificar límites diarios/mensuales
            var validacionLimites = await ValidarLimitesAcumulacion(cliente, puntosPorOtorgar, request.TipoTransaccion);
            if (!validacionLimites.Succeeded)
            {
                return Result<AcumulacionPuntosDto>.Failure(validacionLimites.ErrorMessage);
            }

            // 7. Registrar transacción de puntos
            var transaccion = await CrearTransaccionPuntos(request, tarjeta, puntosPorOtorgar, promocion);

            // 8. Actualizar saldo de puntos en la tarjeta
            var resultadoActualizacion = tarjeta.AcumularPuntos(
                puntosPorOtorgar, 
                request.TipoTransaccion.ToString(),
                request.Comentarios,
                _currentUser.UserId ?? "Sistema");

            if (!resultadoActualizacion.Succeeded)
            {
                return Result<AcumulacionPuntosDto>.Failure($"Error al actualizar saldo de puntos: {resultadoActualizacion.ErrorMessage}");
            }

            // 9. Guardar cambios
            await _transaccionRepository.AgregarAsync(transaccion);
            await _tarjetaRepository.ActualizarAsync(tarjeta);

            // 10. Verificar ascensos de nivel
            await VerificarAscensoNivel(cliente, tarjeta);

            // 11. Registrar logros y bonificaciones especiales
            await ProcesarLogrosEspeciales(cliente, tarjeta, request);

            _logger.LogInformation("Puntos acumulados exitosamente para cliente {ClienteId}: {PuntosOtorgados} puntos. Saldo total: {SaldoTotal}", 
                request.ClienteId, puntosPorOtorgar, tarjeta.SaldoPuntos);

            // 12. Mapear y retornar resultado
            var resultado = new AcumulacionPuntosDto
            {
                ClienteId = cliente.Id,
                TarjetaId = tarjeta.Id,
                PuntosOtorgados = puntosPorOtorgar,
                SaldoAnterior = tarjeta.SaldoPuntos - puntosPorOtorgar,
                SaldoActual = tarjeta.SaldoPuntos,
                TipoTransaccion = request.TipoTransaccion.ToString(),
                MontoCompra = request.MontoCompra,
                MultiplicadorAplicado = calculoResultado.Value.MultiplicadorTotal,
                PromocionAplicada = promocion?.Codigo,
                FechaTransaccion = DateTime.UtcNow,
                TransaccionId = transaccion.Id,
                NuevoNivel = tarjeta.NivelFidelizacion?.Nombre,
                MensajeMotivacional = GenerarMensajeMotivacional(puntosPorOtorgar, tarjeta)
            };

            return Result<AcumulacionPuntosDto>.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al acumular puntos para cliente {ClienteId}", request.ClienteId);
            return Result<AcumulacionPuntosDto>.Failure("Error interno del servidor al procesar la acumulación de puntos");
        }
    }

    private async Task<TarjetaFidelizacion?> ObtenerTarjetaFidelizacion(AcumularPuntosCommand request, Cliente cliente)
    {
        TarjetaFidelizacion? tarjeta = null;

        if (request.TarjetaFidelizacionId.HasValue)
        {
            tarjeta = await _tarjetaRepository.ObtenerPorIdAsync(request.TarjetaFidelizacionId.Value);
            if (tarjeta?.ClienteId != cliente.Id)
            {
                _logger.LogWarning("Tarjeta {TarjetaId} no pertenece al cliente {ClienteId}", 
                    request.TarjetaFidelizacionId, cliente.Id);
                return null;
            }
        }
        else
        {
            // Buscar tarjeta activa del cliente
            tarjeta = await _tarjetaRepository.ObtenerTarjetaActivaPorClienteAsync(cliente.Id);
            if (tarjeta == null)
            {
                _logger.LogWarning("Cliente {ClienteId} no tiene tarjeta de fidelización activa", cliente.Id);
                return null;
            }
        }

        return tarjeta;
    }

    private async Task<Result<Promocion>> ValidarPromocion(string codigoPromocion, TipoTransaccionPuntos tipoTransaccion)
    {
        var promocion = await _promocionRepository.ObtenerPorCodigoAsync(codigoPromocion);
        if (promocion == null)
        {
            return Result<Promocion>.Failure($"Código de promoción '{codigoPromocion}' no existe");
        }

        if (!promocion.Activa || promocion.FechaVencimiento < DateTime.UtcNow)
        {
            return Result<Promocion>.Failure($"Código de promoción '{codigoPromocion}' no está activo o ha expirado");
        }

        if (!promocion.EsAplicableATipoTransaccion(tipoTransaccion.ToString()))
        {
            return Result<Promocion>.Failure($"Código de promoción '{codigoPromocion}' no es aplicable a este tipo de transacción");
        }

        return Result<Promocion>.Success(promocion);
    }

    private async Task<Result<CalculoResultadoPuntos>> CalcularPuntosTransaccion(
        AcumularPuntosCommand request, 
        Cliente cliente, 
        TarjetaFidelizacion tarjeta, 
        Promocion? promocion)
    {
        try
        {
            var parametrosCalculo = new ParametrosCalculoPuntos
            {
                MontoBase = request.MontoCompra,
                TipoTransaccion = request.TipoTransaccion,
                NivelCliente = tarjeta.NivelFidelizacion,
                CategoriaProductos = request.CategoriaProductos,
                Canal = request.Canal,
                EsFechaEspecial = request.EsFechaEspecial,
                TipoFechaEspecial = request.TipoFechaEspecial,
                MultiplicadorEspecial = request.MultiplicadorEspecial,
                PuntosBonus = request.PuntosBonus,
                Promocion = promocion,
                HistorialCliente = await _servicioFidelizacion.ObtenerHistorialClienteAsync(cliente.Id)
            };

            var resultado = await _calculadoraPuntos.CalcularPuntosAsync(parametrosCalculo);
            return Result<CalculoResultadoPuntos>.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculando puntos para cliente {ClienteId}", cliente.Id);
            return Result<CalculoResultadoPuntos>.Failure("Error en el cálculo de puntos");
        }
    }

    private async Task<Result> ValidarLimitesAcumulacion(Cliente cliente, int puntosAOtorgar, TipoTransaccionPuntos tipoTransaccion)
    {
        // Verificar límites diarios
        var acumulacionHoy = await _transaccionRepository.ObtenerAcumulacionDiariaAsync(cliente.Id, DateTime.Today);
        var limiteDiario = await _servicioFidelizacion.ObtenerLimiteDiarioAsync(cliente, tipoTransaccion);

        if (acumulacionHoy + puntosAOtorgar > limiteDiario)
        {
            return Result.Failure($"Se excede el límite diario de acumulación de puntos. Límite: {limiteDiario}, Intentando acumular: {puntosAOtorgar}, Ya acumulado hoy: {acumulacionHoy}");
        }

        // Verificar límites mensuales para tipos especiales
        if (tipoTransaccion == TipoTransaccionPuntos.PromocionEspecial || tipoTransaccion == TipoTransaccionPuntos.AjusteManual)
        {
            var acumulacionMensual = await _transaccionRepository.ObtenerAcumulacionMensualAsync(cliente.Id, DateTime.Now.Year, DateTime.Now.Month, tipoTransaccion);
            var limiteMensual = await _servicioFidelizacion.ObtenerLimiteMensualAsync(cliente, tipoTransaccion);

            if (acumulacionMensual + puntosAOtorgar > limiteMensual)
            {
                return Result.Failure($"Se excede el límite mensual para {tipoTransaccion}. Límite: {limiteMensual}, Ya acumulado este mes: {acumulacionMensual}");
            }
        }

        return Result.Success();
    }

    private async Task<TransaccionPuntos> CrearTransaccionPuntos(
        AcumularPuntosCommand request, 
        TarjetaFidelizacion tarjeta, 
        int puntosOtorgados, 
        Promocion? promocion)
    {
        var transaccion = new TransaccionPuntos
        {
            Id = Guid.NewGuid(),
            TarjetaFidelizacionId = tarjeta.Id,
            TipoTransaccion = request.TipoTransaccion.ToString(),
            MontoCompra = request.MontoCompra,
            PuntosOtorgados = puntosOtorgados,
            FacturaId = request.FacturaId,
            ComandaId = request.ComandaId,
            PromocionId = promocion?.Id,
            Canal = request.Canal,
            Sucursal = request.Sucursal,
            EmpleadoId = request.EmpleadoId,
            ReferenciaExterna = request.ReferenciaExterna,
            Comentarios = request.Comentarios,
            DatosAdicionales = request.DatosAdicionales,
            FechaTransaccion = DateTime.UtcNow,
            CreadoPor = _currentUser.UserId ?? "Sistema"
        };

        return transaccion;
    }

    private async Task VerificarAscensoNivel(Cliente cliente, TarjetaFidelizacion tarjeta)
    {
        try
        {
            var resultadoAscenso = await _servicioFidelizacion.VerificarYProcesarAscensoNivelAsync(tarjeta);
            if (resultadoAscenso.Succeeded && resultadoAscenso.Value.HuboAscenso)
            {
                _logger.LogInformation("Cliente {ClienteId} ascendió al nivel {NuevoNivel}", 
                    cliente.Id, resultadoAscenso.Value.NuevoNivel.Nombre);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error verificando ascenso de nivel para cliente {ClienteId}", cliente.Id);
        }
    }

    private async Task ProcesarLogrosEspeciales(Cliente cliente, TarjetaFidelizacion tarjeta, AcumularPuntosCommand request)
    {
        try
        {
            await _servicioFidelizacion.ProcesarLogrosEspecialesAsync(cliente, tarjeta, request.TipoTransaccion);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error procesando logros especiales para cliente {ClienteId}", cliente.Id);
        }
    }

    private static string GenerarMensajeMotivacional(int puntosOtorgados, TarjetaFidelizacion tarjeta)
    {
        var mensajes = new[]
        {
            $"¡Excelente! Ganaste {puntosOtorgados} puntos. ¡Sigue acumulando!",
            $"¡Increíble! {puntosOtorgados} puntos más en tu cuenta. Total: {tarjeta.SaldoPuntos}",
            $"¡Fantástico! +{puntosOtorgados} puntos. ¡Estás cerca de grandes recompensas!",
            $"¡Bien hecho! {puntosOtorgados} puntos ganados. ¡Tu fidelidad tiene recompensa!",
            $"¡Genial! Sumaste {puntosOtorgados} puntos. Total acumulado: {tarjeta.SaldoPuntos}"
        };

        return mensajes[new Random().Next(mensajes.Length)];
    }
} 