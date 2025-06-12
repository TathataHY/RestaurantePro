namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Servicio para generar y validar tokens JWT para autenticación
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Genera un token JWT para un usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="userName">Nombre del usuario</param>
    /// <param name="email">Email del usuario</param>
    /// <param name="roles">Roles del usuario</param>
    /// <returns>Token JWT generado con su información</returns>
    JwtTokenResponse GenerateToken(string userId, string userName, string email, IList<string> roles);
}

/// <summary>
/// Respuesta con el token JWT generado y su información
/// </summary>
public class JwtTokenResponse
{
    /// <summary>
    /// Token de acceso JWT
    /// </summary>
    public string AccessToken { get; set; }
    
    /// <summary>
    /// Tipo de token (Bearer)
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
    
    /// <summary>
    /// Tiempo de expiración del token en segundos
    /// </summary>
    public int ExpiresIn { get; set; }
    
    /// <summary>
    /// Indica si se requiere renovación del token
    /// </summary>
    public bool RequiresRefresh { get; set; }
} 