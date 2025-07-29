using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;

namespace RestaurantePro.Mobile.Core.Services.Inventory;

public class IngredientesService : IIngredientesService
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;

    public IngredientesService(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
    }

    public async Task<ApiResponse<List<IngredienteSummaryDto>>> ObtenerIngredientesAsync(bool soloActivos = true)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<List<IngredienteSummaryDto>>($"api/inventario/ingredientes/lista?soloActivos={soloActivos}", token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<IngredienteSummaryDto>>.Failure($"Error al obtener ingredientes: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IngredienteDto>> ObtenerIngredienteAsync(Guid id)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<IngredienteDto>($"api/inventario/ingredientes/{id}", token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<IngredienteDto>.Failure($"Error al obtener ingrediente: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<IngredienteSummaryDto>>> BuscarIngredientesAsync(string terminoBusqueda, string? categoria = null, bool? soloDisponibles = null)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var queryParams = new List<string> { $"termino={terminoBusqueda}" };
            if (!string.IsNullOrEmpty(categoria))
                queryParams.Add($"categoria={categoria}");
            if (soloDisponibles.HasValue)
                queryParams.Add($"soloDisponibles={soloDisponibles.Value}");

            var queryString = string.Join("&", queryParams);
            var response = await _apiService.GetAsync<List<IngredienteSummaryDto>>($"api/inventario/ingredientes/buscar?{queryString}", token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<IngredienteSummaryDto>>.Failure($"Error al buscar ingredientes: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IngredienteDto>> CrearIngredienteAsync(IngredienteDto ingrediente)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.PostAsync<IngredienteDto>("api/inventario/ingredientes", ingrediente, token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<IngredienteDto>.Failure($"Error al crear ingrediente: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IngredienteDto>> ActualizarIngredienteAsync(Guid id, IngredienteDto ingrediente)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.PutAsync<IngredienteDto>($"api/inventario/ingredientes/{id}", ingrediente, token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<IngredienteDto>.Failure($"Error al actualizar ingrediente: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> EliminarIngredienteAsync(Guid id)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.DeleteAsync($"api/inventario/ingredientes/{id}", token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Error al eliminar ingrediente: {ex.Message}");
        }
    }

    public async Task<ApiResponse<EstadisticasIngredientesDto>> ObtenerEstadisticasAsync()
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<EstadisticasIngredientesDto>("api/inventario/ingredientes/estadisticas", token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<EstadisticasIngredientesDto>.Failure($"Error al obtener estadísticas: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<IngredienteSummaryDto>>> ObtenerIngredientesBajoStockAsync(int stockMinimo = 10)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<List<IngredienteSummaryDto>>($"api/inventario/ingredientes/bajo-stock?stockMinimo={stockMinimo}", token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<IngredienteSummaryDto>>.Failure($"Error al obtener ingredientes bajo stock: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IngredienteDto>> ActualizarStockAsync(Guid id, int cantidad, bool esEntrada = true)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var request = new { Cantidad = cantidad, EsEntrada = esEntrada };
            var response = await _apiService.PostAsync<IngredienteDto>($"api/inventario/ingredientes/{id}/actualizar-stock", request, token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<IngredienteDto>.Failure($"Error al actualizar stock: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<ReporteValoracionDto>>> GenerarReporteValoracionAsync(FiltroIngredientesDto filtro)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.PostAsync<List<ReporteValoracionDto>>("api/inventario/ingredientes/reporte-valoracion", filtro, token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ReporteValoracionDto>>.Failure($"Error al generar reporte de valoración: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<MovimientoInventarioDto>>> ObtenerMovimientosAsync(Guid ingredienteId)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<List<MovimientoInventarioDto>>($"api/inventario/ingredientes/{ingredienteId}/movimientos", token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<MovimientoInventarioDto>>.Failure($"Error al obtener movimientos: {ex.Message}");
        }
    }
} 