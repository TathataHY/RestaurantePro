namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.CanjearPuntos;

/// <summary>
/// Handler para canjear puntos por recompensas en el programa de fidelización
/// Gestiona el proceso completo de validación, descuento de puntos y entrega de recompensas
/// </summary>
public class CanjearPuntosHandler : IRequestHandler<CanjearPuntosCommand, Result<CanjeoPuntosDto>>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly IRecompensaRepository _recompensaRepository;
    private readonly ICanjeRepository _canjeRepository;
    private readonly ITransaccionPuntosRepository _transaccionRepository;
    private readonly IPromocionRepository _promocionRepository;
    private readonly INotificacionService _notificacionService;
    private readonly IQrCodeService _qrCodeService;
    private readonly IEntregaService _entregaService;
    private readonly IMapper _mapper;
    private readonly ILogger<CanjearPuntosHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IAuditService _auditService;
    private readonly IGamificacionService _gamificacionService;
    private readonly IEstadisticasService _estadisticasService;

    public CanjearPuntosHandler(
        IClienteRepository clienteRepository,
        ITarjetaFidelizacionRepository tarjetaRepository,
        IRecompensaRepository recompensaRepository,
        ICanjeRepository canjeRepository,
        ITransaccionPuntosRepository transaccionRepository,
        IPromocionRepository promocionRepository,
        INotificacionService notificacionService,
        IQrCodeService qrCodeService,
        IEntregaService entregaService,
        IMapper mapper,
        ILogger<CanjearPuntosHandler> logger,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        IAuditService auditService,
        IGamificacionService gamificacionService,
        IEstadisticasService estadisticasService)
    {
        _clienteRepository = clienteRepository;
        _tarjetaRepository = tarjetaRepository;
        _recompensaRepository = recompensaRepository;
        _canjeRepository = canjeRepository;
        _transaccionRepository = transaccionRepository;
        _promocionRepository = promocionRepository;
        _notificacionService = notificacionService;
        _qrCodeService = qrCodeService;
        _entregaService = entregaService;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
        _auditService = auditService;
        _gamificacionService = gamificacionService;
        _estadisticasService = estadisticasService;
    }

    public async Task<Result<CanjeoPuntosDto>> Handle(CanjearPuntosCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando canje de puntos para Cliente {ClienteId}, Recompensa {RecompensaId}",
                request.ClienteId, request.RecompensaId);

            // 1. Obtener y validar el cliente
            var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning("Cliente {ClienteId} no encontrado", request.ClienteId);
                return Result<CanjeoPuntosDto>.Failure("Cliente no encontrado");
            }

            // 2. Obtener y validar la tarjeta de fidelización
            var tarjeta = await ObtenerTarjetaFidelizacion(request, cancellationToken);
            if (tarjeta == null)
            {
                return Result<CanjeoPuntosDto>.Failure("Tarjeta de fidelización no encontrada o inactiva");
            }

            // 3. Obtener y validar la recompensa
            var recompensa = await _recompensaRepository.GetByIdAsync(request.RecompensaId, cancellationToken);
            if (recompensa == null || !recompensa.EstaActiva())
            {
                _logger.LogWarning("Recompensa {RecompensaId} no encontrada o inactiva", request.RecompensaId);
                return Result<CanjeoPuntosDto>.Failure("Recompensa no disponible");
            }

            // 4. Validar elegibilidad del cliente para la recompensa
            var elegibilidadResult = await ValidarElegibilidad(cliente, recompensa, request, cancellationToken);
            if (!elegibilidadResult.IsSuccess)
            {
                return Result<CanjeoPuntosDto>.Failure(elegibilidadResult.Error);
            }

            // 5. Calcular puntos necesarios (considerando promociones)
            var puntosCalculados = await CalcularPuntosNecesarios(recompensa, request, cancellationToken);
            var puntosNecesarios = puntosCalculados.PuntosFinales;

            // 6. Validar saldo de puntos suficiente
            if (tarjeta.SaldoPuntos < puntosNecesarios)
            {
                _logger.LogWarning("Puntos insuficientes. Disponibles: {Disponibles}, Necesarios: {Necesarios}",
                    tarjeta.SaldoPuntos, puntosNecesarios);
                return Result<CanjeoPuntosDto>.Failure(
                    $"Puntos insuficientes. Disponibles: {tarjeta.SaldoPuntos}, Necesarios: {puntosNecesarios}");
            }

            // 7. Verificar stock de recompensa
            var stockResult = await VerificarYReservarStock(recompensa, request.Cantidad, cancellationToken);
            if (!stockResult.IsSuccess)
            {
                return Result<CanjeoPuntosDto>.Failure(stockResult.Error);
            }

            // 8. Crear la transacción de canje
            var canje = await CrearTransaccionCanje(cliente, tarjeta, recompensa, request, puntosNecesarios, cancellationToken);

            try
            {
                // 9. Descontar puntos de la tarjeta
                var saldoAnterior = tarjeta.SaldoPuntos;
                tarjeta.DescontarPuntos(puntosNecesarios, $"Canje de recompensa: {recompensa.Nombre}");
                await _tarjetaRepository.UpdateAsync(tarjeta, cancellationToken);

                // 10. Registrar transacción de puntos
                await RegistrarTransaccionPuntos(tarjeta, puntosNecesarios, canje.Id, cancellationToken);

                // 11. Procesar entrega según el método seleccionado
                var entregaResult = await ProcesarEntrega(canje, request, cancellationToken);
                if (!entregaResult.IsSuccess)
                {
                    _logger.LogError("Error procesando entrega para canje {CanjeId}: {Error}",
                        canje.Id, entregaResult.Error);
                    // Continuar el proceso ya que el canje fue exitoso
                }

                // 12. Generar códigos y documentos
                var codigosResult = await GenerarCodigosYDocumentos(canje, recompensa, request, cancellationToken);

                // 13. Procesar gamificación y logros
                var logrosObtenidos = await _gamificacionService.ProcesarLogrosCanjeAsync(
                    cliente.Id, recompensa, puntosNecesarios, cancellationToken);

                // 14. Actualizar estadísticas
                await _estadisticasService.ActualizarEstadisticasCanjeAsync(
                    cliente.Id, puntosNecesarios, recompensa.Categoria, cancellationToken);

                // 15. Enviar notificación al cliente
                if (request.NotificarCliente)
                {
                    await EnviarNotificacionCanje(cliente, canje, recompensa, codigosResult, cancellationToken);
                }

                // 16. Registrar auditoría
                await _auditService.RegistrarEventoAsync(
                    "CanjePuntos",
                    $"Canje exitoso - Cliente: {cliente.Id}, Recompensa: {recompensa.Nombre}, Puntos: {puntosNecesarios}",
                    cliente.Id,
                    _currentUserService.UserId,
                    cancellationToken);

                // 17. Crear y mapear DTO de respuesta
                var responseDto = await CrearResponseDto(
                    canje, cliente, tarjeta, recompensa, request, 
                    saldoAnterior, puntosNecesarios, puntosCalculados,
                    codigosResult, logrosObtenidos, cancellationToken);

                _logger.LogInformation("Canje de puntos completado exitosamente. CanjeId: {CanjeId}", canje.Id);
                return Result<CanjeoPuntosDto>.Success(responseDto);
            }
            catch (Exception ex)
            {
                // Rollback: devolver stock reservado
                await _recompensaRepository.LiberarStockReservadoAsync(recompensa.Id, request.Cantidad, cancellationToken);
                
                _logger.LogError(ex, "Error durante el proceso de canje para Cliente {ClienteId}", request.ClienteId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando canje de puntos para Cliente {ClienteId}", request.ClienteId);
            return Result<CanjeoPuntosDto>.Failure("Error interno procesando el canje");
        }
    }

    private async Task<TarjetaFidelizacion?> ObtenerTarjetaFidelizacion(CanjearPuntosCommand request, CancellationToken cancellationToken)
    {
        if (request.TarjetaFidelizacionId.HasValue)
        {
            var tarjeta = await _tarjetaRepository.GetByIdAsync(request.TarjetaFidelizacionId.Value, cancellationToken);
            if (tarjeta?.ClienteId == request.ClienteId && tarjeta.EstaActiva())
                return tarjeta;
        }

        // Obtener tarjeta principal del cliente
        return await _tarjetaRepository.GetPrincipalByClienteIdAsync(request.ClienteId, cancellationToken);
    }

    private async Task<Result> ValidarElegibilidad(Cliente cliente, Recompensa recompensa, CanjearPuntosCommand request, CancellationToken cancellationToken)
    {
        // Validar nivel del cliente
        if (!recompensa.EsElegibleParaNivel(cliente.NivelFidelizacion))
        {
            return Result.Failure($"La recompensa '{recompensa.Nombre}' no está disponible para su nivel de fidelización");
        }

        // Validar límites de edad
        if (recompensa.EdadMinima.HasValue && cliente.CalcularEdad() < recompensa.EdadMinima.Value)
        {
            return Result.Failure($"Edad mínima requerida: {recompensa.EdadMinima} años");
        }

        // Validar límites de canje por cliente
        var canjesRecientes = await _canjeRepository.GetCanjesPorClienteEnPeriodoAsync(
            cliente.Id, recompensa.Id, DateTime.UtcNow.AddDays(-30), cancellationToken);

        if (recompensa.LimiteCanjesPorCliente.HasValue && 
            canjesRecientes.Count >= recompensa.LimiteCanjesPorCliente.Value)
        {
            return Result.Failure($"Ha alcanzado el límite de {recompensa.LimiteCanjesPorCliente} canjes por mes para esta recompensa");
        }

        // Validar horarios de disponibilidad
        var horaActual = _dateTimeService.Now.TimeOfDay;
        if (recompensa.HorarioDisponibilidad != null && 
            !recompensa.HorarioDisponibilidad.EstaEnHorario(horaActual))
        {
            return Result.Failure("La recompensa no está disponible en este horario");
        }

        return Result.Success();
    }

    private async Task<(int PuntosFinales, decimal? DescuentoAplicado, string? PromocionAplicada)> CalcularPuntosNecesarios(
        Recompensa recompensa, CanjearPuntosCommand request, CancellationToken cancellationToken)
    {
        var puntosBase = request.PuntosEspecificos ?? (recompensa.PuntosNecesarios * request.Cantidad);
        var descuentoAplicado = 0m;
        string? promocionAplicada = null;

        // Aplicar promoción si se especificó código
        if (!string.IsNullOrEmpty(request.CodigoPromocion))
        {
            var promocion = await _promocionRepository.GetActiveByCodigo(request.CodigoPromocion, cancellationToken);
            if (promocion != null && promocion.EsAplicableACanje(recompensa.Id))
            {
                descuentoAplicado = promocion.CalcularDescuento(puntosBase);
                promocionAplicada = promocion.Codigo;
            }
        }

        var puntosFinales = puntosBase - (int)descuentoAplicado;
        return (puntosFinales, descuentoAplicado, promocionAplicada);
    }

    private async Task<Result> VerificarYReservarStock(Recompensa recompensa, int cantidad, CancellationToken cancellationToken)
    {
        if (recompensa.StockLimitado && recompensa.StockDisponible < cantidad)
        {
            return Result.Failure($"Stock insuficiente. Disponible: {recompensa.StockDisponible}, Solicitado: {cantidad}");
        }

        if (recompensa.StockLimitado)
        {
            await _recompensaRepository.ReservarStockAsync(recompensa.Id, cantidad, cancellationToken);
        }

        return Result.Success();
    }

    private async Task<Canje> CrearTransaccionCanje(
        Cliente cliente, TarjetaFidelizacion tarjeta, Recompensa recompensa,
        CanjearPuntosCommand request, int puntosNecesarios, CancellationToken cancellationToken)
    {
        var canje = new Canje
        {
            Id = Guid.NewGuid(),
            ClienteId = cliente.Id,
            TarjetaFidelizacionId = tarjeta.Id,
            RecompensaId = recompensa.Id,
            Cantidad = request.Cantidad,
            PuntosUtilizados = puntosNecesarios,
            TipoCanje = request.TipoCanje,
            MetodoEntrega = request.MetodoEntrega,
            Estado = request.TipoCanje == TipoCanje.Inmediato ? EstadoCanje.Procesado : EstadoCanje.Pendiente,
            FechaCanje = _dateTimeService.Now,
            FechaEntregaEstimada = CalcularFechaEntregaEstimada(request),
            SucursalRecogida = request.SucursalRecogida,
            DireccionEntrega = request.DireccionEntrega,
            Comentarios = request.Comentarios,
            Canal = request.Canal,
            EmpleadoId = request.EmpleadoId,
            CodigoSeguimiento = GenerarCodigoSeguimiento(),
            PromocionAplicada = request.CodigoPromocion,
            DatosAdicionales = request.DatosAdicionales
        };

        await _canjeRepository.AddAsync(canje, cancellationToken);
        return canje;
    }

    private DateTime? CalcularFechaEntregaEstimada(CanjearPuntosCommand request)
    {
        if (request.FechaPreferida.HasValue)
            return request.FechaPreferida.Value;

        return request.MetodoEntrega switch
        {
            MetodoEntrega.Presencial => _dateTimeService.Now.AddHours(1),
            MetodoEntrega.Digital => _dateTimeService.Now.AddMinutes(15),
            MetodoEntrega.Domicilio => _dateTimeService.Now.AddDays(1),
            MetodoEntrega.Correo => _dateTimeService.Now.AddDays(3),
            _ => _dateTimeService.Now.AddDays(1)
        };
    }

    private string GenerarCodigoSeguimiento()
    {
        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        var random = new Random().Next(1000, 9999);
        return $"CP{timestamp}{random}";
    }

    private async Task RegistrarTransaccionPuntos(TarjetaFidelizacion tarjeta, int puntos, Guid canjeId, CancellationToken cancellationToken)
    {
        var transaccion = new TransaccionPuntos
        {
            Id = Guid.NewGuid(),
            TarjetaFidelizacionId = tarjeta.Id,
            TipoTransaccion = TipoTransaccionPuntos.Canje,
            PuntosMovimiento = -puntos,
            SaldoAnterior = tarjeta.SaldoPuntos + puntos,
            SaldoNuevo = tarjeta.SaldoPuntos,
            CanjeId = canjeId,
            FechaTransaccion = _dateTimeService.Now,
            Descripcion = "Canje de puntos por recompensa"
        };

        await _transaccionRepository.AddAsync(transaccion, cancellationToken);
    }

    private async Task<Result> ProcesarEntrega(Canje canje, CanjearPuntosCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return request.MetodoEntrega switch
            {
                MetodoEntrega.Digital => await _entregaService.ProcesarEntregaDigitalAsync(canje, cancellationToken),
                MetodoEntrega.Domicilio => await _entregaService.ProgramarEntregaDomicilioAsync(canje, request.DireccionEntrega!, cancellationToken),
                MetodoEntrega.Presencial => await _entregaService.NotificarRecogidaPresencialAsync(canje, request.SucursalRecogida!, cancellationToken),
                _ => Result.Success()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando entrega para canje {CanjeId}", canje.Id);
            return Result.Failure("Error procesando la entrega");
        }
    }

    private async Task<(string? CodigoCupon, string? QrCode)> GenerarCodigosYDocumentos(
        Canje canje, Recompensa recompensa, CanjearPuntosCommand request, CancellationToken cancellationToken)
    {
        string? codigoCupon = null;
        string? qrCode = null;

        try
        {
            // Generar código de cupón para recompensas que lo requieren
            if (recompensa.RequiereCupon)
            {
                codigoCupon = GenerarCodigoCupon(canje);
                canje.CodigoCupon = codigoCupon;
            }

            // Generar QR Code para validación
            if (recompensa.RequiereValidacion)
            {
                var qrData = $"CANJE|{canje.Id}|{canje.CodigoSeguimiento}|{codigoCupon}";
                qrCode = await _qrCodeService.GenerarQrCodeAsync(qrData);
                canje.QrCode = qrCode;
            }

            await _canjeRepository.UpdateAsync(canje, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando códigos para canje {CanjeId}", canje.Id);
        }

        return (codigoCupon, qrCode);
    }

    private string GenerarCodigoCupon(Canje canje)
    {
        var timestamp = DateTimeOffset.Now.ToString("yyyyMMddHHmm");
        var canjeShort = canje.Id.ToString("N")[..8].ToUpper();
        return $"RPG{timestamp}{canjeShort}";
    }

    private async Task EnviarNotificacionCanje(
        Cliente cliente, Canje canje, Recompensa recompensa,
        (string? CodigoCupon, string? QrCode) codigos, CancellationToken cancellationToken)
    {
        try
        {
            var mensaje = $"¡Felicidades! Has canjeado exitosamente {recompensa.Nombre}. " +
                         $"Código de seguimiento: {canje.CodigoSeguimiento}";

            await _notificacionService.EnviarNotificacionCanjeAsync(
                cliente.Id,
                "Canje de Recompensa Exitoso",
                mensaje,
                canje,
                codigos.CodigoCupon,
                codigos.QrCode,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando notificación de canje para Cliente {ClienteId}", cliente.Id);
        }
    }

    private async Task<CanjeoPuntosDto> CrearResponseDto(
        Canje canje, Cliente cliente, TarjetaFidelizacion tarjeta, Recompensa recompensa,
        CanjearPuntosCommand request, int saldoAnterior, int puntosUtilizados,
        (int PuntosFinales, decimal? DescuentoAplicado, string? PromocionAplicada) puntosCalculados,
        (string? CodigoCupon, string? QrCode) codigos,
        List<LogroObtenido> logrosObtenidos, CancellationToken cancellationToken)
    {
        var recompensasSugeridas = await _recompensaRepository.GetRecompensasSugeridasAsync(
            cliente.Id, tarjeta.SaldoPuntos, 5, cancellationToken);

        var estadisticas = await _estadisticasService.GetEstadisticasClienteAsync(cliente.Id, cancellationToken);

        return new CanjeoPuntosDto
        {
            CanjeId = canje.Id,
            ClienteId = cliente.Id,
            TarjetaId = tarjeta.Id,
            RecompensaId = recompensa.Id,
            Recompensa = _mapper.Map<RecompensaCanjeadaDto>(recompensa),
            Cantidad = request.Cantidad,
            PuntosUtilizados = puntosUtilizados,
            SaldoAnterior = saldoAnterior,
            SaldoActual = tarjeta.SaldoPuntos,
            TipoCanje = request.TipoCanje.ToString(),
            MetodoEntrega = request.MetodoEntrega.ToString(),
            Estado = canje.Estado,
            FechaCanje = canje.FechaCanje,
            FechaEntregaEstimada = canje.FechaEntregaEstimada,
            CodigoSeguimiento = canje.CodigoSeguimiento,
            CodigoCupon = codigos.CodigoCupon,
            QrCode = codigos.QrCode,
            SucursalRecogida = request.SucursalRecogida,
            DireccionEntrega = request.DireccionEntrega,
            DescuentoAplicado = puntosCalculados.DescuentoAplicado,
            PromocionAplicada = puntosCalculados.PromocionAplicada,
            InstruccionesUso = GenerarInstruccionesUso(recompensa, request.MetodoEntrega),
            FechaVencimiento = CalcularFechaVencimiento(recompensa),
            TerminosCondiciones = recompensa.TerminosCondiciones ?? new List<string>(),
            ContactoSoporte = new ContactoSoporteDto
            {
                Telefono = "1-800-RESTAURANTE",
                Email = "soporte@restaurantepro.com",
                ChatDisponible = true,
                HorarioAtencion = "Lunes a Domingo 7:00 AM - 11:00 PM",
                WhatsApp = "+1-800-555-0123"
            },
            MensajePersonalizado = GenerarMensajePersonalizado(cliente, recompensa),
            Metricas = new MetricasCanjeDto
            {
                TotalCanjes = estadisticas?.TotalCanjes ?? 0,
                PuntosAhorrados = (int)(puntosCalculados.DescuentoAplicado ?? 0),
                PorcentajeDescuento = puntosCalculados.DescuentoAplicado > 0 
                    ? (puntosCalculados.DescuentoAplicado.Value / puntosUtilizados) * 100 
                    : 0,
                LogrosDesbloqueados = logrosObtenidos.Select(l => l.Nombre).ToList()
            },
            RecompensasSugeridas = _mapper.Map<List<RecompensaSugeridaDto>>(recompensasSugeridas),
            UrlCompartir = GenerarUrlCompartir(canje),
            DatosAdicionales = request.DatosAdicionales
        };
    }

    private string GenerarInstruccionesUso(Recompensa recompensa, MetodoEntrega metodoEntrega)
    {
        return metodoEntrega switch
        {
            MetodoEntrega.Digital => $"Su cupón digital para {recompensa.Nombre} está listo. Use el código generado para disfrutar de su recompensa.",
            MetodoEntrega.Presencial => $"Presente este código en cualquiera de nuestras sucursales para reclamar su {recompensa.Nombre}.",
            MetodoEntrega.Domicilio => $"Su {recompensa.Nombre} será entregado en la dirección especificada según la fecha estimada.",
            _ => $"Su recompensa {recompensa.Nombre} está siendo procesada."
        };
    }

    private DateTime? CalcularFechaVencimiento(Recompensa recompensa)
    {
        if (recompensa.DiasValidez.HasValue)
        {
            return _dateTimeService.Now.AddDays(recompensa.DiasValidez.Value);
        }
        return recompensa.FechaVencimiento;
    }

    private string GenerarMensajePersonalizado(Cliente cliente, Recompensa recompensa)
    {
        var mensajes = new[]
        {
            $"¡Excelente elección {cliente.Nombre}! Tu {recompensa.Nombre} te espera.",
            $"¡Felicidades {cliente.Nombre}! Has desbloqueado una increíble recompensa.",
            $"¡Bien hecho {cliente.Nombre}! Disfruta tu {recompensa.Nombre}."
        };

        return mensajes[new Random().Next(mensajes.Length)];
    }

    private string GenerarUrlCompartir(Canje canje)
    {
        return $"https://restaurantepro.com/share/canje/{canje.Id}?code={canje.CodigoSeguimiento}";
    }
} 