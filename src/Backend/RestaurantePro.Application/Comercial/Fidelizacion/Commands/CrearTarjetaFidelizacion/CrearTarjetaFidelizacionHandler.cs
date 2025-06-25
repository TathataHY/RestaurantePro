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

            // Validar puntos iniciales
            if (request.PuntosIniciales < 0)
            {
                _logger.LogWarning("Intento de crear tarjeta con puntos iniciales negativos: {PuntosIniciales}", request.PuntosIniciales);
                return Result.Failure<TarjetaFidelizacionDto>("No se pueden asignar puntos iniciales negativos");
            }

            // Validar configuración especial
            if (request.Configuracion != null)
            {
                if (request.Configuracion.PuntosIniciales < 0)
                {
                    _logger.LogWarning("Configuración con puntos iniciales negativos: {PuntosIniciales}", request.Configuracion.PuntosIniciales);
                    return Result.Failure<TarjetaFidelizacionDto>("La configuración no puede tener puntos iniciales negativos");
                }

                if (request.Configuracion.MultiplicadorPuntos <= 0)
                {
                    _logger.LogWarning("Configuración con multiplicador de puntos inválido: {MultiplicadorPuntos}", request.Configuracion.MultiplicadorPuntos);
                    return Result.Failure<TarjetaFidelizacionDto>("El multiplicador de puntos debe ser mayor que cero");
                }
            }

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

            // 5. Activar la tarjeta si se solicita activación inmediata
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

            // 6. Configurar puntos iniciales si se especificaron
            var puntosIniciales = request.PuntosIniciales > 0 ? request.PuntosIniciales : 
                                (request.Configuracion?.PuntosIniciales ?? 0);
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

            // 7. Aplicar configuraciones especiales si existen
            if (request.Configuracion?.ConfiguracionesEspeciales != null && 
                request.Configuracion.ConfiguracionesEspeciales.Count > 0)
            {
                try
                {
                    AplicarConfiguracionesEspeciales(tarjeta, request.Configuracion);
                    _logger.LogInformation("Configuraciones especiales aplicadas a la tarjeta");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al aplicar configuraciones especiales, continuando con configuración básica");
                }
            }

            // 8. Guardar la tarjeta
            await _tarjetaRepository.AgregarAsync(tarjeta, cancellationToken);

            // 9. Actualizar cliente con referencia a la tarjeta
            cliente.AsociarTarjetaFidelizacion(tarjeta.Id);
            await _clienteRepository.ActualizarAsync(cliente, cancellationToken);

            // 10. Guardar cambios
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);

            // 11. Crear y retornar DTO de respuesta
            var responseDto = CrearResponseDto(tarjeta, cliente);

            // 12. Agregar beneficios especiales al DTO de respuesta si existen
            if (request.Configuracion?.ConfiguracionesEspeciales != null)
            {
                foreach (var config in request.Configuracion.ConfiguracionesEspeciales)
                {
                    if (config.Key == "BeneficioEspecial" && config.Value is string beneficio)
                    {
                        responseDto.BeneficiosDisponibles.Add(new BeneficioDto 
                        { 
                            Id = Guid.NewGuid(),
                            Nombre = "Beneficio Especial",
                            Descripcion = beneficio,
                            TipoBeneficio = "Promocional",
                            Activo = true,
                            PuntosRequeridos = 0,
                            FechaActivacion = _dateTimeService.Now,
                            FechaExpiracion = request.Configuracion.FechaVencimiento ?? _dateTimeService.Now.AddMonths(1)
                        });
                    }
                    else if (config.Key == "TipoPromocion" && config.Value is string tipoPromocion)
                    {
                        responseDto.BeneficiosDisponibles.Add(new BeneficioDto 
                        { 
                            Id = Guid.NewGuid(),
                            Nombre = tipoPromocion,
                            Descripcion = $"Promoción: {tipoPromocion}",
                            TipoBeneficio = "Promoción",
                            Activo = true,
                            PuntosRequeridos = 0,
                            FechaActivacion = _dateTimeService.Now,
                            FechaExpiracion = request.Configuracion.FechaVencimiento ?? _dateTimeService.Now.AddMonths(1)
                        });
                    }
                }
            }

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

    private async Task<string> GenerarCodigoTarjeta(TipoTarjetaFidelizacion tipoTarjeta, CancellationToken cancellationToken)
    {
        string codigo;
        do
        {
            // Generar un código basado en el tipo y timestamp
            var timestamp = _dateTimeService.Now.ToString("yyyyMMddHHmmss");
            var prefijo = tipoTarjeta.ToString().ToUpper().Take(3).Aggregate("", (current, c) => current + c);
            codigo = $"{prefijo}-{timestamp}-{Random.Shared.Next(1000, 9999)}";
        } while (await _tarjetaRepository.ExisteNumeroTarjetaAsync(codigo, cancellationToken));

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

    private void AplicarConfiguracionesEspeciales(TarjetaFidelizacion tarjeta, CrearTarjetaConfiguracion configuracion)
    {
        // Aplicar multiplicador de puntos si está especificado
        if (configuracion.MultiplicadorPuntos > 1.0m)
        {
            tarjeta.ConfigurarMultiplicadorPuntos(configuracion.MultiplicadorPuntos);
            _logger.LogInformation("Multiplicador de puntos configurado: {Multiplicador}", configuracion.MultiplicadorPuntos);
        }

        // Aplicar fecha de vencimiento personalizada si está especificada
        if (configuracion.FechaVencimiento.HasValue)
        {
            tarjeta.ConfigurarFechaExpiracion(configuracion.FechaVencimiento.Value);
            _logger.LogInformation("Fecha de vencimiento configurada: {Fecha}", configuracion.FechaVencimiento.Value);
        }

        // Aplicar límite de puntos mensual si está especificado
        if (configuracion.LimitePuntosMensual.HasValue)
        {
            tarjeta.ConfigurarLimiteMensual(configuracion.LimitePuntosMensual.Value);
            _logger.LogInformation("Límite mensual configurado: {Limite}", configuracion.LimitePuntosMensual.Value);
        }

        // Procesar configuraciones especiales
        if (configuracion.ConfiguracionesEspeciales != null)
        {
            foreach (var config in configuracion.ConfiguracionesEspeciales)
            {
                _logger.LogInformation("Aplicando configuración especial: {Clave}={Valor}", config.Key, config.Value);
                
                // Procesar según el tipo de configuración
                switch (config.Key)
                {
                    case "TipoPromocion":
                        if (config.Value is string tipoPromocion)
                        {
                            tarjeta.AgregarEtiqueta($"Promoción: {tipoPromocion}");
                        }
                        break;
                    case "BeneficioEspecial":
                        if (config.Value is string beneficio)
                        {
                            tarjeta.AgregarEtiqueta($"Beneficio: {beneficio}");
                        }
                        break;
                    default:
                        // Otras configuraciones especiales se pueden agregar como etiquetas genéricas
                        tarjeta.AgregarEtiqueta($"{config.Key}: {config.Value}");
                        break;
                }
            }
        }
    }
} 