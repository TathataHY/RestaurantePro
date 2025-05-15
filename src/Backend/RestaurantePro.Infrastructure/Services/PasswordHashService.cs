using BC = BCrypt.Net.BCrypt;
using RestaurantePro.Domain.Interfaces.Services;

namespace RestaurantePro.Infrastructure.Services
{
    public class PasswordHashService : IPasswordHashService
    {
        public string HashPassword(string password)
        {
            return BC.HashPassword(password, BC.GenerateSalt(12));
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BC.Verify(password, hash);
        }
    }
} 