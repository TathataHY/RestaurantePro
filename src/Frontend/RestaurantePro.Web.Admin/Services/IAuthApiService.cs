using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IAuthApiService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthUserDto?> GetProfileAsync();
    Task<bool> LogoutAsync();
    Task<bool> RefreshTokenAsync();
    Task<AuthResponse?> RefreshTokenAsync(string refreshToken);
}
