using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;

namespace RestaurantePro.Mobile.Core.Services.Inventory;

public class ReservacionesService : IReservacionesService
{
    private readonly IApiService _apiService;

    public ReservacionesService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<ApiResponse<List<ReservacionDto>>> ObtenerReservacionesAsync()
    {
        try
        {
            var response = await _apiService.GetAsync<List<ReservacionDto>>("api/reservaciones");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ReservacionDto>>.Failure($"Error al obtener reservaciones: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ReservacionDto>> ObtenerReservacionAsync(Guid id)
    {
        try
        {
            var response = await _apiService.GetAsync<ReservacionDto>($"api/reservaciones/{id}");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<ReservacionDto>.Failure($"Error al obtener reservación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<ReservacionDto>>> BuscarReservacionesAsync(string terminoBusqueda)
    {
        try
        {
            var response = await _apiService.GetAsync<List<ReservacionDto>>($"api/reservaciones/buscar?termino={terminoBusqueda}");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ReservacionDto>>.Failure($"Error al buscar reservaciones: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ReservacionDto>> CrearReservacionAsync(ReservacionDto reservacion)
    {
        try
        {
            var response = await _apiService.PostAsync<ReservacionDto>("api/reservaciones", reservacion);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<ReservacionDto>.Failure($"Error al crear reservación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ReservacionDto>> ActualizarReservacionAsync(Guid id, ReservacionDto reservacion)
    {
        try
        {
            var response = await _apiService.PutAsync<ReservacionDto>($"api/reservaciones/{id}", reservacion);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<ReservacionDto>.Failure($"Error al actualizar reservación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> EliminarReservacionAsync(Guid id)
    {
        try
        {
            var response = await _apiService.DeleteAsync($"api/reservaciones/{id}");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Error al eliminar reservación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<EstadisticasReservacionesDto>> ObtenerEstadisticasAsync()
    {
        try
        {
            var response = await _apiService.GetAsync<EstadisticasReservacionesDto>("api/reservaciones/estadisticas");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<EstadisticasReservacionesDto>.Failure($"Error al obtener estadísticas: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<ReservacionDto>>> ObtenerReservacionesPorFechaAsync(DateTime fecha)
    {
        try
        {
            var response = await _apiService.GetAsync<List<ReservacionDto>>($"api/reservaciones/por-fecha?fecha={fecha:yyyy-MM-dd}");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ReservacionDto>>.Failure($"Error al obtener reservaciones por fecha: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<ReservacionDto>>> ObtenerReservacionesHoyAsync()
    {
        try
        {
            var response = await _apiService.GetAsync<List<ReservacionDto>>("api/reservaciones/hoy");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ReservacionDto>>.Failure($"Error al obtener reservaciones de hoy: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ReservacionDto>> CambiarEstadoReservacionAsync(Guid id, string nuevoEstado)
    {
        try
        {
            var request = new { NuevoEstado = nuevoEstado };
            var response = await _apiService.PostAsync<ReservacionDto>($"api/reservaciones/{id}/cambiar-estado", request);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<ReservacionDto>.Failure($"Error al cambiar estado de reservación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ReservacionDto>> AsignarMesaAsync(Guid id, string mesa)
    {
        try
        {
            var request = new { Mesa = mesa };
            var response = await _apiService.PostAsync<ReservacionDto>($"api/reservaciones/{id}/asignar-mesa", request);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<ReservacionDto>.Failure($"Error al asignar mesa: {ex.Message}");
        }
    }
} 