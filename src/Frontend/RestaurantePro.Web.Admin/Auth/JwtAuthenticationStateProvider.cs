using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Web.Admin.Services;

namespace RestaurantePro.Web.Admin.Auth;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly TokenStore _tokenStore;

    public JwtAuthenticationStateProvider(TokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await _tokenStore.InitializeAsync();
        
        if (_tokenStore.IsAuthenticated)
        {
            var identity = BuildIdentityFromTokenStore();
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _tokenStore.InitializeAsync();
            var identity = _tokenStore.IsAuthenticated ? BuildIdentityFromTokenStore() : new ClaimsIdentity();
            var authState = new AuthenticationState(new ClaimsPrincipal(identity));
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in InitializeAsync: {ex.Message}");
            // Si hay error, notificar estado no autenticado
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
        }
    }

    public async Task SetAuthAsync(AuthResponse auth)
    {
        await _tokenStore.SetAuthAsync(auth.Token, auth.Expiration, auth.RefreshToken, auth.UserName, auth.Roles);
        var identity = BuildIdentityFromTokenStore();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    // Mantener método síncrono para compatibilidad
    public void SetAuth(AuthResponse auth)
    {
        _tokenStore.Token = auth.Token;
        _tokenStore.Expiration = auth.Expiration;
        _tokenStore.RefreshToken = auth.RefreshToken;
        _tokenStore.UserName = auth.UserName;
        _tokenStore.Roles = auth.Roles;
        var identity = BuildIdentityFromTokenStore();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    public async Task LogoutAsync()
    {
        await _tokenStore.ClearAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }

    // Mantener método síncrono para compatibilidad
    public void Logout()
    {
        _tokenStore.Clear();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }

    private ClaimsIdentity BuildIdentityFromTokenStore()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, _tokenStore.UserName)
        };
        foreach (var role in _tokenStore.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        return new ClaimsIdentity(claims, "jwt");
    }
}


