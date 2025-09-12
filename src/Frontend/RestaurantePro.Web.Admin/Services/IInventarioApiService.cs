using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IInventarioApiService
{
    // Ingredientes
    Task<PaginatedList<IngredienteDto>?> ObtenerIngredientesPaginadosAsync(int pageNumber = 1, int pageSize = 20, InventarioFiltrosDto? filtros = null);
    Task<IngredienteDto?> ObtenerIngredienteAsync(Guid id);
    Task<IngredienteDto?> CrearIngredienteAsync(IngredienteDto ingrediente);
    Task<IngredienteDto?> ActualizarIngredienteAsync(Guid id, IngredienteDto ingrediente);
    Task<bool> EliminarIngredienteAsync(Guid id);
    
    // Estadísticas y Alertas
    Task<InventarioEstadisticasDto?> ObtenerEstadisticasAsync();
    Task<List<AlertaInventarioDto>?> ObtenerAlertasAsync();
    
    // Exportación
    Task<byte[]?> ExportarInventarioAsync(InventarioFiltrosDto? filtros = null);
}
