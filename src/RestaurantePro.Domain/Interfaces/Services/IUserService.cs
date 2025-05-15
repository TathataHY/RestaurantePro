using RestaurantePro.Domain.Entities;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Interfaces.Services
{
    public interface IUserService
    {
        Task<ApplicationUser> GetUserAsync(string userId);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
    }
} 