using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Inventory;

public interface IIngredientesService
{
    Task<ApiResponse<List<IngredienteSummaryDto>>> ObtenerIngredientesAsync(bool soloActivos = true, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<IngredienteSummaryDto>>> BuscarIngredientesAsync(string terminoBusqueda, string? categoria = null, bool? soloDisponibles = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<IngredienteSummaryDto>>> ObtenerIngredientesBajoStockAsync(int stockMinimo = 10, CancellationToken cancellationToken = default);
    Task<ApiResponse<IngredienteDto>> ObtenerIngredienteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IngredienteDto>> CrearIngredienteAsync(IngredienteDto ingrediente, CancellationToken cancellationToken = default);
    Task<ApiResponse<IngredienteDto>> ActualizarIngredienteAsync(Guid id, IngredienteDto ingrediente, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> EliminarIngredienteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<EstadisticasIngredientesDto>> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<List<ReporteValoracionDto>>> GenerarReporteValoracionAsync(FiltroIngredientesDto filtro, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<MovimientoInventarioDto>>> ObtenerMovimientosAsync(Guid ingredienteId, CancellationToken cancellationToken = default);
} 