using RestaurantePro.Mobile.Core.Services.Navigation;

namespace RestaurantePro.Mobile.IntegrationTests.TestBase;

public class FakeNavigationService : INavigationService
{
    public Task NavigateToAsync(string route)
    {
        return Task.CompletedTask;
    }

    public Task NavigateToAsync(string route, IDictionary<string, object> parameters)
    {
        return Task.CompletedTask;
    }

    public Task GoBackAsync()
    {
        return Task.CompletedTask;
    }

    public Task GoToRootAsync()
    {
        return Task.CompletedTask;
    }
}


