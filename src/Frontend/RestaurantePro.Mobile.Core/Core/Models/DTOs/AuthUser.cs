namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// Modelo para el usuario autenticado
/// </summary>
public class AuthUser
{
    /// <summary>
    /// ID único del usuario (GUID)
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre de usuario
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Email del usuario
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica si el email está confirmado
    /// </summary>
    public bool EmailConfirmed { get; set; } = false;
    
    /// <summary>
    /// Nombre del usuario
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Apellido del usuario
    /// </summary>
    public string Apellido { get; set; } = string.Empty;
    
    /// <summary>
    /// Roles del usuario
    /// </summary>
    public List<string> Roles { get; set; } = new();
    
    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
    
    /// <summary>
    /// Indica si el usuario está activo
    /// </summary>
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Respuesta de autenticación
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// Token JWT
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Refresh token para renovar el token principal
    /// </summary>
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// Usuario autenticado
    /// </summary>
    public AuthUser User { get; set; } = new();
    
    /// <summary>
    /// Fecha de expiración del token
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Modelo para solicitud de login
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Email del usuario
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Contraseña del usuario
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica si el usuario quiere que se recuerde su sesión
    /// </summary>
    public bool Recordarme { get; set; } = false;
} 