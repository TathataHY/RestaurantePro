namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.CrearTarjetaFidelizacion;

/// <summary>
/// Handler para crear tarjetas de fidelización
/// Gestiona el proceso completo de creación, configuración y activación de tarjetas
/// </summary>
public class CrearTarjetaFidelizacionHandler : IRequestHandler<CrearTarjetaFidelizacionCommand, Result<TarjetaFidelizacionDto>>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CrearTarjetaFidelizacionHandler> _logger;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeService _dateTimeService;
    private readonly IUnitOfWork _unitOfWork;

    public CrearTarjetaFidelizacionHandler(
        IClienteRepository clienteRepository,
        ITarjetaFidelizacionRepository tarjetaRepository,
        IMapper mapper,
        ILogger<CrearTarjetaFidelizacionHandler> logger,
        ICurrentUserService currentUser,
        IDateTimeService dateTimeService,
        IUnitOfWork unitOfWork)
    {
        _clienteRepository = clienteRepository;
        _tarjetaRepository = tarjetaRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
        _dateTimeService = dateTimeService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TarjetaFidelizacionDto>> Handle(CrearTarjetaFidelizacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando creación de tarjeta de fidelización para Cliente {ClienteId}, Tipo: {TipoTarjeta}",
                request.ClienteId, request.TipoTarjeta);

            // 1. Obtener y validar el cliente
            var cliente = await _clienteRepository.ObtenerPorIdAsync(request.ClienteId, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning("Cliente {ClienteId} no encontrado", request.ClienteId);
                return Result.Failure<TarjetaFidelizacionDto>("Cliente no encontrado");
            }

            // 2. Validar elegibilidad del cliente
            var validacionResult = ValidarElegibilidadCliente(cliente);
            if (!validacionResult.Succeeded)
            {
                var errorMessage = validacionResult.Error ?? "Error de validación";
                return Result.Failure<TarjetaFidelizacionDto>(errorMessage);
            }

            // 3. Generar código único para la tarjeta
            var codigoTarjeta = await GenerarCodigoTarjeta(request.TipoTarjeta, cancellationToken);

            // 4. Crear la tarjeta de fidelización
            var tarjeta = TarjetaFidelizacion.Crear(cliente.Id, codigoTarjeta);

            // 5. Configurar puntos iniciales si se especificaron
            var puntosIniciales = request.Configuracion?.PuntosIniciales ?? 0;
            if (puntosIniciales > 0)
            {
                try
                {
                    var historial = tarjeta.AgregarPuntos(puntosIniciales, "Puntos de bienvenida al crear tarjeta");
                    _logger.LogInformation("Puntos iniciales agregados: {Puntos}", puntosIniciales);
                }
                catch (Exception ex)
                {
                    return Result.Failure<TarjetaFidelizacionDto>($"Error configurando puntos iniciales: {ex.Message}");
                }
            }

            // 6. Activar la tarjeta si se solicita activación inmediata
            if (request.ActivarInmediatamente)
            {
                try
                {
                    tarjeta.Activar();
                    _logger.LogInformation("Tarjeta activada inmediatamente");
                }
                catch (Exception ex)
                {
                    return Result.Failure<TarjetaFidelizacionDto>($"Error activando tarjeta: {ex.Message}");
                }
            }

            // 7. Guardar la tarjeta
            await _tarjetaRepository.AgregarAsync(tarjeta, cancellationToken);

            // 8. Actualizar cliente con referencia a la tarjeta
            cliente.AsociarTarjetaFidelizacion(tarjeta.Id);
            await _clienteRepository.ActualizarAsync(cliente, cancellationToken);

            // 9. Guardar cambios
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);

            // 10. Crear y retornar DTO de respuesta
            var responseDto = CrearResponseDto(tarjeta, cliente);

            _logger.LogInformation("Tarjeta de fidelización creada exitosamente. TarjetaId: {TarjetaId}, Código: {Codigo}",
                tarjeta.Id, tarjeta.Codigo);

            return Result.Success<TarjetaFidelizacionDto>(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando tarjeta de fidelización para Cliente {ClienteId}", request.ClienteId);
            return Result.Failure<TarjetaFidelizacionDto>("Error interno al crear la tarjeta de fidelización");
        }
    }

    private Result ValidarElegibilidadCliente(Cliente cliente)
    {
        // Validar que el cliente esté activo
        if (!cliente.EstaActivo)
        {
            return Result.Failure("El cliente no está activo y no puede tener tarjeta de fidelización");
        }

        // Validar que no tenga ya una tarjeta principal
        if (cliente.TarjetaFidelizacionPrincipalId.HasValue)
        {
            return Result.Failure("El cliente ya tiene una tarjeta de fidelización principal");
        }

        return Result.Success();
    }

    private async Task<string> GenerarCodigoTarjeta(string tipoTarjeta, CancellationToken cancellationToken)
    {
        // Generar un código basado en el tipo y timestamp
        var timestamp = _dateTimeService.Now.ToString("yyyyMMddHHmmss");
        var prefijo = tipoTarjeta.ToUpper().Take(3).Aggregate("", (current, c) => current + c);
        var codigo = $"{prefijo}-{timestamp}-{Random.Shared.Next(1000, 9999)}";

        // Verificar que el código sea único
        var existente = await _tarjetaRepository.ObtenerPorCodigoAsync(codigo, cancellationToken);
        if (existente != null)
        {
            // Si existe, agregar un sufijo adicional
            codigo += $"-{Random.Shared.Next(100, 999)}";
        }

        return codigo;
    }

    private TarjetaFidelizacionDto CrearResponseDto(TarjetaFidelizacion tarjeta, Cliente cliente)
    {
        return new TarjetaFidelizacionDto
        {
            Id = tarjeta.Id,
            NumeroTarjeta = tarjeta.Codigo,
            ClienteId = cliente.Id,
            NombreCliente = cliente.Nombre.NombreCompleto,
            Nivel = tarjeta.NivelFidelizacion,
            PuntosActuales = tarjeta.PuntosDisponibles,
            TotalPuntosGanados = tarjeta.PuntosAcumulados,
            TotalPuntosCanjeados = tarjeta.PuntosAcumulados - tarjeta.PuntosDisponibles,
            FechaEmision = tarjeta.FechaEmision,
            FechaVencimiento = tarjeta.FechaExpiracion,
            Estado = tarjeta.Estado.ToString(),
            Activa = tarjeta.Estado == EstadoTarjeta.Activa,
            Observaciones = "Tarjeta creada automáticamente",
            BeneficiosDisponibles = new List<BeneficioDto>(),
            TransaccionesRecientes = new List<TransaccionPuntosDto>()
        };
    }
} 