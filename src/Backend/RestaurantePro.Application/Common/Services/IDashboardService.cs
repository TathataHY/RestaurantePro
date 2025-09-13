using RestaurantePro.Application.Common.Models.Dashboard;

namespace RestaurantePro.Application.Common.Services;

/// <summary>
/// Servicio para obtener métricas y datos del dashboard administrativo
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Obtiene el resumen completo del dashboard
    /// </summary>
    Task<DashboardResumenDto> ObtenerResumenAsync();

    /// <summary>
    /// Obtiene las métricas principales del dashboard
    /// </summary>
    Task<DashboardMetricasDto> ObtenerMetricasAsync();

    /// <summary>
    /// Obtiene los productos más vendidos
    /// </summary>
    Task<List<DashboardProductoMasVendidoDto>> ObtenerProductosMasVendidosAsync(int cantidad = 5);

    /// <summary>
    /// Obtiene las ventas por período
    /// </summary>
    Task<List<DashboardVentaPorPeriodoDto>> ObtenerVentasPorPeriodoAsync(int dias = 7);

    /// <summary>
    /// Obtiene el estado actual de las mesas
    /// </summary>
    Task<DashboardEstadoMesasDto> ObtenerEstadoMesasAsync();

    /// <summary>
    /// Obtiene el estado de las comandas
    /// </summary>
    Task<DashboardComandasPorEstadoDto> ObtenerComandasPorEstadoAsync();

    /// <summary>
    /// Obtiene los ingresos por hora del día actual
    /// </summary>
    Task<List<DashboardIngresosPorHoraDto>> ObtenerIngresosPorHoraAsync();
}
