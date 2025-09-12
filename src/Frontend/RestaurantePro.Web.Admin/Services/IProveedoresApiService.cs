using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IProveedoresApiService
{
    Task<PaginatedList<ProveedorDto>?> GetProveedoresPaginados(int pageNumber = 1, int pageSize = 20, ProveedorFiltrosDto? filtros = null);
    Task<ProveedorDto?> ObtenerProveedorPorIdAsync(Guid id);
    Task<ProveedorDto?> CrearProveedorAsync(CrearProveedorRequest request);
    Task<ProveedorDto?> ActualizarProveedorAsync(Guid id, ActualizarProveedorRequest request);
    Task<bool> EliminarProveedorAsync(Guid id);
    Task<bool> CambiarEstadoProveedorAsync(Guid id, bool activo);
}
