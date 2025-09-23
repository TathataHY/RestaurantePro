using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Platform;
using RestaurantePro.Mobile.Core.Services.Navigation;
using AuthResponse = RestaurantePro.Mobile.Core.Models.DTOs.AuthResponse; // Forzar el uso de la clase correcta

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

            // 🔍 DEBUG: Descomentando para ver el problema
            System.Diagnostics.Debug.WriteLine($"🔍📡 [AuthService] === RESPUESTA API ===");
            System.Diagnostics.Debug.WriteLine($"🔍📡 [AuthService] apiResponse: {(apiResponse != null ? "✅ NO NULL" : "❌ NULL")}");
            
            if (apiResponse != null)
            {
                System.Diagnostics.Debug.WriteLine($"🔍📡 [AuthService] Success: {apiResponse.Success}");
                System.Diagnostics.Debug.WriteLine($"🔍📡 [AuthService] Message: {apiResponse.Message}");
                System.Diagnostics.Debug.WriteLine($"🔍📡 [AuthService] Errors: [{string.Join(", ", apiResponse.Errors ?? new List<string>())}]");
                System.Diagnostics.Debug.WriteLine($"🔍📡 [AuthService] Data: {(apiResponse.Data != null ? "✅ NO NULL" : "❌ NULL")}");
                
                // Si hay Data, mostrar detalles
                if (apiResponse.Data != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🔍📡 [AuthService] Data.Token: {(!string.IsNullOrEmpty(apiResponse.Data.Token) ? "✅ SÍ" : "❌ NO/EMPTY")}");
                    System.Diagnostics.Debug.WriteLine($"🔍📡 [AuthService] Data.User: {(apiResponse.Data.User != null ? "✅ NO NULL" : "❌ NULL")}");
                }
            }
            System.Diagnostics.Debug.WriteLine($"🔍📡 [AuthService] === FIN RESPUESTA ===");

            if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
            {
                // DEBUG DEL LOGIN
                System.Diagnostics.Debug.WriteLine($"🔍🚀 [AuthService] Login exitoso!");
                System.Diagnostics.Debug.WriteLine($"🔍🚀 [AuthService] Token recibido: {(!string.IsNullOrEmpty(apiResponse.Data.Token) ? "SÍ" : "NO")}");
                System.Diagnostics.Debug.WriteLine($"🔍🚀 [AuthService] Usuario recibido: {(apiResponse.Data.User != null ? apiResponse.Data.User.Email : "NULL")}");
                System.Diagnostics.Debug.WriteLine($"🔍🚀 [AuthService] Roles del usuario: [{string.Join(", ", apiResponse.Data.User?.Roles ?? new List<string>())}]");
                
                // Guardar token y usuario
                _currentToken = apiResponse.Data.Token;
                _currentUser = apiResponse.Data.User;
                
                System.Diagnostics.Debug.WriteLine($"🔍💾 [AuthService] Guardando en memoria: Token={!string.IsNullOrEmpty(_currentToken)}, User={_currentUser?.Email}");
                
                // Guardar en preferencias
                System.Diagnostics.Debug.WriteLine($"🔍💾 [AuthService] Guardando token en SecureStorage...");
                await SaveTokenAsync(apiResponse.Data.Token);
                
                System.Diagnostics.Debug.WriteLine($"🔍💾 [AuthService] Guardando usuario en SecureStorage...");
                await SaveUserAsync(apiResponse.Data.User);
                
                System.Diagnostics.Debug.WriteLine($"🔍💾 [AuthService] Guardando recordarme...");
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
                "Error de autenticación",
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
    /// Verifica si el usuario está autenticado (incluyendo validación de token)
    /// </summary>
    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            // 🔐 MEJORA: Usar EnsureValidTokenAsync para verificar token válido
            return await EnsureValidTokenAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando autenticación");
            System.Diagnostics.Debug.WriteLine($"❌ [IsAuthenticated] Error: {ex.Message}");
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
            // DEBUG DETALLADO
            System.Diagnostics.Debug.WriteLine($"🔍👤 [AuthService] GetCurrentUserAsync iniciando...");
            System.Diagnostics.Debug.WriteLine($"🔍👤 [AuthService] _currentUser en memoria: {(_currentUser == null ? "NULL" : _currentUser.Email)}");
            
            if (_currentUser != null)
            {
                System.Diagnostics.Debug.WriteLine($"🔍✅ [AuthService] Devolviendo usuario de memoria: {_currentUser.Email}, Roles: [{string.Join(", ", _currentUser.Roles ?? new List<string>())}]");
                return _currentUser;
            }

            // Intentar obtener de las preferencias
            System.Diagnostics.Debug.WriteLine($"🔍💾 [AuthService] Buscando en SecureStorage con clave: {UserKey}");
            var userJson = await _secureStorage.GetAsync(UserKey);
            
            System.Diagnostics.Debug.WriteLine($"🔍💾 [AuthService] JSON obtenido: {(string.IsNullOrEmpty(userJson) ? "VACÍO/NULL" : $"[{userJson.Length} chars] {userJson.Substring(0, Math.Min(100, userJson.Length))}...")}");
            
            if (!string.IsNullOrEmpty(userJson))
            {
                System.Diagnostics.Debug.WriteLine($"🔍🔧 [AuthService] Deserializando JSON...");
                _currentUser = System.Text.Json.JsonSerializer.Deserialize<AuthUser>(userJson);
                
                if (_currentUser != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🔍✅ [AuthService] Usuario deserializado: {_currentUser.Email}, Roles: [{string.Join(", ", _currentUser.Roles ?? new List<string>())}]");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"🔍❌ [AuthService] Deserialización resultó en NULL");
                }
                
                return _currentUser;
            }

            System.Diagnostics.Debug.WriteLine($"🔍❌ [AuthService] NO hay usuario en SecureStorage - devolviendo NULL");
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
            System.Diagnostics.Debug.WriteLine($"🔍💾✏️ [SaveUser] Iniciando guardado...");
            System.Diagnostics.Debug.WriteLine($"🔍💾✏️ [SaveUser] Usuario: {user?.Email ?? "NULL"}");
            System.Diagnostics.Debug.WriteLine($"🔍💾✏️ [SaveUser] Roles: [{string.Join(", ", user?.Roles ?? new List<string>())}]");
            
            var userJson = System.Text.Json.JsonSerializer.Serialize(user);
            System.Diagnostics.Debug.WriteLine($"🔍💾✏️ [SaveUser] JSON serializado: [{userJson.Length} chars] {userJson.Substring(0, Math.Min(200, userJson.Length))}...");
            
            await _secureStorage.SetAsync(UserKey, userJson);
            System.Diagnostics.Debug.WriteLine($"🔍💾✅ [SaveUser] Guardado exitoso en SecureStorage con clave: {UserKey}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar usuario");
            System.Diagnostics.Debug.WriteLine($"🔍💾❌ [SaveUser] ERROR: {ex.Message}");
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
            
            // 🔄 PRODUCCIÓN: Margen de 5 minutos para renovación automática (token dura 60 min)
            var safetySkew = TimeSpan.FromMinutes(5);
            var expiresAt = jsonToken.ValidTo;
            var now = DateTime.UtcNow;
            var renewAt = now.Add(safetySkew);
            var isExpired = expiresAt <= renewAt;
            
            // 📝 LOGGING MEJORADO para debug
            System.Diagnostics.Debug.WriteLine($"🕐 [TokenExpiry] Token expira: {expiresAt:yyyy-MM-dd HH:mm:ss} UTC");
            System.Diagnostics.Debug.WriteLine($"🕐 [TokenExpiry] Hora actual: {now:yyyy-MM-dd HH:mm:ss} UTC");
            System.Diagnostics.Debug.WriteLine($"🕐 [TokenExpiry] Renovar en: {renewAt:yyyy-MM-dd HH:mm:ss} UTC");
            System.Diagnostics.Debug.WriteLine($"🕐 [TokenExpiry] ¿Necesita renovación?: {isExpired}");
            
            return isExpired;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar expiración del token");
            System.Diagnostics.Debug.WriteLine($"❌ [TokenExpiry] Error verificando token: {ex.Message}");
            // Si no es un JWT válido, asumir expirado para forzar re-login
            return true;
        }
    }

    /// <summary>
    /// Intenta renovar el token usando el refresh token
    /// </summary>
    private async Task<bool> TryRefreshTokenAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔄 [RefreshToken] Iniciando proceso de renovación...");
            
            // Verificar si el usuario tenía "Recordarme" activado
            var recordarmeStr = await _secureStorage.GetAsync(RecordarmeKey);
            var recordarme = bool.TryParse(recordarmeStr, out var result) && result;

            System.Diagnostics.Debug.WriteLine($"🔄 [RefreshToken] Recordarme: {recordarme}");

            if (!recordarme)
            {
                _logger.LogInformation("Usuario no tiene 'Recordarme' activado - no se puede renovar token");
                System.Diagnostics.Debug.WriteLine($"❌ [RefreshToken] Sin 'Recordarme' - renovación cancelada");
                return false;
            }

            // Obtener el refresh token
            var refreshToken = await _secureStorage.GetAsync(RefreshTokenKey);
            System.Diagnostics.Debug.WriteLine($"🔄 [RefreshToken] RefreshToken disponible: {!string.IsNullOrEmpty(refreshToken)}");
            
            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("No hay refresh token disponible");
                System.Diagnostics.Debug.WriteLine($"❌ [RefreshToken] Sin RefreshToken - renovación imposible");
                return false;
            }

            // Obtener el token actual
            var currentToken = _currentToken ?? await _secureStorage.GetAsync(TokenKey);
            System.Diagnostics.Debug.WriteLine($"🔄 [RefreshToken] Token actual disponible: {!string.IsNullOrEmpty(currentToken)}");
            
            if (string.IsNullOrEmpty(currentToken))
            {
                _logger.LogWarning("No hay token actual disponible");
                System.Diagnostics.Debug.WriteLine($"❌ [RefreshToken] Sin token actual - renovación imposible");
                return false;
            }

            // Llamar al endpoint de refresh
            var refreshRequest = new
            {
                Token = currentToken,
                RefreshToken = refreshToken
            };

            System.Diagnostics.Debug.WriteLine($"🔄 [RefreshToken] Llamando a api/auth/refresh...");
            var response = await _apiService.PostAsync<AuthResponse>("api/auth/refresh", refreshRequest);
            
            System.Diagnostics.Debug.WriteLine($"🔄 [RefreshToken] Respuesta recibida: Success={response?.Success}");
            
            if (response != null && response.Success && response.Data != null)
            {
                _logger.LogInformation("Token renovado exitosamente");
                System.Diagnostics.Debug.WriteLine($"✅ [RefreshToken] Renovación exitosa!");
                
                // Actualizar el token y refresh token
                _currentToken = response.Data.Token;
                await SaveTokenAsync(response.Data.Token);
                
                if (!string.IsNullOrEmpty(response.Data.RefreshToken))
                {
                    await SaveRefreshTokenAsync(response.Data.RefreshToken);
                    System.Diagnostics.Debug.WriteLine($"✅ [RefreshToken] Nuevo RefreshToken guardado");
                }
                
                return true;
            }
            else
            {
                var error = response?.Error ?? "Respuesta nula";
                _logger.LogWarning("Error renovando token: {Error}", error);
                System.Diagnostics.Debug.WriteLine($"❌ [RefreshToken] Error: {error}");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al intentar renovar token");
            System.Diagnostics.Debug.WriteLine($"❌ [RefreshToken] Excepción: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"🚪 [TokenExpired] Token expirado - iniciando limpieza...");
            
            _logger.LogInformation("Token expirado - limpiando sesión");
            
            // 🔒 MEJORA: Solo limpiar tokens, preservar "Recordarme" y email para UX
            var recordarme = await GetRecordarmeAsync();
            var user = await GetCurrentUserAsync();
            var email = user?.Email;
            
            System.Diagnostics.Debug.WriteLine($"🚪 [TokenExpired] Preservando: Recordarme={recordarme}, Email={email}");
            
            // Limpiar solo tokens, no las preferencias de usuario
            _currentToken = null;
            _currentUser = null;
            await _secureStorage.RemoveAsync(TokenKey);
            await _secureStorage.RemoveAsync(UserKey);
            await _secureStorage.RemoveAsync(RefreshTokenKey);
            
            // 💾 MEJORA: Preservar "Recordarme" para mejor UX
            if (recordarme && !string.IsNullOrEmpty(email))
            {
                System.Diagnostics.Debug.WriteLine($"🚪 [TokenExpired] Preservando configuración de 'Recordarme' para {email}");
                // El "Recordarme" ya está guardado, no necesitamos borrarlo
            }
            
            // Navegación suave al login
            try
            {
                System.Diagnostics.Debug.WriteLine($"🚪 [TokenExpired] Navegando al login...");
                await _navigationService.NavigateToAsync("//login");
            }
            catch (Exception navEx)
            {
                _logger.LogWarning(navEx, "No se pudo navegar automáticamente al login tras expiración de token");
                System.Diagnostics.Debug.WriteLine($"❌ [TokenExpired] Error navegando: {navEx.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al manejar token expirado");
            System.Diagnostics.Debug.WriteLine($"❌ [TokenExpired] Error crítico: {ex.Message}");
            // Fallback: logout completo
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

    /// <summary>
    /// Obtiene la preferencia de "Recordarme"
    /// </summary>
    public async Task<bool> GetRecordarmeAsync()
    {
        try
        {
            var recordarmeStr = await _secureStorage.GetAsync(RecordarmeKey);
            return bool.TryParse(recordarmeStr, out var result) && result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener preferencia de Recordarme");
            return false;
        }
    }

    /// <summary>
    /// Obtiene las credenciales guardadas si "Recordarme" está activado
    /// </summary>
    public async Task<(string? email, string? password, bool recordarme)> GetSavedCredentialsAsync()
    {
        try
        {
            var recordarme = await GetRecordarmeAsync();
            
            if (!recordarme)
            {
                return (null, null, false);
            }

            // Solo devolver email si "Recordarme" está activado
            var user = await GetCurrentUserAsync();
            var email = user?.Email;

            // Por seguridad, no guardamos la contraseña en texto plano
            // Solo devolvemos el email para pre-llenar el formulario
            return (email, null, recordarme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener credenciales guardadas");
            return (null, null, false);
        }
    }

    /// <summary>
    /// 🆕 MEJORA: Verifica proactivamente el estado del token y lo renueva si es necesario
    /// </summary>
    public async Task<bool> EnsureValidTokenAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔍 [EnsureValidToken] Verificando estado del token...");
            
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine($"❌ [EnsureValidToken] Sin token - usuario no autenticado");
                return false;
            }
            
            // Si GetTokenAsync() devolvió un token, significa que está válido o fue renovado exitosamente
            System.Diagnostics.Debug.WriteLine($"✅ [EnsureValidToken] Token válido o renovado exitosamente");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar validez del token");
            System.Diagnostics.Debug.WriteLine($"❌ [EnsureValidToken] Error: {ex.Message}");
            return false;
        }
    }
} 