using RestaurantePro.Mobile.Core.Services.Navigation;

namespace RestaurantePro.Mobile.Services;

/// <summary>
/// Implementación real del servicio de navegación para MAUI
/// </summary>
public class NavigationService : INavigationService
{
    public async Task NavigateToAsync(string route)
    {
        try
        {
            if (Application.Current?.MainPage is Shell shell)
            {
                await shell.GoToAsync(route);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[NavigationService] No se puede navegar: MainPage no es Shell");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NavigationService] Error navegando a {route}: {ex.Message}");
            throw;
        }
    }

    public async Task NavigateToAsync(string route, IDictionary<string, object> parameters)
    {
        try
        {
            if (Application.Current?.MainPage is Shell shell)
            {
                await shell.GoToAsync(route, parameters);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[NavigationService] No se puede navegar: MainPage no es Shell");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NavigationService] Error navegando a {route} con parámetros: {ex.Message}");
            throw;
        }
    }

    public async Task GoBackAsync()
    {
        try
        {
            if (Application.Current?.MainPage is Shell shell)
            {
                await shell.GoToAsync("..");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[NavigationService] No se puede navegar: MainPage no es Shell");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NavigationService] Error navegando hacia atrás: {ex.Message}");
            throw;
        }
    }

    public async Task GoBackAsync(IDictionary<string, object> parameters)
    {
        try
        {
            if (Application.Current?.MainPage is Shell shell)
            {
                await shell.GoToAsync("..", parameters);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[NavigationService] No se puede navegar: MainPage no es Shell");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NavigationService] Error navegando hacia atrás con parámetros: {ex.Message}");
            throw;
        }
    }

    public async Task GoToRootAsync()
    {
        try
        {
            if (Application.Current?.MainPage is Shell shell)
            {
                await shell.GoToAsync("//");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[NavigationService] No se puede navegar: MainPage no es Shell");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NavigationService] Error navegando a raíz: {ex.Message}");
            throw;
        }
    }
}
