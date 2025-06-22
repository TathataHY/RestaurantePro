namespace RestaurantePro.Application.Core.Notificaciones.Queries.ObtenerNotificacionPorId;

/// <summary>
/// Handler para obtener una notificación por ID
/// </summary>
public class ObtenerNotificacionPorIdQueryHandler : IRequestHandler<ObtenerNotificacionPorIdQuery, Result<NotificacionDto>>
{
    private readonly INotificacionRepository _notificacionRepository;
    private readonly ILogger<ObtenerNotificacionPorIdQueryHandler> _logger;

    public ObtenerNotificacionPorIdQueryHandler(
        INotificacionRepository notificacionRepository,
        ILogger<ObtenerNotificacionPorIdQueryHandler> logger)
    {
        _notificacionRepository = notificacionRepository;
        _logger = logger;
    }

    public async Task<Result<NotificacionDto>> Handle(ObtenerNotificacionPorIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var notificacion = await _notificacionRepository.ObtenerPorIdAsync(request.NotificacionId, cancellationToken);
            
            if (notificacion == null)
            {
                _logger.LogWarning("Notificación no encontrada: {NotificacionId}", request.NotificacionId);
                return Result.Failure<NotificacionDto>("Notificación no encontrada");
            }

            var notificacionDto = new NotificacionDto
            {
                Id = notificacion.Id,
                Titulo = notificacion.Titulo,
                Mensaje = notificacion.Mensaje,
                Tipo = notificacion.Tipo.ToString(),
                FechaCreacion = notificacion.FechaCreacion,
                FechaLectura = notificacion.FechaLectura,
                EstaLeida = notificacion.EstaLeida,
                EntidadRelacionadaId = notificacion.EntidadRelacionadaId
            };

            _logger.LogInformation("Notificación obtenida: {NotificacionId}", request.NotificacionId);

            return Result.Success(notificacionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener notificación: {NotificacionId}", request.NotificacionId);
            return Result.Failure<NotificacionDto>("Error interno al obtener la notificación");
        }
    }
} 