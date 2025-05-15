using System.Threading.Tasks;

namespace RestaurantePro.Domain.Interfaces.Services
{
    public interface IIdentityService
    {
        Task<(bool Success, string UserId, string Message)> CreateUserAsync(string nombre, string apellido, string email, string password, string rol);
        Task<(bool Success, string UserId, string Message)> ValidateUserAsync(string email, string password);
        Task<bool> UserExistsAsync(string email);
        Task<string> GetUserNameAsync(string userId);
        Task<bool> IsInRoleAsync(string userId, string role);
        Task<bool> AuthorizeAsync(string userId, string policyName);
    }
} 