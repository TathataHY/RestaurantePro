using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;

namespace RestaurantePro.Mobile.Core.Services.Analytics;

/// <summary>
/// Implementación del servicio de analytics y métricas operativas
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;

    public AnalyticsService(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
    }

    /// <summary>
    /// Obtener métricas operativas del día actual
    /// </summary>
    public async Task<ApiResponse<MetricasDiaDto>> ObtenerMetricasDiaAsync()
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<MetricasDiaDto>("api/analytics/metricas-dia", token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<MetricasDiaDto>.Failure($"Error al obtener métricas del día: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtener métricas operativas por rango de fechas
    /// </summary>
    public async Task<ApiResponse<MetricasRangoDto>> ObtenerMetricasRangoAsync(DateTime fechaDesde, DateTime fechaHasta)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var endpoint = $"api/analytics/metricas-rango?fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}";
            var response = await _apiService.GetAsync<MetricasRangoDto>(endpoint, token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<MetricasRangoDto>.Failure($"Error al obtener métricas del rango: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtener el top de productos más vendidos
    /// </summary>
    public async Task<ApiResponse<List<TopProductoDto>>> ObtenerTopProductosAsync(int limite, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var endpoint = $"api/analytics/top-productos?limite={limite}";
            
            if (fechaDesde.HasValue)
                endpoint += $"&fechaDesde={fechaDesde.Value:yyyy-MM-dd}";
            
            if (fechaHasta.HasValue)
                endpoint += $"&fechaHasta={fechaHasta.Value:yyyy-MM-dd}";
            
            var response = await _apiService.GetAsync<List<TopProductoDto>>(endpoint, token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TopProductoDto>>.Failure($"Error al obtener top productos: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtener métricas de ocupación de mesas
    /// </summary>
    public async Task<ApiResponse<OcupacionMesasDto>> ObtenerOcupacionMesasAsync(DateTime fecha)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var endpoint = $"api/analytics/ocupacion-mesas?fecha={fecha:yyyy-MM-dd}";
            var response = await _apiService.GetAsync<OcupacionMesasDto>(endpoint, token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<OcupacionMesasDto>.Failure($"Error al obtener ocupación de mesas: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtener métricas de tiempo promedio de preparación
    /// </summary>
    public async Task<ApiResponse<TiempoPreparacionDto>> ObtenerTiempoPreparacionAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var endpoint = "api/analytics/tiempo-preparacion";
            
            if (fechaDesde.HasValue)
                endpoint += $"?fechaDesde={fechaDesde.Value:yyyy-MM-dd}";
            
            if (fechaHasta.HasValue)
                endpoint += $"{(fechaDesde.HasValue ? "&" : "?")}fechaHasta={fechaHasta.Value:yyyy-MM-dd}";
            
            var response = await _apiService.GetAsync<TiempoPreparacionDto>(endpoint, token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<TiempoPreparacionDto>.Failure($"Error al obtener tiempo de preparación: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtener resumen de ventas por hora
    /// </summary>
    public async Task<ApiResponse<List<VentasHoraDto>>> ObtenerVentasPorHoraAsync(DateTime fecha)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var endpoint = $"api/analytics/ventas-hora?fecha={fecha:yyyy-MM-dd}";
            var response = await _apiService.GetAsync<List<VentasHoraDto>>(endpoint, token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<VentasHoraDto>>.Failure($"Error al obtener ventas por hora: {ex.Message}");
        }
    }
} 