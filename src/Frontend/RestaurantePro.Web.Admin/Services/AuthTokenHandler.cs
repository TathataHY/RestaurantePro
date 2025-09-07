using System.Net.Http.Headers;

namespace RestaurantePro.Web.Admin.Services;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly TokenStore _tokenStore;

    public AuthTokenHandler(TokenStore tokenStore)
    {
        _tokenStore = tokenStore;
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
        return await base.SendAsync(request, cancellationToken);
    }
}

public class TokenStore
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string? RefreshToken { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token) && DateTime.UtcNow < Expiration;
    public void Clear()
    {
        Token = string.Empty;
        Expiration = DateTime.MinValue;
        RefreshToken = null;
        UserName = string.Empty;
        Roles.Clear();
    }
}


