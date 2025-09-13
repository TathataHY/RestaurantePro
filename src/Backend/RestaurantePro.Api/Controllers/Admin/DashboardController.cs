using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Common.Models.Dashboard;
using RestaurantePro.Application.Common.Services;

namespace RestaurantePro.Api.Controllers.Admin;

/// <summary>
/// Controlador para métricas y datos del dashboard administrativo
/// Contexto: Admin
/// </summary>
[ApiController]
[Route("api/admin/dashboard")]
[Produces("application/json")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ILogger<DashboardController> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDashboardService _dashboardService;

    public DashboardController(
        ILogger<DashboardController> logger,
        ICurrentUserService currentUserService,
        IDashboardService dashboardService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
    }

    /// <summary>
    /// Obtiene el resumen completo del dashboard
    /// </summary>
    [HttpGet("resumen")]
    [ProducesResponseType(typeof(ApiResponse<DashboardResumenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DashboardResumenDto>>> ObtenerResumen()
    {
        _logger.LogInformation("📊 GET /api/admin/dashboard/resumen - Usuario: {UserId}", _currentUserService.UserId);

        try
        {
            var resumen = await _dashboardService.ObtenerResumenAsync();
            
            return Ok(ApiResponse<DashboardResumenDto>.SuccessResponse(resumen));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener resumen del dashboard");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno del servidor", "Error interno del servidor"));
        }
    }

    /// <summary>
    /// Obtiene métricas básicas del dashboard
    /// </summary>
    [HttpGet("metricas")]
    [ProducesResponseType(typeof(ApiResponse<DashboardMetricasDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DashboardMetricasDto>>> ObtenerMetricas()
    {
        _logger.LogInformation("📈 GET /api/admin/dashboard/metricas - Usuario: {UserId}", _currentUserService.UserId);

        try
        {
            var metricas = await _dashboardService.ObtenerMetricasAsync();

            return Ok(ApiResponse<DashboardMetricasDto>.SuccessResponse(metricas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener métricas del dashboard");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno del servidor", "Error interno del servidor"));
        }
    }

    /// <summary>
    /// Obtiene productos más vendidos
    /// </summary>
    [HttpGet("productos-mas-vendidos")]
    [ProducesResponseType(typeof(ApiResponse<List<DashboardProductoMasVendidoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<DashboardProductoMasVendidoDto>>>> ObtenerProductosMasVendidos([FromQuery] int cantidad = 5)
    {
        _logger.LogInformation("🍽️ GET /api/admin/dashboard/productos-mas-vendidos - Cantidad: {Cantidad}", cantidad);

        try
        {
            var productos = await _dashboardService.ObtenerProductosMasVendidosAsync(cantidad);

            return Ok(ApiResponse<List<DashboardProductoMasVendidoDto>>.SuccessResponse(productos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener productos más vendidos");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno del servidor", "Error interno del servidor"));
        }
    }

    /// <summary>
    /// Obtiene ventas de los últimos 7 días
    /// </summary>
    [HttpGet("ventas-ultimos-7-dias")]
    [ProducesResponseType(typeof(ApiResponse<List<DashboardVentaPorPeriodoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<DashboardVentaPorPeriodoDto>>>> ObtenerVentasUltimos7Dias()
    {
        _logger.LogInformation("📅 GET /api/admin/dashboard/ventas-ultimos-7-dias");

        try
        {
            var ventas = await _dashboardService.ObtenerVentasPorPeriodoAsync(7);

            return Ok(ApiResponse<List<DashboardVentaPorPeriodoDto>>.SuccessResponse(ventas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener ventas de los últimos 7 días");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno del servidor", "Error interno del servidor"));
        }
    }

    /// <summary>
    /// Obtiene estado actual de las mesas
    /// </summary>
    [HttpGet("estado-mesas")]
    [ProducesResponseType(typeof(ApiResponse<DashboardEstadoMesasDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DashboardEstadoMesasDto>>> ObtenerEstadoMesas()
    {
        _logger.LogInformation("🪑 GET /api/admin/dashboard/estado-mesas");

        try
        {
            var estadoMesas = await _dashboardService.ObtenerEstadoMesasAsync();

            return Ok(ApiResponse<DashboardEstadoMesasDto>.SuccessResponse(estadoMesas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener estado de mesas");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno del servidor", "Error interno del servidor"));
        }
    }

    /// <summary>
    /// Obtiene comandas por estado
    /// </summary>
    [HttpGet("comandas-por-estado")]
    [ProducesResponseType(typeof(ApiResponse<DashboardComandasPorEstadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DashboardComandasPorEstadoDto>>> ObtenerComandasPorEstado()
    {
        _logger.LogInformation("📋 GET /api/admin/dashboard/comandas-por-estado");

        try
        {
            var comandas = await _dashboardService.ObtenerComandasPorEstadoAsync();

            return Ok(ApiResponse<DashboardComandasPorEstadoDto>.SuccessResponse(comandas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener comandas por estado");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno del servidor", "Error interno del servidor"));
        }
    }

    /// <summary>
    /// Obtiene ingresos por hora del día actual
    /// </summary>
    [HttpGet("ingresos-por-hora")]
    [ProducesResponseType(typeof(ApiResponse<List<DashboardIngresosPorHoraDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<DashboardIngresosPorHoraDto>>>> ObtenerIngresosPorHora()
    {
        _logger.LogInformation("⏰ GET /api/admin/dashboard/ingresos-por-hora");

        try
        {
            var ingresos = await _dashboardService.ObtenerIngresosPorHoraAsync();

            return Ok(ApiResponse<List<DashboardIngresosPorHoraDto>>.SuccessResponse(ingresos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener ingresos por hora");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno del servidor", "Error interno del servidor"));
        }
    }

}
