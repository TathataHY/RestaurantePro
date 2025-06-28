using Microsoft.AspNetCore.Authorization;
using MediatR;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerReporteGeneral;
using RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerAlertasInventario;
using RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerAnalisisInventario;
using RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerRecomendacionesCompra;
using RestaurantePro.Application.Inventario.Reportes.Commands.RealizarInventarioFisico;
using RestaurantePro.Application.Inventario.Reportes.Queries.ExportarReporte;
using RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerValorTotalInventario;
using RestaurantePro.Application.Inventario.Reportes.Commands.ExportarInventario;
using RestaurantePro.Application.Inventario.Reportes.DTOs;
using AnalisisInventarioDto = RestaurantePro.Application.Inventario.Reportes.DTOs.AnalisisInventarioDto;
using ReporteExportadoDto = RestaurantePro.Application.Inventario.Reportes.DTOs.ReporteExportadoDto;

namespace RestaurantePro.Api.Controllers.Inventario;

/// <summary>
/// Controlador para la generación de reportes y análisis de inventario
/// </summary>
[ApiController]
[Route("api/inventario/reportes")]
[Produces("application/json")]
[Authorize]
public class ReportesInventarioController : ControllerBase
{
    private readonly ILogger<ReportesInventarioController> _logger;
    private readonly IMediator _mediator;

    public ReportesInventarioController(
        ILogger<ReportesInventarioController> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene el reporte general de inventario
    /// </summary>
    [HttpGet("general")]
    [ProducesResponseType(typeof(ApiResponse<ReporteGeneralInventarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ReporteGeneralInventarioDto>>> GetInventarioGeneral(
        [FromQuery] string? categoria = null,
        [FromQuery] bool incluirDetalles = true)
    {
        _logger.LogInformation("📊 GET /api/inventario/reportes/general - Categoría: {Categoria}, IncluirDetalles: {IncluirDetalles}", 
            categoria, incluirDetalles);

        var query = new ObtenerReporteGeneralQuery
        {
            Categoria = categoria,
            IncluirDetalles = incluirDetalles
        };

        var result = await _mediator.Send(query);
        
        if (result.Succeeded)
        {
            return Ok(ApiResponse<ReporteGeneralInventarioDto>.SuccessResponse(result.Value!, "Reporte general obtenido exitosamente"));
        }

        return BadRequest(ApiResponse<object>.ErrorResponse("Error al obtener reporte general", result.Error ?? "No se pudo generar el reporte"));
    }

    /// <summary>
    /// Obtiene alertas de inventario (stock bajo/crítico)
    /// </summary>
    [HttpGet("alertas")]
    [ProducesResponseType(typeof(ApiResponse<List<AlertaInventarioDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<AlertaInventarioDto>>>> GetAlertas(
        [FromQuery] bool soloCriticas = false,
        [FromQuery] string? categoria = null)
    {
        _logger.LogInformation("🚨 GET /api/inventario/reportes/alertas - SoloCriticas: {SoloCriticas}, Categoria: {Categoria}", soloCriticas, categoria);
        var query = new ObtenerAlertasInventarioQuery
        {
            SoloCriticas = soloCriticas,
            Categoria = categoria
        };
        var result = await _mediator.Send(query);
        if (result.Succeeded)
            return Ok(ApiResponse<List<AlertaInventarioDto>>.SuccessResponse(result.Value!, "Alertas de inventario obtenidas exitosamente"));
        return BadRequest(ApiResponse<object>.ErrorResponse(result.Error ?? "Error al obtener alertas", "No se pudieron obtener las alertas"));
    }

    /// <summary>
    /// Obtiene el análisis de inventario
    /// </summary>
    [HttpGet("analisis")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> GetAnalisisInventario(
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] List<string>? categorias = null,
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] string nivelDetalle = "Completo")
    {
        _logger.LogInformation("📈 GET /api/inventario/reportes/analisis - FechaDesde: {FechaDesde}, FechaHasta: {FechaHasta}", 
            fechaDesde, fechaHasta);

        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            Categorias = categorias,
            UsuarioId = usuarioId ?? Guid.NewGuid(),
            NivelDetalle = nivelDetalle
        };

        var result = await _mediator.Send(query);
        
        if (result.Succeeded)
        {
            return Ok(ApiResponse<object>.SuccessResponse(result.Value!, "Análisis de inventario obtenido exitosamente"));
        }

        return BadRequest(ApiResponse<object>.ErrorResponse("Error al obtener análisis de inventario", result.Error ?? "No se pudo generar el análisis"));
    }

    /// <summary>
    /// Obtiene recomendaciones de compra basadas en el inventario
    /// </summary>
    [HttpGet("recomendaciones-compra")]
    [ProducesResponseType(typeof(ApiResponse<List<RecomendacionCompraDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<RecomendacionCompraDto>>>> GetRecomendacionesCompra(
        [FromQuery] int diasProyeccion = 30)
    {
        _logger.LogInformation("💡 GET /api/inventario/reportes/recomendaciones-compra - DiasProyeccion: {DiasProyeccion}", diasProyeccion);
        var query = new ObtenerRecomendacionesCompraQuery
        {
            DiasProyeccion = diasProyeccion
        };
        var result = await _mediator.Send(query);
        if (result.Succeeded)
            return Ok(ApiResponse<List<RecomendacionCompraDto>>.SuccessResponse(result.Value!, "Recomendaciones de compra obtenidas exitosamente"));
        return BadRequest(ApiResponse<object>.ErrorResponse(result.Error ?? "Error al obtener recomendaciones", "No se pudieron obtener las recomendaciones"));
    }

    /// <summary>
    /// Realiza un inventario físico
    /// </summary>
    [HttpPost("inventario-fisico")]
    [ProducesResponseType(typeof(ApiResponse<ResultadoInventarioFisicoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ResultadoInventarioFisicoDto>>> RealizarInventarioFisico([FromBody] RealizarInventarioFisicoCommand command)
    {
        _logger.LogInformation("📝 POST /api/inventario/reportes/inventario-fisico");
        var result = await _mediator.Send(command);
        if (result.Succeeded)
            return Created(string.Empty, ApiResponse<ResultadoInventarioFisicoDto>.SuccessResponse(result.Value!, "Inventario físico realizado exitosamente"));
        return BadRequest(ApiResponse<object>.ErrorResponse(result.Error ?? "Error al realizar inventario físico", "No se pudo realizar el inventario físico"));
    }

    /// <summary>
    /// Exporta un reporte de inventario
    /// </summary>
    [HttpGet("exportar")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ExportarInventario(
        [FromQuery] string formato = "PDF")
    {
        _logger.LogInformation("📤 GET /api/inventario/reportes/exportar - Formato: {Formato}", formato);

        var query = new ExportarReporteQuery
        {
            Formato = formato
        };

        var result = await _mediator.Send(query);
        
        if (result.Succeeded)
        {
            return Ok(ApiResponse<object>.SuccessResponse(result.Value!, "Reporte exportado exitosamente"));
        }

        return BadRequest(ApiResponse<object>.ErrorResponse("Error al exportar reporte", result.Error ?? "No se pudo exportar el reporte"));
    }

    /// <summary>
    /// Obtiene el valor total del inventario
    /// </summary>
    [HttpGet("valor-total")]
    [ProducesResponseType(typeof(ApiResponse<ValorTotalInventarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ValorTotalInventarioDto>>> GetValorTotalInventario()
    {
        _logger.LogInformation("💰 GET /api/inventario/reportes/valor-total");

        var query = new ObtenerValorTotalInventarioQuery();

        var result = await _mediator.Send(query);
        
        if (result.Succeeded)
        {
            return Ok(ApiResponse<ValorTotalInventarioDto>.SuccessResponse(result.Value!, "Valor total del inventario obtenido exitosamente"));
        }

        return BadRequest(ApiResponse<object>.ErrorResponse("Error al obtener valor total del inventario", result.Error ?? "No se pudo obtener el valor total"));
    }
} 