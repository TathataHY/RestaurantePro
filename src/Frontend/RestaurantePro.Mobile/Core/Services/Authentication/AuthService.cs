using System.Text.Json;
using Microsoft.Maui.Storage;
using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Authentication;

/// <summary>
/// Implementación básica del servicio de autenticación - V1
/// </summary>
public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private const string TokenKey = "auth_token";
    private const string UserKey = "auth_user";

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(string email, string password)
    {
        try
        {
            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Endpoint según el backend de RestaurantePro
            var response = await _httpClient.PostAsync("api/core/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
                {
                    // Guardar token y usuario
                    await SaveTokenAsync(apiResponse.Data.Token);
                    await SaveUserAsync(apiResponse.Data.User);
                    
                    return apiResponse;
                }
            }

            return ApiResponse<AuthResponse>.ErrorResponse("Credenciales inválidas");
        }
        catch (Exception ex)
        {
            return ApiResponse<AuthResponse>.ErrorResponse($"Error de conexión: {ex.Message}");
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        await Task.Delay(1); // Para hacer async
        return Preferences.Get(TokenKey, null);
    }

    public async Task<AuthUser?> GetCurrentUserAsync()
    {
        await Task.Delay(1); // Para hacer async
        var userJson = Preferences.Get(UserKey, null);
        
        if (string.IsNullOrEmpty(userJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<AuthUser>(userJson);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    public async Task LogoutAsync()
    {
        await Task.Delay(1); // Para hacer async
        Preferences.Remove(TokenKey);
        Preferences.Remove(UserKey);
    }

    public async Task SaveTokenAsync(string token)
    {
        await Task.Delay(1); // Para hacer async
        Preferences.Set(TokenKey, token);
    }

    public async Task SaveUserAsync(AuthUser user)
    {
        await Task.Delay(1); // Para hacer async
        var userJson = JsonSerializer.Serialize(user);
        Preferences.Set(UserKey, userJson);
    }
} 