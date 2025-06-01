namespace RestaurantePro.Domain.Operaciones.Services;

/// <summary>
/// Implementación del servicio de validación para reglas de negocio de reservaciones
/// </summary>
public class ValidacionReservacionService : IValidacionReservacionService
{
    private readonly IReservacionRepository _reservacionRepository;
    private readonly ILogger<ValidacionReservacionService> _logger;
    private readonly INotificationManager _notificationManager;

    // Configuración de reglas de negocio
    private readonly TimeSpan _horaApertura = new(11, 0, 0); // 11:00 AM
    private readonly TimeSpan _horaCierre = new(23, 0, 0);   // 11:00 PM
    private readonly int _maxReservacionesPorCliente = 2;
    private readonly int _anticipacionMinimaMinutos = 30;
    private readonly int _anticipacionMaximaDias = 90;

    public ValidacionReservacionService(
        IReservacionRepository reservacionRepository,
        ILogger<ValidacionReservacionService> logger,
        INotificationManager notificationManager)
    {
        _reservacionRepository = reservacionRepository ?? throw new ArgumentNullException(nameof(reservacionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
    }

    /// <inheritdoc />
    public Result ValidarHorarioPermitido(DateTime fechaHora)
    {
        var hora = fechaHora.TimeOfDay;

        // Validar que esté dentro del horario de atención
        if (hora < _horaApertura || hora > _horaCierre)
        {
            return Result.Failure($"Las reservaciones solo se permiten entre {_horaApertura:hh\\:mm} y {_horaCierre:hh\\:mm}");
        }

        // Validar que no sea en días de descanso (ejemplo: domingos)
        if (fechaHora.DayOfWeek == DayOfWeek.Sunday)
        {
            return Result.Failure("No se permiten reservaciones los domingos");
        }

        return Result.Success();
    }

    /// <inheritdoc />
    public Result ValidarCapacidadMesa(Mesa mesa, int numeroPersonas)
    {
        if (mesa == null)
        {
            return Result.Failure("La mesa especificada no existe");
        }

        if (numeroPersonas <= 0)
        {
            return Result.Failure("El número de personas debe ser mayor a cero");
        }

        if (numeroPersonas > mesa.Capacidad)
        {
            return Result.Failure($"La mesa tiene capacidad para {mesa.Capacidad} personas, pero se requieren {numeroPersonas}");
        }

        // Regla de negocio: No permitir reservaciones para una sola persona en mesas grandes
        if (numeroPersonas == 1 && mesa.Capacidad > 4)
        {
            return Result.Failure("No se permiten reservaciones individuales en mesas grandes (más de 4 personas)");
        }

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> ValidarLimiteReservacionesPorCliente(Guid clienteId, DateTime fecha, CancellationToken cancellationToken = default)
    {
        try
        {
            var reservacionesDelDia = await _reservacionRepository.ObtenerPorClienteAsync(clienteId, cancellationToken);
            var reservacionesActivas = reservacionesDelDia.Where(r => 
                r.Fecha.Date == fecha.Date && 
                (r.Estado == EstadoReservacion.Pendiente || r.Estado == EstadoReservacion.Confirmada));

            if (reservacionesActivas.Count() >= _maxReservacionesPorCliente)
            {
                return Result.Failure($"El cliente ya tiene {_maxReservacionesPorCliente} reservaciones para esta fecha");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar límite de reservaciones para cliente {ClienteId}", clienteId);
            return Result.Failure($"Error al validar límite de reservaciones: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public Result ValidarAnticipacionMinima(DateTime fechaReservacion, DateTime? fechaActual = null)
    {
        var ahora = fechaActual ?? DateTime.Now;

        // Validar que la fecha sea futura
        if (fechaReservacion <= ahora)
        {
            return Result.Failure("La fecha de reservación debe ser futura");
        }

        // Validar anticipación mínima
        var diferencia = fechaReservacion - ahora;
        if (diferencia.TotalMinutes < _anticipacionMinimaMinutos)
        {
            return Result.Failure($"La reservación debe realizarse con al menos {_anticipacionMinimaMinutos} minutos de anticipación");
        }

        // Validar anticipación máxima
        if (diferencia.TotalDays > _anticipacionMaximaDias)
        {
            return Result.Failure($"No se pueden hacer reservaciones con más de {_anticipacionMaximaDias} días de anticipación");
        }

        return Result.Success();
    }

    /// <inheritdoc />
    public Result ValidarOcasionEspecial(string tipoOcasion, DateTime fechaHora, int numeroPersonas)
    {
        if (string.IsNullOrEmpty(tipoOcasion))
        {
            return Result.Success(); // No hay ocasión especial
        }

        // Validaciones específicas por tipo de ocasión
        switch (tipoOcasion.ToLowerInvariant())
        {
            case "cumpleanos":
            case "aniversario":
                // Requerir más tiempo para celebraciones
                if (fechaHora.TimeOfDay > new TimeSpan(21, 0, 0)) // Después de 9 PM
                {
                    return Result.Failure("Las celebraciones de cumpleaños y aniversarios deben programarse antes de las 9:00 PM");
                }
                break;

            case "reunionnegocios":
                // Horarios específicos para reuniones de negocios
                if (fechaHora.TimeOfDay < new TimeSpan(12, 0, 0) || fechaHora.TimeOfDay > new TimeSpan(15, 0, 0))
                {
                    return Result.Failure("Las reuniones de negocios se programan preferiblemente entre 12:00 PM y 3:00 PM");
                }
                break;

            case "eventocorporativo":
                // Eventos corporativos requieren grupos grandes
                if (numeroPersonas < 8)
                {
                    return Result.Failure("Los eventos corporativos requieren al menos 8 personas");
                }
                break;
        }

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> ValidarPoliticaCancelacionAsync(Guid reservacionId, Guid clienteId, DateTime fechaCancelacion, CancellationToken cancellationToken = default)
    {
        try
        {
            var reservacion = await _reservacionRepository.ObtenerPorIdAsync(reservacionId, cancellationToken);
            if (reservacion == null)
            {
                return Result.Failure("La reservación no existe");
            }

            if (reservacion.ClienteId != clienteId)
            {
                return Result.Failure("No tiene permisos para cancelar esta reservación");
            }

            // Política: Cancelación hasta 2 horas antes
            var fechaReservacion = reservacion.Fecha.Add(reservacion.Hora);
            var horasAnticipacion = (fechaReservacion - fechaCancelacion).TotalHours;

            if (horasAnticipacion < 2)
            {
                return Result.Failure("La cancelación debe realizarse con al menos 2 horas de anticipación");
            }

            // Validar estado de la reservación
            if (reservacion.Estado == EstadoReservacion.Cancelada)
            {
                return Result.Failure("La reservación ya está cancelada");
            }

            if (reservacion.Estado == EstadoReservacion.Completada)
            {
                return Result.Failure("No se puede cancelar una reservación ya completada");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar política de cancelación para reservación {ReservacionId}", reservacionId);
            return Result.Failure($"Error al validar política de cancelación: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> ValidarRestriccionesDiaEspecialAsync(DateTime fechaHora, int numeroPersonas, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validar días festivos (ejemplo: 24 y 25 de diciembre, 31 de diciembre, 1 de enero)
            var diasFestivos = new[]
            {
                new DateTime(fechaHora.Year, 12, 24), // Nochebuena
                new DateTime(fechaHora.Year, 12, 25), // Navidad
                new DateTime(fechaHora.Year, 12, 31), // Fin de año
                new DateTime(fechaHora.Year, 1, 1),   // Año nuevo
            };

            if (diasFestivos.Contains(fechaHora.Date))
            {
                // Restricciones especiales para días festivos
                if (numeroPersonas < 4)
                {
                    return Result.Failure("En días festivos se requiere un mínimo de 4 personas por reservación");
                }

                if (fechaHora.TimeOfDay < new TimeSpan(18, 0, 0)) // Antes de 6 PM
                {
                    return Result.Failure("En días festivos solo se permiten reservaciones después de las 6:00 PM");
                }
            }

            // Validar días de alta demanda (viernes y sábados)
            if (fechaHora.DayOfWeek == DayOfWeek.Friday || fechaHora.DayOfWeek == DayOfWeek.Saturday)
            {
                // Verificar ocupación alta
                var reservacionesDelDia = await _reservacionRepository.ObtenerPorFechaAsync(fechaHora.Date, cancellationToken);
                var reservacionesActivas = reservacionesDelDia.Where(r => 
                    r.Estado == EstadoReservacion.Confirmada || r.Estado == EstadoReservacion.Pendiente);

                if (reservacionesActivas.Count() > 50) // Límite de reservaciones para días de alta demanda
                {
                    return Result.Failure("Los fines de semana tienen alta demanda. Se recomienda reservar con más anticipación");
                }
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar restricciones de día especial para {FechaHora}", fechaHora);
            return Result.Failure($"Error al validar restricciones: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> ValidarModificacionReservacionAsync(Guid reservacionId, DateTime nuevaFechaHora, int nuevoNumeroPersonas, CancellationToken cancellationToken = default)
    {
        try
        {
            var reservacion = await _reservacionRepository.ObtenerPorIdAsync(reservacionId, cancellationToken);
            if (reservacion == null)
            {
                return Result.Failure("La reservación no existe");
            }

            // Validar que la reservación se pueda modificar
            if (reservacion.Estado != EstadoReservacion.Pendiente && reservacion.Estado != EstadoReservacion.Confirmada)
            {
                return Result.Failure("Solo se pueden modificar reservaciones pendientes o confirmadas");
            }

            // Política: Modificación hasta 4 horas antes
            var fechaReservacionOriginal = reservacion.Fecha.Add(reservacion.Hora);
            var horasAnticipacion = (fechaReservacionOriginal - DateTime.Now).TotalHours;

            if (horasAnticipacion < 4)
            {
                return Result.Failure("Las modificaciones deben realizarse con al menos 4 horas de anticipación");
            }

            // Validar los nuevos valores
            var validacionHorario = ValidarHorarioPermitido(nuevaFechaHora);
            if (!validacionHorario.Succeeded)
            {
                return validacionHorario;
            }

            var validacionAnticipacion = ValidarAnticipacionMinima(nuevaFechaHora);
            if (!validacionAnticipacion.Succeeded)
            {
                return validacionAnticipacion;
            }

            // Validar que no se aumente excesivamente el número de personas
            if (nuevoNumeroPersonas > reservacion.CantidadPersonas + 2)
            {
                return Result.Failure("No se puede aumentar el número de personas en más de 2 por modificación");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar modificación de reservación {ReservacionId}", reservacionId);
            return Result.Failure($"Error al validar modificación: {ex.Message}");
        }
    }
} 