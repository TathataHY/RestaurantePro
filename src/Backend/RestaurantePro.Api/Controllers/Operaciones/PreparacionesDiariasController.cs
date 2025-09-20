using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands;
using RestaurantePro.Application.Operaciones.Preparaciones.Queries;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.CrearPreparacionDiaria;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.ActualizarPreparacionDiaria;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.EliminarPreparacionDiaria;
using RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerPreparacionesDiarias;
using RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerPreparacionDiariaPorId;
using RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerMenuDelDia;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.ConsumirPreparacionDiaria;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.MarcarPreparacionDiariaDisponible;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;

namespace RestaurantePro.Api.Controllers.Operaciones;

/// <summary>
/// Controlador para gestión de preparaciones diarias de cocina
/// </summary>
[ApiController]
[Route("api/operaciones/preparaciones-diarias")]
[Produces("application/json")]
[Authorize]
public class PreparacionesDiariasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PreparacionesDiariasController> _logger;

    public PreparacionesDiariasController(IMediator mediator, ILogger<PreparacionesDiariasController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las preparaciones diarias
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<PreparacionDiariaDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PreparacionDiariaDto>>>> GetPreparacionesDiarias()
    {
        _logger.LogInformation("📋 GET /api/operaciones/preparaciones-diarias");
        
        var query = new ObtenerPreparacionesDiariasQuery();
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<List<PreparacionDiariaDto>>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al obtener preparaciones diarias", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<List<PreparacionDiariaDto>>.SuccessResponse(
            result.Value, "Preparaciones diarias obtenidas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene una preparación diaria específica por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PreparacionDiariaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PreparacionDiariaDto>>> GetPreparacionDiaria(Guid id)
    {
        _logger.LogInformation("🔍 GET /api/operaciones/preparaciones-diarias/{Id}", id);
        
        var query = new ObtenerPreparacionDiariaPorIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Preparación diaria no encontrada", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<PreparacionDiariaDto>.SuccessResponse(
            result.Value, "Preparación diaria obtenida exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene el menú del día (solo preparaciones disponibles, de hoy, no vencidas)
    /// </summary>
    [HttpGet("menu-del-dia")]
    [ProducesResponseType(typeof(ApiResponse<List<PreparacionDiariaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<PreparacionDiariaDto>>>> ObtenerMenuDelDia(
        [FromQuery] DateTime? fecha = null,
        [FromQuery] int? limite = 10)
    {
        _logger.LogInformation("🍽️ GET /api/operaciones/preparaciones-diarias/menu-del-dia - Fecha: {Fecha}, Límite: {Limite}", 
            fecha?.ToString("yyyy-MM-dd") ?? "HOY", limite);

        var query = new ObtenerMenuDelDiaQuery 
        { 
            Fecha = fecha,
            Limite = limite
        };
        
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" }, "Error al obtener menú del día", StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }

        var response = ApiResponse<List<PreparacionDiariaDto>>.SuccessResponse(
            result.Value, "Menú del día obtenido exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Crea una nueva preparación diaria
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PreparacionDiariaDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PreparacionDiariaDto>>> CrearPreparacionDiaria(
        [FromBody] CrearPreparacionDiariaCommand command)
    {
        _logger.LogInformation("➕ POST /api/operaciones/preparaciones-diarias");
        
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al crear preparación diaria", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<PreparacionDiariaDto>.SuccessResponse(
            result.Value, "Preparación diaria creada exitosamente");
            
        return CreatedAtAction(
            nameof(GetPreparacionDiaria),
            new { id = result.Value.Id },
            response);
    }

    /// <summary>
    /// Actualiza una preparación diaria existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PreparacionDiariaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PreparacionDiariaDto>>> ActualizarPreparacionDiaria(
        Guid id, [FromBody] ActualizarPreparacionDiariaCommand command)
    {
        _logger.LogInformation("✏️ PUT /api/operaciones/preparaciones-diarias/{Id}", id);

        // Asignar el ID de la URL al comando
        command.Id = id;

        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var statusCode = (result.Errors ?? new List<string>()).Any(e => e.Contains("no encontrada")) 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al actualizar preparación diaria", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<PreparacionDiariaDto>.SuccessResponse(
            result.Value, "Preparación diaria actualizada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Elimina una preparación diaria
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> EliminarPreparacionDiaria(Guid id)
    {
        _logger.LogInformation("🗑️ DELETE /api/operaciones/preparaciones-diarias/{Id}", id);

        var command = new EliminarPreparacionDiariaCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al eliminar preparación diaria", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<object>.SuccessResponse(
            new object(), "Preparación diaria eliminada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Consume una cantidad específica de una preparación diaria
    /// </summary>
    [HttpPost("{id:guid}/consumir")]
    [ProducesResponseType(typeof(ApiResponse<PreparacionDiariaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PreparacionDiariaDto>>> ConsumirPreparacionDiaria(
        Guid id, [FromBody] ConsumirPreparacionDiariaCommand command)
    {
        _logger.LogInformation("🍽️ POST /api/operaciones/preparaciones-diarias/{Id}/consumir", id);

        // Asignar el ID de la URL al comando
        command.Id = id;
        
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var statusCode = (result.Errors ?? new List<string>()).Any(e => e.Contains("no encontrada")) 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al consumir preparación diaria", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<PreparacionDiariaDto>.SuccessResponse(
            result.Value, "Preparación diaria consumida exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Marca una preparación diaria como disponible
    /// </summary>
    [HttpPost("{id:guid}/disponible")]
    [ProducesResponseType(typeof(ApiResponse<PreparacionDiariaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PreparacionDiariaDto>>> MarcarComoDisponible(Guid id)
    {
        _logger.LogInformation("🟢 POST /api/operaciones/preparaciones-diarias/{Id}/disponible", id);

        var command = new MarcarPreparacionDiariaDisponibleCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var statusCode = (result.Errors ?? new List<string>()).Any(e => e.Contains("no encontrada")) 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al marcar como disponible", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<PreparacionDiariaDto>.SuccessResponse(
            result.Value, "Preparación diaria marcada como disponible exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene preparaciones diarias por estado
    /// </summary>
    [HttpGet("por-estado")]
    [ProducesResponseType(typeof(ApiResponse<List<PreparacionDiariaDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PreparacionDiariaDto>>>> ObtenerPreparacionesDiariasPorEstado([FromQuery] string estado)
    {
        _logger.LogInformation("📋 GET /api/operaciones/preparaciones-diarias/por-estado - Estado: {Estado}", estado);

        try
        {
            var query = new ObtenerPreparacionesDiariasQuery();
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                var errorResponse = ApiResponse<List<PreparacionDiariaDto>>.ErrorResponse(
                    result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al obtener preparaciones diarias", StatusCodes.Status400BadRequest);
                return BadRequest(errorResponse);
            }

            var preparaciones = result.Value;

            if (!string.IsNullOrWhiteSpace(estado))
            {
                preparaciones = preparaciones.Where(p => 
                    p.Estado.ToString().Equals(estado, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            var response = ApiResponse<List<PreparacionDiariaDto>>.SuccessResponse(
                preparaciones, "Preparaciones diarias por estado obtenidas exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener preparaciones diarias por estado: {Estado}", estado);
            var errorResponse = ApiResponse<List<PreparacionDiariaDto>>.ErrorResponse(
                new List<string> { "Error interno al obtener preparaciones diarias por estado" }, "Error de servidor", StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Obtiene preparaciones diarias por producto
    /// </summary>
    [HttpGet("por-producto/{productoId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<PreparacionDiariaDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PreparacionDiariaDto>>>> ObtenerPreparacionesDiariasPorProducto(Guid productoId)
    {
        _logger.LogInformation("🏷️ GET /api/operaciones/preparaciones-diarias/por-producto/{ProductoId}", productoId);

        try
        {
            var query = new ObtenerPreparacionesDiariasQuery();
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                var errorResponse = ApiResponse<List<PreparacionDiariaDto>>.ErrorResponse(
                    result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al obtener preparaciones diarias", StatusCodes.Status400BadRequest);
                return BadRequest(errorResponse);
            }

            var preparaciones = result.Value.Where(p => p.ProductoId == productoId).ToList();

            var response = ApiResponse<List<PreparacionDiariaDto>>.SuccessResponse(
                preparaciones, "Preparaciones diarias por producto obtenidas exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener preparaciones diarias por producto: {ProductoId}", productoId);
            var errorResponse = ApiResponse<List<PreparacionDiariaDto>>.ErrorResponse(
                new List<string> { "Error interno al obtener preparaciones diarias por producto" }, "Error de servidor", StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Obtiene estadísticas de preparaciones diarias
    /// </summary>
    [HttpGet("estadisticas")]
    [ProducesResponseType(typeof(ApiResponse<EstadisticasPreparacionesDiariasDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<EstadisticasPreparacionesDiariasDto>>> ObtenerEstadisticas()
    {
        _logger.LogInformation("📊 GET /api/operaciones/preparaciones-diarias/estadisticas");

        try
        {
            var query = new ObtenerPreparacionesDiariasQuery();
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                var errorResponse = ApiResponse<EstadisticasPreparacionesDiariasDto>.ErrorResponse(
                    result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al obtener preparaciones diarias", StatusCodes.Status400BadRequest);
                return BadRequest(errorResponse);
            }

            var preparaciones = result.Value;

            var estadisticas = new EstadisticasPreparacionesDiariasDto
            {
                TotalPreparaciones = preparaciones.Count,
                // Disponibles: suma total de cantidad disponible (más útil en el header)
                PreparacionesDisponibles = preparaciones.Sum(p => p.CantidadDisponible),
                // Por vencer: conteo basado en la regla EstaPorVencer
                PreparacionesPorVencer = preparaciones.Count(p => p.EstaPorVencer),
                PreparacionesAgotadas = preparaciones.Count(p => p.Estado == EstadoPreparacion.Agotada),
                PreparacionesVencidas = preparaciones.Count(p => p.Estado == EstadoPreparacion.Vencida),
                PreparacionesEnPreparacion = preparaciones.Count(p => p.Estado == EstadoPreparacion.Preparando),
                CantidadTotalPreparada = preparaciones.Sum(p => p.CantidadPreparada),
                CantidadDisponible = preparaciones.Sum(p => p.CantidadDisponible),
                CantidadConsumida = preparaciones.Sum(p => p.CantidadPreparada - p.CantidadDisponible),
                CantidadDesperdiciada = preparaciones.Where(p => p.Estado == EstadoPreparacion.Vencida).Sum(p => p.CantidadDisponible),
                PorcentajeEficiencia = preparaciones.Any() ? 
                    (decimal)preparaciones.Sum(p => p.CantidadPreparada - p.CantidadDisponible) / 
                    preparaciones.Sum(p => p.CantidadPreparada) * 100 : 0
            };

            var response = ApiResponse<EstadisticasPreparacionesDiariasDto>.SuccessResponse(
                estadisticas, "Estadísticas de preparaciones diarias obtenidas exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener estadísticas de preparaciones diarias");
            var errorResponse = ApiResponse<EstadisticasPreparacionesDiariasDto>.ErrorResponse(
                new List<string> { "Error interno al obtener estadísticas" }, "Error de servidor", StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
} 