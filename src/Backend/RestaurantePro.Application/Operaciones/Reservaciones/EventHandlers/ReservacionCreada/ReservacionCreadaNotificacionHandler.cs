namespace RestaurantePro.Application.Operaciones.Reservaciones.EventHandlers.ReservacionCreada;

/// <summary>
/// 📅 Handler que procesa el evento ReservacionCreada para envío de confirmación automática
/// </summary>
public class ReservacionCreadaNotificacionHandler : Domain.Core.Base.Events.Handlers.IDomainEventHandler<Domain.Operaciones.Reservaciones.Events.Reservacion.ReservacionCreada>
{
    private readonly IReservacionRepository _reservacionRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IMesaRepository _mesaRepository;
    private readonly ILogger<ReservacionCreadaNotificacionHandler> _logger;
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    private readonly ISMSService _smsService;

    public ReservacionCreadaNotificacionHandler(
        IReservacionRepository reservacionRepository,
        IClienteRepository clienteRepository,
        IMesaRepository mesaRepository,
        ILogger<ReservacionCreadaNotificacionHandler> logger,
        IMediator mediator,
        IEmailService emailService,
        ISMSService smsService)
    {
        _reservacionRepository = reservacionRepository;
        _clienteRepository = clienteRepository;
        _mesaRepository = mesaRepository;
        _logger = logger;
        _mediator = mediator;
        _emailService = emailService;
        _smsService = smsService;
    }

    /// <summary>
    /// 🚀 Procesa la creación de reservación para envío de confirmación automática
    /// </summary>
    public async Task Handle(Domain.Operaciones.Reservaciones.Events.Reservacion.ReservacionCreada evento, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📅 Iniciando envío de confirmación para Reservación {ReservacionId} - Cliente {ClienteId}", 
            evento.ReservacionId, evento.ClienteId);

        try
        {
            // 1. Obtener reservación completa
            var reservacionResult = await _reservacionRepository.ObtenerPorIdAsync(evento.ReservacionId, cancellationToken);
            if (reservacionResult == null)
            {
                _logger.LogWarning("⚠️ No se encontró la reservación {ReservacionId} para enviar confirmación", evento.ReservacionId);
                return;
            }

            var reservacion = reservacionResult;

            // 2. Obtener información del cliente
            var clienteResult = await _clienteRepository.ObtenerPorIdAsync(evento.ClienteId, cancellationToken);
            if (clienteResult == null)
            {
                _logger.LogWarning("⚠️ No se encontró el cliente {ClienteId} para enviar confirmación de reservación", evento.ClienteId);
                return;
            }

            var cliente = clienteResult;

            // 3. Obtener información de la mesa si está asignada
            Mesa? mesa = null;
            if (reservacion.MesaId != Guid.Empty)
            {
                var mesaResult = await _mesaRepository.ObtenerPorIdAsync(reservacion.MesaId, cancellationToken);
                if (mesaResult != null)
                {
                    mesa = mesaResult;
                }
            }

            // 4. Preparar datos para confirmación
            var datosConfirmacion = await PrepararDatosConfirmacion(reservacion, cliente, mesa, evento, cancellationToken);

            // 5. Enviar email de confirmación
            await EnviarEmailConfirmacion(datosConfirmacion, cancellationToken);

            // 6. Enviar SMS de confirmación
            await EnviarSMSConfirmacion(datosConfirmacion, cancellationToken);

            // 7. Programar recordatorio automático
            await ProgramarRecordatorio(datosConfirmacion, cancellationToken);

            // 8. Notificar al personal del restaurante
            await NotificarPersonalReservacion(datosConfirmacion, cancellationToken);

            // 9. Registrar estadísticas de reservación
            await RegistrarEstadisticasReservacion(reservacion, cliente, evento, cancellationToken);

            _logger.LogInformation("✅ Confirmación enviada exitosamente para Reservación {ReservacionId} al cliente {ClienteNombre}", 
                evento.ReservacionId, cliente.Nombre.NombreCompleto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al enviar confirmación para Reservación {ReservacionId}", evento.ReservacionId);
            throw;
        }
    }

    /// <summary>
    /// 📋 Prepara los datos necesarios para la confirmación de reservación
    /// </summary>
    private async Task<ConfirmacionReservacionData> PrepararDatosConfirmacion(
        Reservacion reservacion,
        Cliente cliente,
        Mesa? mesa,
        Domain.Operaciones.Reservaciones.Events.Reservacion.ReservacionCreada evento,
        CancellationToken cancellationToken)
    {
        var fechaReservacion = evento.Fecha.Date + evento.Hora;
        var tiempoRestante = fechaReservacion - DateTime.UtcNow;

        var datosConfirmacion = new ConfirmacionReservacionData
        {
            ReservacionId = reservacion.Id,
            ClienteId = cliente.Id,
            ClienteNombre = cliente.Nombre.NombreCompleto,
            ClienteEmail = cliente.Email.Value,
            ClienteTelefono = cliente.Telefono?.Value,
            FechaHoraReservacion = fechaReservacion,
            CantidadPersonas = reservacion.CantidadPersonas,
            EstadoReservacion = reservacion.Estado.ToString(),
            ObservacionesEspeciales = reservacion.Observaciones,
            MesaAsignada = mesa?.Numero,
            CapacidadMesa = mesa?.Capacidad,
            TiempoHastaReservacion = tiempoRestante,
            EsReservacionProxima = tiempoRestante.TotalHours <= 24,
            ClienteFidelizado = true, // Valor por defecto
            CodigoConfirmacion = GenerarCodigoConfirmacion(reservacion.Id)
        };

        _logger.LogInformation("📋 Datos de confirmación preparados para Reservación {ReservacionId}: {@DatosConfirmacion}", 
            evento.ReservacionId, new { 
                datosConfirmacion.ClienteEmail,
                datosConfirmacion.FechaHoraReservacion,
                datosConfirmacion.CantidadPersonas,
                datosConfirmacion.MesaAsignada,
                datosConfirmacion.EsReservacionProxima
            });

        return datosConfirmacion;
    }

    /// <summary>
    /// 📧 Envía email de confirmación de reservación
    /// </summary>
    private async Task EnviarEmailConfirmacion(ConfirmacionReservacionData datos, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(datos.ClienteEmail))
        {
            _logger.LogWarning("⚠️ Cliente {ClienteNombre} no tiene email válido, omitiendo email de confirmación", 
                datos.ClienteNombre);
            return;
        }

        try
        {
            // Crear email HTML para mejor presentación
            var emailHtml = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #2c5aa0;'>✅ Reservación Confirmada - RestaurantePro</h2>
                    <p>Estimado/a <strong>{datos.ClienteNombre}</strong>,</p>
                    <p>Su reservación ha sido confirmada exitosamente.</p>
                    
                    <div style='background-color: #f0f8ff; padding: 15px; border-left: 4px solid #2c5aa0; margin: 20px 0;'>
                        <h3>📋 Detalles de la Reservación:</h3>
                        <ul style='list-style: none; padding: 0;'>
                            <li>📅 <strong>Fecha y Hora:</strong> {datos.FechaHoraReservacion:dddd, dd 'de' MMMM 'del' yyyy 'a las' HH:mm}</li>
                            <li>👥 <strong>Cantidad de Personas:</strong> {datos.CantidadPersonas}</li>
                            <li>🏷️ <strong>Código de Confirmación:</strong> <span style='background-color: #ffeb3b; padding: 2px 5px; font-weight: bold;'>{datos.CodigoConfirmacion}</span></li>
                            {(datos.MesaAsignada.HasValue ? $"<li>🪑 <strong>Mesa Asignada:</strong> Mesa #{datos.MesaAsignada}</li>" : "")}
                            {(!string.IsNullOrEmpty(datos.ObservacionesEspeciales) ? $"<li>📝 <strong>Observaciones:</strong> {datos.ObservacionesEspeciales}</li>" : "")}
                        </ul>
                    </div>
                    
                    <p>⏰ Su reservación es en <strong>{FormatearTiempoRestante(datos.TiempoHastaReservacion)}</strong>.</p>
                    <p>¡Esperamos verle pronto en RestaurantePro!</p>
                    
                    <hr style='margin: 30px 0;'>
                    <p style='color: #666; font-size: 12px;'>
                        Si necesita modificar o cancelar su reservación, contáctenos con el código {datos.CodigoConfirmacion}.
                    </p>
                </body>
                </html>";

            await _emailService.SendHtmlEmailAsync(datos.ClienteEmail, 
                $"✅ Reservación Confirmada - {datos.CodigoConfirmacion}", 
                emailHtml);
            
            _logger.LogInformation("✅ Email de confirmación enviado exitosamente a {Email}", datos.ClienteEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al enviar email de confirmación a {Email}", datos.ClienteEmail);
            throw;
        }
    }

    /// <summary>
    /// 📱 Envía SMS de confirmación de reservación
    /// </summary>
    private async Task EnviarSMSConfirmacion(ConfirmacionReservacionData datos, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(datos.ClienteTelefono))
        {
            _logger.LogInformation("ℹ️ Cliente {ClienteNombre} no tiene teléfono configurado, omitiendo SMS de confirmación", 
                datos.ClienteNombre);
            return;
        }

        try
        {
            var mensajeSMS = $"✅ RestaurantePro: Reservación confirmada para {datos.CantidadPersonas} persona(s) " +
                           $"el {datos.FechaHoraReservacion:dd/MM} a las {datos.FechaHoraReservacion:HH:mm}. " +
                           $"Código: {datos.CodigoConfirmacion}. ¡Te esperamos!";

            _logger.LogInformation("📱 Enviando SMS de confirmación a {Telefono}: {Mensaje}", 
                datos.ClienteTelefono, mensajeSMS);

            await _smsService.SendSMSWithTrackingAsync(
                datos.ClienteTelefono, 
                mensajeSMS, 
                datos.ClienteId, 
                "ConfirmacionReservacion");

            _logger.LogInformation("✅ SMS de confirmación enviado exitosamente a {Telefono}", datos.ClienteTelefono);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al enviar SMS de confirmación a {Telefono}", datos.ClienteTelefono);
            // No relanzar la excepción para SMS ya que no es crítico
        }
    }

    /// <summary>
    /// ⏰ Programa recordatorio automático para la reservación
    /// </summary>
    private async Task ProgramarRecordatorio(ConfirmacionReservacionData datos, CancellationToken cancellationToken)
    {
        try
        {
            // Programar recordatorio 24 horas antes y 2 horas antes
            var recordatorios = new List<DateTime>();

            if (datos.TiempoHastaReservacion.TotalHours > 24)
            {
                recordatorios.Add(datos.FechaHoraReservacion.AddHours(-24));
            }

            if (datos.TiempoHastaReservacion.TotalHours > 2)
            {
                recordatorios.Add(datos.FechaHoraReservacion.AddHours(-2));
            }

            foreach (var fechaRecordatorio in recordatorios)
            {
                var recordatorioData = new
                {
                    ReservacionId = datos.ReservacionId,
                    ClienteId = datos.ClienteId,
                    FechaEnvio = fechaRecordatorio,
                    TipoRecordatorio = fechaRecordatorio == datos.FechaHoraReservacion.AddHours(-24) ? "24h" : "2h",
                    Email = datos.ClienteEmail,
                    Telefono = datos.ClienteTelefono
                };

                _logger.LogInformation("⏰ Programando recordatorio {TipoRecordatorio} para {FechaEnvio}: {@RecordatorioData}", 
                    recordatorioData.TipoRecordatorio, fechaRecordatorio, recordatorioData);

                // TODO: Implementar sistema de recordatorios (jobs en background)
                // await _backgroundJobService.ScheduleReminderAsync(recordatorioData, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al programar recordatorios para Reservación {ReservacionId}", datos.ReservacionId);
        }
    }

    /// <summary>
    /// 👥 Notifica al personal del restaurante sobre la nueva reservación
    /// </summary>
    private async Task NotificarPersonalReservacion(ConfirmacionReservacionData datos, CancellationToken cancellationToken)
    {
        var notificacionPersonal = new
        {
            Tipo = "NuevaReservacion",
            ReservacionId = datos.ReservacionId,
            ClienteNombre = datos.ClienteNombre,
            FechaHora = datos.FechaHoraReservacion,
            CantidadPersonas = datos.CantidadPersonas,
            MesaAsignada = datos.MesaAsignada,
            ObservacionesEspeciales = datos.ObservacionesEspeciales,
            ClienteFidelizado = datos.ClienteFidelizado,
            EsReservacionProxima = datos.EsReservacionProxima,
            Mensaje = $"📅 Nueva reservación: {datos.ClienteNombre} para {datos.CantidadPersonas} persona(s) " +
                     $"el {datos.FechaHoraReservacion:dd/MM/yyyy HH:mm}",
            FechaNotificacion = DateTime.UtcNow
        };

        _logger.LogInformation("👥 Enviando notificación al personal: {@NotificacionPersonal}", notificacionPersonal);

        // TODO: Implementar notificación al personal (dashboard en tiempo real, etc.)
        // await _notificationService.SendStaffNotificationAsync(notificacionPersonal, cancellationToken);
    }

    /// <summary>
    /// 📊 Registra estadísticas de la reservación para analytics
    /// </summary>
    private async Task RegistrarEstadisticasReservacion(
        Reservacion reservacion,
        Cliente cliente,
        Domain.Operaciones.Reservaciones.Events.Reservacion.ReservacionCreada evento,
        CancellationToken cancellationToken)
    {
        var fechaReservacion = evento.Fecha.Date + evento.Hora;
        var horaReservacion = evento.Hora.Hours;
        var diaReservacion = evento.Fecha.DayOfWeek;
        var tiempoAnticipacion = fechaReservacion - DateTime.UtcNow;
        var nivelClienteFidelizacion = NivelFidelizacion.Basico; // Valor por defecto

        var estadisticas = new
        {
            FechaHora = DateTime.UtcNow,
            ReservacionId = evento.ReservacionId,
            ClienteId = evento.ClienteId,
            FechaReservacion = fechaReservacion,
            DiaSemanaSolicitado = diaReservacion.ToString(),
            HoraSolicitada = horaReservacion,
            CantidadPersonas = evento.CantidadPersonas,
            TiempoAnticipacion = tiempoAnticipacion,
            TieneEmail = !string.IsNullOrEmpty(cliente.Email.Value),
            TieneTelefono = !string.IsNullOrEmpty(cliente.Telefono?.Value),
            ClienteFidelizado = nivelClienteFidelizacion != NivelFidelizacion.Basico,
            TieneObservaciones = !string.IsNullOrEmpty(reservacion.Observaciones),
            MesaId = evento.MesaId,
            FechaReservacionDentroHorarioComercial = horaReservacion >= 11 && horaReservacion <= 22,
            EsReservacionFinDeSemana = diaReservacion == DayOfWeek.Friday || diaReservacion == DayOfWeek.Saturday || diaReservacion == DayOfWeek.Sunday,
            EsReservacionHoraPico = horaReservacion >= 12 && horaReservacion <= 14 || horaReservacion >= 19 && horaReservacion <= 21
        };

        _logger.LogInformation("📊 Estadísticas de reservación registradas: {@EstadisticasReservacion}", estadisticas);
        
        // TODO: Enviar a sistema de analytics
        // await _analyticsService.RecordReservationStatsAsync(estadisticas, cancellationToken);
    }

    /// <summary>
    /// 🔢 Genera código de confirmación único para la reservación
    /// </summary>
    private string GenerarCodigoConfirmacion(Guid reservacionId)
    {
        var hash = reservacionId.ToString().GetHashCode();
        var codigo = Math.Abs(hash % 100000).ToString("D5");
        return $"RP{codigo}";
    }

    /// <summary>
    /// ⏰ Formatea el tiempo restante hasta la reservación
    /// </summary>
    private string FormatearTiempoRestante(TimeSpan tiempoRestante)
    {
        if (tiempoRestante.TotalDays >= 1)
        {
            return $"{(int)tiempoRestante.TotalDays} día(s)";
        }
        else if (tiempoRestante.TotalHours >= 1)
        {
            return $"{(int)tiempoRestante.TotalHours} hora(s)";
        }
        else
        {
            return $"{(int)tiempoRestante.TotalMinutes} minuto(s)";
        }
    }

    /// <summary>
    /// 📅 Estructura de datos para confirmación de reservación
    /// </summary>
    private class ConfirmacionReservacionData
    {
        public Guid ReservacionId { get; set; }
        public Guid ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;
        public string? ClienteTelefono { get; set; }
        public DateTime FechaHoraReservacion { get; set; }
        public int CantidadPersonas { get; set; }
        public string EstadoReservacion { get; set; } = string.Empty;
        public string? ObservacionesEspeciales { get; set; }
        public int? MesaAsignada { get; set; }
        public int? CapacidadMesa { get; set; }
        public TimeSpan TiempoHastaReservacion { get; set; }
        public bool EsReservacionProxima { get; set; }
        public bool ClienteFidelizado { get; set; }
        public string CodigoConfirmacion { get; set; } = string.Empty;
    }
} 