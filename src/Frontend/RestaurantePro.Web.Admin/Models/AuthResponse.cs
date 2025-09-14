namespace RestaurantePro.Web.Admin.Models;

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime Expiration { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? DomainUserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}


