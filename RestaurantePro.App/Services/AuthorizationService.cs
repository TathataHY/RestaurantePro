using RestaurantePro.App.Models;
using Microsoft.Maui.Storage;

namespace RestaurantePro.App.Services
{
    public class AuthorizationService
    {
        private readonly DatabaseService _databaseService;

        public AuthorizationService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<Usuario> GetCurrentUserAsync()
        {
            var userId = Preferences.Get("UserId", 0);
            if (userId == 0)
                return null;

            return await _databaseService.GetUsuarioByIdAsync(userId);
        }

        public async Task<bool> IsUserInRoleAsync(RolUsuario role)
        {
            var user = await GetCurrentUserAsync();
            return user != null && user.Rol == role;
        }

        public async Task<bool> IsUserAuthorizedAsync(params RolUsuario[] roles)
        {
            var user = await GetCurrentUserAsync();
            return user != null && roles.Contains(user.Rol);
        }
    }
}