using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IProveedoresApiService
{
    Task<PaginatedList<ProveedorDto>?> GetProveedoresPaginados(int pageNumber = 1, int pageSize = 20, ProveedorFiltrosDto? filtros = null);
    Task<ProveedorDto?> ObtenerProveedorPorIdAsync(Guid id);
    Task<ProveedorDto?> CreateProveedor(CrearProveedorRequest request);
    Task<ProveedorDto?> UpdateProveedor(Guid id, CrearProveedorRequest request);
    Task<bool> DeleteProveedor(Guid id);
    Task<bool> CambiarEstadoProveedorAsync(Guid id, bool activo);
    Task<ProveedorEstadisticasDto?> GetProveedorEstadisticas();
    Task<List<ContactoProveedorDto>?> GetContactosProveedor(Guid proveedorId);
    Task<ContactoProveedorDto?> CreateContactoProveedor(CrearContactoProveedorRequest request);
    Task<ContactoProveedorDto?> UpdateContactoProveedor(Guid proveedorId, Guid contactoId, CrearContactoProveedorRequest request);
    Task<bool> DeleteContactoProveedor(Guid proveedorId, Guid contactoId);
    Task<byte[]?> ExportarProveedoresExcel(ExportarProveedoresRequest request);
}
