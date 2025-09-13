using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface ICategoriasApiService
{
    Task<List<CategoriaProductoDto>> ObtenerCategoriasAsync();
    Task<List<CategoriaProductoDto>?> ObtenerAsync(bool soloActivas = false, bool soloInactivas = false, bool ocultarVacias = false);
    Task<CategoriaProductoDto?> ObtenerPorIdAsync(Guid id);
    Task<List<CategoriaProductoDto>?> BuscarAsync(string filtro);
    Task<ApiResponse<CategoriaProductoDto>?> CrearAsync(CreateCategoriaRequest request);
    Task<ApiResponse<CategoriaProductoDto>?> ActualizarAsync(Guid id, UpdateCategoriaRequest request);
    Task<bool> EliminarAsync(Guid id);
    Task<bool> ValidarNombreUnicoAsync(string nombre, Guid? idExcluir = null);
    Task<bool> CambiarEstadoCategoriaAsync(Guid id, bool activa);
}
