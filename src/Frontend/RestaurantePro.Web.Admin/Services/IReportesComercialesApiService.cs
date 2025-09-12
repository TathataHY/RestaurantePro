using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IReportesComercialesApiService
{
    Task<List<ReporteComercialDto>> ObtenerReportesComercialesAsync();
    Task<ReporteComercialDto?> ObtenerReporteComercialPorIdAsync(Guid id);
    Task<byte[]?> GenerarReporteComercialAsync(Guid reporteId, Dictionary<string, object> parametros);
}
