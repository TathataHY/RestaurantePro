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
        try
        {
            _logger.LogInformation("Procesando acumulación de puntos: ClienteId={ClienteId}, TarjetaId={TarjetaId}, Monto={Monto}",
                request.ClienteId, request.TarjetaFidelizacionId, request.MontoCompra);

            // 1. Verificar que el cliente existe
            var cliente = await _clienteRepository.ObtenerPorIdAsync(request.ClienteId);
            if (cliente == null)
            {
                _logger.LogWarning("Cliente {ClienteId} no existe", request.ClienteId);
                return Result.Failure<AcumulacionPuntosDto>($"Cliente con ID {request.ClienteId} no existe");
            }

            if (!cliente.EstaActivo)
            {
                _logger.LogWarning("Cliente {ClienteId} no está activo", request.ClienteId);
                return Result.Failure<AcumulacionPuntosDto>($"Cliente con ID {request.ClienteId} no está activo");
            }

            // 2. Verificar tarjeta de fidelización
            var tarjeta = await ObtenerTarjetaFidelizacion(request, cliente);
            if (tarjeta == null)
            {
                _logger.LogWarning("No se encontró tarjeta de fidelización válida para el cliente {ClienteId}", request.ClienteId);
                return Result.Failure<AcumulacionPuntosDto>("No se encontró una tarjeta de fidelización válida para el cliente");
            }

            // 3. Verificar promoción (si aplica)
            Promocion? promocion = null;
            if (request.PromocionId.HasValue)
            {
                var promocionResult = await ValidarPromocion(request.PromocionId.Value, request.TipoTransaccion);
                if (!promocionResult.Succeeded)
                {
                    return Result.Failure<AcumulacionPuntosDto>(promocionResult.Error);
                }
                promocion = promocionResult.Value;
            }

            // 4. Calcular puntos a otorgar
            var calculoResult = await CalcularPuntosTransaccion(request, cliente, tarjeta, promocion, cancellationToken);
            if (!calculoResult.Succeeded)
            {
                return Result.Failure<AcumulacionPuntosDto>(calculoResult.Error);
            }

            var puntosAOtorgar = calculoResult.Value.TotalPuntos;
            
            if (puntosAOtorgar <= 0)
            {
                return Result.Failure<AcumulacionPuntosDto>($"La cantidad de puntos acumulados no puede ser cero o negativa. Valor: {puntosAOtorgar}");
            }

            // 5. Validar límites de acumulación
            var limiteResult = await ValidarLimitesAcumulacion(cliente, puntosAOtorgar, request.TipoTransaccion);
            if (!limiteResult.Succeeded)
            {
                return Result.Failure<AcumulacionPuntosDto>(limiteResult.Error);
            }

            // 6. Registrar transacción
            var transaccion = CrearTransaccionPuntos(request, tarjeta, puntosAOtorgar, promocion);
            await _transaccionRepository.AgregarAsync(transaccion, cancellationToken);

            // 7. Actualizar saldo de puntos
            var historialPuntos = tarjeta.AgregarPuntos(puntosAOtorgar, $"Acumulación por {request.TipoTransaccion} - Monto: {request.MontoCompra:C}");
            await _tarjetaRepository.ActualizarAsync(tarjeta, cancellationToken);

            // 8. Verificar ascensos de nivel
            await VerificarAscensoNivel(cliente, tarjeta);

            // 9. Procesar logros especiales
            await ProcesarLogrosEspeciales(cliente, tarjeta, request);

            // 10. Generar respuesta
            var dto = new AcumulacionPuntosDto
            {
                TarjetaFidelizacionId = tarjeta.Id,
                ClienteId = cliente.Id,
                PuntosAcumulados = puntosAOtorgar,
                TotalPuntos = tarjeta.PuntosDisponibles,
                FacturaId = request.FacturaId,
                PromocionId = promocion?.Id,
                FechaAcumulacion = DateTime.UtcNow,
                MontoTransaccion = request.MontoCompra,
                FactorMultiplicacion = calculoResult.Value.TasaConversion,
                Concepto = $"Acumulación por {request.TipoTransaccion}",
                TransaccionId = transaccion.Id,
                TienePuntosBonus = request.PuntosBonus > 0,
                PuntosBonus = request.PuntosBonus ?? 0,
                MotivoBonus = request.PuntosBonus > 0 ? "Puntos bonus por transacción especial" : null
            };

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno del servidor al procesar la acumulación de puntos");
            if (ex.InnerException != null)
            {
                _logger.LogError(ex.InnerException, "Inner exception details");
            }
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

    private async Task<Result<Promocion>> ValidarPromocion(int promocionId, TipoTransaccionPuntos tipoTransaccion)
    {
        var promocion = await _promocionRepository.ObtenerPorIdAsync(promocionId);
        if (promocion == null)
        {
            return Result.Failure<Promocion>($"Promoción con ID {promocionId} no existe");
        }

        if (!promocion.EstaVigente())
        {
            return Result.Failure<Promocion>($"Promoción con ID {promocionId} no está activo o ha expirado");
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
        Promocion? promocion,
        CancellationToken cancellationToken)
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
                    parametrosPromocion,
                    cancellationToken);
            }
            else
            {
                // Cálculo normal por compra
                return await _calculadoraPuntos.CalcularPuntosPorCompraAsync(
                    tarjeta.Id, 
                    request.MontoCompra,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculando puntos para transacción");
            return Result.Failure<CalculoResultadoPuntos>($"Error al calcular puntos: {ex.Message}");
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

    private RestaurantePro.Domain.Comercial.Clientes.Entities.TransaccionPuntos CrearTransaccionPuntos(
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

    /// <summary>
    /// Genera un mensaje motivacional para el cliente basado en los puntos acumulados
    /// </summary>
    private string GenerarMensajeMotivacional(int puntosAcumulados, TarjetaFidelizacion tarjeta)
    {
        // Generar mensajes personalizados según puntos acumulados
        if (puntosAcumulados >= 500)
        {
            return $"¡Excelente! Has acumulado {puntosAcumulados} puntos. Estás muy cerca de alcanzar el siguiente nivel.";
        }
        else if (puntosAcumulados >= 100)
        {
            return $"¡Genial! Has sumado {puntosAcumulados} puntos a tu cuenta. Ya tienes {tarjeta.PuntosDisponibles} puntos disponibles.";
        }
        else
        {
            return $"¡Gracias por tu compra! Has acumulado {puntosAcumulados} puntos.";
        }
    }
} 


