using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using RestaurantePro.Web.Admin.Shared;
using RestaurantePro.Web.Admin.UnitTests.Pages;

namespace RestaurantePro.Web.Admin.UnitTests.Shared;

public class MainLayoutTests : TestContext
{
    private readonly Mock<IJSRuntime> _jsRuntimeMock;

    public MainLayoutTests()
    {
        _jsRuntimeMock = new Mock<IJSRuntime>();

        Services.AddSingleton(_jsRuntimeMock.Object);
        Services.AddSingleton<NavigationManager>(new TestNavigationManager("https://localhost:5001/", "https://localhost:5001/"));

        // Configurar JSInterop para manejar llamadas JavaScript
        JSInterop.SetupVoid("document.body.setAttribute", _ => true);
        JSInterop.SetupVoid("localStorage.setItem", _ => true);
    }

    [Fact]
    public void MainLayout_ShouldRender()
    {
        // Act
        var component = RenderComponent<MainLayout>();

        // Assert
        component.Should().NotBeNull();
    }

    [Fact]
    public void MainLayout_ShouldHaveCorrectPageTitle()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        component.Find("title").TextContent.Trim().Should().Be("RestaurantePro Admin");
    }

    [Fact]
    public void MainLayout_ShouldHaveCorrectStructure()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        component.Find(".page").Should().NotBeNull();
        component.Find(".sidebar").Should().NotBeNull();
        component.Find(".main-content").Should().NotBeNull();
        component.Find(".top-row").Should().NotBeNull();
        component.Find(".content").Should().NotBeNull();
    }

    [Fact]
    public void MainLayout_ShouldHaveNavMenu()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        component.Find("nav").Should().NotBeNull();
    }

    [Fact]
    public void MainLayout_ShouldHaveCorrectHeader()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        component.Find("h1").TextContent.Trim().Should().Be("RestaurantePro Admin");
    }

    [Fact]
    public void MainLayout_ShouldHaveThemeToggleButton()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        component.Find(".theme-toggle").Should().NotBeNull();
        component.Find(".theme-toggle").GetAttribute("title").Should().Be("Cambiar tema");
    }

    [Fact]
    public void MainLayout_ShouldHaveCurrentDate()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        component.Find("small").Should().NotBeNull();
        component.Find("small").TextContent.Should().NotBeEmpty();
    }

    [Fact]
    public void MainLayout_ShouldHaveContentArea()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        component.Find("article").Should().NotBeNull();
        component.Find("article").ClassList.Should().Contain("content");
    }

    [Fact]
    public void MainLayout_ShouldHaveCorrectCSSClasses()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        component.Find(".page").Should().NotBeNull();
        component.Find(".sidebar").Should().NotBeNull();
        component.Find(".main-content").Should().NotBeNull();
        component.Find(".top-row").Should().NotBeNull();
        component.Find(".content").Should().NotBeNull();
    }

    [Fact]
    public void MainLayout_ShouldHaveCorrectHeaderClasses()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        var header = component.Find("h1");
        header.ClassList.Should().Contain("h4");
        header.ClassList.Should().Contain("mb-0");
        header.ClassList.Should().Contain("fw-bold");
        header.ClassList.Should().Contain("text-primary");
    }

    [Fact]
    public void MainLayout_ShouldHaveCorrectTopRowClasses()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        var topRow = component.Find(".top-row");
        var flexContainer = topRow.QuerySelector(".d-flex.align-items-center.gap-3");
        flexContainer.Should().NotBeNull();
    }

    [Fact]
    public void MainLayout_ShouldHaveCorrectDateContainerClasses()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        var dateContainer = component.Find(".d-flex.align-items-center.gap-2");
        dateContainer.Should().NotBeNull();
    }

    [Fact]
    public void MainLayout_ShouldHaveCorrectDateClasses()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        var dateElement = component.Find("small");
        dateElement.ClassList.Should().Contain("text-muted");
    }

    [Fact]
    public void MainLayout_ShouldHaveCorrectThemeToggleClasses()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        var themeToggle = component.Find(".theme-toggle");
        themeToggle.ClassList.Should().Contain("theme-toggle");
    }

    [Fact]
    public void MainLayout_ShouldHaveCorrectLayoutStructure()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        var page = component.Find(".page");
        var sidebar = page.QuerySelector(".sidebar");
        var mainContent = page.QuerySelector(".main-content");
        
        sidebar.Should().NotBeNull();
        mainContent.Should().NotBeNull();
    }

    [Fact]
    public void MainLayout_ShouldHaveCorrectMainContentStructure()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        var mainContent = component.Find(".main-content");
        var topRow = mainContent.QuerySelector(".top-row");
        var article = mainContent.QuerySelector("article.content");
        
        topRow.Should().NotBeNull();
        article.Should().NotBeNull();
    }

    [Fact]
    public void MainLayout_ShouldHaveCorrectTopRowStructure()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        var topRow = component.Find(".top-row");
        var flexContainer = topRow.QuerySelector(".d-flex.align-items-center.gap-3");
        var header = flexContainer.QuerySelector("h1");
        var dateContainer = flexContainer.QuerySelector(".d-flex.align-items-center.gap-2");
        
        flexContainer.Should().NotBeNull();
        header.Should().NotBeNull();
        dateContainer.Should().NotBeNull();
    }

    [Fact]
    public void MainLayout_ShouldHaveCorrectDateContainerStructure()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        var dateContainer = component.Find(".d-flex.align-items-center.gap-2");
        var date = dateContainer.QuerySelector("small");
        var themeToggle = dateContainer.QuerySelector(".theme-toggle");
        
        date.Should().NotBeNull();
        themeToggle.Should().NotBeNull();
    }

    [Fact]
    public void MainLayout_ShouldHaveCorrectArticleStructure()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        var article = component.Find("article.content");
        article.Should().NotBeNull();
        article.ClassList.Should().Contain("content");
    }

    [Fact]
    public void MainLayout_ShouldHaveCorrectSidebarStructure()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        var sidebar = component.Find(".sidebar");
        var navMenu = sidebar.QuerySelector("nav");
        
        sidebar.Should().NotBeNull();
        navMenu.Should().NotBeNull();
    }

    [Fact]
    public void MainLayout_ShouldHaveCorrectPageStructure()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        var page = component.Find(".page");
        var sidebar = page.QuerySelector(".sidebar");
        var mainContent = page.QuerySelector(".main-content");
        
        page.Should().NotBeNull();
        sidebar.Should().NotBeNull();
        mainContent.Should().NotBeNull();
    }

    [Fact]
    public void MainLayout_ShouldHaveCorrectElementHierarchy()
    {
        // Arrange & Act
        var component = RenderComponent<MainLayout>();

        // Assert
        var page = component.Find(".page");
        var sidebar = page.QuerySelector(".sidebar");
        var mainContent = page.QuerySelector(".main-content");
        var topRow = mainContent.QuerySelector(".top-row");
        var article = mainContent.QuerySelector("article.content");
        
        page.Should().NotBeNull();
        sidebar.Should().NotBeNull();
        mainContent.Should().NotBeNull();
        topRow.Should().NotBeNull();
        article.Should().NotBeNull();
    }
}
