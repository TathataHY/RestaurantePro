using RestaurantePro.Mobile.Core.Services.Navigation;

namespace RestaurantePro.Mobile.Core.Services.Navigation;

/// <summary>
/// Implementación base abstracta del servicio de navegación
/// </summary>
public abstract class NavigationServiceBase : INavigationService
{
    public abstract Task NavigateToAsync(string route);
    public abstract Task NavigateToAsync(string route, IDictionary<string, object> parameters);
    public abstract Task GoBackAsync();
    public abstract Task GoBackAsync(IDictionary<string, object> parameters);
    public abstract Task GoToRootAsync();
}

/// <summary>
/// Implementación mock del servicio de navegación para pruebas
/// </summary>
public class MockNavigationService : NavigationServiceBase
{
    public List<string> NavigationHistory { get; } = new();
    public List<IDictionary<string, object>> ParameterHistory { get; } = new();

    public override async Task NavigateToAsync(string route)
    {
        NavigationHistory.Add(route);
        ParameterHistory.Add(new Dictionary<string, object>());
        await Task.CompletedTask;
    }

    public override async Task NavigateToAsync(string route, IDictionary<string, object> parameters)
    {
        NavigationHistory.Add(route);
        ParameterHistory.Add(parameters);
        await Task.CompletedTask;
    }

    public override async Task GoBackAsync()
    {
        NavigationHistory.Add("BACK");
        ParameterHistory.Add(new Dictionary<string, object>());
        await Task.CompletedTask;
    }

    public override async Task GoBackAsync(IDictionary<string, object> parameters)
    {
        NavigationHistory.Add("BACK");
        ParameterHistory.Add(parameters);
        await Task.CompletedTask;
    }

    public override async Task GoToRootAsync()
    {
        NavigationHistory.Add("ROOT");
        ParameterHistory.Add(new Dictionary<string, object>());
        await Task.CompletedTask;
    }
}

 