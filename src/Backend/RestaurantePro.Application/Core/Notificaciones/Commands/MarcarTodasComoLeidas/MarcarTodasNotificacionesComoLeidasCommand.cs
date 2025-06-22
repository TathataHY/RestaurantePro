namespace RestaurantePro.Application.Core.Notificaciones.Commands.MarcarTodasComoLeidas;

/// <summary>
/// Comando para marcar todas las notificaciones como leídas
/// </summary>
public class MarcarTodasNotificacionesComoLeidasCommand : IRequest<Result<int>>
{
    /// <summary>
    /// ID del usuario cuyas notificaciones se marcarán como leídas
    /// </summary>
    public Guid UsuarioId { get; set; }
} 