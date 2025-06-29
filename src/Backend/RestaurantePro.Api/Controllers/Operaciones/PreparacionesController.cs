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
} 