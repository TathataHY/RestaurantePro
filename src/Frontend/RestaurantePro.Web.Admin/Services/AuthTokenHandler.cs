using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace RestaurantePro.Web.Admin.Services;

public class AuthData
{
    public string Token { get; set; } = string.Empty;
    public string Expiration { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public string? UserName { get; set; }
    public string? UserId { get; set; }
    public string? DomainUserId { get; set; }
    public string[]? Roles { get; set; }
}

public class AuthTokenHandler : DelegatingHandler
{
    private readonly TokenStore _tokenStore;
    private readonly IAuthApiService? _authService;

    public AuthTokenHandler(TokenStore tokenStore, IAuthApiService authService)
    {
        _tokenStore = tokenStore;
        _authService = authService;
    }

    // Constructor para pruebas unitarias
    public AuthTokenHandler()
    {
        _tokenStore = new TokenStore();
        _authService = null;
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
            !request.RequestUri!.AbsolutePath.Contains("/auth/") &&
            _authService != null)
        {
            var refreshResult = await _authService.RefreshTokenAsync(_tokenStore.RefreshToken);
            if (refreshResult != null)
            {
                // Actualizar el token en el store
                await _tokenStore.SetAuthAsync(
                    refreshResult.Token,
                    refreshResult.Expiration,
                    refreshResult.RefreshToken,
                    _tokenStore.UserName,
                    _tokenStore.UserId,
                    _tokenStore.DomainUserId,
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
                await _tokenStore.ClearAsync();
            }
        }

        return response;
    }
}

public class TokenStore
{
    private readonly IJSRuntime? _jsRuntime;
    private bool _isInitialized = false;

    public TokenStore(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    // Constructor para pruebas unitarias
    public TokenStore()
    {
        _jsRuntime = null;
    }

    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string? RefreshToken { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? DomainUserId { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token) && Expiration > DateTime.Now;

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            // Si no hay jsRuntime (modo prueba), no hacer nada
            if (_jsRuntime == null)
            {
                _isInitialized = true;
                return;
            }
            
            // Esperar un poco para asegurar que JavaScript esté listo
            await Task.Delay(100);
            
            var authData = await _jsRuntime.InvokeAsync<AuthData?>("authPersistence.loadAuth");
            
            if (authData != null)
            {
                Token = authData.Token;
                if (DateTime.TryParse(authData.Expiration, out var exp))
                {
                    Expiration = exp;
                }
                RefreshToken = authData.RefreshToken;
                UserName = authData.UserName ?? string.Empty;
                UserId = authData.UserId ?? string.Empty;
                DomainUserId = authData.DomainUserId;
                Roles = authData.Roles?.ToList() ?? new List<string>();
            }
        }
        catch (Exception ex)
        {
            // Si hay error al acceder a localStorage, continuar con valores por defecto
        }

        _isInitialized = true;
    }

    public async Task SetAuthAsync(string token, DateTime expiration, string? refreshToken, string userName, string userId, string? domainUserId, List<string> roles)
    {
        Token = token;
        Expiration = expiration;
        RefreshToken = refreshToken;
        UserName = userName;
        UserId = userId;
        DomainUserId = domainUserId;
        Roles = roles;

        try
        {
            await _jsRuntime.InvokeVoidAsync("authPersistence.saveAuth", token, expiration.ToString("O"), refreshToken, userName, userId, domainUserId, roles.ToArray());
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
        UserId = string.Empty;
        DomainUserId = null;
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
        UserId = string.Empty;
        DomainUserId = null;
        Roles.Clear();
    }
}


