using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Web.Admin.Pages;
using RestaurantePro.Web.Admin.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndexPage = RestaurantePro.Web.Admin.Pages.Index;

namespace RestaurantePro.Web.Admin.UnitTests.Pages;

public class IndexPageTests : TestContext
{
    private readonly Mock<IDashboardApiService> _dashboardApiMock;

    public IndexPageTests()
    {
        _dashboardApiMock = new Mock<IDashboardApiService>();
        
        Services.AddSingleton(_dashboardApiMock.Object);
        Services.AddSingleton<IDashboardApiService>(_dashboardApiMock.Object);
        Services.AddSingleton<TestNavigationManager>();
        
        // Configurar JSInterop para manejar llamadas JavaScript
        JSInterop.SetupVoid("console.log", _ => true);
        JSInterop.SetupVoid("dashboardCharts.destruirGrafico", _ => true);
        JSInterop.Setup<object>("dashboardCharts.crearGraficoVentasHora", _ => true);
        JSInterop.Setup<object>("dashboardCharts.crearGraficoIngresosCategoria", _ => true);
    }

    [Fact]
    public void IndexPage_ShouldRender()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto
            {
                VentasHoy = 1500.00m,
                CrecimientoVentas = 12.5m,
                TotalMesas = 20,
                MesasOcupadas = 15,
                ComandasActivas = 8
            },
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        component.Should().NotBeNull();
    }

    [Fact]
    public void IndexPage_ShouldHaveCorrectPageTitle()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        component.Find("h1").TextContent.Trim().Should().Be("Dashboard Ejecutivo");
    }

    [Fact]
    public void IndexPage_ShouldHaveCorrectDescription()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        component.FindAll("p").Should().Contain(p => p.TextContent.Contains("Resumen de métricas clave y operaciones."));
    }

    [Fact]
    public void IndexPage_ShouldHaveCustomizeWidgetsButton()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        component.FindAll("button").Should().Contain(b => b.TextContent.Contains("Personalizar Widgets"));
    }

    [Fact]
    public void IndexPage_ShouldDisplayErrorState()
    {
        // Arrange
        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ThrowsAsync(new Exception("Error de red"));

        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        component.FindAll("p").Should().Contain(p => p.TextContent.Contains("Error al cargar el dashboard"));
        component.FindAll("button").Should().Contain(b => b.TextContent.Contains("Reintentar"));
    }

    [Fact]
    public void IndexPage_ShouldHandleRetryButton()
    {
        // Arrange
        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ThrowsAsync(new Exception("Error de red"));

        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        var retryButton = component.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Reintentar"));
        retryButton.Should().NotBeNull();
        retryButton!.TextContent.Should().Contain("Reintentar");
    }

    [Fact]
    public void IndexPage_ShouldHaveCorrectLayout()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        component.Find(".bg-gray-50.min-h-screen").Should().NotBeNull();
        component.Find(".mx-auto.max-w-7xl.px-4.sm\\:px-6.lg\\:px-8.py-8").Should().NotBeNull();
    }

    [Fact]
    public void IndexPage_ShouldCallDashboardApi()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        _dashboardApiMock.Verify(x => x.ObtenerDashboardAsync(), Times.Once);
    }

    [Fact]
    public void IndexPage_ShouldHandleNullDashboardResponse()
    {
        // Arrange
        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync((DashboardResumenDto?)null);

        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        component.Should().NotBeNull();
        // El componente debería mostrar el estado de error cuando no hay datos
        component.FindAll("p").Should().Contain(p => p.TextContent.Contains("Error al cargar el dashboard"));
    }

    [Fact]
    public void IndexPage_ShouldHaveMaterialIcons()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        component.FindAll("span").Should().Contain(s => s.GetAttribute("class") != null && s.GetAttribute("class")!.Contains("material-symbols-outlined"));
    }

    [Fact]
    public void IndexPage_ShouldHaveProperStructure()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        component.Find("h1").Should().NotBeNull();
        component.FindAll("div").Should().HaveCountGreaterThan(0);
        component.FindAll("button").Should().HaveCountGreaterThan(0);
    }
}