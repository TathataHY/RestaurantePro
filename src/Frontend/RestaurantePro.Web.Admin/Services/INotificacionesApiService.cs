using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface INotificacionesApiService
{
    Task<List<NotificacionDto>> ObtenerNotificacionesAsync();
    Task<NotificacionDto?> ObtenerNotificacionPorIdAsync(Guid id);
    Task<NotificacionDto?> CrearNotificacionAsync(CrearNotificacionRequest request);
    Task<bool> MarcarComoLeidaAsync(Guid id);
    Task<bool> EliminarNotificacionAsync(Guid id);
    Task<int> ObtenerCantidadNoLeidasAsync();
    Task<int> MarcarTodasComoLeidasAsync();
}
