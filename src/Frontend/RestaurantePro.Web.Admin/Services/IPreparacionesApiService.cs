using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IPreparacionesApiService
{
    Task<List<PreparacionDto>> ObtenerPreparacionesAsync();
    Task<PaginatedList<PreparacionDto>?> ObtenerPreparacionesPaginadasAsync(int pageNumber, int pageSize, PreparacionFiltrosDto? filtros = null);
    Task<PreparacionDto?> ObtenerPreparacionAsync(int id);
    Task<PreparacionDto?> ObtenerPreparacionPorIdAsync(int id);
    Task<PreparacionEstadisticasDto?> ObtenerEstadisticasAsync();
    Task<PreparacionDto?> CrearPreparacionAsync(CrearPreparacionRequest request);
    Task<PreparacionDto?> ActualizarPreparacionAsync(int id, ActualizarPreparacionRequest request);
    Task<bool> EliminarPreparacionAsync(int id);
    Task<bool> IniciarPreparacionAsync(int id);
    Task<bool> CompletarPreparacionAsync(int id);
    Task<bool> CambiarEstadoPreparacionAsync(int id, EstadoPreparacion estado);
    Task<byte[]?> ExportarExcelAsync(PreparacionFiltrosDto? filtros = null);
    Task<List<dynamic>?> ObtenerCocinerosAsync();
}
