using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;

namespace RestaurantePro.Mobile.Core.Services.Inventory;

public class PreparacionesService : IPreparacionesService
{
    private readonly IApiService _apiService;

    public PreparacionesService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<ApiResponse<List<PreparacionDto>>> ObtenerPreparacionesAsync(bool soloDisponibles = true)
    {
        try
        {
            var response = await _apiService.GetAsync<List<PreparacionDto>>($"api/preparaciones?soloDisponibles={soloDisponibles}");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PreparacionDto>>.Failure($"Error al obtener preparaciones: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PreparacionDto>> ObtenerPreparacionAsync(Guid id)
    {
        try
        {
            var response = await _apiService.GetAsync<PreparacionDto>($"api/preparaciones/{id}");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PreparacionDto>.Failure($"Error al obtener preparación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<PreparacionDto>>> BuscarPreparacionesAsync(string terminoBusqueda)
    {
        try
        {
            var response = await _apiService.GetAsync<List<PreparacionDto>>($"api/preparaciones/buscar?termino={terminoBusqueda}");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PreparacionDto>>.Failure($"Error al buscar preparaciones: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PreparacionDto>> CrearPreparacionAsync(PreparacionDto preparacion)
    {
        try
        {
            var response = await _apiService.PostAsync<PreparacionDto>("api/preparaciones", preparacion);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PreparacionDto>.Failure($"Error al crear preparación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PreparacionDto>> ActualizarPreparacionAsync(Guid id, PreparacionDto preparacion)
    {
        try
        {
            var response = await _apiService.PutAsync<PreparacionDto>($"api/preparaciones/{id}", preparacion);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PreparacionDto>.Failure($"Error al actualizar preparación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> EliminarPreparacionAsync(Guid id)
    {
        try
        {
            var response = await _apiService.DeleteAsync($"api/preparaciones/{id}");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Error al eliminar preparación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<EstadisticasPreparacionesDto>> ObtenerEstadisticasAsync()
    {
        try
        {
            var response = await _apiService.GetAsync<EstadisticasPreparacionesDto>("api/preparaciones/estadisticas");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<EstadisticasPreparacionesDto>.Failure($"Error al obtener estadísticas: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<PreparacionDto>>> ObtenerPreparacionesPorCategoriaAsync(string categoria)
    {
        try
        {
            var response = await _apiService.GetAsync<List<PreparacionDto>>($"api/preparaciones/por-categoria?categoria={categoria}");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PreparacionDto>>.Failure($"Error al obtener preparaciones por categoría: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PreparacionDto>> CambiarDisponibilidadAsync(Guid id, bool disponible)
    {
        try
        {
            var request = new { Disponible = disponible };
            var response = await _apiService.PostAsync<PreparacionDto>($"api/preparaciones/{id}/cambiar-disponibilidad", request);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PreparacionDto>.Failure($"Error al cambiar disponibilidad: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PreparacionDto>> IniciarPreparacionAsync(Guid id, IniciarPreparacionDto dto)
    {
        try
        {
            var response = await _apiService.PostAsync<PreparacionDto>($"api/preparaciones/{id}/iniciar", dto);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PreparacionDto>.Failure($"Error al iniciar preparación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PreparacionDto>> CompletarPreparacionAsync(Guid id)
    {
        try
        {
            var response = await _apiService.PostAsync<PreparacionDto>($"api/preparaciones/{id}/completar", new { });
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PreparacionDto>.Failure($"Error al completar preparación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PreparacionDto>> CancelarPreparacionAsync(Guid id, CancelarPreparacionDto dto)
    {
        try
        {
            var response = await _apiService.PostAsync<PreparacionDto>($"api/preparaciones/{id}/cancelar", dto);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PreparacionDto>.Failure($"Error al cancelar preparación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<PreparacionDto>>> ObtenerColaPreparacionesAsync()
    {
        try
        {
            var response = await _apiService.GetAsync<List<PreparacionDto>>("api/preparaciones/cola");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PreparacionDto>>.Failure($"Error al obtener cola de preparaciones: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<PreparacionDto>>> ObtenerPreparacionesPorEstadoAsync(string estado)
    {
        try
        {
            var response = await _apiService.GetAsync<List<PreparacionDto>>($"api/preparaciones/estado/{estado}");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PreparacionDto>>.Failure($"Error al obtener preparaciones por estado: {ex.Message}");
        }
    }
} 