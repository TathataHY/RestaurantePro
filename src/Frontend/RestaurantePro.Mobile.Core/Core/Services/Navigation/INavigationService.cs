namespace RestaurantePro.Mobile.Core.Services.Navigation;

/// <summary>
/// Servicio de navegación - V1 Fundamental
/// </summary>
public interface INavigationService
{
    Task NavigateToAsync(string route);
    Task NavigateToAsync(string route, IDictionary<string, object> parameters);
    Task GoBackAsync();
    Task GoToRootAsync();
} 