using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Api.Common;
using RestaurantePro.Domain.Core.Analytics.DTOs;
using RestaurantePro.Domain.Core.Analytics.Interfaces;
using RestaurantePro.Domain.Core.Analytics.Services;
using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Api.Controllers.Core;

/// <summary>
/// Controlador para gestionar métricas y analytics operativos
/// </summary>
[ApiController]
[Route("api/analytics")]
[Produces("application/json")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene métricas operativas del día actual
    /// </summary>
    [HttpGet("metricas-dia")]
    [ProducesResponseType(typeof(ApiResponse<MetricasDiaDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> ObtenerMetricasDia()
    {
        try
        {
            var metricas = await _analyticsService.ObtenerMetricasDiaAsync();
            return Ok(ApiResponse<MetricasDiaDto>.SuccessResponse(metricas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener métricas del día");
            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { "Error al obtener métricas del día" },
                "Error interno del servidor",
                500));
        }
    }

    /// <summary>
    /// Obtiene métricas operativas por rango de fechas
    /// </summary>
    [HttpGet("metricas-rango")]
    [ProducesResponseType(typeof(ApiResponse<MetricasRangoDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> ObtenerMetricasRango([FromQuery] DateTime fechaDesde, [FromQuery] DateTime fechaHasta)
    {
        try
        {
            if (fechaDesde > fechaHasta)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { "La fecha desde debe ser menor o igual a la fecha hasta" },
                    "Parámetros inválidos",
                    400));
            }

            var metricas = await _analyticsService.ObtenerMetricasRangoAsync(fechaDesde, fechaHasta);
            return Ok(ApiResponse<MetricasRangoDto>.SuccessResponse(metricas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener métricas por rango de fechas");
            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { "Error al obtener métricas por rango de fechas" },
                "Error interno del servidor",
                500));
        }
    }

    /// <summary>
    /// Obtiene el top de productos más vendidos
    /// </summary>
    [HttpGet("top-productos")]
    [ProducesResponseType(typeof(ApiResponse<List<TopProductoDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> ObtenerTopProductos([FromQuery] int? limite = 10, [FromQuery] DateTime? fechaDesde = null, [FromQuery] DateTime? fechaHasta = null)
    {
        try
        {
            var topProductos = await _analyticsService.ObtenerTopProductosAsync(limite ?? 10, fechaDesde, fechaHasta);
            return Ok(ApiResponse<List<TopProductoDto>>.SuccessResponse(topProductos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener top de productos");
            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { "Error al obtener top de productos" },
                "Error interno del servidor",
                500));
        }
    }

    /// <summary>
    /// Obtiene métricas de ocupación de mesas
    /// </summary>
    [HttpGet("ocupacion-mesas")]
    [ProducesResponseType(typeof(ApiResponse<OcupacionMesasDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> ObtenerOcupacionMesas([FromQuery] DateTime? fecha = null)
    {
        try
        {
            var ocupacion = await _analyticsService.ObtenerOcupacionMesasAsync(fecha ?? DateTime.Today);
            return Ok(ApiResponse<OcupacionMesasDto>.SuccessResponse(ocupacion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ocupación de mesas");
            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { "Error al obtener ocupación de mesas" },
                "Error interno del servidor",
                500));
        }
    }

    /// <summary>
    /// Obtiene métricas de tiempo promedio de preparación
    /// </summary>
    [HttpGet("tiempo-preparacion")]
    [ProducesResponseType(typeof(ApiResponse<TiempoPreparacionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> ObtenerTiempoPreparacion([FromQuery] DateTime? fechaDesde = null, [FromQuery] DateTime? fechaHasta = null)
    {
        try
        {
            var tiempoPreparacion = await _analyticsService.ObtenerTiempoPreparacionAsync(fechaDesde, fechaHasta);
            return Ok(ApiResponse<TiempoPreparacionDto>.SuccessResponse(tiempoPreparacion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tiempo de preparación");
            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { "Error al obtener tiempo de preparación" },
                "Error interno del servidor",
                500));
        }
    }

    /// <summary>
    /// Obtiene resumen de ventas por hora
    /// </summary>
    [HttpGet("ventas-hora")]
    [ProducesResponseType(typeof(ApiResponse<List<VentasHoraDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> ObtenerVentasPorHora([FromQuery] DateTime fecha)
    {
        try
        {
            var ventasHora = await _analyticsService.ObtenerVentasPorHoraAsync(fecha);
            return Ok(ApiResponse<List<VentasHoraDto>>.SuccessResponse(ventasHora));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por hora");
            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<string> { "Error al obtener ventas por hora" },
                "Error interno del servidor",
                500));
        }
    }
} 