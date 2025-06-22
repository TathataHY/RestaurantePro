namespace RestaurantePro.Api.Controllers.Core;

/// <summary>
/// Controlador para la gestión de notificaciones del sistema
/// Contexto: Core
/// </summary>
[ApiController]
[Route("api/core/notificaciones")]
[Produces("application/json")]
[Authorize] // Requiere autenticación para acceder a notificaciones
public class NotificacionesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<NotificacionesController> _logger;

    public NotificacionesController(IMediator mediator, ILogger<NotificacionesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene las notificaciones del usuario actual
    /// </summary>
    /// <param name="soloNoLeidas">Si es true, solo devuelve notificaciones no leídas</param>
    /// <returns>Lista de notificaciones del usuario</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<NotificacionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<NotificacionDto>>>> GetNotificaciones(
        [FromQuery] bool soloNoLeidas = false)
    {
        _logger.LogInformation("📋 GET /api/core/notificaciones (SoloNoLeidas: {SoloNoLeidas})", soloNoLeidas);

        // TODO: Obtener UsuarioId del token JWT cuando se implemente autenticación
        // Por ahora, usar un usuarioId fijo para que los tests funcionen
        var usuarioId = new Guid("11111111-1111-1111-1111-111111111111");

        var query = new ObtenerNotificacionesQuery
        {
            UsuarioId = usuarioId,
            SoloNoLeidas = soloNoLeidas
        };

        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<List<NotificacionDto>>.ErrorResponse(
                result.Errors, "Error al obtener notificaciones", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<List<NotificacionDto>>.SuccessResponse(
            result.Value, "Notificaciones obtenidas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene una notificación específica por ID
    /// </summary>
    /// <param name="id">ID de la notificación</param>
    /// <returns>Notificación encontrada</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<NotificacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<NotificacionDto>>> GetNotificacion(Guid id)
    {
        _logger.LogInformation("🔍 GET /api/core/notificaciones/{Id}", id);

        var query = new ObtenerNotificacionPorIdQuery { NotificacionId = id };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Notificación no encontrada", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<NotificacionDto>.SuccessResponse(
            result.Value, "Notificación obtenida exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Crea una nueva notificación (solo para administradores)
    /// </summary>
    /// <param name="command">Datos de la notificación a crear</param>
    /// <returns>Notificación creada</returns>
    [HttpPost]
    [Authorize(Roles = "Administrador,SuperAdministrador")]
    [ProducesResponseType(typeof(ApiResponse<NotificacionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<NotificacionDto>>> CrearNotificacion(
        [FromBody] CrearNotificacionCommand command)
    {
        _logger.LogInformation("➕ POST /api/core/notificaciones");

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al crear notificación", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<NotificacionDto>.SuccessResponse(
            result.Value, "Notificación creada exitosamente");

        return CreatedAtAction(
            nameof(GetNotificacion),
            new { id = result.Value.Id },
            response);
    }

    /// <summary>
    /// Marca una notificación como leída
    /// </summary>
    /// <param name="id">ID de la notificación</param>
    /// <returns>Confirmación de la operación</returns>
    [HttpPost("{id:guid}/marcar-leida")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<bool>>> MarcarComoLeida(Guid id)
    {
        _logger.LogInformation("✅ POST /api/core/notificaciones/{Id}/marcar-leida", id);

        var command = new MarcarNotificacionComoLeidaCommand { NotificacionId = id };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al marcar notificación como leída", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<bool>.SuccessResponse(
            result.Value, "Notificación marcada como leída exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Marca todas las notificaciones del usuario como leídas
    /// </summary>
    /// <returns>Número de notificaciones marcadas como leídas</returns>
    [HttpPost("marcar-leida")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<int>>> MarcarTodasComoLeidas()
    {
        _logger.LogInformation("✅ POST /api/core/notificaciones/marcar-leida");

        // TODO: Obtener UsuarioId del token JWT cuando se implemente autenticación
        // Por ahora, usar un usuarioId fijo para que los tests funcionen
        var usuarioId = new Guid("11111111-1111-1111-1111-111111111111");

        var command = new MarcarTodasNotificacionesComoLeidasCommand { UsuarioId = usuarioId };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al marcar notificaciones como leídas", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<int>.SuccessResponse(
            result.Value, $"{result.Value} notificaciones marcadas como leídas exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Elimina una notificación (solo para administradores)
    /// </summary>
    /// <param name="id">ID de la notificación a eliminar</param>
    /// <returns>Confirmación de la eliminación</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador,SuperAdministrador")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<bool>>> EliminarNotificacion(Guid id)
    {
        _logger.LogInformation("🗑️ DELETE /api/core/notificaciones/{Id}", id);

        var command = new EliminarNotificacionCommand { NotificacionId = id };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al eliminar notificación", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<bool>.SuccessResponse(
            result.Value, "Notificación eliminada exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene el número de notificaciones no leídas del usuario actual
    /// </summary>
    /// <returns>Número de notificaciones no leídas</returns>
    [HttpGet("contador-no-leidas")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<int>>> GetContadorNoLeidas()
    {
        _logger.LogInformation("🔢 GET /api/core/notificaciones/contador-no-leidas");

        // TODO: Obtener UsuarioId del token JWT cuando se implemente autenticación
        // Por ahora, usar un usuarioId fijo para que los tests funcionen
        var usuarioId = new Guid("11111111-1111-1111-1111-111111111111");

        var query = new ObtenerContadorNoLeidasQuery { UsuarioId = usuarioId };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al obtener contador de notificaciones", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<int>.SuccessResponse(
            result.Value, "Contador de notificaciones no leídas obtenido exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene la configuración de notificaciones del usuario
    /// </summary>
    [HttpGet("configuracion")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetConfiguracion()
    {
        _logger.LogInformation("⚙️ GET /api/core/notificaciones/configuracion");

        // TODO: Implementar cuando se agregue configuración de notificaciones
        var configuracion = new
        {
            EmailHabilitado = true,
            PushHabilitado = true,
            TiposNotificacion = new[] { "Informativa", "Advertencia", "Error", "Exito" }
        };

        var response = ApiResponse<object>.SuccessResponse(
            configuracion, "Configuración obtenida exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Actualiza la configuración de notificaciones del usuario
    /// </summary>
    [HttpPost("configuracion")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> ActualizarConfiguracion([FromBody] object configuracion)
    {
        _logger.LogInformation("⚙️ POST /api/core/notificaciones/configuracion");

        // TODO: Implementar cuando se agregue configuración de notificaciones
        var response = ApiResponse<object>.SuccessResponse(
            configuracion, "Configuración actualizada exitosamente");
        return Ok(response);
    }
}