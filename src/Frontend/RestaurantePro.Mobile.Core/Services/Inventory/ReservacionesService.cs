using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;

namespace RestaurantePro.Mobile.Core.Services.Inventory;

public class ReservacionesService : IReservacionesService
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;

    public ReservacionesService(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
    }

    public async Task<ApiResponse<List<ReservacionDto>>> ObtenerReservacionesAsync()
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<List<ReservacionDto>>("api/operaciones/reservaciones", token);
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
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<ReservacionDto>($"api/operaciones/reservaciones/{id}", token);
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
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<List<ReservacionDto>>($"api/operaciones/reservaciones/buscar?termino={terminoBusqueda}", token);
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
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.PostAsync<ReservacionDto>("api/operaciones/reservaciones", reservacion, token);
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
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.PutAsync<ReservacionDto>($"api/operaciones/reservaciones/{id}", reservacion, token);
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
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.DeleteAsync($"api/operaciones/reservaciones/{id}", token);
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
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<EstadisticasReservacionesDto>("api/operaciones/reservaciones/estadisticas", token);
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
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<List<ReservacionDto>>($"api/operaciones/reservaciones?fecha={fecha:yyyy-MM-dd}", token);
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
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<List<ReservacionDto>>("api/operaciones/reservaciones/hoy", token);
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
            var token = await _authService.GetTokenAsync();
            var request = new { NuevoEstado = nuevoEstado };
            var response = await _apiService.PostAsync<ReservacionDto>($"api/operaciones/reservaciones/{id}/cambiar-estado", request, token);
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
            var token = await _authService.GetTokenAsync();
            var request = new { Mesa = mesa };
            var response = await _apiService.PostAsync<ReservacionDto>($"api/operaciones/reservaciones/{id}/asignar-mesa", request, token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<ReservacionDto>.Failure($"Error al asignar mesa: {ex.Message}");
        }
    }
} 