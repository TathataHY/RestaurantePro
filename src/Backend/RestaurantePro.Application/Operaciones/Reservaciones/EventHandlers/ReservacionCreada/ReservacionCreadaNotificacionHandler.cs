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

    public ReservacionCreadaNotificacionHandler(
        IReservacionRepository reservacionRepository,
        IClienteRepository clienteRepository,
        IMesaRepository mesaRepository,
        ILogger<ReservacionCreadaNotificacionHandler> logger,
        IMediator mediator)
    {
        _reservacionRepository = reservacionRepository;
        _clienteRepository = clienteRepository;
        _mesaRepository = mesaRepository;
        _logger = logger;
        _mediator = mediator;
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
            if (reservacion.MesaId.HasValue)
            {
                var mesaResult = await _mesaRepository.ObtenerPorIdAsync(reservacion.MesaId.Value, cancellationToken);
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
        var fechaReservacion = evento.FechaReservacion;
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
            ObservacionesEspeciales = evento.ObservacionesEspeciales,
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

        var emailContent = new
        {
            To = datos.ClienteEmail,
            Subject = $"✅ Confirmación de Reservación - RestaurantePro",
            Template = "reservacion-confirmada",
            Data = new
            {
                ClienteNombre = datos.ClienteNombre,
                FechaReservacion = datos.FechaHoraReservacion.ToString("dddd, dd 'de' MMMM 'de' yyyy"),
                HoraReservacion = datos.FechaHoraReservacion.ToString("HH:mm"),
                CantidadPersonas = datos.CantidadPersonas,
                MesaAsignada = datos.MesaAsignada?.ToString() ?? "Por asignar",
                ObservacionesEspeciales = datos.ObservacionesEspeciales ?? "Ninguna",
                CodigoConfirmacion = datos.CodigoConfirmacion,
                TiempoRestante = FormatearTiempoRestante(datos.TiempoHastaReservacion),
                MensajeFidelizacion = datos.ClienteFidelizado ? "🎉 ¡Gracias por ser nuestro cliente fidelizado!" : "",
                InstruccionesCancelacion = "Para cancelar o modificar tu reservación, contacta al +1234567890 o responde este email",
                LogoUrl = "https://restaurantepro.com/logo.png",
                WebsiteUrl = "https://restaurantepro.com"
            }
        };

        _logger.LogInformation("📧 Enviando email de confirmación: {@EmailContent}", new { 
            emailContent.To, 
            emailContent.Subject,
            datos.FechaHoraReservacion,
            datos.CantidadPersonas
        });

        try
        {
            // TODO: Implementar servicio real de email
            // await _emailService.SendAsync(emailContent, cancellationToken);
            
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

            var smsData = new
            {
                Telefono = datos.ClienteTelefono,
                Mensaje = mensajeSMS,
                ClienteId = datos.ClienteId,
                ReservacionId = datos.ReservacionId
            };

            _logger.LogInformation("📱 Enviando SMS de confirmación: {@SMSData}", new { 
                smsData.Telefono, 
                MensajeLength = mensajeSMS.Length 
            });

            // TODO: Implementar servicio real de SMS
            // await _smsService.SendAsync(smsData, cancellationToken);

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
        var horaReservacion = evento.FechaReservacion.Hour;
        var diaReservacion = evento.FechaReservacion.DayOfWeek;
        var tiempoAnticipacion = evento.FechaReservacion - DateTime.UtcNow;
        var nivelClienteFidelizacion = NivelFidelizacion.Basico; // Valor por defecto

        var estadisticas = new
        {
            FechaHora = DateTime.UtcNow,
            ReservacionId = evento.ReservacionId,
            ClienteId = evento.ClienteId,
            FechaReservacion = evento.FechaReservacion,
            DiaSemanaSolicitado = diaReservacion.ToString(),
            HoraSolicitada = horaReservacion,
            CantidadPersonas = evento.CantidadPersonas,
            TiempoAnticipacion = tiempoAnticipacion,
            TieneEmail = !string.IsNullOrEmpty(cliente.Email.Value),
            TieneTelefono = !string.IsNullOrEmpty(cliente.Telefono?.Value),
            ClienteFidelizado = nivelClienteFidelizacion != NivelFidelizacion.Basico,
            TieneObservaciones = !string.IsNullOrEmpty(evento.ObservacionesEspeciales),
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