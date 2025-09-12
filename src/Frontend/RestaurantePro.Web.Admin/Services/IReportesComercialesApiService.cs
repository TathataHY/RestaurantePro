using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IReportesComercialesApiService
{
    Task<List<ReporteComercialDto>> ObtenerReportesComercialesAsync();
    Task<ReporteComercialDto?> ObtenerReporteComercialPorIdAsync(Guid id);
    Task<byte[]?> GenerarReporteComercialAsync(Guid reporteId, Dictionary<string, object> parametros);
    Task<object?> ObtenerAnalisisClientesAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<object?> ObtenerSegmentacionClientesAsync();
    Task<object?> ObtenerAnalisisProductosAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<object?> ObtenerRentabilidadProductosAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<object?> ObtenerAnalisisPromocionesAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<object?> ObtenerTendenciasVentasAsync(DateTime fechaInicio, DateTime fechaFin);
}
