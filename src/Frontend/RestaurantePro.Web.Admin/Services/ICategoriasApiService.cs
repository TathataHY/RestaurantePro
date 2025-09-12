using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface ICategoriasApiService
{
    Task<List<CategoriaProductoDto>> ObtenerCategoriasAsync();
    Task<CategoriaProductoDto?> ObtenerCategoriaPorIdAsync(Guid id);
    Task<CategoriaProductoDto?> CrearCategoriaAsync(CreateCategoriaRequest request);
    Task<CategoriaProductoDto?> ActualizarCategoriaAsync(Guid id, UpdateCategoriaRequest request);
    Task<bool> EliminarCategoriaAsync(Guid id);
    Task<bool> CambiarEstadoCategoriaAsync(Guid id, bool activa);
}
