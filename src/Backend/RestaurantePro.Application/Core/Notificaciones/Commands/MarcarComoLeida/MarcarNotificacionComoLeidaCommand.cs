namespace RestaurantePro.Application.Core.Notificaciones.Commands.MarcarComoLeida;

/// <summary>
/// Comando para marcar una notificación como leída
/// </summary>
public class MarcarNotificacionComoLeidaCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// ID de la notificación a marcar como leída
    /// </summary>
    public Guid NotificacionId { get; set; }
} 