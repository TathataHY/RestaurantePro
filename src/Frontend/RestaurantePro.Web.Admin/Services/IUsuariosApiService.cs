using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IUsuariosApiService
{
    Task<List<UsuarioDto>> ObtenerUsuariosAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? filtro = null,
        bool soloActivos = true,
        string orderBy = "NombreCompleto",
        string orderDirection = "asc");

    Task<UsuarioDto?> ObtenerPorIdAsync(Guid id);
    Task<UsuarioDto?> CrearAsync(CrearUsuarioRequest request);
    Task<UsuarioDto?> ActualizarAsync(Guid id, ActualizarUsuarioRequest request);
    Task<bool> EliminarAsync(Guid id);
    Task<bool> CambiarEstadoAsync(Guid id, bool activo);
    Task<List<string>> ObtenerRolesDisponiblesAsync();
}
