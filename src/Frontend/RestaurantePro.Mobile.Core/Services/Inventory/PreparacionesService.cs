using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;

namespace RestaurantePro.Mobile.Core.Services.Inventory;

public class PreparacionesService : IPreparacionesService
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;

    public PreparacionesService(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
    }

    public async Task<ApiResponse<List<PreparacionDto>>> ObtenerPreparacionesAsync(bool soloDisponibles = true, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<List<PreparacionDto>>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            // El backend devuelve una respuesta paginada, necesitamos extraer los Items
            var response = await _apiService.GetAsync<PreparacionesPaginadasDto>("api/operaciones/preparaciones", token, cancellationToken);
            
            if (response.Succeeded && response.Data != null)
            {
                // Extraer la lista de preparaciones de la respuesta paginada
                var preparaciones = response.Data.Items;
                return ApiResponse<List<PreparacionDto>>.SuccessResponse(preparaciones, response.Message);
            }
            
            return ApiResponse<List<PreparacionDto>>.Failure(response.Error ?? "Error al obtener preparaciones");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PreparacionDto>>.Failure($"Error al obtener preparaciones: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PreparacionDto>> ObtenerPreparacionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<PreparacionDto>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<PreparacionDto>($"api/operaciones/preparaciones/{id}", token, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PreparacionDto>.Failure($"Error al obtener preparación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<PreparacionDto>>> BuscarPreparacionesAsync(string terminoBusqueda, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<List<PreparacionDto>>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<List<PreparacionDto>>($"api/operaciones/preparaciones/buscar?termino={terminoBusqueda}", token, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PreparacionDto>>.Failure($"Error al buscar preparaciones: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PreparacionDto>> CrearPreparacionAsync(PreparacionDto preparacion, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<PreparacionDto>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.PostAsync<PreparacionDto>("api/operaciones/preparaciones", preparacion, token, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PreparacionDto>.Failure($"Error al crear preparación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PreparacionDto>> ActualizarPreparacionAsync(Guid id, PreparacionDto preparacion, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<PreparacionDto>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.PutAsync<PreparacionDto>($"api/operaciones/preparaciones/{id}", preparacion, token, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PreparacionDto>.Failure($"Error al actualizar preparación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> EliminarPreparacionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<bool>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.DeleteAsync($"api/operaciones/preparaciones/{id}", token, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Error al eliminar preparación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<EstadisticasPreparacionesDto>> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<EstadisticasPreparacionesDto>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<EstadisticasPreparacionesDto>("api/operaciones/preparaciones/estadisticas", token, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<EstadisticasPreparacionesDto>.Failure($"Error al obtener estadísticas: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<PreparacionDto>>> ObtenerPreparacionesPorCategoriaAsync(string categoria, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<List<PreparacionDto>>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<List<PreparacionDto>>($"api/operaciones/preparaciones/por-categoria?categoria={categoria}", token, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PreparacionDto>>.Failure($"Error al obtener preparaciones por categoría: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PreparacionDto>> CambiarDisponibilidadAsync(Guid id, bool disponible, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<PreparacionDto>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            // El backend usa el endpoint /disponible, no /cambiar-disponibilidad
            var request = new { Disponible = disponible };
            var response = await _apiService.PostAsync<PreparacionDto>($"api/operaciones/preparaciones/{id}/disponible", request, token, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PreparacionDto>.Failure($"Error al cambiar disponibilidad: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PreparacionDto>> IniciarPreparacionAsync(Guid id, IniciarPreparacionDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<PreparacionDto>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.PostAsync<PreparacionDto>($"api/operaciones/preparaciones/{id}/iniciar", dto, token, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PreparacionDto>.Failure($"Error al iniciar preparación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PreparacionDto>> CompletarPreparacionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<PreparacionDto>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.PostAsync<PreparacionDto>($"api/operaciones/preparaciones/{id}/completar", new { }, token, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PreparacionDto>.Failure($"Error al completar preparación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PreparacionDto>> CancelarPreparacionAsync(Guid id, CancelarPreparacionDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<PreparacionDto>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.PostAsync<PreparacionDto>($"api/operaciones/preparaciones/{id}/cancelar", dto, token, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PreparacionDto>.Failure($"Error al cancelar preparación: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<PreparacionDto>>> ObtenerColaPreparacionesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<List<PreparacionDto>>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<List<PreparacionDto>>("api/operaciones/preparaciones/cola", token, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PreparacionDto>>.Failure($"Error al obtener cola de preparaciones: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<PreparacionDto>>> ObtenerPreparacionesPorEstadoAsync(string estado, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<List<PreparacionDto>>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            // El backend usa el endpoint /por-estado, no /estado/{estado}
            var response = await _apiService.GetAsync<List<PreparacionDto>>($"api/operaciones/preparaciones/por-estado?estado={estado}", token, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PreparacionDto>>.Failure($"Error al obtener preparaciones por estado: {ex.Message}");
        }
    }
} 