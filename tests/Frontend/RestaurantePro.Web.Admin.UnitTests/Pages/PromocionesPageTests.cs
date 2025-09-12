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

namespace RestaurantePro.Web.Admin.UnitTests.Pages;

public class PromocionesPageTests : TestContext
{
    private readonly Mock<IPromocionesApiService> _promocionesApiMock;

    public PromocionesPageTests()
    {
        _promocionesApiMock = new Mock<IPromocionesApiService>();
        var productosApiMock = new Mock<IProductosApiService>();
        
        Services.AddSingleton(_promocionesApiMock.Object);
        Services.AddSingleton<IPromocionesApiService>(_promocionesApiMock.Object);
        Services.AddSingleton(productosApiMock.Object);
        Services.AddSingleton<IProductosApiService>(productosApiMock.Object);
        Services.AddSingleton<TestNavigationManager>();
        
        // Configurar JSInterop para manejar llamadas JavaScript
        JSInterop.SetupVoid("alert", _ => true);
        JSInterop.Setup<bool>("confirm", _ => true);
    }

    [Fact]
    public void PromocionesPage_ShouldRenderCorrectly()
    {
        // Arrange
        var promociones = new List<PromocionDto>
        {
            new PromocionDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Descuento 20%",
                Codigo = "DESC20",
                Tipo = TipoPromocion.Porcentaje,
                ValorDescuento = 20,
                FechaInicio = DateTime.Today,
                FechaFin = DateTime.Today.AddDays(30),
                EstaActiva = true
            }
        };

        var paginacion = new PaginatedList<PromocionDto>
        {
            Items = promociones,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 1,
            TotalPages = 1
        };
        var estadisticas = new PromocionEstadisticasDto
        {
            TotalPromociones = 10,
            PromocionesActivas = 5,
            PromocionesPendientes = 2,
            PromocionesExpiradas = 3,
            TotalUsos = 150,
            DescuentoTotalAplicado = 5000
        };

        _promocionesApiMock.Setup(x => x.ObtenerPromocionesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(paginacion);
        _promocionesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Promociones>();

        // Assert
        component.Should().NotBeNull();
        component.Find("h1").TextContent.Should().Be("Gestión de Promociones");
    }

    [Fact]
    public async Task PromocionesPage_ShouldLoadPromocionesOnInit()
    {
        // Arrange
        var promociones = new List<PromocionDto>
        {
            new PromocionDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Descuento 20%",
                Codigo = "DESC20",
                Tipo = TipoPromocion.Porcentaje,
                ValorDescuento = 20,
                FechaInicio = DateTime.Today,
                FechaFin = DateTime.Today.AddDays(30),
                EstaActiva = true
            }
        };

        var paginacion = new PaginatedList<PromocionDto>
        {
            Items = promociones,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 1,
            TotalPages = 1
        };
        var estadisticas = new PromocionEstadisticasDto
        {
            TotalPromociones = 10,
            PromocionesActivas = 5,
            PromocionesPendientes = 2,
            PromocionesExpiradas = 3,
            TotalUsos = 150,
            DescuentoTotalAplicado = 5000
        };

        _promocionesApiMock.Setup(x => x.ObtenerPromocionesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(paginacion);
        _promocionesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Promociones>();
        component.WaitForAssertion(() => component.FindAll(".card").Count.Should().BeGreaterThan(0));

        // Assert
        _promocionesApiMock.Verify(x => x.ObtenerPromocionesAsync(1, 20), Times.Once);
        _promocionesApiMock.Verify(x => x.ObtenerEstadisticasAsync(), Times.Once);
    }

    [Fact]
    public void PromocionesPage_ShouldDisplayPromocionesList()
    {
        // Arrange
        var promociones = new List<PromocionDto>
        {
            new PromocionDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Descuento 20%",
                Codigo = "DESC20",
                Tipo = TipoPromocion.Porcentaje,
                ValorDescuento = 20,
                FechaInicio = DateTime.Today,
                FechaFin = DateTime.Today.AddDays(30),
                EstaActiva = true
            },
            new PromocionDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Descuento $50",
                Codigo = "DESC50",
                Tipo = TipoPromocion.MontoFijo,
                ValorDescuento = 50,
                FechaInicio = DateTime.Today,
                FechaFin = DateTime.Today.AddDays(15),
                EstaActiva = false
            }
        };

        var paginacion = new PaginatedList<PromocionDto>
        {
            Items = promociones,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 2,
            TotalPages = 1
        };
        var estadisticas = new PromocionEstadisticasDto
        {
            TotalPromociones = 2,
            PromocionesActivas = 1,
            PromocionesPendientes = 0,
            PromocionesExpiradas = 1,
            TotalUsos = 25,
            DescuentoTotalAplicado = 1000
        };

        _promocionesApiMock.Setup(x => x.ObtenerPromocionesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(paginacion);
        _promocionesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Promociones>();

        // Assert
        component.FindAll(".card").Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void PromocionesPage_ShouldHaveActionButtons()
    {
        // Arrange
        var promociones = new List<PromocionDto>();
        var paginacion = new PaginatedList<PromocionDto>
        {
            Items = promociones,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var estadisticas = new PromocionEstadisticasDto();

        _promocionesApiMock.Setup(x => x.ObtenerPromocionesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(paginacion);
        _promocionesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Promociones>();

        // Assert - Buscar por texto en lugar de onclick
        component.FindAll("button").Should().HaveCountGreaterThan(0);
        var buttons = component.FindAll("button");
        buttons.Should().Contain(b => b.TextContent.Contains("Filtros"));
        buttons.Should().Contain(b => b.TextContent.Contains("Nueva Promoción"));
    }

    [Fact]
    public void PromocionesPage_ShouldDisplayStatistics()
    {
        // Arrange
        var promociones = new List<PromocionDto>();
        var paginacion = new PaginatedList<PromocionDto>
        {
            Items = promociones,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var estadisticas = new PromocionEstadisticasDto
        {
            TotalPromociones = 10,
            PromocionesActivas = 5,
            PromocionesPendientes = 2,
            PromocionesExpiradas = 3,
            TotalUsos = 150,
            DescuentoTotalAplicado = 5000
        };

        _promocionesApiMock.Setup(x => x.ObtenerPromocionesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(paginacion);
        _promocionesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Promociones>();

        // Assert
        component.FindAll(".card").Should().HaveCountGreaterThan(0);
        component.Markup.Should().Contain("10"); // TotalPromociones
        component.Markup.Should().Contain("5");  // PromocionesActivas
        component.Markup.Should().Contain("2");  // PromocionesPendientes
        component.Markup.Should().Contain("3");  // PromocionesExpiradas
        component.Markup.Should().Contain("150"); // TotalUsos
    }

    [Fact]
    public void PromocionesPage_ShouldHandleEmptyState()
    {
        // Arrange
        var promociones = new List<PromocionDto>();
        var paginacion = new PaginatedList<PromocionDto>
        {
            Items = promociones,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var estadisticas = new PromocionEstadisticasDto();

        _promocionesApiMock.Setup(x => x.ObtenerPromocionesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(paginacion);
        _promocionesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Promociones>();

        // Assert
        component.Should().NotBeNull();
        component.Find("h1").TextContent.Should().Be("Gestión de Promociones");
    }

    [Fact]
    public async Task PromocionesPage_ShouldHandleErrorWhenLoadingPromociones()
    {
        // Arrange
        _promocionesApiMock.Setup(x => x.ObtenerPromocionesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("Error de red"));
        _promocionesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ThrowsAsync(new Exception("Error de red"));

        // Act
        var component = RenderComponent<Promociones>();

        // Assert
        component.Should().NotBeNull();
        component.Find("h1").TextContent.Should().Be("Gestión de Promociones");
    }

    [Fact]
    public void PromocionesPage_ShouldBeResponsive()
    {
        // Arrange
        var promociones = new List<PromocionDto>();
        var paginacion = new PaginatedList<PromocionDto>
        {
            Items = promociones,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var estadisticas = new PromocionEstadisticasDto();

        _promocionesApiMock.Setup(x => x.ObtenerPromocionesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(paginacion);
        _promocionesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Promociones>();

        // Assert
        component.Find(".d-sm-flex").Should().NotBeNull();
        component.FindAll(".col-md-2").Should().HaveCount(6); // 6 tarjetas de estadísticas
    }

    [Fact]
    public void PromocionesPage_ShouldHaveCorrectPageTitle()
    {
        // Arrange
        var promociones = new List<PromocionDto>();
        var paginacion = new PaginatedList<PromocionDto>
        {
            Items = promociones,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var estadisticas = new PromocionEstadisticasDto();

        _promocionesApiMock.Setup(x => x.ObtenerPromocionesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(paginacion);
        _promocionesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Promociones>();

        // Assert - Buscar el título en el h1
        component.Find("h1").TextContent.Should().Be("Gestión de Promociones");
    }

    [Fact]
    public void PromocionesPage_ShouldHaveFilterButton()
    {
        // Arrange
        var promociones = new List<PromocionDto>();
        var paginacion = new PaginatedList<PromocionDto>
        {
            Items = promociones,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var estadisticas = new PromocionEstadisticasDto();

        _promocionesApiMock.Setup(x => x.ObtenerPromocionesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(paginacion);
        _promocionesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Promociones>();

        // Assert - Buscar por texto en lugar de onclick
        var buttons = component.FindAll("button");
        var filterButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Filtros"));
        filterButton.Should().NotBeNull();
        filterButton!.TextContent.Should().Contain("Filtros");
    }

    [Fact]
    public void PromocionesPage_ShouldHaveNewPromocionButton()
    {
        // Arrange
        var promociones = new List<PromocionDto>();
        var paginacion = new PaginatedList<PromocionDto>
        {
            Items = promociones,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var estadisticas = new PromocionEstadisticasDto();

        _promocionesApiMock.Setup(x => x.ObtenerPromocionesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(paginacion);
        _promocionesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Promociones>();

        // Assert - Buscar por texto en lugar de onclick
        var buttons = component.FindAll("button");
        var newButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Nueva Promoción"));
        newButton.Should().NotBeNull();
        newButton!.TextContent.Should().Contain("Nueva Promoción");
    }
}
