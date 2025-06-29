using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.RegistrarMovimiento;
using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerMovimientosIngrediente;
using RestaurantePro.Application.Inventario.MovimientosInventario.Commands.ActualizarMovimiento;
using RestaurantePro.Application.Inventario.MovimientosInventario.Commands.EliminarMovimiento;
using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;
using RestaurantePro.Application.Inventario.MovimientosInventario.Queries.GenerarReporteMovimientos;
using RestaurantePro.Application.Inventario.MovimientosInventario.Queries.ObtenerMovimientoPorId;
using RestaurantePro.Application.Inventario.MovimientosInventario.Queries.ObtenerMovimientosInventario;
using RestaurantePro.Application.Inventario.MovimientosInventario.Queries.ObtenerMovimientosPorTipo;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;
using System.Security.Claims;

namespace RestaurantePro.Api.Controllers.Inventario;

[ApiController]
[Route("api/inventario/movimientos")]
[Produces("application/json")]
[Authorize]
public class MovimientosInventarioController : ControllerBase
{
    private readonly ILogger<MovimientosInventarioController> _logger;
    private readonly IMediator _mediator;

    public MovimientosInventarioController(
        ILogger<MovimientosInventarioController> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene todos los movimientos de inventario con filtros y paginación
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<MovimientoInventarioDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedList<MovimientoInventarioDto>>>> GetMovimientos(
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] TipoMovimientoInventario? tipoMovimiento = null,
        [FromQuery] Guid? ingredienteId = null,
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] string? filtroMotivo = null,
        [FromQuery] string ordenarPor = "Fecha",
        [FromQuery] string direccionOrdenamiento = "desc")
    {
        _logger.LogInformation("📦 GET /api/inventario/movimientos");
        
        try
        {
            var query = new ObtenerMovimientosInventarioQuery
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TipoMovimiento = tipoMovimiento,
                IngredienteId = ingredienteId,
                UsuarioId = usuarioId,
                Buscar = filtroMotivo,
                OrdenarPor = ordenarPor,
                DireccionOrdenamiento = direccionOrdenamiento
            };

            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                var response = ApiResponse<PaginatedList<MovimientoInventarioDto>>.SuccessResponse(
                    result.Value, "Movimientos obtenidos exitosamente");
                return Ok(response);
            }
            else
            {
                var errorResponse = ApiResponse<PaginatedList<MovimientoInventarioDto>>.ErrorResponse(
                    result.Error ?? "Error al obtener movimientos", 
                    "No se pudieron obtener los movimientos");
                return BadRequest(errorResponse);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener movimientos de inventario");
            var errorResponse = ApiResponse<PaginatedList<MovimientoInventarioDto>>.ErrorResponse(
                "Error interno del servidor", "Error al obtener movimientos");
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Obtiene un movimiento específico por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MovimientoInventarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MovimientoInventarioDto>>> GetMovimiento(Guid id)
    {
        _logger.LogInformation("🔍 GET /api/inventario/movimientos/{Id}", id);
        
        try
        {
            var query = new ObtenerMovimientoPorIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                var response = ApiResponse<MovimientoInventarioDto>.SuccessResponse(
                    result.Value, "Movimiento obtenido exitosamente");
                return Ok(response);
            }
            else
            {
                var errorResponse = ApiResponse<MovimientoInventarioDto>.ErrorResponse(
                    result.Error ?? "Movimiento no encontrado", 
                    "No se pudo obtener el movimiento");
                return NotFound(errorResponse);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener movimiento {Id}", id);
            var errorResponse = ApiResponse<MovimientoInventarioDto>.ErrorResponse(
                "Error interno del servidor", "Error al obtener movimiento");
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Registra un nuevo movimiento de inventario
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> RegistrarMovimiento(
        [FromBody] RegistrarMovimientoRequest request)
    {
        _logger.LogInformation("➕ POST /api/inventario/movimientos");
        _logger.LogInformation("📋 Request recibido: IngredienteId={IngredienteId}, TipoMovimiento={TipoMovimiento}, Cantidad={Cantidad}, UsuarioId={UsuarioId}", 
            request.IngredienteId, request.TipoMovimiento, request.Cantidad, request.UsuarioId);
        
        try
        {
            // Si no se envía UsuarioId, obtenerlo del usuario autenticado
            var usuarioId = request.UsuarioId;
            if (usuarioId == null || usuarioId == Guid.Empty)
            {
                _logger.LogInformation("🔍 UsuarioId no proporcionado, buscando en claims de autenticación");
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "userid" || c.Type == "UserId" || c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedId))
                {
                    usuarioId = parsedId;
                    _logger.LogInformation("✅ UsuarioId obtenido de claims: {UsuarioId}", usuarioId);
                }
                else
                {
                    _logger.LogWarning("❌ No se pudo obtener UsuarioId de claims de autenticación");
                    var errorResponse = ApiResponse<bool>.ErrorResponse(
                        "No se pudo determinar el usuario que realiza la operación.",
                        "Usuario no autenticado o sin claim de ID.");
                    return BadRequest(errorResponse);
                }
            }
            else
            {
                _logger.LogInformation("✅ UsuarioId proporcionado en request: {UsuarioId}", usuarioId);
            }

            _logger.LogInformation("🔍 Validando tipo de movimiento: {TipoMovimiento}", request.TipoMovimiento);
            if (request.TipoMovimiento != TipoMovimientoInventario.Ingreso && request.TipoMovimiento != TipoMovimientoInventario.Egreso)
            {
                _logger.LogWarning("❌ Tipo de movimiento no válido: {TipoMovimiento}", request.TipoMovimiento);
                var errorResponse = ApiResponse<bool>.ErrorResponse(
                    "Tipo de movimiento no válido.",
                    "El tipo de movimiento debe ser Ingreso o Egreso.");
                return BadRequest(errorResponse);
            }

            _logger.LogInformation("📝 Creando comando RegistrarMovimientoCommand");
            var command = new RegistrarMovimientoCommand
            {
                IngredienteId = request.IngredienteId,
                Cantidad = request.Cantidad,
                TipoMovimiento = request.TipoMovimiento,
                Motivo = request.Motivo,
                Observaciones = request.Observaciones,
                UsuarioId = usuarioId,
                Fecha = request.Fecha
            };

            _logger.LogInformation("🚀 Enviando comando al mediator");
            var result = await _mediator.Send(command);

            if (result.Succeeded)
            {
                _logger.LogInformation("✅ Movimiento registrado exitosamente");
                var response = ApiResponse<Guid>.SuccessResponse(
                    result.Value, "Movimiento registrado exitosamente");
                return CreatedAtAction(nameof(GetMovimiento), new { id = result.Value }, response);
            }
            else
            {
                _logger.LogWarning("❌ Error al registrar movimiento: {Error}", result.Error);
                var errorResponse = ApiResponse<bool>.ErrorResponse(
                    result.Error ?? "Error al registrar movimiento", 
                    "No se pudo registrar el movimiento");
                return BadRequest(errorResponse);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al registrar movimiento de inventario");
            var errorResponse = ApiResponse<bool>.ErrorResponse(
                "Error interno del servidor", "Error al registrar movimiento");
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Actualiza un movimiento de inventario existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MovimientoInventarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MovimientoInventarioDto>>> ActualizarMovimiento(
        Guid id, [FromBody] ActualizarMovimientoRequest request)
    {
        _logger.LogInformation("✏️ PUT /api/inventario/movimientos/{Id}", id);
        
        try
        {
            // Obtener UsuarioId del usuario autenticado
            var usuarioId = GetCurrentUserId();
            if (usuarioId == Guid.Empty)
            {
                var errorResponse = ApiResponse<MovimientoInventarioDto>.ErrorResponse(
                    "No se pudo determinar el usuario que realiza la operación.",
                    "Usuario no autenticado.");
                return BadRequest(errorResponse);
            }

            var command = new ActualizarMovimientoCommand
            {
                Id = id,
                Cantidad = request.Cantidad,
                CostoUnitario = request.CostoUnitario,
                Motivo = request.Motivo,
                Observaciones = request.Observaciones,
                UsuarioId = usuarioId
            };

            _logger.LogInformation("🚀 Enviando comando al mediator");
            var result = await _mediator.Send(command);

            _logger.LogInformation("🔍 Resultado del handler: Succeeded={Succeeded}, Error={Error}", result.Succeeded, result.Error);

            if (result.Succeeded)
            {
                _logger.LogInformation("✅ Handler devolvió Success, creando respuesta OK");
                var response = ApiResponse<MovimientoInventarioDto>.SuccessResponse(
                    result.Value, "Movimiento actualizado exitosamente");
                _logger.LogInformation("📤 Devolviendo respuesta OK con datos: Id={Id}", result.Value?.Id);
                return Ok(response);
            }
            else
            {
                _logger.LogWarning("❌ Handler devolvió Failure: {Error}", result.Error);
                var errorResponse = ApiResponse<MovimientoInventarioDto>.ErrorResponse(
                    result.Error ?? "Error al actualizar movimiento", 
                    "No se pudo actualizar el movimiento");
                return NotFound(errorResponse);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al actualizar movimiento {Id}", id);
            var errorResponse = ApiResponse<MovimientoInventarioDto>.ErrorResponse(
                "Error interno del servidor", "Error al actualizar movimiento");
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Elimina un movimiento de inventario
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> EliminarMovimiento(Guid id)
    {
        _logger.LogInformation("🗑️ DELETE /api/inventario/movimientos/{Id}", id);
        
        try
        {
            // Obtener UsuarioId del usuario autenticado
            var usuarioId = GetCurrentUserId();
            if (usuarioId == Guid.Empty)
            {
                var errorResponse = ApiResponse<bool>.ErrorResponse(
                    "No se pudo determinar el usuario que realiza la operación.",
                    "Usuario no autenticado.");
                return BadRequest(errorResponse);
            }

            var command = new EliminarMovimientoCommand
            {
                Id = id,
                UsuarioId = usuarioId,
                MotivoEliminacion = "Eliminación por usuario"
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("🔍 Resultado del handler: Succeeded={Succeeded}, Error={Error}", result.Succeeded, result.Error);

            if (result.Succeeded)
            {
                _logger.LogInformation("✅ Handler devolvió Success, creando respuesta OK");
                var response = ApiResponse<bool>.SuccessResponse(
                    result.Value, "Movimiento eliminado exitosamente");
                _logger.LogInformation("📤 Devolviendo respuesta OK con resultado: {Result}", result.Value);
                return Ok(response);
            }
            else
            {
                _logger.LogWarning("❌ Handler devolvió Failure: {Error}", result.Error);
                var errorResponse = ApiResponse<bool>.ErrorResponse(
                    result.Error ?? "Error al eliminar movimiento", 
                    "No se pudo eliminar el movimiento");
                return NotFound(errorResponse);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al eliminar movimiento {Id}", id);
            var errorResponse = ApiResponse<bool>.ErrorResponse(
                "Error interno del servidor", "Error al eliminar movimiento");
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Obtiene movimientos de un ingrediente específico
    /// </summary>
    [HttpGet("ingrediente/{ingredienteId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<MovimientoInventarioDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<MovimientoInventarioDto>>>> GetMovimientosPorIngrediente(
        Guid ingredienteId,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] int? limite = null)
    {
        _logger.LogInformation("🥕 GET /api/inventario/movimientos/ingrediente/{IngredienteId}", ingredienteId);
        
        try
        {
            var query = new ObtenerMovimientosIngredienteQuery
            {
                IngredienteId = ingredienteId,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                Limite = limite
            };

            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                var response = ApiResponse<List<MovimientoInventarioDto>>.SuccessResponse(
                    result.Value, "Movimientos del ingrediente obtenidos exitosamente");
                return Ok(response);
            }
            else
            {
                var errorResponse = ApiResponse<List<MovimientoInventarioDto>>.ErrorResponse(
                    result.Error ?? "Error al obtener movimientos del ingrediente", 
                    "No se pudieron obtener los movimientos");
                return BadRequest(errorResponse);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener movimientos del ingrediente {IngredienteId}", ingredienteId);
            var errorResponse = ApiResponse<List<MovimientoInventarioDto>>.ErrorResponse(
                "Error interno del servidor", "Error al obtener movimientos del ingrediente");
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Obtiene movimientos por tipo específico
    /// </summary>
    [HttpGet("tipo/{tipo}")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<MovimientoInventarioDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedList<MovimientoInventarioDto>>>> GetMovimientosPorTipo(
        string tipo,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null,
        [FromQuery] Guid? ingredienteId = null)
    {
        _logger.LogInformation("🏷️ GET /api/inventario/movimientos/tipo/{Tipo}", tipo);
        
        try
        {
            if (!Enum.TryParse<TipoMovimientoInventario>(tipo, true, out var tipoMovimiento))
            {
                var errorResponse = ApiResponse<PaginatedList<MovimientoInventarioDto>>.ErrorResponse(
                    "Tipo de movimiento no válido",
                    "El tipo debe ser 'Ingreso' o 'Egreso'");
                return BadRequest(errorResponse);
            }

            var query = new ObtenerMovimientosPorTipoQuery
            {
                TipoMovimiento = tipoMovimiento,
                PageNumber = pageNumber,
                PageSize = pageSize,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                IngredienteId = ingredienteId
            };

            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                var response = ApiResponse<PaginatedList<MovimientoInventarioDto>>.SuccessResponse(
                    result.Value, $"Movimientos de tipo {tipo} obtenidos exitosamente");
                return Ok(response);
            }
            else
            {
                var errorResponse = ApiResponse<PaginatedList<MovimientoInventarioDto>>.ErrorResponse(
                    result.Error ?? "Error al obtener movimientos por tipo", 
                    "No se pudieron obtener los movimientos");
                return BadRequest(errorResponse);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener movimientos por tipo {Tipo}", tipo);
            var errorResponse = ApiResponse<PaginatedList<MovimientoInventarioDto>>.ErrorResponse(
                "Error interno del servidor", "Error al obtener movimientos por tipo");
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Genera un reporte de movimientos de inventario
    /// </summary>
    [HttpGet("reporte")]
    [ProducesResponseType(typeof(ApiResponse<RestaurantePro.Application.Inventario.MovimientosInventario.DTOs.ReporteMovimientosDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RestaurantePro.Application.Inventario.MovimientosInventario.DTOs.ReporteMovimientosDto>>> GenerarReporte(
        [FromQuery] GenerarReporteMovimientosQuery query)
    {
        var result = await _mediator.Send(query);
        return result.Succeeded 
            ? Ok(ApiResponse<RestaurantePro.Application.Inventario.MovimientosInventario.DTOs.ReporteMovimientosDto>.SuccessResponse((RestaurantePro.Application.Inventario.MovimientosInventario.DTOs.ReporteMovimientosDto)result.Value!, "Reporte generado exitosamente"))
            : BadRequest(ApiResponse<RestaurantePro.Application.Inventario.MovimientosInventario.DTOs.ReporteMovimientosDto>.ErrorResponse(result.Error ?? "Error al generar reporte", "No se pudo generar el reporte"));
    }

    /// <summary>
    /// Obtiene el ID del usuario actual desde los claims de autenticación
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => 
            c.Type == "sub" || 
            c.Type == "userid" || 
            c.Type == "UserId" || 
            c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return Guid.Empty;
    }
}

/// <summary>
/// Request para registrar un nuevo movimiento de inventario
/// </summary>
public class RegistrarMovimientoRequest
{
    public Guid IngredienteId { get; set; }
    public decimal Cantidad { get; set; }
    public TipoMovimientoInventario TipoMovimiento { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public Guid? UsuarioId { get; set; }
    public DateTime? Fecha { get; set; }
}

/// <summary>
/// Request para actualizar un movimiento de inventario
/// </summary>
public class ActualizarMovimientoRequest
{
    public decimal? Cantidad { get; set; }
    public decimal? CostoUnitario { get; set; }
    public string? Motivo { get; set; }
    public string? Observaciones { get; set; }
} 