using RestaurantePro.Application.Common.Models.Dashboard;

namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Servicio para obtener métricas y datos del dashboard administrativo
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Obtiene el resumen completo del dashboard con filtros opcionales
    /// </summary>
    /// <param name="periodo">Período de filtro: hoy, ayer, semana, mes</param>
    /// <param name="turno">Turno de filtro: todos, mañana, tarde, noche, madrugada</param>
    /// <returns>Resumen completo del dashboard</returns>
    Task<DashboardResumenDto> ObtenerResumenAsync(string? periodo = "hoy", string? turno = "todos");

    /// <summary>
    /// Obtiene métricas básicas del dashboard
    /// </summary>
    /// <param name="periodo">Período de filtro: hoy, ayer, semana, mes</param>
    /// <param name="turno">Turno de filtro: todos, mañana, tarde, noche, madrugada</param>
    /// <returns>Métricas del dashboard</returns>
    Task<DashboardMetricasDto> ObtenerMetricasAsync(string? periodo = "hoy", string? turno = "todos");

    /// <summary>
    /// Obtiene productos más vendidos
    /// </summary>
    /// <param name="cantidad">Cantidad de productos a retornar</param>
    /// <param name="periodo">Período de filtro: hoy, ayer, semana, mes</param>
    /// <param name="turno">Turno de filtro: todos, mañana, tarde, noche, madrugada</param>
    /// <returns>Lista de productos más vendidos</returns>
    Task<List<DashboardProductoMasVendidoDto>> ObtenerProductosMasVendidosAsync(int cantidad = 5, string? periodo = "hoy", string? turno = "todos");

    /// <summary>
    /// Obtiene ventas por período
    /// </summary>
    /// <param name="dias">Número de días a consultar</param>
    /// <param name="periodo">Período de filtro: hoy, ayer, semana, mes</param>
    /// <param name="turno">Turno de filtro: todos, mañana, tarde, noche, madrugada</param>
    /// <returns>Lista de ventas por período</returns>
    Task<List<DashboardVentaPorPeriodoDto>> ObtenerVentasPorPeriodoAsync(int dias = 7, string? periodo = "hoy", string? turno = "todos");

    /// <summary>
    /// Obtiene estado actual de las mesas
    /// </summary>
    /// <returns>Estado de las mesas</returns>
    Task<DashboardEstadoMesasDto> ObtenerEstadoMesasAsync();

    /// <summary>
    /// Obtiene detalles de todas las mesas para el mapa interactivo
    /// </summary>
    /// <returns>Lista de detalles de mesas</returns>
    Task<List<MesaDetalleDto>> ObtenerMesasDetalleAsync();

    /// <summary>
    /// Obtiene comandas por estado
    /// </summary>
    /// <param name="periodo">Período de filtro: hoy, ayer, semana, mes</param>
    /// <param name="turno">Turno de filtro: todos, mañana, tarde, noche, madrugada</param>
    /// <returns>Comandas por estado</returns>
    Task<DashboardComandasPorEstadoDto> ObtenerComandasPorEstadoAsync(string? periodo = "hoy", string? turno = "todos");

    /// <summary>
    /// Obtiene ingresos por hora
    /// </summary>
    /// <param name="periodo">Período de filtro: hoy, ayer, semana, mes</param>
    /// <param name="turno">Turno de filtro: todos, mañana, tarde, noche, madrugada</param>
    /// <returns>Lista de ingresos por hora</returns>
    Task<List<DashboardIngresosPorHoraDto>> ObtenerIngresosPorHoraAsync(string? periodo = "hoy", string? turno = "todos");
}
