using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Services.Navigation;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services.Navigation;

/// <summary>
/// Pruebas de integración básicas para el servicio de navegación
/// </summary>
public class NavigationServiceIntegrationTests
{
    private readonly INavigationService _navigationService;
    private readonly ILogger<NavigationServiceIntegrationTests> _logger;

    public NavigationServiceIntegrationTests()
    {
        _logger = NullLogger<NavigationServiceIntegrationTests>.Instance;
        _navigationService = new MockNavigationService();
    }

    [Fact]
    public async Task NavigateToAsync_WithValidRoute_ShouldCompleteSuccessfully()
    {
        // Arrange
        var route = "//dashboard";

        // Act
        var exception = await Record.ExceptionAsync(async () =>
            await _navigationService.NavigateToAsync(route));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task NavigateToAsync_WithRouteAndParameters_ShouldCompleteSuccessfully()
    {
        // Arrange
        var route = "//mesa-detalle";
        var parameters = new Dictionary<string, object>
        {
            { "mesaId", Guid.NewGuid() },
            { "nombre", "Mesa 1" }
        };

        // Act
        var exception = await Record.ExceptionAsync(async () =>
            await _navigationService.NavigateToAsync(route, parameters));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task GoBackAsync_ShouldCompleteSuccessfully()
    {
        // Act
        var exception = await Record.ExceptionAsync(async () =>
            await _navigationService.GoBackAsync());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task GoToRootAsync_ShouldCompleteSuccessfully()
    {
        // Act
        var exception = await Record.ExceptionAsync(async () =>
            await _navigationService.GoToRootAsync());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task NavigationFlow_ShouldHandleMultipleOperations()
    {
        // Arrange
        var routes = new[] { "//dashboard", "//mesas", "//comandas" };

        // Act & Assert
        foreach (var route in routes)
        {
            var exception = await Record.ExceptionAsync(async () =>
                await _navigationService.NavigateToAsync(route));
            Assert.Null(exception);
        }

        // Go back twice
        for (int i = 0; i < 2; i++)
        {
            var exception = await Record.ExceptionAsync(async () =>
                await _navigationService.GoBackAsync());
            Assert.Null(exception);
        }

        // Go to root
        var rootException = await Record.ExceptionAsync(async () =>
            await _navigationService.GoToRootAsync());
        Assert.Null(rootException);
    }
} 