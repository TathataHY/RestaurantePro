using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Platform;
using RestaurantePro.Mobile.Core.Services.Navigation;

namespace RestaurantePro.Mobile.Core.Services.Authentication;

/// <summary>
/// Servicio de autenticación usando JWT tokens
/// </summary>
public class AuthService : IAuthService
{
    private readonly IApiService _apiService;
    private readonly ILogger<AuthService> _logger;
    private readonly ISecureStorageService _secureStorage;
    private readonly INavigationService _navigationService;
    
    private const string TokenKey = "auth_token";
    private const string UserKey = "auth_user";
    private const string RecordarmeKey = "auth_recordarme";
    private const string RefreshTokenKey = "auth_refresh_token";
    
    private AuthUser? _currentUser;
    private string? _currentToken;

    public AuthService(IApiService apiService, ILogger<AuthService> logger, ISecureStorageService secureStorage, INavigationService navigationService)
    {
        _apiService = apiService;
        _logger = logger;
        _secureStorage = secureStorage;
        _navigationService = navigationService;
    }

    /// <summary>
    /// Realiza el login del usuario
    /// </summary>
    public async Task<ApiResponse<AuthResponse>> LoginAsync(string email, string password, bool recordarme = false)
    {
        try
        {
            // DEBUG: Comentado para flujo normal - descomentar solo si hay problemas
            // ShowDebugPopup("🔐 Iniciando Login", $"Email: {email}\nPassword: {new string('*', password.Length)}");
            
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
                Password = password,
                Recordarme = recordarme
            };

            // DEBUG: Comentado para flujo normal - descomentar solo si hay problemas
            // ShowDebugPopup("📡 Llamando endpoint", "api/auth/login");

            var apiResponse = await _apiService.PostAsync<AuthResponse>("api/auth/login", loginRequest);

            // DEBUG: Comentado para flujo normal - descomentar solo si hay problemas
            // if (apiResponse != null)
            // {
            //     ShowDebugPopup("📥 Respuesta API", $"Success: {apiResponse.Success}\nMessage: {apiResponse.Message}\nErrors: {string.Join(", ", apiResponse.Errors ?? new List<string>())}");
            // }

            if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
            {
                // Guardar token y usuario
                _currentToken = apiResponse.Data.Token;
                _currentUser = apiResponse.Data.User;
                
                // Guardar en preferencias
                await SaveTokenAsync(apiResponse.Data.Token);
                await SaveUserAsync(apiResponse.Data.User);
                await SaveRecordarmeAsync(recordarme);
                
                // Guardar refresh token si está disponible
                if (!string.IsNullOrEmpty(apiResponse.Data.RefreshToken))
                {
                    await SaveRefreshTokenAsync(apiResponse.Data.RefreshToken);
                }
                
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
            // DEBUG: Comentado para flujo normal - descomentar solo si hay problemas
            // ShowDebugPopup("💥 EXCEPCIÓN en Login", $"Tipo: {ex.GetType().Name}\nMensaje: {ex.Message}\nStackTrace: {ex.StackTrace}");
            
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
                // Verificar si el token está expirado
                if (IsTokenExpired(_currentToken))
                {
                    _logger.LogInformation("Token expirado, intentando renovar...");
                    var renewed = await TryRefreshTokenAsync();
                    if (renewed)
                    {
                        return _currentToken;
                    }
                    else
                    {
                        // Si no se pudo renovar, limpiar sesión
                        await HandleTokenExpiredAsync();
                        return null;
                    }
                }
                return _currentToken;
            }

            // Intentar obtener de las preferencias
            var token = await _secureStorage.GetAsync(TokenKey);
            
            if (!string.IsNullOrEmpty(token))
            {
                // Verificar si el token está expirado
                if (IsTokenExpired(token))
                {
                    _logger.LogInformation("Token expirado, intentando renovar...");
                    var renewed = await TryRefreshTokenAsync();
                    if (renewed)
                    {
                        return _currentToken;
                    }
                    else
                    {
                        // Si no se pudo renovar, limpiar sesión
                        await HandleTokenExpiredAsync();
                        return null;
                    }
                }
                
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
    /// Obtiene el UserId (claim sub/userid) desde el JWT actual
    /// </summary>
    public async Task<string?> GetUserIdAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return null;

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var userId = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                         ?? jwt.Claims.FirstOrDefault(c => c.Type == "userid")?.Value
                         ?? jwt.Claims.FirstOrDefault(c => c.Type.EndsWith("/nameidentifier"))?.Value;
            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener UserId del token");
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
            await _secureStorage.RemoveAsync(RecordarmeKey);
            await _secureStorage.RemoveAsync(RefreshTokenKey);

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



    /// <summary>
    /// Verifica si el token JWT está expirado
    /// </summary>
    private bool IsTokenExpired(string token)
    {
        try
        {
            // Decodificar el token JWT para obtener la fecha de expiración
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            
            // Considerar un margen (skew) para renovar antes de que expire definitivamente
            var safetySkew = TimeSpan.FromSeconds(60);
            return jsonToken.ValidTo <= DateTime.UtcNow.Add(safetySkew);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar expiración del token");
            return true; // Si hay error, asumir que está expirado
        }
    }

    /// <summary>
    /// Intenta renovar el token usando el refresh token
    /// </summary>
    private async Task<bool> TryRefreshTokenAsync()
    {
        try
        {
            // Verificar si el usuario tenía "Recordarme" activado
            var recordarmeStr = await _secureStorage.GetAsync(RecordarmeKey);
            var recordarme = bool.TryParse(recordarmeStr, out var result) && result;

            if (!recordarme)
            {
                _logger.LogInformation("Usuario no tiene 'Recordarme' activado - no se puede renovar token");
                return false;
            }

            // Obtener el refresh token
            var refreshToken = await _secureStorage.GetAsync(RefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("No hay refresh token disponible");
                return false;
            }

            // Obtener el token actual
            var currentToken = _currentToken ?? await _secureStorage.GetAsync(TokenKey);
            if (string.IsNullOrEmpty(currentToken))
            {
                _logger.LogWarning("No hay token actual disponible");
                return false;
            }

            // Llamar al endpoint de refresh
            var refreshRequest = new
            {
                Token = currentToken,
                RefreshToken = refreshToken
            };

            var response = await _apiService.PostAsync<AuthResponse>("api/auth/refresh", refreshRequest);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Token renovado exitosamente");
                
                // Actualizar el token y refresh token
                _currentToken = response.Data.Token;
                await SaveTokenAsync(response.Data.Token);
                
                if (!string.IsNullOrEmpty(response.Data.RefreshToken))
                {
                    await SaveRefreshTokenAsync(response.Data.RefreshToken);
                }
                
                return true;
            }
            else
            {
                _logger.LogWarning("Error renovando token: {Error}", response.Error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al intentar renovar token");
            return false;
        }
    }

    /// <summary>
    /// Maneja cuando el token está expirado
    /// </summary>
    private async Task HandleTokenExpiredAsync()
    {
        try
        {
            _logger.LogInformation("Token expirado - limpiando sesión");
            // Limpiar la sesión actual
            await LogoutAsync();
            // Navegación suave al login
            try
            {
                await _navigationService.NavigateToAsync("//login");
            }
            catch (Exception navEx)
            {
                _logger.LogWarning(navEx, "No se pudo navegar automáticamente al login tras expiración de token");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al manejar token expirado");
            await LogoutAsync();
        }
    }

    /// <summary>
    /// Muestra un popup de debug con información detallada
    /// </summary>
    private void ShowDebugPopup(string title, string message)
    {
#if DEBUG
        try
        {
            // Llamar al DebugService usando reflection ya que está en otro proyecto
            var debugServiceType = Type.GetType("RestaurantePro.Mobile.Services.DebugService, RestaurantePro.Mobile");
            if (debugServiceType != null)
            {
                var method = debugServiceType.GetMethod("ShowDebugPopup", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                method?.Invoke(null, new object[] { title, message });
            }
        }
        catch { /* Ignorar errores si no está disponible */ }
#endif
    }

    /// <summary>
    /// Guarda la preferencia de "Recordarme"
    /// </summary>
    private async Task SaveRecordarmeAsync(bool recordarme)
    {
        try
        {
            await _secureStorage.SetAsync(RecordarmeKey, recordarme.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar preferencia de Recordarme");
        }
    }

    /// <summary>
    /// Guarda el refresh token
    /// </summary>
    private async Task SaveRefreshTokenAsync(string refreshToken)
    {
        try
        {
            await _secureStorage.SetAsync(RefreshTokenKey, refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar refresh token");
        }
    }
} 