using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;

namespace RestaurantePro.Mobile.Core.Services.Inventory;

public class IngredientesService : IIngredientesService
{
    private readonly IApiService _apiService;

    public IngredientesService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<ApiResponse<List<IngredienteSummaryDto>>> ObtenerIngredientesAsync(bool soloActivos = true)
    {
        try
        {
            var response = await _apiService.GetAsync<List<IngredienteSummaryDto>>($"api/ingredientes?soloActivos={soloActivos}");
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
            var response = await _apiService.GetAsync<IngredienteDto>($"api/ingredientes/{id}");
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
            var queryParams = new List<string> { $"termino={terminoBusqueda}" };
            if (!string.IsNullOrEmpty(categoria))
                queryParams.Add($"categoria={categoria}");
            if (soloDisponibles.HasValue)
                queryParams.Add($"soloDisponibles={soloDisponibles.Value}");

            var queryString = string.Join("&", queryParams);
            var response = await _apiService.GetAsync<List<IngredienteSummaryDto>>($"api/ingredientes/buscar?{queryString}");
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
            var response = await _apiService.PostAsync<IngredienteDto>("api/ingredientes", ingrediente);
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
            var response = await _apiService.PutAsync<IngredienteDto>($"api/ingredientes/{id}", ingrediente);
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
            var response = await _apiService.DeleteAsync($"api/ingredientes/{id}");
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
            var response = await _apiService.GetAsync<EstadisticasIngredientesDto>("api/ingredientes/estadisticas");
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
            var response = await _apiService.GetAsync<List<IngredienteSummaryDto>>($"api/ingredientes/bajo-stock?stockMinimo={stockMinimo}");
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
            var request = new { Cantidad = cantidad, EsEntrada = esEntrada };
            var response = await _apiService.PostAsync<IngredienteDto>($"api/ingredientes/{id}/actualizar-stock", request);
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
            var response = await _apiService.PostAsync<List<ReporteValoracionDto>>("api/ingredientes/reporte-valoracion", filtro);
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
            var response = await _apiService.GetAsync<List<MovimientoInventarioDto>>($"api/ingredientes/{ingredienteId}/movimientos");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<MovimientoInventarioDto>>.Failure($"Error al obtener movimientos: {ex.Message}");
        }
    }
} 