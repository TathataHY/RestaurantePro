using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Platform;

namespace RestaurantePro.Mobile.Core.Services.Authentication;

/// <summary>
/// Servicio de autenticación usando JWT tokens
/// </summary>
public class AuthService : IAuthService
{
    private readonly IApiService _apiService;
    private readonly ILogger<AuthService> _logger;
    private readonly ISecureStorageService _secureStorage;
    
    private const string TokenKey = "auth_token";
    private const string UserKey = "auth_user";
    
    private AuthUser? _currentUser;
    private string? _currentToken;

    public AuthService(IApiService apiService, ILogger<AuthService> logger, ISecureStorageService secureStorage)
    {
        _apiService = apiService;
        _logger = logger;
        _secureStorage = secureStorage;
    }

    /// <summary>
    /// Realiza el login del usuario
    /// </summary>
    public async Task<ApiResponse<AuthResponse>> LoginAsync(string email, string password)
    {
        try
        {
            // Validar parámetros
            if (string.IsNullOrWhiteSpace(email))
            {
                return ApiResponse<AuthResponse>.ErrorResponse("Email es requerido");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return ApiResponse<AuthResponse>.ErrorResponse("Password es requerido");
            }

            _logger.LogInformation("Iniciando proceso de login para usuario: {Email}", email);

            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            var apiResponse = await _apiService.PostAsync<AuthResponse>("api/auth/login", loginRequest);

            if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
            {
                // Guardar token y usuario
                _currentToken = apiResponse.Data.Token;
                _currentUser = apiResponse.Data.User;
                
                // Guardar en preferencias
                await SaveTokenAsync(apiResponse.Data.Token);
                await SaveUserAsync(apiResponse.Data.User);
                
                _logger.LogInformation("Login exitoso para usuario: {Email}", email);
                
                return ApiResponse<AuthResponse>.SuccessResponse(apiResponse.Data, "Login exitoso");
            }

            _logger.LogWarning("Login fallido para usuario: {Email}", email);
            return ApiResponse<AuthResponse>.ErrorResponse(
                apiResponse?.Errors ?? new List<string> { "Error de autenticación" },
                "Credenciales inválidas",
                401);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el proceso de login para usuario: {Email}", email);
            return ApiResponse<AuthResponse>.ErrorResponse(ex.Message, "Error interno del servidor", 500);
        }
    }

    /// <summary>
    /// Verifica si el usuario está autenticado
    /// </summary>
    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            return !string.IsNullOrEmpty(token);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Obtiene el token actual
    /// </summary>
    public async Task<string?> GetTokenAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(_currentToken))
            {
                return _currentToken;
            }

            // Intentar obtener de las preferencias
            var token = await _secureStorage.GetAsync(TokenKey);
            
            if (!string.IsNullOrEmpty(token))
            {
                _currentToken = token;
                return token;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener token");
            return null;
        }
    }

    /// <summary>
    /// Obtiene el usuario actual
    /// </summary>
    public async Task<AuthUser?> GetCurrentUserAsync()
    {
        try
        {
            if (_currentUser != null)
            {
                return _currentUser;
            }

            // Intentar obtener de las preferencias
            var userJson = await _secureStorage.GetAsync(UserKey);
            
            if (!string.IsNullOrEmpty(userJson))
            {
                _currentUser = System.Text.Json.JsonSerializer.Deserialize<AuthUser>(userJson);
                return _currentUser;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario actual");
            return null;
        }
    }

    /// <summary>
    /// Cierra la sesión del usuario
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            _logger.LogInformation("Cerrando sesión del usuario");

            // Limpiar datos en memoria
            _currentToken = null;
            _currentUser = null;

            // Limpiar datos guardados
            await _secureStorage.RemoveAsync(TokenKey);
            await _secureStorage.RemoveAsync(UserKey);

            _logger.LogInformation("Sesión cerrada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cerrar sesión");
        }
    }

    /// <summary>
    /// Guarda el token en el almacenamiento seguro
    /// </summary>
    private async Task SaveTokenAsync(string token)
    {
        try
        {
            await _secureStorage.SetAsync(TokenKey, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar token");
        }
    }

    /// <summary>
    /// Guarda el usuario en el almacenamiento seguro
    /// </summary>
    private async Task SaveUserAsync(AuthUser user)
    {
        try
        {
            var userJson = System.Text.Json.JsonSerializer.Serialize(user);
            await _secureStorage.SetAsync(UserKey, userJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar usuario");
        }
    }
} 