namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.CrearTarjetaFidelizacion;

/// <summary>
/// Handler para crear tarjetas de fidelización
/// Gestiona el proceso completo de creación, configuración y activación de tarjetas
/// </summary>
public class CrearTarjetaFidelizacionHandler : IRequestHandler<CrearTarjetaFidelizacionCommand, Result<TarjetaFidelizacionDto>>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly ITransaccionPuntosRepository _transaccionRepository;
    private readonly IPromocionRepository _promocionRepository;
    private readonly IBeneficioRepository _beneficioRepository;
    private readonly ITarjetaFidelizacionBuilder _tarjetaBuilder;
    private readonly IGeneradorNumeroTarjetaService _generadorNumero;
    private readonly IQrCodeService _qrCodeService;
    private readonly INotificacionService _notificacionService;
    private readonly IEnvioTarjetaService _envioTarjetaService;
    private readonly IMapper _mapper;
    private readonly ILogger<CrearTarjetaFidelizacionHandler> _logger;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeService _dateTimeService;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public CrearTarjetaFidelizacionHandler(
        IClienteRepository clienteRepository,
        ITarjetaFidelizacionRepository tarjetaRepository,
        ITransaccionPuntosRepository transaccionRepository,
        IPromocionRepository promocionRepository,
        IBeneficioRepository beneficioRepository,
        ITarjetaFidelizacionBuilder tarjetaBuilder,
        IGeneradorNumeroTarjetaService generadorNumero,
        IQrCodeService qrCodeService,
        INotificacionService notificacionService,
        IEnvioTarjetaService envioTarjetaService,
        IMapper mapper,
        ILogger<CrearTarjetaFidelizacionHandler> logger,
        ICurrentUserService currentUser,
        IDateTimeService dateTimeService,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _clienteRepository = clienteRepository;
        _tarjetaRepository = tarjetaRepository;
        _transaccionRepository = transaccionRepository;
        _promocionRepository = promocionRepository;
        _beneficioRepository = beneficioRepository;
        _tarjetaBuilder = tarjetaBuilder;
        _generadorNumero = generadorNumero;
        _qrCodeService = qrCodeService;
        _notificacionService = notificacionService;
        _envioTarjetaService = envioTarjetaService;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
        _dateTimeService = dateTimeService;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TarjetaFidelizacionDto>> Handle(CrearTarjetaFidelizacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando creación de tarjeta de fidelización para Cliente {ClienteId}, Tipo: {TipoTarjeta}",
                request.ClienteId, request.TipoTarjeta);

            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // 1. Obtener y validar el cliente
                var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId, cancellationToken);
                if (cliente == null)
                {
                    _logger.LogWarning("Cliente {ClienteId} no encontrado", request.ClienteId);
                    return Result<TarjetaFidelizacionDto>.Failure("Cliente no encontrado");
                }

                // 2. Validar elegibilidad y restricciones del cliente
                var validacionElegibilidad = await ValidarElegibilidadCliente(cliente, request, cancellationToken);
                if (!validacionElegibilidad.Succeeded)
                {
                    return Result<TarjetaFidelizacionDto>.Failure(validacionElegibilidad.Error);
                }

                // 3. Procesar promoción si existe
                Promocion? promocion = null;
                if (!string.IsNullOrEmpty(request.CodigoPromocion))
                {
                    var promocionResult = await ProcesarPromocion(request.CodigoPromocion, request, cancellationToken);
                    if (!promocionResult.Succeeded)
                    {
                        return Result<TarjetaFidelizacionDto>.Failure(promocionResult.Error);
                    }
                    promocion = promocionResult.Value;
                }

                // 4. Generar número de tarjeta único
                var numeroTarjeta = await GenerarNumeroTarjeta(request, cancellationToken);

                // 5. Configurar fechas de activación y vencimiento
                var fechas = ConfigurarFechasTarjeta(request);

                // 6. Crear configuración de tarjeta
                var configuracion = CrearConfiguracionTarjeta(request, promocion);

                // 7. Construir la tarjeta usando el builder del dominio
                var tarjetaResult = _tarjetaBuilder
                    .ParaCliente(cliente.Id)
                    .ConNumero(numeroTarjeta)
                    .DeTipo(request.TipoTarjeta)
                    .ConNivel(request.NivelInicial)
                    .ConFechaActivacion(fechas.FechaActivacion)
                    .ConFechaVencimiento(fechas.FechaVencimiento)
                    .ConConfiguracion(configuracion)
                    .EsPrincipal(request.EsPrincipal)
                    .ConSucursalEmision(request.SucursalEmision)
                    .ConCanal(request.Canal)
                    .ConMotivoEmision(request.MotivoEmision)
                    .ConPersonalizacion(request.Personalizacion)
                    .ConDatosAdicionales(request.DatosAdicionales)
                    .Construir();

                if (!tarjetaResult.Succeeded)
                {
                    _logger.LogWarning("Error al construir tarjeta: {Error}", tarjetaResult.Error);
                    return Result<TarjetaFidelizacionDto>.Failure(tarjetaResult.Error);
                }

                var tarjeta = tarjetaResult.Value;

                // 8. Generar códigos QR y de barras
                await GenerarCodigosIdentificacion(tarjeta, cancellationToken);

                // 9. Si es tarjeta principal, desactivar otras tarjetas principales
                if (request.EsPrincipal)
                {
                    await DesactivarTarjetasPrincipalesExistentes(cliente.Id, cancellationToken);
                }

                // 10. Guardar la tarjeta
                await _tarjetaRepository.AddAsync(tarjeta, cancellationToken);

                // 11. Procesar puntos iniciales si los hay
                if (request.PuntosIniciales > 0)
                {
                    await ProcesarPuntosIniciales(tarjeta, request.PuntosIniciales, promocion, cancellationToken);
                }

                // 12. Activar beneficios especiales
                if (request.BeneficiosEspeciales?.Any() == true)
                {
                    await ActivarBeneficiosEspeciales(tarjeta.Id, request.BeneficiosEspeciales, cancellationToken);
                }

                // 13. Registrar auditoría
                await _auditService.RegistrarEventoAsync(
                    "TarjetaFidelizacionCreada",
                    $"Tarjeta {tarjeta.NumeroTarjeta} creada para cliente {cliente.NombreCompleto}",
                    cliente.Id,
                    _currentUser.UserId,
                    new { TipoTarjeta = request.TipoTarjeta, NivelInicial = request.NivelInicial },
                    cancellationToken);

                // 14. Enviar notificación al cliente
                if (request.NotificarCliente)
                {
                    await EnviarNotificacionCreacion(cliente, tarjeta, cancellationToken);
                }

                // 15. Procesar envío de tarjeta física si corresponde
                if (request.EnviarTarjetaFisica)
                {
                    await ProgramarEnvioTarjetaFisica(tarjeta, request.DireccionEnvio, cancellationToken);
                }

                // 16. Confirmar transacción
                await transaction.CommitAsync(cancellationToken);

                // 17. Construir y retornar DTO de respuesta
                var responseDto = await CrearResponseDto(tarjeta, cliente, cancellationToken);

                _logger.LogInformation("Tarjeta de fidelización creada exitosamente. TarjetaId: {TarjetaId}, Número: {NumeroTarjeta}",
                    tarjeta.Id, tarjeta.NumeroTarjeta);

                return Result<TarjetaFidelizacionDto>.Success(responseDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando tarjeta de fidelización para Cliente {ClienteId}", request.ClienteId);
            return Result<TarjetaFidelizacionDto>.Failure("Error interno al crear la tarjeta de fidelización");
        }
    }

    private async Task<Result> ValidarElegibilidadCliente(Cliente cliente, CrearTarjetaFidelizacionCommand request, CancellationToken cancellationToken)
    {
        // Validar que el cliente esté activo
        if (!cliente.Activo)
        {
            return Result.Failure("El cliente no está activo y no puede tener tarjeta de fidelización");
        }

        // Validar que no tenga tarjeta principal si se quiere crear una principal
        if (request.EsPrincipal)
        {
            var tarjetaPrincipalExistente = await _tarjetaRepository.GetPrincipalByClienteIdAsync(cliente.Id, cancellationToken);
            if (tarjetaPrincipalExistente != null && tarjetaPrincipalExistente.EstaActiva())
            {
                return Result.Failure("El cliente ya tiene una tarjeta principal activa");
            }
        }

        // Validar límites por tipo de tarjeta
        var validacionTipo = await ValidarLimiteTipoTarjeta(cliente, request.TipoTarjeta, cancellationToken);
        if (!validacionTipo.Succeeded)
        {
            return Result.Failure<bool>(validacionTipo.Error);
        }

        return Result.Success();
    }

    private async Task<Result> ValidarLimiteTipoTarjeta(Cliente cliente, TipoTarjetaFidelizacion tipoTarjeta, CancellationToken cancellationToken)
    {
        var tarjetasExistentes = await _tarjetaRepository.GetByClienteIdAsync(cliente.Id, cancellationToken);
        var tarjetasActivas = tarjetasExistentes.Where(t => t.EstaActiva()).ToList();

        return tipoTarjeta switch
        {
            TipoTarjetaFidelizacion.Estandar => tarjetasActivas.Count >= 3 
                ? Result.Failure("El cliente no puede tener más de 3 tarjetas estándar activas") 
                : Result.Success(),
            
            TipoTarjetaFidelizacion.Premium => tarjetasActivas.Any(t => t.TipoTarjeta == TipoTarjetaFidelizacion.Premium) 
                ? Result.Failure("El cliente ya tiene una tarjeta Premium") 
                : Result.Success(),
            
            TipoTarjetaFidelizacion.Vip => tarjetasActivas.Any(t => t.TipoTarjeta == TipoTarjetaFidelizacion.Vip) 
                ? Result.Failure("El cliente ya tiene una tarjeta VIP") 
                : Result.Success(),
            
            _ => Result.Success()
        };
    }

    private async Task<Result<Promocion>> ProcesarPromocion(string codigoPromocion, CrearTarjetaFidelizacionCommand request, CancellationToken cancellationToken)
    {
        var promocion = await _promocionRepository.GetActiveByCodigo(codigoPromocion, cancellationToken);
        
        if (promocion == null)
        {
            return Result<Promocion>.Failure($"Código de promoción '{codigoPromocion}' no encontrado o expirado");
        }

        if (!promocion.EsAplicableACreacionTarjeta(request.TipoTarjeta))
        {
            return Result<Promocion>.Failure($"Promoción '{codigoPromocion}' no es aplicable a tarjetas tipo {request.TipoTarjeta}");
        }

        return Result<Promocion>.Success(promocion);
    }

    private async Task<string> GenerarNumeroTarjeta(CrearTarjetaFidelizacionCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.NumeroTarjeta))
        {
            // Verificar que el número personalizado sea único
            var existeNumero = await _tarjetaRepository.ExisteNumeroTarjetaAsync(request.NumeroTarjeta, cancellationToken);
            if (existeNumero)
            {
                throw new InvalidOperationException($"El número de tarjeta '{request.NumeroTarjeta}' ya existe");
            }
            return request.NumeroTarjeta;
        }

        // Generar número automático según el tipo de tarjeta
        return await _generadorNumero.GenerarNumeroAsync(request.TipoTarjeta, request.ClienteId, cancellationToken);
    }

    private (DateTime FechaActivacion, DateTime FechaVencimiento) ConfigurarFechasTarjeta(CrearTarjetaFidelizacionCommand request)
    {
        var fechaActivacion = request.FechaActivacion ?? _dateTimeService.Now;
        
        var fechaVencimiento = request.FechaVencimiento ?? request.TipoTarjeta switch
        {
            TipoTarjetaFidelizacion.Estandar => fechaActivacion.AddYears(2),
            TipoTarjetaFidelizacion.Premium => fechaActivacion.AddYears(3),
            TipoTarjetaFidelizacion.Vip => fechaActivacion.AddYears(5),
            TipoTarjetaFidelizacion.Corporativa => fechaActivacion.AddYears(3),
            TipoTarjetaFidelizacion.Empleado => fechaActivacion.AddYears(1),
            TipoTarjetaFidelizacion.Promocional => fechaActivacion.AddMonths(6),
            _ => fechaActivacion.AddYears(2)
        };

        return (fechaActivacion, fechaVencimiento);
    }

    private ConfiguracionTarjeta CrearConfiguracionTarjeta(CrearTarjetaFidelizacionCommand request, Promocion? promocion)
    {
        var configuracionBase = ObtenerConfiguracionBasePorTipo(request.TipoTarjeta);
        
        // Aplicar configuración personalizada si se proporciona
        if (request.Configuracion != null)
        {
            configuracionBase.MultiplicadorPuntos = request.Configuracion.MultiplicadorPuntos;
            configuracionBase.DescuentoBase = request.Configuracion.DescuentoBase;
            configuracionBase.LimitePuntosDiario = request.Configuracion.LimitePuntosDiario;
            configuracionBase.LimitePuntosMensual = request.Configuracion.LimitePuntosMensual;
            configuracionBase.DiasExpiracionPuntos = request.Configuracion.DiasExpiracionPuntos;
            configuracionBase.AcumularEnPromociones = request.Configuracion.AcumularEnPromociones;
            configuracionBase.PermiteCanjearDescuentos = request.Configuracion.PermiteCanjearDescuentos;
            configuracionBase.AccesoEventosExclusivos = request.Configuracion.AccesoEventosExclusivos;
            configuracionBase.NotificacionesActivas = request.Configuracion.NotificacionesActivas;
        }

        // Aplicar bonificaciones de promoción
        if (promocion != null)
        {
            configuracionBase.MultiplicadorPuntos *= promocion.MultiplicadorBonificacion ?? 1.0m;
            configuracionBase.DescuentoBase += promocion.DescuentoAdicional ?? 0m;
        }

        return configuracionBase;
    }

    private ConfiguracionTarjeta ObtenerConfiguracionBasePorTipo(TipoTarjetaFidelizacion tipoTarjeta)
    {
        return tipoTarjeta switch
        {
            TipoTarjetaFidelizacion.Estandar => new ConfiguracionTarjeta
            {
                MultiplicadorPuntos = 1.0m,
                DescuentoBase = 0m,
                LimitePuntosDiario = 1000,
                LimitePuntosMensual = 20000,
                DiasExpiracionPuntos = 365,
                AcumularEnPromociones = true,
                PermiteCanjearDescuentos = true,
                AccesoEventosExclusivos = false,
                NotificacionesActivas = true
            },
            
            TipoTarjetaFidelizacion.Premium => new ConfiguracionTarjeta
            {
                MultiplicadorPuntos = 1.5m,
                DescuentoBase = 5m,
                LimitePuntosDiario = 2500,
                LimitePuntosMensual = 60000,
                DiasExpiracionPuntos = 540,
                AcumularEnPromociones = true,
                PermiteCanjearDescuentos = true,
                AccesoEventosExclusivos = true,
                NotificacionesActivas = true
            },
            
            TipoTarjetaFidelizacion.Vip => new ConfiguracionTarjeta
            {
                MultiplicadorPuntos = 2.0m,
                DescuentoBase = 10m,
                LimitePuntosDiario = 5000,
                LimitePuntosMensual = 120000,
                DiasExpiracionPuntos = 730,
                AcumularEnPromociones = true,
                PermiteCanjearDescuentos = true,
                AccesoEventosExclusivos = true,
                NotificacionesActivas = true
            },
            
            TipoTarjetaFidelizacion.Corporativa => new ConfiguracionTarjeta
            {
                MultiplicadorPuntos = 1.2m,
                DescuentoBase = 3m,
                LimitePuntosDiario = 3000,
                LimitePuntosMensual = 80000,
                DiasExpiracionPuntos = 365,
                AcumularEnPromociones = false,
                PermiteCanjearDescuentos = true,
                AccesoEventosExclusivos = false,
                NotificacionesActivas = true
            },
            
            TipoTarjetaFidelizacion.Empleado => new ConfiguracionTarjeta
            {
                MultiplicadorPuntos = 2.5m,
                DescuentoBase = 20m,
                LimitePuntosDiario = 2000,
                LimitePuntosMensual = 40000,
                DiasExpiracionPuntos = 180,
                AcumularEnPromociones = true,
                PermiteCanjearDescuentos = true,
                AccesoEventosExclusivos = true,
                NotificacionesActivas = true
            },
            
            _ => new ConfiguracionTarjeta() // Configuración por defecto
        };
    }

    private async Task GenerarCodigosIdentificacion(TarjetaFidelizacion tarjeta, CancellationToken cancellationToken)
    {
        try
        {
            // Generar QR Code
            var qrData = $"TARJETA|{tarjeta.Id}|{tarjeta.NumeroTarjeta}|{tarjeta.ClienteId}";
            tarjeta.QrCode = await _qrCodeService.GenerarQrCodeAsync(qrData);

            // Generar código de barras
            tarjeta.CodigoBarras = GenerarCodigoBarras(tarjeta.NumeroTarjeta);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generando códigos de identificación para tarjeta {TarjetaId}", tarjeta.Id);
        }
    }

    private string GenerarCodigoBarras(string numeroTarjeta)
    {
        // Generar código de barras tipo Code128
        return $"RPG{numeroTarjeta.Replace("-", "")}";
    }

    private async Task DesactivarTarjetasPrincipalesExistentes(Guid clienteId, CancellationToken cancellationToken)
    {
        var tarjetasPrincipales = await _tarjetaRepository.GetTarjetasPrincipalesByClienteIdAsync(clienteId, cancellationToken);
        
        foreach (var tarjeta in tarjetasPrincipales.Where(t => t.EstaActiva()))
        {
            tarjeta.CambiarAPrincipal(false);
            await _tarjetaRepository.UpdateAsync(tarjeta, cancellationToken);
        }
    }

    private async Task ProcesarPuntosIniciales(TarjetaFidelizacion tarjeta, int puntosIniciales, Promocion? promocion, CancellationToken cancellationToken)
    {
        // Acumular puntos iniciales
        var resultadoAcumulacion = tarjeta.AcumularPuntos(
            puntosIniciales,
            "Bienvenida",
            $"Puntos de bienvenida por creación de tarjeta {tarjeta.TipoTarjeta}",
            _currentUser.UserId ?? "Sistema");

        if (resultadoAcumulacion.Succeeded)
        {
            // Registrar transacción de puntos
            var transaccion = new TransaccionPuntos
            {
                Id = Guid.NewGuid(),
                TarjetaFidelizacionId = tarjeta.Id,
                TipoTransaccion = TipoTransaccionPuntos.Bienvenida,
                PuntosMovimiento = puntosIniciales,
                SaldoAnterior = 0,
                SaldoNuevo = puntosIniciales,
                PromocionId = promocion?.Id,
                FechaTransaccion = _dateTimeService.Now,
                Descripcion = "Puntos de bienvenida por creación de tarjeta",
                Canal = "Sistema",
                CreadoPor = _currentUser.UserId ?? "Sistema"
            };

            await _transaccionRepository.AddAsync(transaccion, cancellationToken);
        }
    }

    private async Task ActivarBeneficiosEspeciales(Guid tarjetaId, List<string> beneficiosEspeciales, CancellationToken cancellationToken)
    {
        foreach (var beneficioNombre in beneficiosEspeciales)
        {
            var beneficio = await _beneficioRepository.GetByNombreAsync(beneficioNombre, cancellationToken);
            if (beneficio != null)
            {
                await _beneficioRepository.ActivarBeneficioParaTarjetaAsync(tarjetaId, beneficio.Id, cancellationToken);
            }
        }
    }

    private async Task EnviarNotificacionCreacion(Cliente cliente, TarjetaFidelizacion tarjeta, CancellationToken cancellationToken)
    {
        try
        {
            var mensaje = $"¡Bienvenido al programa de fidelización! Tu tarjeta {tarjeta.TipoTarjeta} #{tarjeta.NumeroTarjeta} está lista. " +
                         $"Saldo inicial: {tarjeta.SaldoPuntos} puntos.";

            await _notificacionService.EnviarNotificacionTarjetaAsync(
                cliente.Id,
                "Tarjeta de Fidelización Creada",
                mensaje,
                tarjeta,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando notificación de creación de tarjeta para Cliente {ClienteId}", cliente.Id);
        }
    }

    private async Task ProgramarEnvioTarjetaFisica(TarjetaFidelizacion tarjeta, string? direccionEnvio, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(direccionEnvio))
        {
            _logger.LogWarning("No se puede enviar tarjeta física sin dirección para TarjetaId {TarjetaId}", tarjeta.Id);
            return;
        }

        try
        {
            await _envioTarjetaService.ProgramarEnvioAsync(
                tarjeta.Id,
                direccionEnvio,
                _dateTimeService.Now.AddBusinessDays(3), // 3 días hábiles
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error programando envío de tarjeta física para TarjetaId {TarjetaId}", tarjeta.Id);
        }
    }

    private async Task<TarjetaFidelizacionDto> CrearResponseDto(TarjetaFidelizacion tarjeta, Cliente cliente, CancellationToken cancellationToken)
    {
        // Obtener información adicional para el DTO
        var transaccionesRecientes = await _transaccionRepository.GetRecientesByTarjetaIdAsync(tarjeta.Id, 5, cancellationToken);
        var recompensasDisponibles = await _tarjetaRepository.GetRecompensasDisponiblesAsync(tarjeta.Id, 10, cancellationToken);
        var siguienteNivel = await _tarjetaRepository.GetSiguienteNivelAsync(tarjeta.NivelFidelizacion, cancellationToken);
        var estadisticas = await _tarjetaRepository.GetEstadisticasAsync(tarjeta.Id, cancellationToken);

        return new TarjetaFidelizacionDto
        {
            Id = tarjeta.Id,
            ClienteId = cliente.Id,
            Cliente = _mapper.Map<ClienteBasicoDto>(cliente),
            NumeroTarjeta = tarjeta.NumeroTarjeta,
            TipoTarjeta = tarjeta.TipoTarjeta.ToString(),
            NivelFidelizacion = tarjeta.NivelFidelizacion.ToString(),
            SaldoPuntos = tarjeta.SaldoPuntos,
            TotalPuntosAcumulados = tarjeta.TotalPuntosAcumulados,
            TotalPuntosCanjeados = tarjeta.TotalPuntosCanjeados,
            Estado = (EstadoTarjeta)tarjeta.Estado,
            EsPrincipal = tarjeta.EsPrincipal,
            FechaEmision = tarjeta.FechaEmision,
            FechaActivacion = tarjeta.FechaActivacion,
            FechaVencimiento = tarjeta.FechaVencimiento,
            UltimaTransaccion = tarjeta.UltimaTransaccion,
            Configuracion = _mapper.Map<ConfiguracionTarjetaDto>(tarjeta.Configuracion),
            Personalizacion = _mapper.Map<PersonalizacionTarjetaDto>(tarjeta.Personalizacion),
            BeneficiosActivos = _mapper.Map<List<BeneficioTarjetaDto>>(tarjeta.BeneficiosActivos),
            SiguienteNivel = _mapper.Map<SiguienteNivelDto>(siguienteNivel),
            TransaccionesRecientes = _mapper.Map<List<TransaccionRecienteDto>>(transaccionesRecientes),
            RecompensasDisponibles = _mapper.Map<List<RecompensaDisponibleDto>>(recompensasDisponibles),
            Estadisticas = _mapper.Map<EstadisticasTarjetaDto>(estadisticas),
            SucursalEmision = tarjeta.SucursalEmision,
            Canal = tarjeta.Canal,
            MotivoEmision = tarjeta.MotivoEmision,
            QrCode = tarjeta.QrCode,
            CodigoBarras = tarjeta.CodigoBarras,
            UrlTarjetaDigital = GenerarUrlTarjetaDigital(tarjeta),
            DatosAdicionales = tarjeta.DatosAdicionales,
            PreferenciasCliente = tarjeta.PreferenciasCliente
        };
    }

    private string GenerarUrlTarjetaDigital(TarjetaFidelizacion tarjeta)
    {
        return $"https://restaurantepro.com/mi-tarjeta/{tarjeta.Id}?token={GenerarTokenSeguro(tarjeta)}";
    }

    private string GenerarTokenSeguro(TarjetaFidelizacion tarjeta)
    {
        // Generar token seguro para acceso a tarjeta digital
        var data = $"{tarjeta.Id}|{tarjeta.NumeroTarjeta}|{_dateTimeService.Now:yyyyMMdd}";
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data)).Replace("=", "").Replace("+", "-").Replace("/", "_");
    }
}

/// <summary>
/// Extensiones para cálculos de fechas de negocio
/// </summary>
public static class DateTimeExtensions
{
    public static DateTime AddBusinessDays(this DateTime startDate, int businessDays)
    {
        var direction = Math.Sign(businessDays);
        var currentDate = startDate;
        
        while (businessDays != 0)
        {
            currentDate = currentDate.AddDays(direction);
            if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
            {
                businessDays -= direction;
            }
        }
        
        return currentDate;
    }
} 