using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Core.Usuarios.Commands.CrearUsuario;
using RestaurantePro.Application.Core.Usuarios.Commands.ActualizarUsuario;
using RestaurantePro.Application.Core.Usuarios.Commands.EliminarUsuario;
using RestaurantePro.Application.Core.Usuarios.Commands.CambiarRolUsuario;
using RestaurantePro.Application.Core.Usuarios.Commands.ResetPasswordUsuario;
using RestaurantePro.Application.Core.Usuarios.Queries.ObtenerUsuariosPaginados;
using RestaurantePro.Application.Core.Usuarios.Queries.ObtenerUsuarioPorId;
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
    [Authorize(Roles = "Administrador,Gerente")]
    [ProducesResponseType(typeof(ApiResponse<List<UsuarioDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<UsuarioDto>>>> GetUsuarios(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? filtro = null,
        [FromQuery] bool soloActivos = true,
        [FromQuery] string orderBy = "NombreCompleto",
        [FromQuery] string orderDirection = "asc")
    {
        _logger.LogInformation("👥 GET /api/core/usuarios - Página: {PageNumber}, Tamaño: {PageSize}", pageNumber, pageSize);

        var query = new ObtenerUsuariosPaginadosQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Filtro = filtro,
            SoloActivos = soloActivos,
            OrderBy = orderBy,
            OrderDirection = orderDirection
        };

        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Error al obtener usuarios",
                StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<List<UsuarioDto>>.SuccessResponse(
            result.Value, "Usuarios obtenidos exitosamente");
        return Ok(response);
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

        var query = ObtenerUsuarioPorIdQuery.Create(id);
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Usuario no encontrado",
                StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<UsuarioDto>.SuccessResponse(
            result.Value, "Usuario obtenido exitosamente");
        return Ok(response);
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
            new List<string> { result.Error ?? "Error desconocido" },
            "Error al crear usuario",
            StatusCodes.Status400BadRequest));
    }

    /// <summary>
    /// Actualiza un usuario existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> ActualizarUsuario(Guid id, [FromBody] ActualizarUsuarioCommand command)
    {
        _logger.LogInformation("✏️ PUT /api/core/usuarios/{Id}", id);

        // Asignar el ID de la URL al comando
        command.UsuarioId = id;

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var statusCode = result.Errors.Any(e => e.Contains("no encontrado")) 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al actualizar usuario", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<UsuarioDto>.SuccessResponse(
            result.Value, "Usuario actualizado exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Elimina un usuario
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> EliminarUsuario(Guid id)
    {
        _logger.LogInformation("🗑️ DELETE /api/core/usuarios/{Id}", id);

        // TODO: Obtener el ID del usuario actual del token JWT
        var usuarioEliminadorId = Guid.NewGuid(); // Temporal para testing

        var command = EliminarUsuarioCommand.Create(id, usuarioEliminadorId);
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Error al eliminar usuario",
                StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<bool>.SuccessResponse(
            result.Value, "Usuario eliminado exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene el perfil del usuario actual
    /// </summary>
    [HttpGet("perfil")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> ObtenerPerfilActual()
    {
        _logger.LogInformation("👤 GET /api/core/usuarios/perfil");

        // TODO: Obtener el ID del usuario actual del token JWT
        var usuarioId = Guid.NewGuid(); // Temporal para testing

        var query = ObtenerUsuarioPorIdQuery.Create(usuarioId);
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Perfil no encontrado",
                StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<UsuarioDto>.SuccessResponse(
            result.Value, "Perfil obtenido exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Cambia el rol de un usuario
    /// </summary>
    [HttpPost("{id:guid}/cambiar-rol")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> CambiarRol(Guid id, [FromBody] CambiarRolUsuarioCommand command)
    {
        _logger.LogInformation("🔄 POST /api/core/usuarios/{Id}/cambiar-rol", id);

        // Asignar el ID de la URL al comando
        command.Id = id;

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var statusCode = result.Error?.Contains("no encontrado") == true
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" }, "Error al cambiar rol", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<UsuarioDto>.SuccessResponse(
            result.Value, "Rol cambiado exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Resetea la contraseña de un usuario
    /// </summary>
    [HttpPost("{id:guid}/reset-password")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword(Guid id)
    {
        _logger.LogInformation("🔑 POST /api/core/usuarios/{Id}/reset-password", id);

        // TODO: Obtener el ID del usuario actual del token JWT
        var usuarioReseteadorId = Guid.NewGuid(); // Temporal para testing

        var command = ResetPasswordUsuarioCommand.Create(id, usuarioReseteadorId);
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new List<string> { result.Error ?? "Error desconocido" },
                "Error al resetear contraseña",
                StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<bool>.SuccessResponse(
            result.Value, "Contraseña reseteada exitosamente");
        return Ok(response);
    }
} 