using Microsoft.AspNetCore.Http;
using RestaurantePro.Core.Interfaces;
using System.Security.Claims;
using System.Linq;

namespace RestaurantePro.Infrastructure.Services
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string CurrentUser => 
            _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistema";

        public string CurrentRole
        {
            get
            {
                var roles = _httpContextAccessor.HttpContext?.User?.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .FirstOrDefault();
                return roles ?? "Invitado";
            }
        }

        public ClaimsPrincipal User => 
            _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();

        public bool IsInRole(string role) =>
            _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }
}