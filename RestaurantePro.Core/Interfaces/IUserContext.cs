using System.Security.Claims;

namespace RestaurantePro.Core.Interfaces
{
    public interface IUserContext
    {
        string CurrentUser { get; }
        string CurrentRole { get; }
        ClaimsPrincipal User { get; }
        bool IsInRole(string role);
    }
} 