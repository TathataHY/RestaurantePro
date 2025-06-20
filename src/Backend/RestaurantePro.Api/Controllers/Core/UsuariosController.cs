using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Core.Usuarios.Commands.CrearUsuario;
using RestaurantePro.Application.Core.Usuarios.Commands.ActualizarUsuario;
using RestaurantePro.Application.Core.Usuarios.DTOs;

namespace RestaurantePro.Api.Controllers.Core;

/// <summary>
/// Controlador para la gestión de usuarios
/// Contexto: Core
/// </summary>
[ApiController]
[Route("api/core/usuarios")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(IMediator mediator, ILogger<UsuariosController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene todos los usuarios con paginación
    /// </summary>
    [HttpGet]
    // [Authorize(Roles = "Administrador,Gerente")] // TEMPORAL: Deshabilitado para testing
    [ProducesResponseType(typeof(ApiResponse<List<UsuarioDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status501NotImplemented)]
    public async Task<ActionResult<ApiResponse<List<UsuarioDto>>>> GetUsuarios(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? filtro = null,
        [FromQuery] bool soloActivos = true,
        [FromQuery] string orderBy = "Nombre",
        [FromQuery] string orderDirection = "asc")
    {
        _logger.LogInformation("👥 GET /api/core/usuarios - Página: {PageNumber}, Tamaño: {PageSize}", pageNumber, pageSize);

        // TODO: Implementar query para obtener usuarios
        return StatusCode(StatusCodes.Status501NotImplemented, 
            ApiResponse<object>.ErrorResponse(
                new List<string> { "Funcionalidad pendiente de implementación" },
                "Obtener usuarios no implementado",
                StatusCodes.Status501NotImplemented));
    }

    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> GetUsuario(Guid id)
    {
        _logger.LogInformation("👤 GET /api/core/usuarios/{Id}", id);

        // TODO: Implementar query para obtener usuario por ID
        return StatusCode(StatusCodes.Status501NotImplemented,
            ApiResponse<object>.ErrorResponse(
                new List<string> { "Funcionalidad pendiente de implementación" },
                "Obtener usuario por ID no implementado",
                StatusCodes.Status501NotImplemented));
    }

    /// <summary>
    /// Crea un nuevo usuario
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> CrearUsuario([FromBody] CrearUsuarioCommand command)
    {
        _logger.LogInformation("➕ POST /api/core/usuarios - Creando usuario: {Email}", command.Email);

        var result = await _mediator.Send(command);

        if (result.Succeeded)
        {
            var response = ApiResponse<UsuarioDto>.SuccessResponse(result.Value, "Usuario creado exitosamente");
            response.StatusCode = StatusCodes.Status201Created;
            
            return CreatedAtAction(
                nameof(GetUsuario),
                new { id = result.Value.Id },
                response);
        }

        return BadRequest(ApiResponse<object>.ErrorResponse(
            new List<string> { result.Error },
            "Error al crear usuario",
            StatusCodes.Status400BadRequest));
    }

    /// <summary>
    /// Actualiza un usuario existente (Pendiente de implementación)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status501NotImplemented)]
    public async Task<ActionResult<ApiResponse<object>>> ActualizarUsuario(Guid id)
    {
        _logger.LogInformation("✏️ PUT /api/core/usuarios/{Id}", id);

        // TODO: Implementar actualización de usuario
        return StatusCode(StatusCodes.Status501NotImplemented,
            ApiResponse<object>.ErrorResponse(
                new List<string> { "Funcionalidad pendiente de implementación" },
                "Actualizar usuario no implementado",
                StatusCodes.Status501NotImplemented));
    }

    /// <summary>
    /// Elimina un usuario (Pendiente de implementación)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status501NotImplemented)]
    public async Task<ActionResult<ApiResponse<object>>> EliminarUsuario(Guid id)
    {
        _logger.LogInformation("🗑️ DELETE /api/core/usuarios/{Id}", id);

        // TODO: Implementar eliminación de usuario
        return StatusCode(StatusCodes.Status501NotImplemented,
            ApiResponse<object>.ErrorResponse(
                new List<string> { "Funcionalidad pendiente de implementación" },
                "Eliminar usuario no implementado",
                StatusCodes.Status501NotImplemented));
    }

    /// <summary>
    /// Obtiene el perfil del usuario actual
    /// </summary>
    [HttpGet("perfil")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> ObtenerPerfilActual()
    {
        _logger.LogInformation("👤 GET /api/core/usuarios/perfil");

        // TODO: Implementar obtener usuario actual del token JWT
        return StatusCode(StatusCodes.Status501NotImplemented,
            ApiResponse<object>.ErrorResponse(
                new List<string> { "Funcionalidad pendiente de implementación" },
                "Obtener perfil actual no implementado",
                StatusCodes.Status501NotImplemented));
    }
} 