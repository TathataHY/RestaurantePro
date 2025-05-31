using EstadoReservacionDomain = RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionPorId;

/// <summary>
/// Handler para ObtenerReservacionPorIdQuery
/// </summary>
public class ObtenerReservacionPorIdHandler : IRequestHandler<ObtenerReservacionPorIdQuery, Result<ReservacionDto>>
{
    private readonly IReservacionRepository _reservacionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerReservacionPorIdHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public ObtenerReservacionPorIdHandler(
        IReservacionRepository reservacionRepository,
        IMapper mapper,
        ILogger<ObtenerReservacionPorIdHandler> logger,
        ICurrentUserService currentUser)
    {
        _reservacionRepository = reservacionRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<ReservacionDto>> Handle(ObtenerReservacionPorIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔍 Buscando reservación con ID {ReservacionId}", request.Id);

            // Buscar la reservación por ID
            var reservacion = await _reservacionRepository.ObtenerPorIdAsync(request.Id, cancellationToken);

            if (reservacion == null)
            {
                var mensajeError = $"Reservación con ID {request.Id} no encontrada";
                _logger.LogWarning("⚠️ {MensajeError}", mensajeError);
                return Result.Failure<ReservacionDto>(mensajeError);
            }

            // Mapear a DTO
            var reservacionDto = _mapper.Map<ReservacionDto>(reservacion);

            // Log de información detallada según el estado
            _logger.LogDebug("📋 Reservación encontrada - Cliente: {ClienteId}, Mesa: {MesaId}, Estado: {Estado}, Fecha: {Fecha}", 
                reservacion.ClienteId, 
                reservacion.MesaId, 
                reservacion.Estado, 
                reservacion.FechaReservacion.ToShortDateString());

            // Log personalizado según el estado de la reservación
            switch (reservacion.Estado)
            {
                case EstadoReservacionDomain.Pendiente:
                    _logger.LogInformation("📝 Reservación {Id} está en estado Pendiente (pendiente de confirmación)", request.Id);
                    break;
                case EstadoReservacionDomain.Confirmada:
                    _logger.LogInformation("✅ Reservación {Id} está confirmada para {Fecha} a las {Hora}", 
                        request.Id, 
                        reservacion.Fecha.ToShortDateString(), 
                        reservacion.Hora.ToString(@"hh\:mm"));
                    break;
                case EstadoReservacionDomain.Completada:
                    _logger.LogInformation("🎉 Reservación {Id} completada exitosamente", request.Id);
                    break;
                case EstadoReservacionDomain.Cancelada:
                    _logger.LogInformation("❌ Reservación {Id} fue cancelada", request.Id);
                    break;
                case EstadoReservacionDomain.NoShow:
                    _logger.LogInformation("🚫 Reservación {Id} marcada como No Show", request.Id);
                    break;
            }

            // Log adicional si hay observaciones
            if (!string.IsNullOrEmpty(reservacion.Observaciones))
            {
                _logger.LogDebug("📝 Observaciones: {Observaciones}", reservacion.Observaciones);
            }

            // Log información de capacidad y tiempo
            _logger.LogDebug("👥 Cantidad de personas: {CantidadPersonas}, Duración estimada: {DuracionEstimada} minutos", 
                reservacion.CantidadPersonas, 
                reservacion.DuracionEstimada.TotalMinutes);

            _logger.LogInformation("✅ Reservación {Id} obtenida exitosamente", request.Id);

            return Result.Success(reservacionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener reservación por ID {ReservacionId}", request.Id);
            return Result.Failure<ReservacionDto>("Error interno del servidor al obtener la reservación");
        }
    }
} 