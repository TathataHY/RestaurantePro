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
    private readonly IServicioFidelizacion _servicioFidelizacion;
    private readonly IMapper _mapper;
    private readonly ILogger<AcumularPuntosHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public AcumularPuntosHandler(
        IClienteRepository clienteRepository,
        ITarjetaFidelizacionRepository tarjetaRepository,
        ITransaccionPuntosRepository transaccionRepository,
        IPromocionRepository promocionRepository,
        ICalculadoraPuntosService calculadoraPuntos,
        IServicioFidelizacion servicioFidelizacion,
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
                return Result.Failure<AcumulacionPuntosDto>("El cliente especificado no existe");
            }

            // 2. Verificar que el cliente esté activo y elegible para puntos
            if (!cliente.EstaActivo)
            {
                _logger.LogWarning("Cliente {ClienteId} no elegible para acumulación de puntos. Activo: {Activo}", 
                    request.ClienteId, cliente.EstaActivo);
                return Result.Failure<AcumulacionPuntosDto>("El cliente no está elegible para acumular puntos");
            }

            // 3. Obtener o verificar tarjeta de fidelización
            var tarjeta = await ObtenerTarjetaFidelizacion(request, cliente);
            if (tarjeta == null)
            {
                return Result.Failure<AcumulacionPuntosDto>("Error al obtener la tarjeta de fidelización del cliente");
            }

            // 4. Validar promoción si se especifica
            Promocion? promocion = null;
            if (!string.IsNullOrEmpty(request.CodigoPromocion))
            {
                var resultadoPromocion = await ValidarPromocion(request.CodigoPromocion, request.TipoTransaccion);
                if (!resultadoPromocion.Succeeded)
                {
                    return Result.Failure<AcumulacionPuntosDto>(resultadoPromocion.Error ?? "Error validando promoción");
                }
                promocion = resultadoPromocion.Value;
            }

            // 5. Calcular puntos base y aplicar multiplicadores
            var calculoResultado = await CalcularPuntosTransaccion(request, cliente, tarjeta, promocion);
            if (!calculoResultado.Succeeded)
            {
                return Result.Failure<AcumulacionPuntosDto>(calculoResultado.Error ?? "Error calculando puntos");
            }

            var puntosPorOtorgar = calculoResultado.Value.TotalPuntos;

            // 6. Verificar límites diarios/mensuales
            var validacionLimites = await ValidarLimitesAcumulacion(cliente, puntosPorOtorgar, request.TipoTransaccion);
            if (!validacionLimites.Succeeded)
            {
                return Result.Failure<AcumulacionPuntosDto>(validacionLimites.Error ?? "Error validando límites");
            }

            // 7. Registrar transacción de puntos
            var transaccion = await CrearTransaccionPuntos(request, tarjeta, puntosPorOtorgar, promocion);

            // 8. Actualizar saldo de puntos en la tarjeta
            var historialPuntos = tarjeta.AgregarPuntos(
                puntosPorOtorgar, 
                $"Acumulación por {request.TipoTransaccion} - Monto: {request.MontoCompra:C}");

            // 9. Guardar cambios
            await _transaccionRepository.AgregarAsync(transaccion);
            await _tarjetaRepository.ActualizarAsync(tarjeta);

            // 10. Verificar ascensos de nivel
            await VerificarAscensoNivel(cliente, tarjeta);

            // 11. Registrar logros y bonificaciones especiales
            await ProcesarLogrosEspeciales(cliente, tarjeta, request);

            _logger.LogInformation("Puntos acumulados exitosamente para cliente {ClienteId}: {PuntosOtorgados} puntos. Saldo total: {SaldoTotal}", 
                request.ClienteId, puntosPorOtorgar, tarjeta.PuntosDisponibles);

            // 12. Mapear y retornar resultado
            var resultado = new AcumulacionPuntosDto
            {
                ClienteId = cliente.Id,
                TarjetaFidelizacionId = tarjeta.Id,
                PuntosAcumulados = puntosPorOtorgar,
                TotalPuntos = tarjeta.PuntosDisponibles,
                MontoTransaccion = request.MontoCompra,
                FactorMultiplicacion = calculoResultado.Value.TasaConversion,
                FechaAcumulacion = DateTime.UtcNow,
                Concepto = $"Acumulación por {request.TipoTransaccion}",
                FacturaId = request.FacturaId,
                PromocionId = promocion?.Id,
                TienePuntosBonus = request.PuntosBonus > 0,
                PuntosBonus = request.PuntosBonus ?? 0,
                MotivoBonus = request.PuntosBonus > 0 ? "Puntos bonus por transacción especial" : null
            };

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al acumular puntos para cliente {ClienteId}", request.ClienteId);
            return Result.Failure<AcumulacionPuntosDto>("Error interno del servidor al procesar la acumulación de puntos");
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
            tarjeta = await _tarjetaRepository.ObtenerTarjetaActivaPorClienteIdAsync(cliente.Id);
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
            return Result.Failure<Promocion>($"Código de promoción '{codigoPromocion}' no existe");
        }

        if (!promocion.EstaVigente())
        {
            return Result.Failure<Promocion>($"Código de promoción '{codigoPromocion}' no está activo o ha expirado");
        }

        // TODO: Implementar validación de tipo de transacción en Promocion
        // if (!promocion.EsAplicableATipoTransaccion(tipoTransaccion.ToString()))
        // {
        //     return Result.Failure<Promocion>($"Código de promoción '{codigoPromocion}' no es aplicable a este tipo de transacción");
        // }

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
            // Si hay promoción, calcular puntos por promoción
            if (promocion != null)
            {
                var parametrosPromocion = new Dictionary<string, object>
                {
                    ["MontoCompra"] = request.MontoCompra,
                    ["TipoTransaccion"] = request.TipoTransaccion.ToString(),
                    ["Canal"] = request.Canal ?? "Presencial",
                    ["EsFechaEspecial"] = request.EsFechaEspecial,
                    ["PuntosBonus"] = request.PuntosBonus ?? 0
                };

                return await _calculadoraPuntos.CalcularPuntosPorPromocionAsync(
                    tarjeta.Id, 
                    promocion.Id, 
                    parametrosPromocion);
            }
            else
            {
                // Calcular puntos por compra normal
                return await _calculadoraPuntos.CalcularPuntosPorCompraAsync(
                    tarjeta.Id, 
                    request.MontoCompra);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculando puntos para cliente {ClienteId}", cliente.Id);
            return Result.Failure<CalculoResultadoPuntos>("Error en el cálculo de puntos");
        }
    }

    private async Task<Result> ValidarLimitesAcumulacion(Cliente cliente, int puntosAOtorgar, TipoTransaccionPuntos tipoTransaccion)
    {
        // TODO: Implementar validación de límites cuando se implementen los métodos del repositorio
        // Por ahora, usar límites predeterminados simples
        
        // Verificar límites diarios básicos
        var limiteDiario = tipoTransaccion switch
        {
            TipoTransaccionPuntos.AjusteManual => 500,
            TipoTransaccionPuntos.PromocionEspecial => 1000,
            _ => 5000 // Límite general
        };

        if (puntosAOtorgar > limiteDiario)
        {
            return Result.Failure($"Se excede el límite diario de acumulación de puntos. Límite: {limiteDiario}, Intentando acumular: {puntosAOtorgar}");
        }

        return Result.Success();
    }

    private async Task<RestaurantePro.Domain.Comercial.Clientes.Entities.TransaccionPuntos> CrearTransaccionPuntos(
        AcumularPuntosCommand request, 
        TarjetaFidelizacion tarjeta, 
        int puntosOtorgados, 
        Promocion? promocion)
    {
        // Convertir TipoTransaccionPuntos de Command a Domain
        var tipoTransaccionDomain = ConvertirTipoTransaccion(request.TipoTransaccion);
        
        // Usar el constructor de la entidad Domain
        var transaccion = new RestaurantePro.Domain.Comercial.Clientes.Entities.TransaccionPuntos(
            tarjetaFidelizacionId: tarjeta.Id,
            clienteId: tarjeta.ClienteId,
            tipo: tipoTransaccionDomain,
            puntos: puntosOtorgados,
            saldoResultante: tarjeta.PuntosDisponibles + puntosOtorgados,
            descripcion: $"Acumulación por {request.TipoTransaccion} - Monto: {request.MontoCompra:C}",
            usuarioId: Guid.Parse(_currentUser.UserId ?? Guid.Empty.ToString()),
            montoAsociado: request.MontoCompra,
            referenciaExterna: request.ReferenciaExterna,
            promocionId: promocion?.Id,
            facturaId: request.FacturaId,
            observaciones: request.Comentarios
        );

        return transaccion;
    }

    private RestaurantePro.Domain.Comercial.Clientes.Enums.TipoTransaccionPuntos ConvertirTipoTransaccion(TipoTransaccionPuntos tipoCommand)
    {
        return tipoCommand switch
        {
            TipoTransaccionPuntos.Compra => RestaurantePro.Domain.Comercial.Clientes.Enums.TipoTransaccionPuntos.AcumulacionCompra,
            TipoTransaccionPuntos.PromocionEspecial => RestaurantePro.Domain.Comercial.Clientes.Enums.TipoTransaccionPuntos.AcumulacionPromocion,
            TipoTransaccionPuntos.AjusteManual => RestaurantePro.Domain.Comercial.Clientes.Enums.TipoTransaccionPuntos.AjustePositivo,
            TipoTransaccionPuntos.Referido => RestaurantePro.Domain.Comercial.Clientes.Enums.TipoTransaccionPuntos.AcumulacionPromocion,
            TipoTransaccionPuntos.Cumpleanos => RestaurantePro.Domain.Comercial.Clientes.Enums.TipoTransaccionPuntos.AcumulacionPromocion,
            TipoTransaccionPuntos.Aniversario => RestaurantePro.Domain.Comercial.Clientes.Enums.TipoTransaccionPuntos.AcumulacionPromocion,
            TipoTransaccionPuntos.Resena => RestaurantePro.Domain.Comercial.Clientes.Enums.TipoTransaccionPuntos.AcumulacionPromocion,
            TipoTransaccionPuntos.CheckInSocial => RestaurantePro.Domain.Comercial.Clientes.Enums.TipoTransaccionPuntos.AcumulacionPromocion,
            TipoTransaccionPuntos.Evento => RestaurantePro.Domain.Comercial.Clientes.Enums.TipoTransaccionPuntos.AcumulacionPromocion,
            TipoTransaccionPuntos.Suscripcion => RestaurantePro.Domain.Comercial.Clientes.Enums.TipoTransaccionPuntos.AcumulacionPromocion,
            _ => RestaurantePro.Domain.Comercial.Clientes.Enums.TipoTransaccionPuntos.AcumulacionCompra
        };
    }

    private async Task VerificarAscensoNivel(Cliente cliente, TarjetaFidelizacion tarjeta)
    {
        try
        {
            // TODO: Implementar VerificarYProcesarAscensoNivelAsync en IServicioFidelizacion
            // var resultadoAscenso = await _servicioFidelizacion.VerificarYProcesarAscensoNivelAsync(tarjeta);
            // if (resultadoAscenso.Succeeded && resultadoAscenso.Value.HuboAscenso)
            // {
            //     _logger.LogInformation("Cliente {ClienteId} ascendió al nivel {NuevoNivel}", 
            //         cliente.Id, resultadoAscenso.Value.NuevoNivel.Nombre);
            // }
            _logger.LogInformation("Verificación de ascenso de nivel pendiente de implementación para cliente {ClienteId}", cliente.Id);
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
            // TODO: Implementar ProcesarLogrosEspecialesAsync en IServicioFidelizacion
            // await _servicioFidelizacion.ProcesarLogrosEspecialesAsync(cliente, tarjeta, request.TipoTransaccion);
            _logger.LogInformation("Procesamiento de logros especiales pendiente de implementación para cliente {ClienteId}", cliente.Id);
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
            $"¡Increíble! {puntosOtorgados} puntos más en tu cuenta. Total: {tarjeta.PuntosDisponibles}",
            $"¡Fantástico! +{puntosOtorgados} puntos. ¡Estás cerca de grandes recompensas!",
            $"¡Bien hecho! {puntosOtorgados} puntos ganados. ¡Tu fidelidad tiene recompensa!",
            $"¡Genial! Sumaste {puntosOtorgados} puntos. Total acumulado: {tarjeta.PuntosDisponibles}"
        };

        return mensajes[new Random().Next(mensajes.Length)];
    }
} 


