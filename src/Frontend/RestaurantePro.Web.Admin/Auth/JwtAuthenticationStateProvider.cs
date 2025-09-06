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

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_tokenStore.IsAuthenticated)
        {
            var identity = BuildIdentityFromTokenStore();
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }

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


