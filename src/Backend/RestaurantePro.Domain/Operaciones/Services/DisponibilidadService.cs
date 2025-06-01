namespace RestaurantePro.Domain.Operaciones.Services;

/// <summary>
/// Implementación del servicio de disponibilidad para reservaciones y mesas
/// </summary>
public class DisponibilidadService : IDisponibilidadService
{
    private readonly IReservacionRepository _reservacionRepository;
    private readonly IMesaRepository _mesaRepository;
    private readonly ILogger<DisponibilidadService> _logger;
    private readonly INotificationManager _notificationManager;

    public DisponibilidadService(
        IReservacionRepository reservacionRepository,
        IMesaRepository mesaRepository,
        ILogger<DisponibilidadService> logger,
        INotificationManager notificationManager)
    {
        _reservacionRepository = reservacionRepository ?? throw new ArgumentNullException(nameof(reservacionRepository));
        _mesaRepository = mesaRepository ?? throw new ArgumentNullException(nameof(mesaRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
    }

    /// <inheritdoc />
    public async Task<Result> VerificarDisponibilidadAsync(Guid mesaId, DateTime fechaHora, CancellationToken cancellationToken = default)
    {
        _notificationManager.CreateNewNotification();

        try
        {
            // Validar que la mesa exista
            var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId);
            if (mesa == null)
            {
                _notificationManager.AddError($"La mesa con ID {mesaId} no existe", nameof(mesaId));
                return _notificationManager.ToResult();
            }

            // Verificar disponibilidad en el repositorio
            var disponible = await _reservacionRepository.VerificarDisponibilidadMesaAsync(
                mesaId, 
                fechaHora.Date, 
                fechaHora.TimeOfDay, 
                90, // Duración por defecto
                cancellationToken);

            if (!disponible)
            {
                _notificationManager.AddError("La mesa no está disponible en el horario solicitado", "Disponibilidad");
                return _notificationManager.ToResult();
            }

            _logger.LogInformation("Mesa {MesaId} disponible para {FechaHora}", mesaId, fechaHora);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar disponibilidad de mesa {MesaId}", mesaId);
            _notificationManager.AddError($"Error interno al verificar disponibilidad: {ex.Message}", "VerificarDisponibilidad");
            return _notificationManager.ToResult();
        }
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<Guid>>> VerificarDisponibilidadGeneralAsync(DateTime fechaHora, int numeroPersonas, CancellationToken cancellationToken = default)
    {
        _notificationManager.CreateNewNotification();

        try
        {
            // Obtener mesas disponibles
            var mesasDisponibles = await _reservacionRepository.ObtenerMesasDisponiblesAsync(
                fechaHora.Date,
                fechaHora.TimeOfDay,
                numeroPersonas,
                90, // Duración por defecto
                cancellationToken);

            _logger.LogInformation("Encontradas {CantidadMesas} mesas disponibles para {NumeroPersonas} personas en {FechaHora}", 
                mesasDisponibles.Count(), numeroPersonas, fechaHora);

            return Result.Success(mesasDisponibles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar disponibilidad general para {NumeroPersonas} personas", numeroPersonas);
            _notificationManager.AddError($"Error al verificar disponibilidad: {ex.Message}", "VerificarDisponibilidadGeneral");
            return _notificationManager.ToResult<IEnumerable<Guid>>(Enumerable.Empty<Guid>());
        }
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<DateTime>>> ObtenerAlternativasHorarioAsync(DateTime fechaHora, int numeroPersonas, int rangoMinutos = 120, CancellationToken cancellationToken = default)
    {
        _notificationManager.CreateNewNotification();

        try
        {
            var alternativas = new List<DateTime>();
            var fecha = fechaHora.Date;
            var horaInicial = fechaHora.TimeOfDay;

            // Buscar alternativas hacia adelante y hacia atrás
            var incrementos = new[] { 30, 60, 90, 120 }; // minutos

            foreach (var incremento in incrementos.Where(i => i <= rangoMinutos))
            {
                // Hacia adelante
                var horarioAdelante = fecha.Add(horaInicial.Add(TimeSpan.FromMinutes(incremento)));
                if (await TieneDisponibilidadAsync(horarioAdelante, numeroPersonas, cancellationToken))
                {
                    alternativas.Add(horarioAdelante);
                }

                // Hacia atrás
                if (horaInicial.TotalMinutes >= incremento)
                {
                    var horarioAtras = fecha.Add(horaInicial.Subtract(TimeSpan.FromMinutes(incremento)));
                    if (await TieneDisponibilidadAsync(horarioAtras, numeroPersonas, cancellationToken))
                    {
                        alternativas.Add(horarioAtras);
                    }
                }
            }

            return Result.Success<IEnumerable<DateTime>>(alternativas.OrderBy(h => h));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener alternativas de horario");
            _notificationManager.AddError($"Error al obtener alternativas: {ex.Message}", "ObtenerAlternativas");
            return _notificationManager.ToResult<IEnumerable<DateTime>>(Enumerable.Empty<DateTime>());
        }
    }

    /// <inheritdoc />
    public async Task<Result> ValidarCapacidadMesaAsync(Guid mesaId, int numeroPersonas, CancellationToken cancellationToken = default)
    {
        _notificationManager.CreateNewNotification();

        try
        {
            var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId);
            if (mesa == null)
            {
                _notificationManager.AddError($"La mesa con ID {mesaId} no existe", nameof(mesaId));
                return _notificationManager.ToResult();
            }

            if (mesa.Capacidad < numeroPersonas)
            {
                _notificationManager.AddError($"La mesa tiene capacidad para {mesa.Capacidad} personas, pero se requieren {numeroPersonas}", "Capacidad");
                return _notificationManager.ToResult();
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar capacidad de mesa {MesaId}", mesaId);
            _notificationManager.AddError($"Error al validar capacidad: {ex.Message}", "ValidarCapacidad");
            return _notificationManager.ToResult();
        }
    }

    /// <inheritdoc />
    public async Task<Result<decimal>> CalcularOcupacionAsync(DateTime fechaHora, CancellationToken cancellationToken = default)
    {
        _notificationManager.CreateNewNotification();

        try
        {
            // Obtener todas las mesas
            var todasLasMesas = await _mesaRepository.ObtenerTodasAsync();
            var totalCapacidad = todasLasMesas.Sum(m => m.Capacidad);

            if (totalCapacidad == 0)
            {
                return Result.Success(0m);
            }

            // Obtener reservaciones activas en el horario
            var reservaciones = await _reservacionRepository.ObtenerPorFechaAsync(fechaHora.Date, cancellationToken);
            var reservacionesActivas = reservaciones.Where(r => 
                r.Estado == EstadoReservacion.Confirmada || r.Estado == EstadoReservacion.Pendiente);

            var capacidadOcupada = reservacionesActivas.Sum(r => r.CantidadPersonas);
            var porcentajeOcupacion = (decimal)capacidadOcupada / totalCapacidad * 100;

            return Result.Success(Math.Round(porcentajeOcupacion, 2));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular ocupación para {FechaHora}", fechaHora);
            _notificationManager.AddError($"Error al calcular ocupación: {ex.Message}", "CalcularOcupacion");
            return _notificationManager.ToResult<decimal>(0m);
        }
    }

    #region Métodos Privados

    private async Task<bool> TieneDisponibilidadAsync(DateTime fechaHora, int numeroPersonas, CancellationToken cancellationToken)
    {
        try
        {
            var mesasDisponibles = await _reservacionRepository.ObtenerMesasDisponiblesAsync(
                fechaHora.Date,
                fechaHora.TimeOfDay,
                numeroPersonas,
                90,
                cancellationToken);

            return mesasDisponibles.Any();
        }
        catch
        {
            return false;
        }
    }

    #endregion
} 