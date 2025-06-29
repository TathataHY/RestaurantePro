using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Api.Models.Requests;

namespace RestaurantePro.Api.Controllers.Core;

/// <summary>
/// Controlador para autenticación y gestión de usuarios
/// Contexto: Core
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IIdentityService identityService, ILogger<AuthController> logger)
    {
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Inicia sesión de un usuario
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("🔐 POST /api/auth/login - Usuario: {Email}", request.Email);

        try
        {
            var result = await _identityService.AuthenticateAsync(request.Email, request.Password);

            if (!result.Succeeded)
            {
                _logger.LogWarning("❌ Login fallido para usuario {Email}: {Error}", request.Email, result.Error);
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Credenciales inválidas", 
                    "Usuario o contraseña incorrectos",
                    StatusCodes.Status401Unauthorized));
            }

            _logger.LogInformation("✅ Login exitoso para usuario {Email}", request.Email);
            var response = ApiResponse<AuthResponse>.SuccessResponse(result.Value, "Inicio de sesión exitoso");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inesperado durante login para usuario {Email}", request.Email);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "Error interno del servidor", 
                "Error durante el proceso de autenticación",
                StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Registra un nuevo usuario
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<string>>> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("📝 POST /api/auth/register - Usuario: {Email}", request.Email);

        try
        {
            var result = await _identityService.RegisterAsync(
                request.Nombre, 
                request.Apellidos, 
                request.Email, 
                request.Username, 
                request.Password, 
                request.Rol);

            if (!result.Succeeded)
            {
                var errores = result.Errors?.ToList() ?? new List<string> { "Error desconocido" };
                var erroresStr = string.Join(" | ", errores);
                _logger.LogWarning("❌ Registro fallido para usuario {Email}: {Errores}", request.Email, erroresStr);
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    errores, 
                    "Error durante el registro",
                    StatusCodes.Status400BadRequest));
            }

            _logger.LogInformation("✅ Registro exitoso para usuario {Email} con ID {UserId}", request.Email, result.Value);
            var response = ApiResponse<string>.SuccessResponse(result.Value, "Usuario registrado exitosamente");
            response.StatusCode = StatusCodes.Status201Created;
            return CreatedAtAction(nameof(GetUserProfile), new { id = result.Value }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inesperado durante registro para usuario {Email}", request.Email);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                new List<string> { ex.Message },
                "Error durante el proceso de registro",
                StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Obtiene el perfil del usuario autenticado
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserProfile()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse(
                "Token inválido", 
                "No se pudo identificar al usuario",
                StatusCodes.Status401Unauthorized));
        }

        _logger.LogInformation("👤 GET /api/auth/profile - Usuario: {UserId}", userId);

        try
        {
            var user = await _identityService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Usuario no encontrado", 
                    "El usuario no existe o ha sido eliminado",
                    StatusCodes.Status404NotFound));
            }

            var response = ApiResponse<UserDto>.SuccessResponse(user, "Perfil obtenido exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo perfil para usuario {UserId}", userId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "Error interno del servidor", 
                "Error al obtener el perfil del usuario",
                StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Cambia la contraseña del usuario autenticado
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse(
                "Token inválido", 
                "No se pudo identificar al usuario",
                StatusCodes.Status401Unauthorized));
        }

        _logger.LogInformation("🔒 POST /api/auth/change-password - Usuario: {UserId}", userId);

        try
        {
            var result = await _identityService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                _logger.LogWarning("❌ Cambio de contraseña fallido para usuario {UserId}: {Error}", userId, result.Error);
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    result.Errors, 
                    "Error al cambiar la contraseña",
                    StatusCodes.Status400BadRequest));
            }

            _logger.LogInformation("✅ Contraseña cambiada exitosamente para usuario {UserId}", userId);
            var response = ApiResponse<bool>.SuccessResponse(true, "Contraseña cambiada exitosamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error cambiando contraseña para usuario {UserId}", userId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "Error interno del servidor", 
                "Error durante el cambio de contraseña",
                StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Cierra la sesión del usuario (logout)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> Logout()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userid")?.Value;
        _logger.LogInformation("🚪 POST /api/auth/logout - Usuario: {UserId}", userId);

        // En una implementación real, aquí invalidaríamos el token
        // Por ahora, solo devolvemos éxito
        var response = ApiResponse<bool>.SuccessResponse(true, "Sesión cerrada exitosamente");
        return Ok(response);
    }
} 