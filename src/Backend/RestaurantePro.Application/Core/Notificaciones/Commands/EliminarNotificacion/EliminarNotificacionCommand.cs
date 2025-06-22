namespace RestaurantePro.Application.Core.Notificaciones.Commands.EliminarNotificacion;

/// <summary>
/// Comando para eliminar una notificación
/// </summary>
public class EliminarNotificacionCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// ID de la notificación a eliminar
    /// </summary>
    public Guid NotificacionId { get; set; }
} 