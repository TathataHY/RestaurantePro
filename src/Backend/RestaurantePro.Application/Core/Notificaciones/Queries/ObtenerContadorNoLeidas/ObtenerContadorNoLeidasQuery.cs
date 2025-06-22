namespace RestaurantePro.Application.Core.Notificaciones.Queries.ObtenerContadorNoLeidas;

/// <summary>
/// Query para obtener el contador de notificaciones no leídas
/// </summary>
public class ObtenerContadorNoLeidasQuery : IRequest<Result<int>>
{
    /// <summary>
    /// ID del usuario para contar sus notificaciones no leídas
    /// </summary>
    public Guid UsuarioId { get; set; }
} 