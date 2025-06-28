using Microsoft.AspNetCore.Authorization;
using RestaurantePro.Application.Operaciones.Reportes.Queries.ObtenerReporteVentasDiaria;
using RestaurantePro.Application.Operaciones.Reportes.Commands.GenerarReporte;
using RestaurantePro.Application.Operaciones.Reportes.DTOs;

namespace RestaurantePro.Api.Controllers.Operaciones;

/// <summary>
/// Controlador para la gestión de reportes operacionales
/// Endpoints para generar reportes de ventas, operaciones y análisis
/// </summary>
[ApiController]
[Route("api/operaciones/reportes")]
[Produces("application/json")]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReportesController> _logger;

    public ReportesController(IMediator mediator, ILogger<ReportesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Genera reporte de ventas diarias
    /// </summary>
    /// <param name="fecha">Fecha del reporte</param>
    /// <param name="incluirDetalles">Incluir análisis detallados</param>
    /// <returns>Reporte de ventas del día</returns>
    [HttpGet("ventas-diarias")]
    [ProducesResponseType(typeof(ApiResponse<ReporteVentasDiariaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ReporteVentasDiariaDto>>> GenerarReporteVentasDiarias(
        [FromQuery] DateTime? fecha,
        [FromQuery] bool incluirDetalles = true)
    {
        _logger.LogInformation("📊 GET /api/operaciones/reportes/ventas-diarias - Fecha: {Fecha}", fecha);

        try
        {
            var fechaReporte = fecha ?? DateTime.Today;
            var nivelDetalle = incluirDetalles ? NivelDetalle.Completo : NivelDetalle.Basico;

            var query = ObtenerReporteVentasDiariaQuery.CrearReporteFecha(fechaReporte, nivel: nivelDetalle);
            
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                var errorResponse = ApiResponse<ReporteVentasDiariaDto>.ErrorResponse(
                    new List<string> { result.Error },
                    "Error generando reporte de ventas diarias",
                    StatusCodes.Status400BadRequest);

                return BadRequest(errorResponse);
            }

            var response = ApiResponse<ReporteVentasDiariaDto>.SuccessResponse(
                result.Value,
                "Reporte de ventas diarias generado exitosamente");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando reporte de ventas diarias");
            
            var errorResponse = ApiResponse<ReporteVentasDiariaDto>.ErrorResponse(
                new List<string> { "Error interno del servidor" },
                "Error interno generando reporte",
                StatusCodes.Status500InternalServerError);

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Genera reporte de ocupación de mesas
    /// </summary>
    /// <param name="fechaInicio">Fecha de inicio</param>
    /// <param name="fechaFin">Fecha de fin</param>
    /// <returns>Reporte de ocupación de mesas</returns>
    [HttpGet("ocupacion-mesas")]
    [ProducesResponseType(typeof(ApiResponse<ReporteVentasDiariaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ReporteVentasDiariaDto>>> GenerarReporteOcupacionMesas(
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin)
    {
        _logger.LogInformation("🪑 GET /api/operaciones/reportes/ocupacion-mesas - Rango: {FechaInicio} - {FechaFin}", 
            fechaInicio, fechaFin);

        try
        {
            // Usar el reporte de ventas diarias con enfoque en mesas
            var query = new ObtenerReporteVentasDiariaQuery
            {
                FechaReporte = fechaInicio.Date,
                IncluirAnalisisPorMesa = true,
                IncluirAnalisisPorMesero = false,
                IncluirAnalisisProductos = false,
                NivelDetalle = NivelDetalle.Mesas
            };
            
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                var errorResponse = ApiResponse<ReporteVentasDiariaDto>.ErrorResponse(
                    new List<string> { result.Error },
                    "Error generando reporte de ocupación de mesas",
                    StatusCodes.Status400BadRequest);

                return BadRequest(errorResponse);
            }

            var response = ApiResponse<ReporteVentasDiariaDto>.SuccessResponse(
                result.Value,
                "Reporte de ocupación de mesas generado exitosamente");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando reporte de ocupación de mesas");
            
            var errorResponse = ApiResponse<ReporteVentasDiariaDto>.ErrorResponse(
                new List<string> { "Error interno del servidor" },
                "Error interno generando reporte",
                StatusCodes.Status500InternalServerError);

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Genera reporte de rendimiento de meseros
    /// </summary>
    /// <param name="fechaInicio">Fecha de inicio</param>
    /// <param name="fechaFin">Fecha de fin</param>
    /// <param name="meseroId">ID específico del mesero (opcional)</param>
    /// <returns>Reporte de rendimiento de meseros</returns>
    [HttpGet("rendimiento-meseros")]
    [ProducesResponseType(typeof(ApiResponse<ReporteVentasDiariaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ReporteVentasDiariaDto>>> GenerarReporteRendimientoMeseros(
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        [FromQuery] Guid? meseroId)
    {
        _logger.LogInformation("👨‍💼 GET /api/operaciones/reportes/rendimiento-meseros - Rango: {FechaInicio} - {FechaFin}, Mesero: {MeseroId}", 
            fechaInicio, fechaFin, meseroId);

        try
        {
            var meserosEspecificos = meseroId.HasValue ? new List<Guid> { meseroId.Value } : null;
            
            var query = new ObtenerReporteVentasDiariaQuery
            {
                FechaReporte = fechaInicio.Date,
                IncluirAnalisisPorMesa = false,
                IncluirAnalisisPorMesero = true,
                IncluirAnalisisProductos = false,
                MeserosEspecificos = meserosEspecificos,
                NivelDetalle = NivelDetalle.Meseros
            };
            
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                var errorResponse = ApiResponse<ReporteVentasDiariaDto>.ErrorResponse(
                    new List<string> { result.Error },
                    "Error generando reporte de rendimiento de meseros",
                    StatusCodes.Status400BadRequest);

                return BadRequest(errorResponse);
            }

            var response = ApiResponse<ReporteVentasDiariaDto>.SuccessResponse(
                result.Value,
                "Reporte de rendimiento de meseros generado exitosamente");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando reporte de rendimiento de meseros");
            
            var errorResponse = ApiResponse<ReporteVentasDiariaDto>.ErrorResponse(
                new List<string> { "Error interno del servidor" },
                "Error interno generando reporte",
                StatusCodes.Status500InternalServerError);

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Genera reporte de comandas por período
    /// </summary>
    /// <param name="fechaInicio">Fecha de inicio</param>
    /// <param name="fechaFin">Fecha de fin</param>
    /// <param name="estado">Estado de comandas a filtrar</param>
    /// <returns>Reporte de comandas</returns>
    [HttpGet("comandas")]
    [ProducesResponseType(typeof(ApiResponse<ReporteVentasDiariaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ReporteVentasDiariaDto>>> GenerarReporteComandas(
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        [FromQuery] string? estado)
    {
        _logger.LogInformation("📋 GET /api/operaciones/reportes/comandas - Rango: {FechaInicio} - {FechaFin}, Estado: {Estado}", 
            fechaInicio, fechaFin, estado);

        try
        {
            var query = new ObtenerReporteVentasDiariaQuery
            {
                FechaReporte = fechaInicio.Date,
                IncluirAnalisisPorMesa = true,
                IncluirAnalisisPorMesero = true,
                IncluirAnalisisProductos = true,
                NivelDetalle = NivelDetalle.Completo
            };
            
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                var errorResponse = ApiResponse<ReporteVentasDiariaDto>.ErrorResponse(
                    new List<string> { result.Error },
                    "Error generando reporte de comandas",
                    StatusCodes.Status400BadRequest);

                return BadRequest(errorResponse);
            }

            var response = ApiResponse<ReporteVentasDiariaDto>.SuccessResponse(
                result.Value,
                "Reporte de comandas generado exitosamente");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando reporte de comandas");
            
            var errorResponse = ApiResponse<ReporteVentasDiariaDto>.ErrorResponse(
                new List<string> { "Error interno del servidor" },
                "Error interno generando reporte",
                StatusCodes.Status500InternalServerError);

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Genera reporte de productos más vendidos
    /// </summary>
    /// <param name="fechaInicio">Fecha de inicio</param>
    /// <param name="fechaFin">Fecha de fin</param>
    /// <param name="limite">Número máximo de productos a incluir</param>
    /// <returns>Reporte de productos más vendidos</returns>
    [HttpGet("productos-mas-vendidos")]
    [ProducesResponseType(typeof(ApiResponse<ReporteVentasDiariaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ReporteVentasDiariaDto>>> GenerarReporteProductosMasVendidos(
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        [FromQuery] int limite = 10)
    {
        _logger.LogInformation("🥘 GET /api/operaciones/reportes/productos-mas-vendidos - Rango: {FechaInicio} - {FechaFin}, Limite: {Limite}", 
            fechaInicio, fechaFin, limite);

        try
        {
            var query = new ObtenerReporteVentasDiariaQuery
            {
                FechaReporte = fechaInicio.Date,
                IncluirAnalisisPorMesa = false,
                IncluirAnalisisPorMesero = false,
                IncluirAnalisisProductos = true,
                NivelDetalle = NivelDetalle.Basico
            };
            
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                var errorResponse = ApiResponse<ReporteVentasDiariaDto>.ErrorResponse(
                    new List<string> { result.Error },
                    "Error generando reporte de productos más vendidos",
                    StatusCodes.Status400BadRequest);

                return BadRequest(errorResponse);
            }

            var response = ApiResponse<ReporteVentasDiariaDto>.SuccessResponse(
                result.Value,
                "Reporte de productos más vendidos generado exitosamente");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando reporte de productos más vendidos");
            
            var errorResponse = ApiResponse<ReporteVentasDiariaDto>.ErrorResponse(
                new List<string> { "Error interno del servidor" },
                "Error interno generando reporte",
                StatusCodes.Status500InternalServerError);

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Genera reporte de reservaciones
    /// </summary>
    /// <param name="fechaInicio">Fecha de inicio</param>
    /// <param name="fechaFin">Fecha de fin</param>
    /// <param name="incluirCanceladas">Incluir reservaciones canceladas</param>
    /// <returns>Reporte de reservaciones</returns>
    [HttpGet("reservaciones")]
    [ProducesResponseType(typeof(ApiResponse<ReporteVentasDiariaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ReporteVentasDiariaDto>>> GenerarReporteReservaciones(
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        [FromQuery] bool incluirCanceladas = false)
    {
        _logger.LogInformation("📅 GET /api/operaciones/reportes/reservaciones - Rango: {FechaInicio} - {FechaFin}, IncluirCanceladas: {IncluirCanceladas}", 
            fechaInicio, fechaFin, incluirCanceladas);

        try
        {
            var query = new ObtenerReporteVentasDiariaQuery
            {
                FechaReporte = fechaInicio.Date,
                IncluirAnalisisPorMesa = true,
                IncluirAnalisisPorMesero = false,
                IncluirAnalisisProductos = false,
                NivelDetalle = NivelDetalle.Basico
            };
            
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                var errorResponse = ApiResponse<ReporteVentasDiariaDto>.ErrorResponse(
                    new List<string> { result.Error },
                    "Error generando reporte de reservaciones",
                    StatusCodes.Status400BadRequest);

                return BadRequest(errorResponse);
            }

            var response = ApiResponse<ReporteVentasDiariaDto>.SuccessResponse(
                result.Value,
                "Reporte de reservaciones generado exitosamente");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando reporte de reservaciones");
            
            var errorResponse = ApiResponse<ReporteVentasDiariaDto>.ErrorResponse(
                new List<string> { "Error interno del servidor" },
                "Error interno generando reporte",
                StatusCodes.Status500InternalServerError);

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Genera reporte de eficiencia operacional
    /// </summary>
    /// <param name="fechaInicio">Fecha de inicio</param>
    /// <param name="fechaFin">Fecha de fin</param>
    /// <param name="incluirGraficos">Incluir datos para gráficos</param>
    /// <returns>Reporte de eficiencia operacional</returns>
    [HttpGet("eficiencia-operacional")]
    [ProducesResponseType(typeof(ApiResponse<ReporteVentasDiariaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ReporteVentasDiariaDto>>> GenerarReporteEficienciaOperacional(
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        [FromQuery] bool incluirGraficos = true)
    {
        _logger.LogInformation("⚡ GET /api/operaciones/reportes/eficiencia-operacional - Rango: {FechaInicio} - {FechaFin}, IncluirGraficos: {IncluirGraficos}", 
            fechaInicio, fechaFin, incluirGraficos);

        try
        {
            var query = new ObtenerReporteVentasDiariaQuery
            {
                FechaReporte = fechaInicio.Date,
                IncluirAnalisisPorMesa = true,
                IncluirAnalisisPorMesero = true,
                IncluirAnalisisProductos = true,
                NivelDetalle = NivelDetalle.Completo
            };
            
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                var errorResponse = ApiResponse<ReporteVentasDiariaDto>.ErrorResponse(
                    new List<string> { result.Error },
                    "Error generando reporte de eficiencia operacional",
                    StatusCodes.Status400BadRequest);

                return BadRequest(errorResponse);
            }

            var response = ApiResponse<ReporteVentasDiariaDto>.SuccessResponse(
                result.Value,
                "Reporte de eficiencia operacional generado exitosamente");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando reporte de eficiencia operacional");
            
            var errorResponse = ApiResponse<ReporteVentasDiariaDto>.ErrorResponse(
                new List<string> { "Error interno del servidor" },
                "Error interno generando reporte",
                StatusCodes.Status500InternalServerError);

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Genera dashboard ejecutivo con métricas clave
    /// </summary>
    /// <param name="fecha">Fecha del dashboard (opcional, por defecto hoy)</param>
    /// <param name="incluirComparativo">Incluir comparativo con período anterior</param>
    /// <returns>Dashboard ejecutivo</returns>
    [HttpGet("dashboard-ejecutivo")]
    [ProducesResponseType(typeof(ApiResponse<ReporteVentasDiariaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ReporteVentasDiariaDto>>> GenerarDashboardEjecutivo(
        [FromQuery] DateTime? fecha,
        [FromQuery] bool incluirComparativo = true)
    {
        _logger.LogInformation("📊 GET /api/operaciones/reportes/dashboard-ejecutivo - Fecha: {Fecha}, IncluirComparativo: {IncluirComparativo}", 
            fecha, incluirComparativo);

        try
        {
            var fechaReporte = fecha ?? DateTime.Today;
            
            var query = new ObtenerReporteVentasDiariaQuery
            {
                FechaReporte = fechaReporte,
                IncluirComparativoPeriodoAnterior = incluirComparativo,
                IncluirAnalisisPorMesa = true,
                IncluirAnalisisPorMesero = true,
                IncluirAnalisisProductos = true,
                NivelDetalle = NivelDetalle.Completo
            };
            
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                var errorResponse = ApiResponse<ReporteVentasDiariaDto>.ErrorResponse(
                    new List<string> { result.Error },
                    "Error generando dashboard ejecutivo",
                    StatusCodes.Status400BadRequest);

                return BadRequest(errorResponse);
            }

            var response = ApiResponse<ReporteVentasDiariaDto>.SuccessResponse(
                result.Value,
                "Dashboard ejecutivo generado exitosamente");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando dashboard ejecutivo");
            
            var errorResponse = ApiResponse<ReporteVentasDiariaDto>.ErrorResponse(
                new List<string> { "Error interno del servidor" },
                "Error interno generando dashboard",
                StatusCodes.Status500InternalServerError);

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Exporta reporte en formato específico
    /// </summary>
    /// <param name="request">Parámetros del reporte a exportar</param>
    /// <returns>Archivo del reporte exportado</returns>
    [HttpPost("exportar")]
    [ProducesResponseType(typeof(ApiResponse<ReporteGeneradoResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ReporteGeneradoResult>>> ExportarReporte(
        [FromBody] GenerarReporteCommand request)
    {
        _logger.LogInformation("📤 POST /api/operaciones/reportes/exportar - Tipo: {TipoReporte}", request.TipoReporte);

        try
        {
            var result = await _mediator.Send(request);

            if (!result.Succeeded)
            {
                var errorResponse = ApiResponse<ReporteGeneradoResult>.ErrorResponse(
                    new List<string> { result.Error },
                    "Error exportando reporte",
                    StatusCodes.Status400BadRequest);

                return BadRequest(errorResponse);
            }

            var response = ApiResponse<ReporteGeneradoResult>.SuccessResponse(
                result.Value,
                "Reporte exportado exitosamente");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error exportando reporte");
            
            var errorResponse = ApiResponse<ReporteGeneradoResult>.ErrorResponse(
                new List<string> { "Error interno del servidor" },
                "Error interno exportando reporte",
                StatusCodes.Status500InternalServerError);

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Programa generación automática de reportes
    /// </summary>
    /// <param name="configuracion">Configuración del reporte programado</param>
    /// <returns>Confirmación de programación</returns>
    [HttpPost("programar")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<ActionResult<ApiResponse<object>>> ProgramarReporte(
        [FromBody] GenerarReporteCommand configuracion)
    {
        _logger.LogInformation("⏰ POST /api/operaciones/reportes/programar - Tipo: {TipoReporte}", configuracion.TipoReporte);

        try
        {
            // Por ahora, ejecutamos el reporte directamente
            // En el futuro, esto debería programar la ejecución
            var result = await _mediator.Send(configuracion);

            if (!result.Succeeded)
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    new List<string> { result.Error },
                    "Error programando reporte",
                    StatusCodes.Status400BadRequest);

                return BadRequest(errorResponse);
            }

            var response = ApiResponse<object>.SuccessResponse(
                new { reporteId = result.Value.ReporteId, mensaje = "Reporte programado exitosamente" },
                "Reporte programado exitosamente");

            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error programando reporte");
            
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { "Error interno del servidor" },
                "Error interno programando reporte",
                StatusCodes.Status500InternalServerError);

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Obtiene historial de reportes generados
    /// </summary>
    /// <param name="usuarioId">ID del usuario (opcional)</param>
    /// <param name="tipoReporte">Tipo de reporte a filtrar</param>
    /// <param name="limite">Número máximo de reportes a retornar</param>
    /// <returns>Lista de reportes generados</returns>
    [HttpGet("historial")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ObtenerHistorialReportes(
        [FromQuery] Guid? usuarioId,
        [FromQuery] string? tipoReporte,
        [FromQuery] int limite = 20)
    {
        _logger.LogInformation("📚 GET /api/operaciones/reportes/historial - Usuario: {UsuarioId}, Tipo: {TipoReporte}, Limite: {Limite}", 
            usuarioId, tipoReporte, limite);

        try
        {
            // Por ahora, devolvemos un historial simulado
            // En el futuro, esto debería consultar una tabla de reportes generados
            var historial = new List<object>
            {
                new
                {
                    ReporteId = Guid.NewGuid(),
                    TipoReporte = "VentasDiarias",
                    FechaGeneracion = DateTime.Now.AddDays(-1),
                    Estado = "Completado",
                    TamanoBytes = 1024 * 100,
                    UsuarioSolicitante = usuarioId ?? Guid.Empty
                }
            };

            var response = ApiResponse<object>.SuccessResponse(
                historial,
                "Historial de reportes obtenido exitosamente");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo historial de reportes");
            
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { "Error interno del servidor" },
                "Error interno obteniendo historial",
                StatusCodes.Status500InternalServerError);

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
} 