namespace RestaurantePro.Application.Core.Notificaciones.Queries.ObtenerNotificacionPorId;

/// <summary>
/// Query para obtener una notificación por ID
/// </summary>
public class ObtenerNotificacionPorIdQuery : IRequest<Result<NotificacionDto>>
{
    /// <summary>
    /// ID de la notificación a obtener
    /// </summary>
    public Guid NotificacionId { get; set; }
} 