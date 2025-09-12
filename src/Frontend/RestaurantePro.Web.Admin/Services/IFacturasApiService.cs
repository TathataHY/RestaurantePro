using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IFacturasApiService
{
    // Métodos básicos
    Task<List<FacturaDto>> ObtenerFacturasAsync();
    Task<PaginatedList<FacturaDto>?> ObtenerFacturasAsync(FacturaFiltrosDto filtros);
    Task<FacturaDto?> ObtenerFacturaPorIdAsync(Guid id);
    Task<FacturaDto?> CrearFacturaAsync(CrearFacturaRequest request);
    Task<FacturaDto?> ActualizarFacturaAsync(Guid id, ActualizarFacturaRequest request);
    Task<bool> EliminarFacturaAsync(Guid id);
    
    // Métodos específicos de la página
    Task<FacturaEstadisticasDto?> ObtenerEstadisticasAsync();
    Task<ApiResponse<bool>?> CancelarFacturaAsync(CancelarFacturaRequest request);
    Task<ApiResponse<FacturaPagoDto>?> RegistrarPagoAsync(RegistrarPagoRequest request);
    Task<ApiResponse<byte[]>?> ReimprimirFacturaAsync(ReimprimirFacturaRequest request);
    Task<ApiResponse<byte[]>?> ExportarFacturasAsync(FacturaFiltrosDto filtros, string formato = "Excel");
    Task<byte[]?> GenerarPDFFacturaAsync(Guid id);
}
