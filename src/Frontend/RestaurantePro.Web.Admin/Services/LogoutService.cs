using RestaurantePro.Web.Admin.Auth;

namespace RestaurantePro.Web.Admin.Services;

public static class LogoutService
{
    private static IServiceScopeFactory? _serviceScopeFactory;

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    }

    public static async Task LogoutAsync()
    {
        if (_serviceScopeFactory == null)
        {
            return;
        }

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var authState = scope.ServiceProvider.GetRequiredService<JwtAuthenticationStateProvider>();
            
            // Limpiar autenticación
            await authState.LogoutAsync();
            
            // La navegación se manejará desde JavaScript
        }
        catch (Exception ex)
        {
            // Error silencioso - no mostrar logs de debug
        }
    }
}
