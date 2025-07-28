using RestaurantePro.Domain.Core.Analytics.DTOs;

namespace RestaurantePro.Domain.Core.Analytics.Interfaces;

/// <summary>
/// Interfaz para el servicio de analytics y métricas operativas
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Obtiene métricas operativas del día actual
    /// </summary>
    Task<MetricasDiaDto> ObtenerMetricasDiaAsync();

    /// <summary>
    /// Obtiene métricas operativas por rango de fechas
    /// </summary>
    Task<MetricasRangoDto> ObtenerMetricasRangoAsync(DateTime fechaDesde, DateTime fechaHasta);

    /// <summary>
    /// Obtiene el top de productos más vendidos
    /// </summary>
    Task<List<TopProductoDto>> ObtenerTopProductosAsync(int limite, DateTime? fechaDesde = null, DateTime? fechaHasta = null);

    /// <summary>
    /// Obtiene métricas de ocupación de mesas
    /// </summary>
    Task<OcupacionMesasDto> ObtenerOcupacionMesasAsync(DateTime fecha);

    /// <summary>
    /// Obtiene métricas de tiempo promedio de preparación
    /// </summary>
    Task<TiempoPreparacionDto> ObtenerTiempoPreparacionAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null);

    /// <summary>
    /// Obtiene resumen de ventas por hora
    /// </summary>
    Task<List<VentasHoraDto>> ObtenerVentasPorHoraAsync(DateTime fecha);
} 