using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IReservacionesApiService
{
    // Obtener reservaciones
    Task<PaginatedList<ReservacionDto>?> ObtenerReservacionesAsync(ReservacionFiltrosDto filtros);
    Task<ReservacionDto?> ObtenerReservacionPorIdAsync(Guid id);
    
    // CRUD de reservaciones
    Task<ReservacionDto?> CrearReservacionAsync(CrearReservacionRequest request);
    Task<ReservacionDto?> ActualizarReservacionAsync(Guid id, ActualizarReservacionRequest request);
    Task<bool> EliminarReservacionAsync(Guid id);
    
    // Estados de reservaciones
    Task<bool> ConfirmarReservacionAsync(ConfirmarReservacionRequest request);
    Task<bool> CancelarReservacionAsync(CancelarReservacionRequest request);
    Task<bool> MarcarLlegadaAsync(MarcarLlegadaRequest request);
    
    // Estadísticas y reportes
    Task<ReservacionEstadisticasDto?> ObtenerEstadisticasAsync();
}
