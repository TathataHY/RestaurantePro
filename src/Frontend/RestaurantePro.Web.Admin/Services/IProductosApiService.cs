using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IProductosApiService
{
    // Métodos básicos
    Task<PaginatedList<ProductoDto>?> ObtenerProductosAsync(int pageNumber = 1, int pageSize = 20, string? filtro = null);
    Task<ProductoDto?> ObtenerProductoPorIdAsync(Guid id);
    Task<ProductoDto?> CrearProductoAsync(CrearProductoRequest request);
    Task<ProductoDto?> ActualizarProductoAsync(Guid id, ActualizarProductoRequest request);
    Task<bool> EliminarProductoAsync(Guid id);
    Task<bool> CambiarEstadoProductoAsync(Guid id, bool activo);
    
    // Métodos específicos de la página
    Task<List<CategoriaProductoDto>> ObtenerCategoriasAsync();
    Task<PaginatedList<ProductoDto>> ObtenerProductosPaginadosAsync(int pageNumber, int pageSize, string? filtro, Guid? categoriaId, bool soloActivos, string orderBy, string orderDirection);
    Task<ProductoDto?> ObtenerPorIdAsync(Guid id);
    Task<ProductoDto?> CrearAsync(CreateProductoRequest dto);
    Task<ProductoDto?> ActualizarAsync(UpdateProductoRequest dto);
    Task<bool> EliminarAsync(Guid id);
}
