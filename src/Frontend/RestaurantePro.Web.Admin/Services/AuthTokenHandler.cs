using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace RestaurantePro.Web.Admin.Services;

public class AuthData
{
    public string Token { get; set; } = string.Empty;
    public string Expiration { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public string? UserName { get; set; }
    public string[]? Roles { get; set; }
}

public class AuthTokenHandler : DelegatingHandler
{
    private readonly TokenStore _tokenStore;
    private readonly IAuthApiService _authService;

    public AuthTokenHandler(TokenStore tokenStore, IAuthApiService authService)
    {
        _tokenStore = tokenStore;
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _tokenStore.Token;
        if (!string.IsNullOrWhiteSpace(token))
        {
            // Si ya hay Authorization (por ejemplo Basic), no sobrescribir. Añadir solo X-Bearer-Token
            if (request.Headers.Authorization == null || string.IsNullOrWhiteSpace(request.Headers.Authorization.Scheme))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            if (!request.Headers.Contains("X-Bearer-Token"))
            {
                request.Headers.Add("X-Bearer-Token", token);
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Si recibimos un 401 y tenemos un refresh token, intentar renovar
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && 
            !string.IsNullOrWhiteSpace(_tokenStore.RefreshToken) &&
            !request.RequestUri!.AbsolutePath.Contains("/auth/"))
        {
            Console.WriteLine("🔑 Token expirado, intentando renovar...");
            
            var refreshResult = await _authService.RefreshTokenAsync(_tokenStore.RefreshToken);
            if (refreshResult != null)
            {
                Console.WriteLine("✅ Token renovado exitosamente");
                
                // Actualizar el token en el store
                await _tokenStore.SetAuthAsync(
                    refreshResult.Token,
                    refreshResult.Expiration,
                    refreshResult.RefreshToken,
                    _tokenStore.UserName,
                    _tokenStore.Roles
                );

                // Reintentar la solicitud original con el nuevo token
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshResult.Token);
                if (request.Headers.Contains("X-Bearer-Token"))
                {
                    request.Headers.Remove("X-Bearer-Token");
                }
                request.Headers.Add("X-Bearer-Token", refreshResult.Token);

                return await base.SendAsync(request, cancellationToken);
            }
            else
            {
                Console.WriteLine("❌ No se pudo renovar el token, limpiando autenticación");
                await _tokenStore.ClearAsync();
            }
        }

        return response;
    }
}

public class TokenStore
{
    private readonly IJSRuntime _jsRuntime;
    private bool _isInitialized = false;

    public TokenStore(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string? RefreshToken { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token) && Expiration > DateTime.UtcNow;

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            Console.WriteLine("TokenStore: Iniciando carga de autenticación...");
            
            // Esperar un poco para asegurar que JavaScript esté listo
            await Task.Delay(100);
            
            var authData = await _jsRuntime.InvokeAsync<AuthData?>("authPersistence.loadAuth");
            Console.WriteLine($"TokenStore: authData = {authData != null}");
            
            if (authData != null)
            {
                Token = authData.Token;
                if (DateTime.TryParse(authData.Expiration, out var exp))
                {
                    Expiration = exp;
                }
                RefreshToken = authData.RefreshToken;
                UserName = authData.UserName ?? string.Empty;
                Roles = authData.Roles?.ToList() ?? new List<string>();
                
                Console.WriteLine($"TokenStore: Token cargado - Token: {!string.IsNullOrWhiteSpace(Token)}");
                Console.WriteLine($"TokenStore: Expiration: {Expiration:yyyy-MM-dd HH:mm:ss} UTC");
                Console.WriteLine($"TokenStore: Now: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                Console.WriteLine($"TokenStore: IsAuthenticated: {IsAuthenticated}");
            }
            else
            {
                Console.WriteLine("TokenStore: No se encontraron datos de autenticación");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading auth from localStorage: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            // Si hay error al acceder a localStorage, continuar con valores por defecto
        }

        _isInitialized = true;
    }

    public async Task SetAuthAsync(string token, DateTime expiration, string? refreshToken, string userName, List<string> roles)
    {
        Token = token;
        Expiration = expiration;
        RefreshToken = refreshToken;
        UserName = userName;
        Roles = roles;

        try
        {
            await _jsRuntime.InvokeVoidAsync("authPersistence.saveAuth", token, expiration.ToString("O"), refreshToken, userName, roles.ToArray());
        }
        catch
        {
            // Si hay error al guardar en localStorage, continuar
        }
    }

    public async Task ClearAsync()
    {
        Token = string.Empty;
        Expiration = DateTime.MinValue;
        RefreshToken = null;
        UserName = string.Empty;
        Roles.Clear();

        try
        {
            await _jsRuntime.InvokeVoidAsync("authPersistence.clearAuth");
        }
        catch
        {
            // Si hay error al limpiar localStorage, continuar
        }
    }

    // Mantener método Clear() para compatibilidad
    public void Clear()
    {
        Token = string.Empty;
        Expiration = DateTime.MinValue;
        RefreshToken = null;
        UserName = string.Empty;
        Roles.Clear();
    }
}


