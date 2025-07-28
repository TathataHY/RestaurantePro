using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Inventory;

public interface IReservacionesService
{
    Task<ApiResponse<List<ReservacionDto>>> ObtenerReservacionesAsync();
    Task<ApiResponse<ReservacionDto>> ObtenerReservacionAsync(Guid id);
    Task<ApiResponse<List<ReservacionDto>>> BuscarReservacionesAsync(string terminoBusqueda);
    Task<ApiResponse<ReservacionDto>> CrearReservacionAsync(ReservacionDto reservacion);
    Task<ApiResponse<ReservacionDto>> ActualizarReservacionAsync(Guid id, ReservacionDto reservacion);
    Task<ApiResponse<bool>> EliminarReservacionAsync(Guid id);
    Task<ApiResponse<EstadisticasReservacionesDto>> ObtenerEstadisticasAsync();
    Task<ApiResponse<List<ReservacionDto>>> ObtenerReservacionesPorFechaAsync(DateTime fecha);
    Task<ApiResponse<List<ReservacionDto>>> ObtenerReservacionesHoyAsync();
    Task<ApiResponse<ReservacionDto>> CambiarEstadoReservacionAsync(Guid id, string nuevoEstado);
    Task<ApiResponse<ReservacionDto>> AsignarMesaAsync(Guid id, string mesa);
} 