namespace RestaurantePro.Application.Core.Notificaciones.Commands.EliminarNotificacion;

/// <summary>
/// Handler para eliminar una notificación
/// </summary>
public class EliminarNotificacionCommandHandler : IRequestHandler<EliminarNotificacionCommand, Result<bool>>
{
    private readonly INotificacionRepository _notificacionRepository;
    private readonly ILogger<EliminarNotificacionCommandHandler> _logger;

    public EliminarNotificacionCommandHandler(
        INotificacionRepository notificacionRepository,
        ILogger<EliminarNotificacionCommandHandler> logger)
    {
        _notificacionRepository = notificacionRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(EliminarNotificacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Buscar la notificación
            var notificacion = await _notificacionRepository.ObtenerPorIdAsync(request.NotificacionId, cancellationToken);
            
            if (notificacion == null)
            {
                _logger.LogWarning("Notificación no encontrada: {NotificacionId}", request.NotificacionId);
                return Result.Failure<bool>("Notificación no encontrada");
            }

            // Eliminar la notificación
            await _notificacionRepository.EliminarAsync(notificacion, cancellationToken);
            await _notificacionRepository.GuardarCambiosAsync(cancellationToken);

            _logger.LogInformation("Notificación eliminada: {NotificacionId}", request.NotificacionId);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar notificación: {NotificacionId}", request.NotificacionId);
            return Result.Failure<bool>("Error interno al eliminar la notificación");
        }
    }
} 