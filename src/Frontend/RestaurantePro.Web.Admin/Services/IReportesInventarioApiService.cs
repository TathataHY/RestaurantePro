using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IReportesInventarioApiService
{
    Task<List<ReporteInventarioDto>> ObtenerReportesInventarioAsync();
    Task<ReporteInventarioDto?> ObtenerReporteInventarioPorIdAsync(Guid id);
    Task<byte[]?> GenerarReporteInventarioAsync(Guid reporteId, Dictionary<string, object> parametros);
}
