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
    Task<PaginatedList<ProductoDto>> ObtenerProductosPaginadosAsync(
        int pageNumber, 
        int pageSize, 
        string? filtro, 
        Guid? categoriaId, 
        bool soloActivos, 
        decimal? precioMinimo = null,
        decimal? precioMaximo = null,
        DateTime? fechaCreacionDesde = null,
        DateTime? fechaCreacionHasta = null,
        int? popularidadMinima = null,
        int? popularidadMaxima = null,
        string orderBy = "Nombre", 
        string orderDirection = "asc");
    Task<ProductoDto?> ObtenerPorIdAsync(Guid id);
    Task<ProductoDto?> CrearAsync(CreateProductoRequest dto);
    Task<ProductoDto?> ActualizarAsync(UpdateProductoRequest dto);
    Task<bool> EliminarAsync(Guid id);
    Task<EstadisticasProductosDto?> ObtenerEstadisticasAsync();
    Task<string?> SubirImagenAsync(Guid productoId, Stream archivo, string nombreArchivo, string contentType);
}
