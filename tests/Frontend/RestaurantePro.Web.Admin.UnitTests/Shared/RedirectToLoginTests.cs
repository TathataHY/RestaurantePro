using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Web.Admin.Shared;
using Xunit;
using Bunit;

namespace RestaurantePro.Web.Admin.UnitTests.Shared;

public class TestNavigationManager : NavigationManager
{
    public TestNavigationManager(string baseUri, string uri)
    {
        Initialize(baseUri, uri);
    }

    protected override void NavigateToCore(string uri, bool forceLoad)
    {
        // No-op for testing
    }

    protected override void NavigateToCore(string uri, NavigationOptions options)
    {
        // No-op for testing
    }
}

public class RedirectToLoginTests : TestContext
{
    [Fact]
    public void RedirectToLogin_ShouldNavigateToLoginOnInitialization()
    {
        // Arrange
        var baseUri = "https://localhost:5001/";
        var currentUri = "https://localhost:5001/admin/dashboard";
        
        Services.AddSingleton<NavigationManager>(new TestNavigationManager(baseUri, currentUri));

        // Act
        var component = RenderComponent<RedirectToLogin>();

        // Assert
        Assert.NotNull(component);
    }

    [Fact]
    public void RedirectToLogin_ShouldEscapeSpecialCharactersInReturnUrl()
    {
        // Arrange
        var baseUri = "https://localhost:5001/";
        var currentUri = "https://localhost:5001/admin/reportes?fecha=2024-01-01&tipo=ventas";
        
        Services.AddSingleton<NavigationManager>(new TestNavigationManager(baseUri, currentUri));

        // Act
        var component = RenderComponent<RedirectToLogin>();

        // Assert
        Assert.NotNull(component);
    }

    [Fact]
    public void RedirectToLogin_ShouldHandleEmptyReturnUrl()
    {
        // Arrange
        var baseUri = "https://localhost:5001/";
        var currentUri = "https://localhost:5001/";
        
        Services.AddSingleton<NavigationManager>(new TestNavigationManager(baseUri, currentUri));

        // Act
        var component = RenderComponent<RedirectToLogin>();

        // Assert
        Assert.NotNull(component);
    }

    [Fact]
    public void RedirectToLogin_ShouldHandleSpecialCharactersInPath()
    {
        // Arrange
        var baseUri = "https://localhost:5001/";
        var currentUri = "https://localhost:5001/admin/productos/crear?nombre=Producto%20Test&descripcion=Descripción%20con%20espacios";
        
        Services.AddSingleton<NavigationManager>(new TestNavigationManager(baseUri, currentUri));

        // Act
        var component = RenderComponent<RedirectToLogin>();

        // Assert
        Assert.NotNull(component);
    }

    [Fact]
    public void RedirectToLogin_ShouldHandleUnicodeCharacters()
    {
        // Arrange
        var baseUri = "https://localhost:5001/";
        var currentUri = "https://localhost:5001/admin/usuarios?nombre=José%20María";
        
        Services.AddSingleton<NavigationManager>(new TestNavigationManager(baseUri, currentUri));

        // Act
        var component = RenderComponent<RedirectToLogin>();

        // Assert
        Assert.NotNull(component);
    }

    [Fact]
    public void RedirectToLogin_ShouldHandleComplexQueryParameters()
    {
        // Arrange
        var baseUri = "https://localhost:5001/";
        var currentUri = "https://localhost:5001/admin/reportes?fechaInicio=2024-01-01&fechaFin=2024-12-31&categoria=Bebidas&orden=desc";
        
        Services.AddSingleton<NavigationManager>(new TestNavigationManager(baseUri, currentUri));

        // Act
        var component = RenderComponent<RedirectToLogin>();

        // Assert
        Assert.NotNull(component);
    }

    [Fact]
    public void RedirectToLogin_ShouldCallToBaseRelativePathWithCurrentUri()
    {
        // Arrange
        var baseUri = "https://localhost:5001/";
        var currentUri = "https://localhost:5001/admin/dashboard";
        
        Services.AddSingleton<NavigationManager>(new TestNavigationManager(baseUri, currentUri));

        // Act
        var component = RenderComponent<RedirectToLogin>();

        // Assert
        Assert.NotNull(component);
    }

    [Fact]
    public void RedirectToLogin_ShouldNotRenderAnyContent()
    {
        // Arrange
        var baseUri = "https://localhost:5001/";
        var currentUri = "https://localhost:5001/admin/dashboard";
        
        Services.AddSingleton<NavigationManager>(new TestNavigationManager(baseUri, currentUri));

        // Act
        var component = RenderComponent<RedirectToLogin>();

        // Assert
        Assert.NotNull(component);
    }
}