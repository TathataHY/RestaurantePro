using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IProductosApiService
{
    Task<PaginatedList<ProductoDto>?> ObtenerProductosAsync(int pageNumber = 1, int pageSize = 20, string? filtro = null);
    Task<ProductoDto?> ObtenerProductoPorIdAsync(Guid id);
    Task<ProductoDto?> CrearProductoAsync(CrearProductoRequest request);
    Task<ProductoDto?> ActualizarProductoAsync(Guid id, ActualizarProductoRequest request);
    Task<bool> EliminarProductoAsync(Guid id);
    Task<bool> CambiarEstadoProductoAsync(Guid id, bool activo);
}
