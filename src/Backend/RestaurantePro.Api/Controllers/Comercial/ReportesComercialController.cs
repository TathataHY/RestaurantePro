using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerReporteVentas;
using RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerReporteClientes;
using RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerReporteProductos;
using RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerReporteFidelizacion;
using RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerReportePromociones;
using RestaurantePro.Application.Comercial.Reportes.DTOs;

namespace RestaurantePro.Api.Controllers.Comercial;

[ApiController]
[Route("api/comercial/reportes")]
[Produces("application/json")]
public class ReportesComercialController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReportesComercialController> _logger;

    public ReportesComercialController(IMediator mediator, ILogger<ReportesComercialController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene reporte de ventas por período
    /// </summary>
    [HttpGet("ventas")]
    [ProducesResponseType(typeof(ApiResponse<ReporteVentasDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> ObtenerReporteVentas(
        [FromQuery] DateTime fechaInicio, 
        [FromQuery] DateTime fechaFin,
        [FromQuery] string? segmento = null,
        [FromQuery] string? tipoVenta = null,
        [FromQuery] bool incluirCanceladas = false)
    {
        try
    {
            _logger.LogInformation("📊 Solicitando reporte de ventas desde {FechaInicio} hasta {FechaFin}", 
                fechaInicio.ToShortDateString(), fechaFin.ToShortDateString());

            var query = new ObtenerReporteVentasQuery
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Segmento = segmento,
                TipoVenta = tipoVenta,
                IncluirCanceladas = incluirCanceladas
            };

            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<ReporteVentasDto>.SuccessResponse(result.Value));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error, "Error al obtener reporte de ventas"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo reporte de ventas");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno del servidor", "Error interno del servidor", 500));
        }
    }

    /// <summary>
    /// Obtiene reporte de clientes con análisis
    /// </summary>
    [HttpGet("clientes")]
    [ProducesResponseType(typeof(ApiResponse<ReporteClientesDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> ObtenerReporteClientes(
        [FromQuery] bool soloActivos = true,
        [FromQuery] string? segmento = null,
        [FromQuery] DateTime? fechaRegistroDesde = null,
        [FromQuery] DateTime? fechaRegistroHasta = null)
    {
        try
    {
            _logger.LogInformation("👥 Solicitando reporte de clientes");

            var query = new ObtenerReporteClientesQuery
            {
                SoloActivos = soloActivos,
                Segmento = segmento,
                FechaRegistroDesde = fechaRegistroDesde,
                FechaRegistroHasta = fechaRegistroHasta
            };

            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<ReporteClientesDto>.SuccessResponse(result.Value));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error, "Error al obtener reporte de clientes"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo reporte de clientes");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno del servidor", "Error interno del servidor", 500));
        }
    }

    /// <summary>
    /// Obtiene reporte de productos más vendidos
    /// </summary>
    [HttpGet("productos")]
    [ProducesResponseType(typeof(ApiResponse<ReporteProductosDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> ObtenerReporteProductos(
        [FromQuery] DateTime fechaInicio, 
        [FromQuery] DateTime fechaFin,
        [FromQuery] int topProductos = 10,
        [FromQuery] string? categoria = null)
    {
        try
    {
            _logger.LogInformation("🍽️ Solicitando reporte de productos desde {FechaInicio} hasta {FechaFin}", 
                fechaInicio.ToShortDateString(), fechaFin.ToShortDateString());

            var query = new ObtenerReporteProductosQuery
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TopProductos = topProductos,
                Categoria = categoria
            };

            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<ReporteProductosDto>.SuccessResponse(result.Value));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error, "Error al obtener reporte de productos"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo reporte de productos");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno del servidor", "Error interno del servidor", 500));
        }
    }

    /// <summary>
    /// Obtiene reporte de fidelización
    /// </summary>
    [HttpGet("fidelizacion")]
    [ProducesResponseType(typeof(ApiResponse<ReporteFidelizacionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> ObtenerReporteFidelizacion(
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        [FromQuery] bool soloActivas = true)
    {
        try
    {
            _logger.LogInformation("🎯 Solicitando reporte de fidelización desde {FechaInicio} hasta {FechaFin}", 
                fechaInicio.ToShortDateString(), fechaFin.ToShortDateString());

            var query = new ObtenerReporteFidelizacionQuery
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                SoloActivas = soloActivas
            };

            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<ReporteFidelizacionDto>.SuccessResponse(result.Value));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error, "Error al obtener reporte de fidelización"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo reporte de fidelización");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno del servidor", "Error interno del servidor", 500));
        }
    }

    /// <summary>
    /// Obtiene reporte de promociones y efectividad
    /// </summary>
    [HttpGet("promociones")]
    [ProducesResponseType(typeof(ApiResponse<ReportePromocionesDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> ObtenerReportePromociones(
        [FromQuery] DateTime fechaInicio, 
        [FromQuery] DateTime fechaFin,
        [FromQuery] bool soloActivas = true)
    {
        try
    {
            _logger.LogInformation("🎉 Solicitando reporte de promociones desde {FechaInicio} hasta {FechaFin}", 
                fechaInicio.ToShortDateString(), fechaFin.ToShortDateString());

            var query = new ObtenerReportePromocionesQuery
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                SoloActivas = soloActivas
            };

            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                return Ok(ApiResponse<ReportePromocionesDto>.SuccessResponse(result.Value));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error, "Error al obtener reporte de promociones"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo reporte de promociones");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno del servidor", "Error interno del servidor", 500));
        }
    }
} 