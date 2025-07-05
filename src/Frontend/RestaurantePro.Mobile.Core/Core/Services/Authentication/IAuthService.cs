using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Authentication;

/// <summary>
/// Servicio de autenticación - V1 Fundamental
/// </summary>
public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> LoginAsync(string email, string password);
    Task<string?> GetTokenAsync();
    Task<AuthUser?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
    Task LogoutAsync();
} 