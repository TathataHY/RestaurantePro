using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands;
using RestaurantePro.Application.Operaciones.Preparaciones.Queries;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.CrearPreparacion;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.ActualizarPreparacion;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.IniciarPreparacion;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.CompletarPreparacion;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.CancelarPreparacion;
using RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerPreparacionesPaginadas;
using RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerPreparacionPorId;
using RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerColaPreparaciones;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.MarcarComoDisponible;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;

namespace RestaurantePro.Api.Controllers.Operaciones;

/// <summary>
/// Controlador para gestión de preparaciones de cocina
/// </summary>
[ApiController]
[Route("api/operaciones/preparaciones")]
[Produces("application/json")]
[Authorize]
public class PreparacionesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PreparacionesController> _logger;

    public PreparacionesController(IMediator mediator, ILogger<PreparacionesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las preparaciones con paginación
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<PreparacionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedList<PreparacionDto>>>> GetPreparaciones(
        [FromQuery] ObtenerPreparacionesPaginadasQuery query)
    {
        _logger.LogInformation("📋 GET /api/operaciones/preparaciones");
        
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<PaginatedList<PreparacionDto>>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al obtener preparaciones", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<PaginatedList<PreparacionDto>>.SuccessResponse(
            result.Value, "Preparaciones obtenidas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene una preparación específica por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PreparacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PreparacionDto>>> GetPreparacion(Guid id)
    {
        _logger.LogInformation("🔍 GET /api/operaciones/preparaciones/{Id}", id);
        
        var query = new ObtenerPreparacionPorIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Preparación no encontrada", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<PreparacionDto>.SuccessResponse(
            result.Value, "Preparación obtenida exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Crea una nueva preparación
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PreparacionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PreparacionDto>>> CrearPreparacion(
        [FromBody] CrearPreparacionCommand command)
    {
        _logger.LogInformation("➕ POST /api/operaciones/preparaciones");
        
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al crear preparación", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<PreparacionDto>.SuccessResponse(
            result.Value, "Preparación creada exitosamente");
            
        return CreatedAtAction(
            nameof(GetPreparacion),
            new { id = result.Value.Id },
            response);
    }

    /// <summary>
    /// Actualiza una preparación existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PreparacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PreparacionDto>>> ActualizarPreparacion(
        Guid id, [FromBody] ActualizarPreparacionCommand command)
    {
        _logger.LogInformation("✏️ PUT /api/operaciones/preparaciones/{Id}", id);

        // Asignar el ID de la URL al comando
        command.Id = id;

        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var statusCode = (result.Errors ?? new List<string>()).Any(e => e.Contains("no encontrada")) 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al actualizar preparación", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<PreparacionDto>.SuccessResponse(
            result.Value, "Preparación actualizada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Inicia una preparación
    /// </summary>
    [HttpPost("{id:guid}/iniciar")]
    [ProducesResponseType(typeof(ApiResponse<PreparacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PreparacionDto>>> IniciarPreparacion(
        Guid id, [FromBody] IniciarPreparacionCommand command)
    {
        _logger.LogInformation("🚀 POST /api/operaciones/preparaciones/{Id}/iniciar", id);

        // Asignar el ID de la URL al comando
        command.Id = id;
        
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al iniciar preparación", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<PreparacionDto>.SuccessResponse(
            result.Value, "Preparación iniciada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Completa una preparación
    /// </summary>
    [HttpPost("{id:guid}/completar")]
    [ProducesResponseType(typeof(ApiResponse<PreparacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PreparacionDto>>> CompletarPreparacion(Guid id)
    {
        _logger.LogInformation("✅ POST /api/operaciones/preparaciones/{Id}/completar", id);

        var command = new CompletarPreparacionCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al completar preparación", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<PreparacionDto>.SuccessResponse(
            result.Value, "Preparación completada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Cancela una preparación
    /// </summary>
    [HttpPost("{id:guid}/cancelar")]
    [ProducesResponseType(typeof(ApiResponse<PreparacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PreparacionDto>>> CancelarPreparacion(
        Guid id, [FromBody] CancelarPreparacionCommand command)
    {
        _logger.LogInformation("❌ POST /api/operaciones/preparaciones/{Id}/cancelar", id);

        // Asignar el ID de la URL al comando
        command.Id = id;
        
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al cancelar preparación", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<PreparacionDto>.SuccessResponse(
            result.Value, "Preparación cancelada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene la cola de preparaciones pendientes
    /// </summary>
    [HttpGet("cola")]
    [ProducesResponseType(typeof(ApiResponse<List<PreparacionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PreparacionDto>>>> ObtenerColaPreparaciones()
    {
        _logger.LogInformation("📋 GET /api/operaciones/preparaciones/cola");
        
        var query = new ObtenerColaPreparacionesQuery();
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<List<PreparacionDto>>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al obtener cola de preparaciones", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<List<PreparacionDto>>.SuccessResponse(
            result.Value, "Cola de preparaciones obtenida exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Marca una preparación como disponible
    /// </summary>
    [HttpPost("{id:guid}/disponible")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarcarComoDisponible(Guid id, [FromBody] MarcarComoDisponibleCommand command)
    {
        _logger.LogInformation("🟢 POST /api/operaciones/preparaciones/{Id}/disponible", id);
        command.PreparacionId = id;
        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al marcar como disponible", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }
        var response = ApiResponse<object>.SuccessResponse(new object(), "Preparación marcada como disponible exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Busca preparaciones por término
    /// </summary>
    [HttpGet("buscar")]
    [ProducesResponseType(typeof(ApiResponse<List<PreparacionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PreparacionDto>>>> BuscarPreparaciones([FromQuery] string termino)
    {
        _logger.LogInformation("🔍 GET /api/operaciones/preparaciones/buscar - Término: {Termino}", termino);

        try
        {
            var query = new ObtenerPreparacionesPaginadasQuery
            {
                PageNumber = 1,
                PageSize = 1000 // Obtener todas para buscar
            };
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                var errorResponse = ApiResponse<List<PreparacionDto>>.ErrorResponse(
                    result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al obtener preparaciones", StatusCodes.Status400BadRequest);
                return BadRequest(errorResponse);
            }

            var preparaciones = result.Value.Items;

            if (!string.IsNullOrWhiteSpace(termino))
            {
                preparaciones = preparaciones.Where(p => 
                    p.NombreProducto.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                    p.Estado.ToString().Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                    p.NumeroComanda.Contains(termino, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            var response = ApiResponse<List<PreparacionDto>>.SuccessResponse(
                preparaciones, "Preparaciones encontradas exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al buscar preparaciones: {Termino}", termino);
            var errorResponse = ApiResponse<List<PreparacionDto>>.ErrorResponse(
                new List<string> { "Error interno al buscar preparaciones" }, "Error de servidor", StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Obtiene estadísticas de preparaciones
    /// </summary>
    [HttpGet("estadisticas")]
    [ProducesResponseType(typeof(ApiResponse<EstadisticasPreparacionesDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<EstadisticasPreparacionesDto>>> ObtenerEstadisticas()
    {
        _logger.LogInformation("📊 GET /api/operaciones/preparaciones/estadisticas");

        try
        {
            var query = new ObtenerPreparacionesPaginadasQuery
            {
                PageNumber = 1,
                PageSize = 10000 // Obtener todas para estadísticas
            };
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                var errorResponse = ApiResponse<EstadisticasPreparacionesDto>.ErrorResponse(
                    result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al obtener preparaciones", StatusCodes.Status400BadRequest);
                return BadRequest(errorResponse);
            }

            var preparaciones = result.Value.Items;

            var estadisticas = new EstadisticasPreparacionesDto
            {
                TotalPreparaciones = preparaciones.Count,
                PreparacionesDisponibles = preparaciones.Count(p => p.Estado == EstadoPreparacion.Disponible),
                PreparacionesPorVencer = preparaciones.Count(p => p.Estado == EstadoPreparacion.PorVencer),
                PreparacionesAgotadas = preparaciones.Count(p => p.Estado == EstadoPreparacion.Agotada),
                PreparacionesVencidas = preparaciones.Count(p => p.Estado == EstadoPreparacion.Vencida),
                CantidadTotalPreparada = preparaciones.Sum(p => p.Cantidad),
                CantidadDisponible = preparaciones.Where(p => p.Estado == EstadoPreparacion.Disponible).Sum(p => p.Cantidad),
                CantidadConsumida = preparaciones.Where(p => p.Estado == EstadoPreparacion.Agotada).Sum(p => p.Cantidad),
                CantidadDesperdiciada = preparaciones.Where(p => p.Estado == EstadoPreparacion.Vencida).Sum(p => p.Cantidad),
                PorcentajeEficiencia = preparaciones.Any() ? 
                    (decimal)preparaciones.Where(p => p.Estado == EstadoPreparacion.Agotada).Sum(p => p.Cantidad) / 
                    preparaciones.Sum(p => p.Cantidad) * 100 : 0
            };

            var response = ApiResponse<EstadisticasPreparacionesDto>.SuccessResponse(
                estadisticas, "Estadísticas de preparaciones obtenidas exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener estadísticas de preparaciones");
            var errorResponse = ApiResponse<EstadisticasPreparacionesDto>.ErrorResponse(
                new List<string> { "Error interno al obtener estadísticas" }, "Error de servidor", StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Obtiene preparaciones por categoría
    /// </summary>
    [HttpGet("por-categoria")]
    [ProducesResponseType(typeof(ApiResponse<List<PreparacionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PreparacionDto>>>> ObtenerPreparacionesPorCategoria([FromQuery] string categoria)
    {
        _logger.LogInformation("🏷️ GET /api/operaciones/preparaciones/por-categoria - Categoría: {Categoria}", categoria);

        try
        {
            var query = new ObtenerPreparacionesPaginadasQuery
            {
                PageNumber = 1,
                PageSize = 1000 // Obtener todas para filtrar
            };
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                var errorResponse = ApiResponse<List<PreparacionDto>>.ErrorResponse(
                    result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al obtener preparaciones", StatusCodes.Status400BadRequest);
                return BadRequest(errorResponse);
            }

            var preparaciones = result.Value.Items;

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                // Como PreparacionDto no tiene Categoria, filtramos por nombre de producto que contenga la categoría
                preparaciones = preparaciones.Where(p => 
                    p.NombreProducto.Contains(categoria, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            var response = ApiResponse<List<PreparacionDto>>.SuccessResponse(
                preparaciones, "Preparaciones por categoría obtenidas exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener preparaciones por categoría: {Categoria}", categoria);
            var errorResponse = ApiResponse<List<PreparacionDto>>.ErrorResponse(
                new List<string> { "Error interno al obtener preparaciones por categoría" }, "Error de servidor", StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Obtiene preparaciones por estado
    /// </summary>
    [HttpGet("por-estado")]
    [ProducesResponseType(typeof(ApiResponse<List<PreparacionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PreparacionDto>>>> ObtenerPreparacionesPorEstado([FromQuery] string estado)
    {
        _logger.LogInformation("📋 GET /api/operaciones/preparaciones/por-estado - Estado: {Estado}", estado);

        try
        {
            var query = new ObtenerPreparacionesPaginadasQuery
            {
                PageNumber = 1,
                PageSize = 1000 // Obtener todas para filtrar
            };
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                var errorResponse = ApiResponse<List<PreparacionDto>>.ErrorResponse(
                    result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al obtener preparaciones", StatusCodes.Status400BadRequest);
                return BadRequest(errorResponse);
            }

            var preparaciones = result.Value.Items;

            if (!string.IsNullOrWhiteSpace(estado))
            {
                preparaciones = preparaciones.Where(p => 
                    p.Estado.ToString().Equals(estado, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            var response = ApiResponse<List<PreparacionDto>>.SuccessResponse(
                preparaciones, "Preparaciones por estado obtenidas exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener preparaciones por estado: {Estado}", estado);
            var errorResponse = ApiResponse<List<PreparacionDto>>.ErrorResponse(
                new List<string> { "Error interno al obtener preparaciones por estado" }, "Error de servidor", StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
} 