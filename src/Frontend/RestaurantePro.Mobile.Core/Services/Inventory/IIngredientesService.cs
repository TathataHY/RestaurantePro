using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Inventory;

public interface IIngredientesService
{
    Task<ApiResponse<List<IngredienteSummaryDto>>> ObtenerIngredientesAsync(bool soloActivos = true);
    Task<ApiResponse<List<IngredienteSummaryDto>>> BuscarIngredientesAsync(string terminoBusqueda, string? categoria = null, bool? soloDisponibles = null);
    Task<ApiResponse<List<IngredienteSummaryDto>>> ObtenerIngredientesBajoStockAsync(int stockMinimo = 10);
    Task<ApiResponse<IngredienteDto>> ObtenerIngredienteAsync(Guid id);
    Task<ApiResponse<IngredienteDto>> CrearIngredienteAsync(IngredienteDto ingrediente);
    Task<ApiResponse<IngredienteDto>> ActualizarIngredienteAsync(Guid id, IngredienteDto ingrediente);
    Task<ApiResponse<bool>> EliminarIngredienteAsync(Guid id);
    Task<ApiResponse<EstadisticasIngredientesDto>> ObtenerEstadisticasAsync();
    Task<ApiResponse<List<ReporteValoracionDto>>> GenerarReporteValoracionAsync(FiltroIngredientesDto filtro);
    Task<ApiResponse<List<MovimientoInventarioDto>>> ObtenerMovimientosAsync(Guid ingredienteId);
} 