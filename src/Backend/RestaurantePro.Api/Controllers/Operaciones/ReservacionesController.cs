using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Operaciones.Reservaciones.Commands;
using RestaurantePro.Application.Operaciones.Reservaciones.Queries;
using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
using RestaurantePro.Application.Operaciones.Reservaciones.Commands.CrearReservacion;
using RestaurantePro.Application.Operaciones.Reservaciones.Commands.ActualizarReservacion;
using RestaurantePro.Application.Operaciones.Reservaciones.Commands.ConfirmarReservacion;
using RestaurantePro.Application.Operaciones.Reservaciones.Commands.ReprogramarReservacion;
using RestaurantePro.Application.Operaciones.Reservaciones.Commands.CancelarReservacion;
using RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionesPaginadas;
using RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionPorId;
using RestaurantePro.Application.Operaciones.Reservaciones.Queries.VerificarDisponibilidad;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Api.Controllers.Operaciones;

/// <summary>
/// Controlador para gestión de reservaciones del restaurante
/// </summary>
[ApiController]
[Route("api/operaciones/reservaciones")]
[Produces("application/json")]
public class ReservacionesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReservacionesController> _logger;

    public ReservacionesController(IMediator mediator, ILogger<ReservacionesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las reservaciones con paginación
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<ReservacionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedList<ReservacionDto>>>> GetReservaciones(
        [FromQuery] ObtenerReservacionesPaginadasQuery query)
    {
        _logger.LogInformation("📋 GET /api/operaciones/reservaciones");
        
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<PaginatedList<ReservacionDto>>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al obtener reservaciones", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<PaginatedList<ReservacionDto>>.SuccessResponse(
            result.Value, "Reservaciones obtenidas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene una reservación específica por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ReservacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ReservacionDto>>> GetReservacion(Guid id)
    {
        _logger.LogInformation("🔍 GET /api/operaciones/reservaciones/{Id}", id);
        
        var query = new ObtenerReservacionPorIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Reservación no encontrada", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<ReservacionDto>.SuccessResponse(
            result.Value, "Reservación obtenida exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Crea una nueva reservación
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ReservacionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ReservacionDto>>> CrearReservacion(
        [FromBody] CrearReservacionCommand command)
    {
        _logger.LogInformation("➕ POST /api/operaciones/reservaciones");
        
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al crear reservación", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<ReservacionDto>.SuccessResponse(
            result.Value, "Reservación creada exitosamente");
            
        return CreatedAtAction(
            nameof(GetReservacion),
            new { id = result.Value.Id },
            response);
    }

    /// <summary>
    /// Actualiza una reservación existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ReservacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ReservacionDto>>> ActualizarReservacion(
        Guid id, [FromBody] ActualizarReservacionCommand command)
    {
        _logger.LogInformation("✏️ PUT /api/operaciones/reservaciones/{Id}", id);

        // Asignar el ID de la URL al comando
        command.Id = id;

        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var statusCode = (result.Errors ?? new List<string>()).Any(e => e.Contains("no encontrada")) 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al actualizar reservación", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<ReservacionDto>.SuccessResponse(
            result.Value, "Reservación actualizada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Cancela una reservación
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ReservacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ReservacionDto>>> CancelarReservacion(Guid id)
    {
        _logger.LogInformation("🗑️ DELETE /api/operaciones/reservaciones/{Id}", id);

        // Crear comando por defecto para DELETE
        var command = new CancelarReservacionCommand
        {
            Id = id,
            ReservacionId = id,
            Motivo = MotivoCancelacion.ClienteSolicita,
            MotivoDetalle = "Cancelación solicitada por el usuario",
            NotificarCliente = true,
            LiberarMesaInmediatamente = true
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al cancelar reservación", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<ReservacionDto>.SuccessResponse(
            result.Value, "Reservación cancelada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Cancela una reservación con datos específicos
    /// </summary>
    [HttpPost("{id:guid}/cancelar")]
    [ProducesResponseType(typeof(ApiResponse<ReservacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ReservacionDto>>> CancelarReservacionConComando(Guid id, [FromBody] CancelarReservacionCommand command)
    {
        _logger.LogInformation("🗑️ POST /api/operaciones/reservaciones/{Id}/cancelar", id);

        // Asignar el ID de la URL al comando
        command.Id = id;
        command.ReservacionId = id;
        
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al cancelar reservación", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<ReservacionDto>.SuccessResponse(
            result.Value, "Reservación cancelada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Confirma una reservación
    /// </summary>
    [HttpPost("{id:guid}/confirmar")]
    [ProducesResponseType(typeof(ApiResponse<ReservacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ReservacionDto>>> ConfirmarReservacion(Guid id, [FromBody] ConfirmarReservacionCommand command)
    {
        _logger.LogInformation("✅ POST /api/operaciones/reservaciones/{Id}/confirmar", id);

        // Asignar el id de la URL a ambas propiedades para compatibilidad
        command.Id = id;
        command.ReservacionId = id;
        
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al confirmar reservación", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<ReservacionDto>.SuccessResponse(
            result.Value, "Reservación confirmada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Reprograma una reservación
    /// </summary>
    [HttpPost("{id:guid}/reprogramar")]
    [ProducesResponseType(typeof(ApiResponse<ReservacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ReservacionDto>>> ReprogramarReservacion(
        Guid id, [FromBody] ReprogramarReservacionCommand command)
    {
        _logger.LogInformation("🔄 POST /api/operaciones/reservaciones/{Id}/reprogramar", id);

        command.Id = id;
        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
        {
            var statusCode = (result.Errors ?? new List<string>()).Any(e => e.Contains("no encontrada")) 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al reprogramar reservación", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<ReservacionDto>.SuccessResponse(
            result.Value, "Reservación reprogramada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Verifica disponibilidad para una fecha y hora específica
    /// </summary>
    [HttpGet("disponibilidad")]
    [ProducesResponseType(typeof(ApiResponse<DisponibilidadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DisponibilidadDto>>> VerificarDisponibilidad(
        [FromQuery] VerificarDisponibilidadQuery query)
    {
        _logger.LogInformation("🔍 GET /api/operaciones/reservaciones/disponibilidad");
        
        var result = await _mediator.Send(query);
        
        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<DisponibilidadDto>.ErrorResponse(
                result.Errors ?? new List<string> { result.Error ?? "Error desconocido" }, "Error al verificar disponibilidad", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<DisponibilidadDto>.SuccessResponse(
            result.Value, "Disponibilidad verificada exitosamente");
        return Ok(response);
    }
}