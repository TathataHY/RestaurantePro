namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// Modelo de usuario autenticado - V1 Fundamental
/// </summary>
public class AuthUser
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public List<string> Permisos { get; set; } = new();
    public DateTime FechaUltimoAcceso { get; set; }
}

/// <summary>
/// Respuesta de autenticación de la API
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime TokenExpiry { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public AuthUser User { get; set; } = new();
}

/// <summary>
/// Solicitud de login
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
} 