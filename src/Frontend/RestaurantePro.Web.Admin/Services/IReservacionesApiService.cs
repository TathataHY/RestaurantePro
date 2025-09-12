using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IReservacionesApiService
{
    Task<List<ReservacionDto>> ObtenerReservacionesAsync();
    Task<ReservacionDto?> ObtenerReservacionPorIdAsync(Guid id);
    Task<ReservacionDto?> CrearReservacionAsync(CrearReservacionRequest request);
    Task<ReservacionDto?> ActualizarReservacionAsync(Guid id, ActualizarReservacionRequest request);
    Task<bool> EliminarReservacionAsync(Guid id);
    Task<bool> CambiarEstadoReservacionAsync(Guid id, EstadoReservacion estado);
}
