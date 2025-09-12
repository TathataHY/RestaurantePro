using RestaurantePro.Mobile.Core.Services.Navigation;

namespace RestaurantePro.Mobile.UnitTests.Services.Navigation;

public class NavigationServiceTests
{
    private readonly NavigationService _navigationService;

    public NavigationServiceTests()
    {
        _navigationService = new NavigationService();
    }

    #region NavigateToAsync Tests

    [Fact]
    public async Task NavigateToAsync_WithRoute_ShouldCompleteSuccessfully()
    {
        // Arrange
        var route = "test-route";

        // Act & Assert
        await _navigationService.NavigateToAsync(route);
        // No exception should be thrown
    }

    [Fact]
    public async Task NavigateToAsync_WithEmptyRoute_ShouldCompleteSuccessfully()
    {
        // Arrange
        var route = "";

        // Act & Assert
        await _navigationService.NavigateToAsync(route);
        // No exception should be thrown
    }

    [Fact]
    public async Task NavigateToAsync_WithNullRoute_ShouldCompleteSuccessfully()
    {
        // Arrange
        string? route = null;

        // Act & Assert
        await _navigationService.NavigateToAsync(route!);
        // No exception should be thrown
    }

    #endregion

    #region NavigateToAsync With Parameters Tests

    [Fact]
    public async Task NavigateToAsync_WithRouteAndParameters_ShouldCompleteSuccessfully()
    {
        // Arrange
        var route = "test-route";
        var parameters = new Dictionary<string, object>
        {
            { "id", 123 },
            { "name", "test" }
        };

        // Act & Assert
        await _navigationService.NavigateToAsync(route, parameters);
        // No exception should be thrown
    }

    [Fact]
    public async Task NavigateToAsync_WithRouteAndEmptyParameters_ShouldCompleteSuccessfully()
    {
        // Arrange
        var route = "test-route";
        var parameters = new Dictionary<string, object>();

        // Act & Assert
        await _navigationService.NavigateToAsync(route, parameters);
        // No exception should be thrown
    }

    [Fact]
    public async Task NavigateToAsync_WithRouteAndNullParameters_ShouldCompleteSuccessfully()
    {
        // Arrange
        var route = "test-route";
        IDictionary<string, object>? parameters = null;

        // Act & Assert
        await _navigationService.NavigateToAsync(route, parameters!);
        // No exception should be thrown
    }

    #endregion

    #region GoBackAsync Tests

    [Fact]
    public async Task GoBackAsync_ShouldCompleteSuccessfully()
    {
        // Act & Assert
        await _navigationService.GoBackAsync();
        // No exception should be thrown
    }

    [Fact]
    public async Task GoBackAsync_WithParameters_ShouldCompleteSuccessfully()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "result", "success" }
        };

        // Act & Assert
        await _navigationService.GoBackAsync(parameters);
        // No exception should be thrown
    }

    [Fact]
    public async Task GoBackAsync_WithEmptyParameters_ShouldCompleteSuccessfully()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();

        // Act & Assert
        await _navigationService.GoBackAsync(parameters);
        // No exception should be thrown
    }

    [Fact]
    public async Task GoBackAsync_WithNullParameters_ShouldCompleteSuccessfully()
    {
        // Arrange
        IDictionary<string, object>? parameters = null;

        // Act & Assert
        await _navigationService.GoBackAsync(parameters!);
        // No exception should be thrown
    }

    #endregion

    #region GoToRootAsync Tests

    [Fact]
    public async Task GoToRootAsync_ShouldCompleteSuccessfully()
    {
        // Act & Assert
        await _navigationService.GoToRootAsync();
        // No exception should be thrown
    }

    #endregion

    #region Multiple Operations Tests

    [Fact]
    public async Task MultipleNavigationOperations_ShouldCompleteSuccessfully()
    {
        // Arrange
        var route1 = "first-route";
        var route2 = "second-route";
        var parameters = new Dictionary<string, object> { { "test", "value" } };

        // Act & Assert
        await _navigationService.NavigateToAsync(route1);
        await _navigationService.NavigateToAsync(route2, parameters);
        await _navigationService.GoBackAsync();
        await _navigationService.GoToRootAsync();
        // No exception should be thrown
    }

    #endregion
}
