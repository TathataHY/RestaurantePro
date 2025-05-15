using RestaurantePro.Domain.Entities;

namespace RestaurantePro.Domain.Interfaces.Services
{
    public interface IJwtGenerator
    {
        string GenerateToken(ApplicationUser usuario);
    }
} 