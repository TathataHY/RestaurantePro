using Microsoft.AspNetCore.Authorization;
using RestaurantePro.Application.Comercial.Fidelizacion.Queries.ObtenerTarjetasFidelizacion;
using RestaurantePro.Application.Comercial.Fidelizacion.Queries.ObtenerTarjetaFidelizacionPorId;
using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;
using RestaurantePro.Application.Comercial.Fidelizacion.Commands;
using RestaurantePro.Application.Comercial.Fidelizacion.Commands.ActualizarTarjetaFidelizacion;
using RestaurantePro.Application.Comercial.Fidelizacion.Commands.ActivarTarjetaFidelizacion;
using RestaurantePro.Application.Comercial.Fidelizacion.Commands.DesactivarTarjetaFidelizacion;
using RestaurantePro.Application.Comercial.Fidelizacion.Commands.EliminarTarjetaFidelizacion;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Application.Comercial.Fidelizacion.Commands.CrearTarjetaFidelizacion;

namespace RestaurantePro.Api.Controllers.Comercial;

/// <summary>
/// Controlador para la gestión de tarjetas de fidelización
/// Endpoints para CRUD completo de tarjetas y operaciones de puntos
/// </summary>
[ApiController]
[Route("api/comercial/tarjetas-fidelizacion")]
[Produces("application/json")]
[Authorize]
public class TarjetasFidelizacionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TarjetasFidelizacionController> _logger;

    public TarjetasFidelizacionController(IMediator mediator, ILogger<TarjetasFidelizacionController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las tarjetas de fidelización con filtros opcionales
    /// </summary>
    /// <param name="estado">Filtrar por estado de tarjeta</param>
    /// <param name="nivel">Filtrar por nivel de fidelización</param>
    /// <param name="clienteId">Filtrar por cliente específico</param>
    /// <param name="pageNumber">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <returns>Lista de tarjetas de fidelización</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<TarjetaFidelizacionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<TarjetaFidelizacionDto>>>> GetTarjetas(
        [FromQuery] EstadoTarjeta? estado = null,
        [FromQuery] NivelFidelizacion? nivel = null,
        [FromQuery] Guid? clienteId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("🎫 GET /api/comercial/tarjetas-fidelizacion - Estado: {Estado}, Nivel: {Nivel}, ClienteId: {ClienteId}", 
            estado, nivel, clienteId);

        var query = new ObtenerTarjetasFidelizacionQuery
        {
            Estado = estado,
            Nivel = nivel,
            ClienteId = clienteId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<List<TarjetaFidelizacionDto>>.ErrorResponse(
                result.Errors, "Error al obtener tarjetas de fidelización", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<List<TarjetaFidelizacionDto>>.SuccessResponse(
            result.Value, "Tarjetas de fidelización obtenidas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene una tarjeta de fidelización específica por ID
    /// </summary>
    /// <param name="id">ID de la tarjeta</param>
    /// <returns>Tarjeta de fidelización</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TarjetaFidelizacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TarjetaFidelizacionDto>>> GetTarjeta(Guid id)
    {
        _logger.LogInformation("🔍 GET /api/comercial/tarjetas-fidelizacion/{Id}", id);

        var query = new ObtenerTarjetaFidelizacionPorIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Tarjeta de fidelización no encontrada", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(
            result.Value, "Tarjeta de fidelización obtenida exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Crea una nueva tarjeta de fidelización
    /// </summary>
    /// <param name="command">Datos de la tarjeta a crear</param>
    /// <returns>Tarjeta creada</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TarjetaFidelizacionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<TarjetaFidelizacionDto>>> PostTarjeta([FromBody] CrearTarjetaFidelizacionCommand command)
    {
        _logger.LogInformation("➕ POST /api/comercial/tarjetas-fidelizacion");

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al crear tarjeta de fidelización", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(
            result.Value, "Tarjeta de fidelización creada exitosamente");
            
        return CreatedAtAction(
            nameof(GetTarjeta),
            new { id = result.Value.Id },
            response);
    }

    /// <summary>
    /// Actualiza una tarjeta de fidelización existente
    /// </summary>
    /// <param name="id">ID de la tarjeta</param>
    /// <param name="command">Datos actualizados de la tarjeta</param>
    /// <returns>Tarjeta actualizada</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TarjetaFidelizacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TarjetaFidelizacionDto>>> PutTarjeta(Guid id, [FromBody] ActualizarTarjetaFidelizacionCommand command)
    {
        _logger.LogInformation("✏️ PUT /api/comercial/tarjetas-fidelizacion/{Id}", id);

        // Asignar el ID de la URL al comando
        command.Id = id;

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var statusCode = result.Errors.Any(e => e.Contains("no encontrada")) 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al actualizar tarjeta de fidelización", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(
            result.Value, "Tarjeta de fidelización actualizada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Activar una tarjeta de fidelización
    /// </summary>
    /// <param name="id">ID de la tarjeta</param>
    /// <returns>Resultado de la activación</returns>
    [HttpPatch("{id:guid}/activar")]
    [ProducesResponseType(typeof(ApiResponse<TarjetaFidelizacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TarjetaFidelizacionDto>>> ActivarTarjeta(Guid id)
    {
        _logger.LogInformation("🔓 PATCH /api/comercial/tarjetas-fidelizacion/{Id}/activar", id);

        var command = new ActivarTarjetaFidelizacionCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var statusCode = result.Errors.Any(e => e.Contains("no encontrada")) 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al activar tarjeta de fidelización", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(
            result.Value, "Tarjeta de fidelización activada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Desactivar una tarjeta de fidelización
    /// </summary>
    /// <param name="id">ID de la tarjeta</param>
    /// <returns>Resultado de la desactivación</returns>
    [HttpPatch("{id:guid}/desactivar")]
    [ProducesResponseType(typeof(ApiResponse<TarjetaFidelizacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TarjetaFidelizacionDto>>> DesactivarTarjeta(Guid id)
    {
        _logger.LogInformation("🔒 PATCH /api/comercial/tarjetas-fidelizacion/{Id}/desactivar", id);

        var command = new DesactivarTarjetaFidelizacionCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var statusCode = result.Errors.Any(e => e.Contains("no encontrada")) 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al desactivar tarjeta de fidelización", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(
            result.Value, "Tarjeta de fidelización desactivada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Agregar puntos a una tarjeta de fidelización
    /// </summary>
    /// <param name="id">ID de la tarjeta</param>
    /// <param name="command">Datos de los puntos a agregar</param>
    /// <returns>Resultado de la operación</returns>
    [HttpPost("{id:guid}/puntos")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> AgregarPuntos(Guid id, [FromBody] object command)
    {
        _logger.LogInformation("➕ POST /api/comercial/tarjetas-fidelizacion/{Id}/puntos", id);

        var response = ApiResponse<object>.ErrorResponse(
            new List<string> { "Endpoint no implementado aún" },
            "Endpoint no implementado aún",
            StatusCodes.Status501NotImplemented);

        return StatusCode(StatusCodes.Status501NotImplemented, response);
    }

    /// <summary>
    /// Canjear puntos de una tarjeta de fidelización
    /// </summary>
    /// <param name="id">ID de la tarjeta</param>
    /// <param name="command">Datos del canje de puntos</param>
    /// <returns>Resultado del canje</returns>
    [HttpPost("{id:guid}/canjear")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> CanjearPuntos(Guid id, [FromBody] object command)
    {
        _logger.LogInformation("🎁 POST /api/comercial/tarjetas-fidelizacion/{Id}/canjear", id);

        var response = ApiResponse<object>.ErrorResponse(
            new List<string> { "Endpoint no implementado aún" },
            "Endpoint no implementado aún",
            StatusCodes.Status501NotImplemented);

        return StatusCode(StatusCodes.Status501NotImplemented, response);
    }

    /// <summary>
    /// Obtener historial de puntos de una tarjeta
    /// </summary>
    /// <param name="id">ID de la tarjeta</param>
    /// <returns>Historial de puntos</returns>
    [HttpGet("{id:guid}/historial")]
    [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetHistorialPuntos(Guid id)
    {
        _logger.LogInformation("📊 GET /api/comercial/tarjetas-fidelizacion/{Id}/historial", id);

        var response = ApiResponse<List<object>>.ErrorResponse(
            new List<string> { "Endpoint no implementado aún" },
            "Endpoint no implementado aún",
            StatusCodes.Status501NotImplemented);

        return StatusCode(StatusCodes.Status501NotImplemented, response);
    }

    /// <summary>
    /// Obtener estadísticas de una tarjeta
    /// </summary>
    /// <param name="id">ID de la tarjeta</param>
    /// <returns>Estadísticas de la tarjeta</returns>
    [HttpGet("{id:guid}/estadisticas")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> GetEstadisticas(Guid id)
    {
        _logger.LogInformation("📈 GET /api/comercial/tarjetas-fidelizacion/{Id}/estadisticas", id);

        var response = ApiResponse<object>.ErrorResponse(
            new List<string> { "Endpoint no implementado aún" },
            "Endpoint no implementado aún",
            StatusCodes.Status501NotImplemented);

        return StatusCode(StatusCodes.Status501NotImplemented, response);
    }

    /// <summary>
    /// Eliminar una tarjeta de fidelización
    /// </summary>
    /// <param name="id">ID de la tarjeta</param>
    /// <returns>Resultado de la eliminación</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTarjeta(Guid id)
    {
        _logger.LogInformation("🗑️ DELETE /api/comercial/tarjetas-fidelizacion/{Id}", id);

        var command = new EliminarTarjetaFidelizacionCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al eliminar tarjeta de fidelización", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<bool>.SuccessResponse(
            result.Value, "Tarjeta de fidelización eliminada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene todas las tarjetas de fidelización de un cliente específico
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <returns>Lista de tarjetas de fidelización del cliente</returns>
    [HttpGet("cliente/{clienteId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<TarjetaFidelizacionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<TarjetaFidelizacionDto>>>> GetTarjetasPorCliente(Guid clienteId)
    {
        _logger.LogInformation("👤 GET /api/comercial/tarjetas-fidelizacion/cliente/{ClienteId}", clienteId);

        var query = new ObtenerTarjetasFidelizacionQuery
        {
            ClienteId = clienteId,
            PageNumber = 1,
            PageSize = 100 // Asumimos máximo 100 por cliente
        };

        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<List<TarjetaFidelizacionDto>>.ErrorResponse(
                result.Errors, "No se encontraron tarjetas para el cliente", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<List<TarjetaFidelizacionDto>>.SuccessResponse(
            result.Value, "Tarjetas de fidelización del cliente obtenidas exitosamente");
        return Ok(response);
    }
} 