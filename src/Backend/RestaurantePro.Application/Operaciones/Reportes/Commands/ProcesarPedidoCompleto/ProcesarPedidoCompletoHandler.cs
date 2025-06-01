namespace RestaurantePro.Application.Operaciones.Reportes.Commands.ProcesarPedidoCompleto;

/// <summary>
/// Handler para procesar pedidos completos desde comanda hasta facturación
/// Orquesta todo el workflow de procesamiento de pedidos
/// </summary>
public class ProcesarPedidoCompletoHandler : IRequestHandler<ProcesarPedidoCompletoCommand, Result<ProcesarPedidoCompletoDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IFacturaRepository _facturaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IMesaRepository _mesaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProcesarPedidoCompletoHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeService _dateTimeService;
    private readonly IMediator _mediator;
    private readonly IServicioFacturacion _servicioFacturacion;
    private readonly IComercialServiceFacade _comercialServiceFacade;

    public ProcesarPedidoCompletoHandler(
        IComandaRepository comandaRepository,
        IFacturaRepository facturaRepository,
        IClienteRepository clienteRepository,
        IMesaRepository mesaRepository,
        IMapper mapper,
        ILogger<ProcesarPedidoCompletoHandler> logger,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IDateTimeService dateTimeService,
        IMediator mediator,
        IServicioFacturacion servicioFacturacion,
        IComercialServiceFacade comercialServiceFacade)
    {
        _comandaRepository = comandaRepository;
        _facturaRepository = facturaRepository;
        _clienteRepository = clienteRepository;
        _mesaRepository = mesaRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _dateTimeService = dateTimeService;
        _mediator = mediator;
        _servicioFacturacion = servicioFacturacion;
        _comercialServiceFacade = comercialServiceFacade;
    }

    public async Task<Result<ProcesarPedidoCompletoDto>> Handle(ProcesarPedidoCompletoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Iniciando procesamiento completo de pedido - Comanda: {ComandaId}, Tipo Pago: {TipoPago}",
            request.ComandaId, request.TipoPago);

        try
        {
            // 1. Validar y obtener comanda
            var comandaResult = await ValidarYObtenerComanda(request.ComandaId, cancellationToken);
            if (!comandaResult.Succeeded)
            {
                return Result.Failure<ProcesarPedidoCompletoDto>(comandaResult.Error);
            }

            var comanda = comandaResult.Value;

            // 2. Procesar pago si es necesario
            var pagoResult = await ProcesarPago(comanda, request, cancellationToken);
            if (!pagoResult.Succeeded)
            {
                return Result.Failure<ProcesarPedidoCompletoDto>(pagoResult.Error);
            }

            // 3. Finalizar comanda si no está finalizada
            var finalizacionResult = await FinalizarComandaSiEsNecesario(comanda, cancellationToken);
            if (!finalizacionResult.Succeeded)
            {
                return Result.Failure<ProcesarPedidoCompletoDto>(finalizacionResult.Error);
            }

            // 4. Crear factura
            var facturaResult = await CrearFactura(comanda, request, cancellationToken);
            if (!facturaResult.Succeeded)
            {
                return Result.Failure<ProcesarPedidoCompletoDto>(facturaResult.Error);
            }

            var factura = facturaResult.Value;

            // 5. Procesar fidelización si hay cliente
            var fidelizacionResult = await ProcesarFidelizacion(comanda, factura, cancellationToken);

            // 6. Liberar mesa
            var liberacionResult = await LiberarMesa(comanda, cancellationToken);

            // 7. Generar resultado consolidado
            var resultado = await GenerarResultadoConsolidado(comanda, factura, request, fidelizacionResult, liberacionResult, cancellationToken);

            _logger.LogInformation("✅ Pedido procesado completamente - Comanda: {ComandaId}, Factura: {FacturaId}",
                request.ComandaId, factura.Id);

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error procesando pedido completo {ComandaId}: {Error}",
                request.ComandaId, ex.Message);
            return Result.Failure<ProcesarPedidoCompletoDto>("Error interno al procesar el pedido completo");
        }
    }

    private async Task<Result<Comanda>> ValidarYObtenerComanda(Guid comandaId, CancellationToken cancellationToken)
    {
        var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
        if (comanda == null)
        {
            _logger.LogWarning("⚠️ Comanda no encontrada: {ComandaId}", comandaId);
            return Result.Failure<Comanda>("La comanda especificada no existe");
        }

        if (comanda.Estado == EstadoComanda.Cancelada)
        {
            _logger.LogWarning("⚠️ Intento de procesar comanda cancelada: {ComandaId}", comandaId);
            return Result.Failure<Comanda>("No se puede procesar una comanda cancelada");
        }

        if (!comanda.ItemsComanda.Any())
        {
            _logger.LogWarning("⚠️ Comanda sin items: {ComandaId}", comandaId);
            return Result.Failure<Comanda>("No se puede procesar una comanda sin items");
        }

        return Result.Success(comanda);
    }

    private async Task<Result> ProcesarPago(Comanda comanda, ProcesarPedidoCompletoCommand request, CancellationToken cancellationToken)
    {
        if (request.RequierePago && request.InfoPago != null)
        {
            try
            {
                _logger.LogInformation("💳 Procesando pago para comanda {ComandaId} - Tipo: {TipoPago}, Monto: {Monto}",
                    comanda.Id, request.TipoPago, comanda.Total);

                // Aquí se integraría con el sistema de pagos real
                // Por ahora simulamos el procesamiento
                if (request.TipoPago == "Tarjeta")
                {
                    var pagoTarjetaResult = await ProcesarPagoTarjeta(comanda, request.InfoPago, cancellationToken);
                    if (!pagoTarjetaResult.Succeeded)
                    {
                        return pagoTarjetaResult;
                    }
                }

                _logger.LogInformation("✅ Pago procesado exitosamente para comanda {ComandaId}", comanda.Id);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error procesando pago para comanda {ComandaId}: {Error}", comanda.Id, ex.Message);
                return Result.Failure("Error procesando el pago");
            }
        }

        return Result.Success();
    }

    private async Task<Result> ProcesarPagoTarjeta(Comanda comanda, InfoPagoDto infoPago, CancellationToken cancellationToken)
    {
        // Simulación de procesamiento de pago con tarjeta
        // En implementación real, aquí se llamaría al gateway de pagos
        
        if (string.IsNullOrEmpty(infoPago.NumeroTarjeta) || infoPago.NumeroTarjeta.Length < 16)
        {
            return Result.Failure("Número de tarjeta inválido");
        }

        if (infoPago.MontoTotal != comanda.Total)
        {
            return Result.Failure("El monto del pago no coincide con el total de la comanda");
        }

        // Simular tiempo de procesamiento
        await Task.Delay(500, cancellationToken);

        _logger.LogInformation("💳 Pago con tarjeta procesado - Comanda: {ComandaId}, Monto: {Monto}",
            comanda.Id, infoPago.MontoTotal);

        return Result.Success();
    }

    private async Task<Result> FinalizarComandaSiEsNecesario(Comanda comanda, CancellationToken cancellationToken)
    {
        if (comanda.Estado != EstadoComanda.Finalizada)
        {
            try
            {
                var finalizarCommand = new FinalizarComandaCommand
                {
                    ComandaId = comanda.Id,
                    UsuarioId = _currentUserService.UserId,
                    FechaFinalizacion = _dateTimeService.Now,
                    ObservacionesFinalizacion = "Finalizada automáticamente al procesar pedido completo",
                    ValidarTodosItemsListos = true,
                    NotificarMesero = false
                };

                var result = await _mediator.Send(finalizarCommand, cancellationToken);
                if (!result.Succeeded)
                {
                    return Result.Failure($"Error finalizando comanda: {result.Error}");
                }

                _logger.LogInformation("✅ Comanda finalizada automáticamente: {ComandaId}", comanda.Id);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error finalizando comanda {ComandaId}: {Error}", comanda.Id, ex.Message);
                return Result.Failure("Error finalizando la comanda");
            }
        }

        return Result.Success();
    }

    private async Task<Result<Factura>> CrearFactura(Comanda comanda, ProcesarPedidoCompletoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var crearFacturaCommand = new CrearFacturaCommand
            {
                ComandasIds = new List<Guid> { comanda.Id },
                TipoFactura = request.TipoFactura ?? "Normal",
                NombreCliente = request.NombreCliente,
                IdentificacionCliente = request.IdentificacionCliente,
                DireccionCliente = request.DireccionCliente,
                TelefonoCliente = request.TelefonoCliente,
                EmailCliente = request.EmailCliente,
                ObservacionesFactura = request.ObservacionesFactura,
                UsuarioId = _currentUserService.UserId
            };

            var result = await _mediator.Send(crearFacturaCommand, cancellationToken);
            if (!result.Succeeded)
            {
                return Result.Failure<Factura>($"Error creando factura: {result.Error}");
            }

            var facturaDto = result.Value;
            var factura = await _facturaRepository.ObtenerPorIdAsync(facturaDto.Id, cancellationToken);

            _logger.LogInformation("📄 Factura creada: {FacturaId} para comanda {ComandaId}", facturaDto.Id, comanda.Id);
            return Result.Success(factura!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creando factura para comanda {ComandaId}: {Error}", comanda.Id, ex.Message);
            return Result.Failure<Factura>("Error creando la factura");
        }
    }

    private async Task<Result> ProcesarFidelizacion(Comanda comanda, Factura factura, CancellationToken cancellationToken)
    {
        if (comanda.ClienteId.HasValue)
        {
            try
            {
                var resultado = await _comercialServiceFacade.AcumularPuntosPorCompraAsync(
                    comanda.ClienteId.Value,
                    factura.Total,
                    factura.Id,
                    "Compra - Procesamiento pedido completo",
                    cancellationToken);

                if (resultado.Succeeded)
                {
                    _logger.LogInformation("🎯 Puntos de fidelización acumulados para cliente {ClienteId}: {Puntos}",
                        comanda.ClienteId.Value, resultado.Value);
                    return Result.Success();
                }
                else
                {
                    _logger.LogWarning("⚠️ Error acumulando puntos de fidelización: {Error}", resultado.Error);
                    return Result.Failure(resultado.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error procesando fidelización para cliente {ClienteId}: {Error}",
                    comanda.ClienteId.Value, ex.Message);
                return Result.Failure("Error procesando fidelización");
            }
        }

        return Result.Success();
    }

    private async Task<Result> LiberarMesa(Comanda comanda, CancellationToken cancellationToken)
    {
        if (comanda.MesaId.HasValue)
        {
            try
            {
                var liberarMesaCommand = new LiberarMesaCommand
                {
                    MesaId = comanda.MesaId.Value,
                    UsuarioId = _currentUserService.UserId,
                    MotivoLiberacion = "Mesa liberada al completar pedido",
                    LimpiezaRequerida = true,
                    NotificarPersonalLimpieza = true
                };

                var result = await _mediator.Send(liberarMesaCommand, cancellationToken);
                if (result.Succeeded)
                {
                    _logger.LogInformation("🪑 Mesa liberada exitosamente: {MesaId}", comanda.MesaId.Value);
                    return Result.Success();
                }
                else
                {
                    _logger.LogWarning("⚠️ Error liberando mesa {MesaId}: {Error}", comanda.MesaId.Value, result.Error);
                    return Result.Failure(result.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error liberando mesa {MesaId}: {Error}", comanda.MesaId.Value, ex.Message);
                return Result.Failure("Error liberando la mesa");
            }
        }

        return Result.Success();
    }

    private async Task<ProcesarPedidoCompletoDto> GenerarResultadoConsolidado(
        Comanda comanda,
        Factura factura,
        ProcesarPedidoCompletoCommand request,
        Result fidelizacionResult,
        Result liberacionResult,
        CancellationToken cancellationToken)
    {
        var comandaDto = _mapper.Map<ComandaDto>(comanda);
        var facturaDto = _mapper.Map<FacturaDto>(factura);

        var resultado = new ProcesarPedidoCompletoDto
        {
            ComandaId = comanda.Id,
            FacturaId = factura.Id,
            Comanda = comandaDto,
            Factura = facturaDto,
            TipoPago = request.TipoPago,
            MontoTotal = factura.Total,
            FechaProcesamiento = _dateTimeService.Now,
            UsuarioId = _currentUserService.UserId,
            PagoExitoso = request.RequierePago ? true : null,
            FidelizacionProcesada = fidelizacionResult.Succeeded,
            MesaLiberada = liberacionResult.Succeeded,
            Observaciones = request.ObservacionesFactura
        };

        // Agregar información adicional si hay cliente
        if (comanda.ClienteId.HasValue)
        {
            var cliente = await _clienteRepository.ObtenerPorIdAsync(comanda.ClienteId.Value, cancellationToken);
            if (cliente != null)
            {
                resultado.ClienteId = cliente.Id;
                resultado.NombreCliente = cliente.Nombre.ToString();
            }
        }

        return resultado;
    }
} 