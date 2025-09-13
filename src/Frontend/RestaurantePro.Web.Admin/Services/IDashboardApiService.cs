using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IDashboardApiService
{
    Task<DashboardResumenDto?> ObtenerDashboardAsync(string? periodo = "hoy", string? turno = "todos");
    Task<DashboardMetricasDto?> ObtenerMetricasAsync();
    Task<List<ProductoMasVendidoDto>> ObtenerProductosMasVendidosAsync();
    Task<List<VentaPorPeriodoDto>> ObtenerVentasUltimos7DiasAsync();
    Task<EstadoMesasDto?> ObtenerEstadoMesasAsync();
}
