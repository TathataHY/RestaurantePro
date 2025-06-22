namespace RestaurantePro.Application.Core.Notificaciones.Queries.ObtenerNotificaciones;

/// <summary>
/// Query para obtener notificaciones
/// </summary>
public class ObtenerNotificacionesQuery : IRequest<Result<List<NotificacionDto>>>
{
    /// <summary>
    /// ID del usuario cuyas notificaciones se obtendrán
    /// </summary>
    public Guid UsuarioId { get; set; }
    
    /// <summary>
    /// Si true, solo retorna notificaciones no leídas
    /// </summary>
    public bool SoloNoLeidas { get; set; } = false;
} 