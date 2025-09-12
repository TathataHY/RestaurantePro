using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IReportesApiService
{
    Task<List<ReporteDto>> ObtenerReportesAsync();
    Task<ReporteDto?> ObtenerReportePorIdAsync(Guid id);
    Task<byte[]?> GenerarReporteAsync(Guid reporteId, Dictionary<string, object> parametros);
    Task<List<TipoReporte>> ObtenerTiposReporteAsync();
}
