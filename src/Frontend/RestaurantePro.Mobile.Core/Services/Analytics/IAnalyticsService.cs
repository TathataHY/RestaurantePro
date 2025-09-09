using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Analytics;

/// <summary>
/// Interfaz para el servicio de analytics y métricas operativas
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Obtener métricas operativas del día actual
    /// </summary>
    /// <returns>Métricas del día</returns>
    Task<ApiResponse<MetricasDiaDto>> ObtenerMetricasDiaAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtener métricas operativas por rango de fechas
    /// </summary>
    /// <param name="fechaDesde">Fecha desde</param>
    /// <param name="fechaHasta">Fecha hasta</param>
    /// <returns>Métricas del rango de fechas</returns>
    Task<ApiResponse<MetricasRangoDto>> ObtenerMetricasRangoAsync(DateTime fechaDesde, DateTime fechaHasta, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtener el top de productos más vendidos
    /// </summary>
    /// <param name="limite">Número máximo de productos</param>
    /// <param name="fechaDesde">Fecha desde (opcional)</param>
    /// <param name="fechaHasta">Fecha hasta (opcional)</param>
    /// <returns>Lista de productos más vendidos</returns>
    Task<ApiResponse<List<TopProductoDto>>> ObtenerTopProductosAsync(int limite, DateTime? fechaDesde = null, DateTime? fechaHasta = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtener métricas de ocupación de mesas
    /// </summary>
    /// <param name="fecha">Fecha para consultar</param>
    /// <returns>Métricas de ocupación de mesas</returns>
    Task<ApiResponse<OcupacionMesasDto>> ObtenerOcupacionMesasAsync(DateTime fecha, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtener métricas de tiempo promedio de preparación
    /// </summary>
    /// <param name="fechaDesde">Fecha desde (opcional)</param>
    /// <param name="fechaHasta">Fecha hasta (opcional)</param>
    /// <returns>Métricas de tiempo de preparación</returns>
    Task<ApiResponse<TiempoPreparacionDto>> ObtenerTiempoPreparacionAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtener resumen de ventas por hora
    /// </summary>
    /// <param name="fecha">Fecha para consultar</param>
    /// <returns>Lista de ventas por hora</returns>
    Task<ApiResponse<List<VentasHoraDto>>> ObtenerVentasPorHoraAsync(DateTime fecha, CancellationToken cancellationToken = default);
} 