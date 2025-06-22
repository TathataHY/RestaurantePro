namespace RestaurantePro.Application.Core.Notificaciones.Commands.MarcarComoLeida;

/// <summary>
/// Handler para marcar una notificación como leída
/// </summary>
public class MarcarNotificacionComoLeidaCommandHandler : IRequestHandler<MarcarNotificacionComoLeidaCommand, Result<bool>>
{
    private readonly INotificacionRepository _notificacionRepository;
    private readonly ILogger<MarcarNotificacionComoLeidaCommandHandler> _logger;

    public MarcarNotificacionComoLeidaCommandHandler(
        INotificacionRepository notificacionRepository,
        ILogger<MarcarNotificacionComoLeidaCommandHandler> logger)
    {
        _notificacionRepository = notificacionRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(MarcarNotificacionComoLeidaCommand request, CancellationToken cancellationToken)
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

            // Marcar como leída
            notificacion.MarcarComoLeida();

            // Guardar cambios
            await _notificacionRepository.GuardarCambiosAsync(cancellationToken);

            _logger.LogInformation("Notificación marcada como leída: {NotificacionId}", request.NotificacionId);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar notificación como leída: {NotificacionId}", request.NotificacionId);
            return Result.Failure<bool>("Error interno al marcar la notificación como leída");
        }
    }
} 