using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Services.Navigation;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services.Navigation;

/// <summary>
/// Pruebas de integración avanzadas para el servicio de navegación
/// </summary>
public class NavigationServiceAdvancedIntegrationTests
{
    private readonly MockNavigationService _navigationService;
    private readonly ILogger<NavigationServiceAdvancedIntegrationTests> _logger;

    public NavigationServiceAdvancedIntegrationTests()
    {
        _logger = NullLogger<NavigationServiceAdvancedIntegrationTests>.Instance;
        _navigationService = new MockNavigationService();
    }

    [Fact]
    public async Task NavigationHistory_ShouldTrackAllNavigationOperations()
    {
        // Arrange
        var expectedRoutes = new[] { "//dashboard", "//mesas", "//comandas", "BACK", "ROOT" };

        // Act
        await _navigationService.NavigateToAsync("//dashboard");
        await _navigationService.NavigateToAsync("//mesas");
        await _navigationService.NavigateToAsync("//comandas");
        await _navigationService.GoBackAsync();
        await _navigationService.GoToRootAsync();

        // Assert
        Assert.Equal(expectedRoutes.Length, _navigationService.NavigationHistory.Count);
        for (int i = 0; i < expectedRoutes.Length; i++)
        {
            Assert.Equal(expectedRoutes[i], _navigationService.NavigationHistory[i]);
        }
    }

    [Fact]
    public async Task ParameterHistory_ShouldTrackNavigationParameters()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();
        var mesaParams = new Dictionary<string, object> { { "mesaId", mesaId } };
        var comandaParams = new Dictionary<string, object> { { "comandaId", comandaId } };

        // Act
        await _navigationService.NavigateToAsync("//mesa-detalle", mesaParams);
        await _navigationService.NavigateToAsync("//comanda-detalle", comandaParams);

        // Assert
        Assert.Equal(2, _navigationService.ParameterHistory.Count);
        Assert.Equal(mesaId, _navigationService.ParameterHistory[0]["mesaId"]);
        Assert.Equal(comandaId, _navigationService.ParameterHistory[1]["comandaId"]);
    }

    [Fact]
    public async Task NavigationWithComplexParameters_ShouldHandleVariousDataTypes()
    {
        // Arrange
        var complexParams = new Dictionary<string, object>
        {
            { "id", Guid.NewGuid() },
            { "nombre", "Mesa VIP" },
            { "capacidad", 8 },
            { "activa", true },
            { "fecha", DateTime.Now },
            { "precio", 150.50m }
        };

        // Act
        var exception = await Record.ExceptionAsync(async () =>
            await _navigationService.NavigateToAsync("//mesa-detalle", complexParams));

        // Assert
        Assert.Null(exception);
        Assert.Single(_navigationService.ParameterHistory);
        Assert.Equal(complexParams.Count, _navigationService.ParameterHistory[0].Count);
    }

    [Fact]
    public async Task NavigationWithEmptyParameters_ShouldHandleGracefully()
    {
        // Arrange
        var emptyParams = new Dictionary<string, object>();

        // Act
        var exception = await Record.ExceptionAsync(async () =>
            await _navigationService.NavigateToAsync("//dashboard", emptyParams));

        // Assert
        Assert.Null(exception);
        Assert.Single(_navigationService.ParameterHistory);
        Assert.Empty(_navigationService.ParameterHistory[0]);
    }

    [Fact]
    public async Task NavigationWithNullParameters_ShouldHandleGracefully()
    {
        // Act
        var exception = await Record.ExceptionAsync(async () =>
            await _navigationService.NavigateToAsync("//dashboard", null!));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task NavigationWithSpecialCharacters_ShouldHandleRouteEncoding()
    {
        // Arrange
        var specialRoute = "//mesa-detalle/123/estado=ocupada";
        var specialParams = new Dictionary<string, object>
        {
            { "estado", "ocupada" },
            { "observacion", "Cliente VIP - Mesa reservada" }
        };

        // Act
        var exception = await Record.ExceptionAsync(async () =>
            await _navigationService.NavigateToAsync(specialRoute, specialParams));

        // Assert
        Assert.Null(exception);
        Assert.Contains(specialRoute, _navigationService.NavigationHistory);
    }

    [Fact]
    public async Task RapidNavigation_ShouldHandleConcurrentOperations()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_navigationService.NavigateToAsync($"//page-{i}"));
        }

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, _navigationService.NavigationHistory.Count);
        for (int i = 0; i < 10; i++)
        {
            Assert.Contains($"//page-{i}", _navigationService.NavigationHistory);
        }
    }

    [Fact]
    public async Task NavigationWithBackParameters_ShouldTrackBackNavigation()
    {
        // Arrange
        var backParams = new Dictionary<string, object> { { "refresh", true } };

        // Act
        await _navigationService.NavigateToAsync("//dashboard");
        await _navigationService.GoBackAsync(backParams);

        // Assert
        Assert.Equal(2, _navigationService.NavigationHistory.Count);
        Assert.Equal("BACK", _navigationService.NavigationHistory[1]);
        Assert.True((bool)_navigationService.ParameterHistory[1]["refresh"]);
    }

    [Fact]
    public async Task DeepNavigationStack_ShouldHandleMultipleBackOperations()
    {
        // Arrange
        var routes = new[] { "//dashboard", "//mesas", "//comandas", "//productos", "//categorias" };

        // Act
        foreach (var route in routes)
        {
            await _navigationService.NavigateToAsync(route);
        }

        // Go back multiple times
        for (int i = 0; i < 3; i++)
        {
            await _navigationService.GoBackAsync();
        }

        // Assert
        Assert.Equal(routes.Length + 3, _navigationService.NavigationHistory.Count);
        Assert.Equal(3, _navigationService.NavigationHistory.Count(x => x == "BACK"));
    }

    [Fact]
    public async Task NavigationWithLargeParameterSets_ShouldHandleMemoryEfficiently()
    {
        // Arrange
        var largeParams = new Dictionary<string, object>();
        for (int i = 0; i < 100; i++)
        {
            largeParams[$"param{i}"] = $"value{i}";
        }

        // Act
        var exception = await Record.ExceptionAsync(async () =>
            await _navigationService.NavigateToAsync("//large-data", largeParams));

        // Assert
        Assert.Null(exception);
        Assert.Equal(100, _navigationService.ParameterHistory[0].Count);
    }

    [Fact]
    public async Task NavigationService_ShouldMaintainStateBetweenOperations()
    {
        // Arrange
        var initialCount = _navigationService.NavigationHistory.Count;

        // Act
        await _navigationService.NavigateToAsync("//test1");
        var countAfterFirst = _navigationService.NavigationHistory.Count;
        
        await _navigationService.NavigateToAsync("//test2");
        var countAfterSecond = _navigationService.NavigationHistory.Count;

        // Assert
        Assert.Equal(0, initialCount);
        Assert.Equal(1, countAfterFirst);
        Assert.Equal(2, countAfterSecond);
        Assert.Equal("//test1", _navigationService.NavigationHistory[0]);
        Assert.Equal("//test2", _navigationService.NavigationHistory[1]);
    }
} 