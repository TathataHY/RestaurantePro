using RestaurantePro.Mobile.Core.Models.DTOs;
using AuthResponse = RestaurantePro.Mobile.Core.Models.DTOs.AuthResponse; // Forzar el uso de la clase correcta

namespace RestaurantePro.Mobile.Core.Services.Authentication;

/// <summary>
/// Servicio de autenticación - V1 Fundamental
/// </summary>
public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> LoginAsync(string email, string password, bool recordarme = false);
    Task<string?> GetTokenAsync();
    Task<AuthUser?> GetCurrentUserAsync();
    /// <summary>
    /// Obtiene el UserId (GUID) desde el token JWT actual
    /// </summary>
    Task<string?> GetUserIdAsync();
    Task<bool> IsAuthenticatedAsync();
    Task LogoutAsync();
    
    /// <summary>
    /// Obtiene la preferencia de "Recordarme"
    /// </summary>
    Task<bool> GetRecordarmeAsync();
    
    /// <summary>
    /// Obtiene las credenciales guardadas si "Recordarme" está activado
    /// </summary>
    Task<(string? email, string? password, bool recordarme)> GetSavedCredentialsAsync();
    
    /// <summary>
    /// Verifica proactivamente el estado del token y lo renueva si es necesario
    /// </summary>
    Task<bool> EnsureValidTokenAsync();
} 