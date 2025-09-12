using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RestaurantePro.Web.Admin.Auth;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Web.Admin.Shared;
using RestaurantePro.Web.Admin.UnitTests.Pages;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RestaurantePro.Web.Admin.UnitTests.Shared;

public class NavMenuTests : TestContext
{
    private readonly Mock<TokenStore> _tokenStoreMock;
    private readonly JwtAuthenticationStateProvider _authStateProvider;

    public NavMenuTests()
    {
        _tokenStoreMock = new Mock<TokenStore>();
        _authStateProvider = new JwtAuthenticationStateProvider(_tokenStoreMock.Object);

        Services.AddSingleton(_authStateProvider);
        Services.AddSingleton<AuthenticationStateProvider>(_authStateProvider);
        Services.AddSingleton<NavigationManager>(new TestNavigationManager("https://localhost:5001/", "https://localhost:5001/"));

        // Configurar JSInterop para manejar llamadas JavaScript
        JSInterop.SetupVoid("console.log", _ => true);
    }

    [Fact]
    public void NavMenu_ShouldRender()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.Should().NotBeNull();
    }

    [Fact]
    public void NavMenu_ShouldHaveCorrectBrand()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.Find(".navbar-brand").TextContent.Trim().Should().Contain("RestaurantePro");
    }

    [Fact]
    public void NavMenu_ShouldHaveCorrectBrandIcon()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.Find(".navbar-brand span").ClassList.Should().Contain("material-symbols-outlined");
    }

    [Fact]
    public void NavMenu_ShouldHaveCorrectStructure()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.Find(".navbar").Should().NotBeNull();
        component.Find(".nav-scrollable").Should().NotBeNull();
        component.Find("nav").Should().NotBeNull();
        component.Find(".nav-bottom").Should().NotBeNull();
    }

    [Fact]
    public void NavMenu_ShouldHaveAllNavSections()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.FindAll(".nav-section").Should().HaveCount(6); // Dashboard, Productos, Operaciones, Comercial, Inventario, Reportes, Sistema
    }

    [Fact]
    public void NavMenu_ShouldHaveCorrectSectionTitles()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.FindAll(".nav-section-title").Should().Contain(t => t.TextContent.Contains("Productos"));
        component.FindAll(".nav-section-title").Should().Contain(t => t.TextContent.Contains("Operaciones"));
        component.FindAll(".nav-section-title").Should().Contain(t => t.TextContent.Contains("Comercial"));
        component.FindAll(".nav-section-title").Should().Contain(t => t.TextContent.Contains("Inventario"));
        component.FindAll(".nav-section-title").Should().Contain(t => t.TextContent.Contains("Reportes"));
        component.FindAll(".nav-section-title").Should().Contain(t => t.TextContent.Contains("Sistema"));
    }

    [Fact]
    public void NavMenu_ShouldHaveDashboardLink()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        var dashboardLink = component.Find("a[href='/']");
        dashboardLink.Should().NotBeNull();
        dashboardLink.TextContent.Trim().Should().Contain("Dashboard");
    }

    [Fact]
    public void NavMenu_ShouldHaveProductosLinks()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.Find("a[href='productos']").Should().NotBeNull();
        component.Find("a[href='categorias']").Should().NotBeNull();
        component.Find("a[href='recetas']").Should().NotBeNull();
    }

    [Fact]
    public void NavMenu_ShouldHaveOperacionesLinks()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.Find("a[href='mesas']").Should().NotBeNull();
        component.Find("a[href='comandas']").Should().NotBeNull();
        component.Find("a[href='reservaciones']").Should().NotBeNull();
        component.Find("a[href='preparaciones']").Should().NotBeNull();
    }

    [Fact]
    public void NavMenu_ShouldHaveComercialLinks()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.Find("a[href='clientes']").Should().NotBeNull();
        component.Find("a[href='facturas']").Should().NotBeNull();
        component.Find("a[href='promociones']").Should().NotBeNull();
    }

    [Fact]
    public void NavMenu_ShouldHaveInventarioLinks()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.Find("a[href='inventario']").Should().NotBeNull();
        component.Find("a[href='proveedores']").Should().NotBeNull();
    }

    [Fact]
    public void NavMenu_ShouldHaveReportesLinks()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.Find("a[href='reportes']").Should().NotBeNull();
    }

    [Fact]
    public void NavMenu_ShouldHaveSistemaLinks()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.Find("a[href='usuarios']").Should().NotBeNull();
        component.Find("a[href='notificaciones']").Should().NotBeNull();
    }

    [Fact]
    public void NavMenu_ShouldHaveCorrectIcons()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.FindAll(".material-symbols-outlined").Should().HaveCountGreaterThan(0);
        component.FindAll(".material-symbols-outlined").Should().Contain(i => i.TextContent.Contains("restaurant_menu"));
        component.FindAll(".material-symbols-outlined").Should().Contain(i => i.TextContent.Contains("dashboard"));
        component.FindAll(".material-symbols-outlined").Should().Contain(i => i.TextContent.Contains("inventory_2"));
        component.FindAll(".material-symbols-outlined").Should().Contain(i => i.TextContent.Contains("category"));
        component.FindAll(".material-symbols-outlined").Should().Contain(i => i.TextContent.Contains("restaurant"));
    }

    [Fact]
    public void NavMenu_ShouldHaveCorrectLinkTexts()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.FindAll("a").Should().Contain(a => a.TextContent.Contains("Dashboard"));
        component.FindAll("a").Should().Contain(a => a.TextContent.Contains("Productos"));
        component.FindAll("a").Should().Contain(a => a.TextContent.Contains("Categorías"));
        component.FindAll("a").Should().Contain(a => a.TextContent.Contains("Recetas"));
        component.FindAll("a").Should().Contain(a => a.TextContent.Contains("Mesas"));
        component.FindAll("a").Should().Contain(a => a.TextContent.Contains("Comandas"));
        component.FindAll("a").Should().Contain(a => a.TextContent.Contains("Reservaciones"));
        component.FindAll("a").Should().Contain(a => a.TextContent.Contains("Preparaciones"));
        component.FindAll("a").Should().Contain(a => a.TextContent.Contains("Clientes"));
        component.FindAll("a").Should().Contain(a => a.TextContent.Contains("Facturas"));
        component.FindAll("a").Should().Contain(a => a.TextContent.Contains("Promociones"));
        component.FindAll("a").Should().Contain(a => a.TextContent.Contains("Inventario"));
        component.FindAll("a").Should().Contain(a => a.TextContent.Contains("Proveedores"));
        component.FindAll("a").Should().Contain(a => a.TextContent.Contains("Reportes"));
        component.FindAll("a").Should().Contain(a => a.TextContent.Contains("Usuarios"));
        component.FindAll("a").Should().Contain(a => a.TextContent.Contains("Notificaciones"));
    }

    [Fact]
    public void NavMenu_ShouldHaveCorrectCSSClasses()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.Find(".navbar").Should().NotBeNull();
        component.Find(".navbar-brand").Should().NotBeNull();
        component.Find(".nav-scrollable").Should().NotBeNull();
        component.Find("nav.flex-column").Should().NotBeNull();
        component.Find(".nav-bottom").Should().NotBeNull();
    }

    [Fact]
    public void NavMenu_ShouldHaveCorrectNavItemClasses()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.FindAll(".nav-item").Should().HaveCountGreaterThan(0);
        component.FindAll(".nav-link").Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void NavMenu_ShouldHaveCorrectNavSectionClasses()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.FindAll(".nav-section").Should().HaveCountGreaterThan(0);
        component.FindAll(".nav-section-title").Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void NavMenu_ShouldHaveCorrectNavBottomClasses()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.Find(".nav-bottom").Should().NotBeNull();
    }

    [Fact]
    public void NavMenu_ShouldHaveCorrectNavLinkClasses()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.FindAll(".nav-link").Should().HaveCountGreaterThan(0);
        component.FindAll(".nav-link").Should().AllSatisfy(link => 
            link.ClassList.Should().Contain("nav-link"));
    }

    [Fact]
    public void NavMenu_ShouldHaveCorrectNavItemStructure()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.FindAll(".nav-item").Should().AllSatisfy(item => 
            item.QuerySelector(".nav-link").Should().NotBeNull());
    }

    [Fact]
    public void NavMenu_ShouldHaveCorrectNavSectionStructure()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.FindAll(".nav-section").Should().AllSatisfy(section => 
            section.QuerySelector(".nav-item").Should().NotBeNull());
    }

    [Fact]
    public void NavMenu_ShouldHaveCorrectNavLinkStructure()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal());
        var authStateTask = Task.FromResult(authState);

        // Act
        var component = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(authStateTask));

        // Assert
        component.FindAll(".nav-link").Should().AllSatisfy(link => 
            link.QuerySelector(".material-symbols-outlined").Should().NotBeNull());
    }
}
