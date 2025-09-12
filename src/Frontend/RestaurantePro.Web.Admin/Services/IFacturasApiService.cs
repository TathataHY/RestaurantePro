using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IFacturasApiService
{
    Task<List<FacturaDto>> ObtenerFacturasAsync();
    Task<FacturaDto?> ObtenerFacturaPorIdAsync(Guid id);
    Task<FacturaDto?> CrearFacturaAsync(CrearFacturaRequest request);
    Task<FacturaDto?> ActualizarFacturaAsync(Guid id, ActualizarFacturaRequest request);
    Task<bool> EliminarFacturaAsync(Guid id);
    Task<byte[]?> GenerarPDFFacturaAsync(Guid id);
}
