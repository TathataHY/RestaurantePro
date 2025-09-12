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

public class ProveedoresPageTests : TestContext
{
    private readonly Mock<IProveedoresApiService> _proveedoresApiMock;

    public ProveedoresPageTests()
    {
        _proveedoresApiMock = new Mock<IProveedoresApiService>();
        Services.AddSingleton(_proveedoresApiMock.Object);
        Services.AddSingleton<IProveedoresApiService>(_proveedoresApiMock.Object);
        Services.AddSingleton<TestNavigationManager>();
        
        // Configurar JSInterop para manejar llamadas JavaScript
        JSInterop.SetupVoid("alert", _ => true);
        JSInterop.Setup<bool>("confirm", _ => true);
        JSInterop.SetupVoid("descargarArchivo", _ => true);
    }

    [Fact]
    public void ProveedoresPage_ShouldRender()
    {
        // Arrange
        var proveedores = new List<ProveedorDto>();
        var paginacion = new PaginatedList<ProveedorDto>
        {
            Items = proveedores,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var estadisticas = new ProveedorEstadisticasDto();

        _proveedoresApiMock.Setup(x => x.GetProveedoresPaginados(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProveedorFiltrosDto>()))
            .ReturnsAsync(paginacion);
        _proveedoresApiMock.Setup(x => x.GetProveedorEstadisticas())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Proveedores>();

        // Assert
        component.Should().NotBeNull();
    }

    [Fact]
    public async Task ProveedoresPage_ShouldLoadProveedores()
    {
        // Arrange
        var proveedores = new List<ProveedorDto>
        {
            new ProveedorDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Proveedor Test 1",
                Ruc = "12345678901",
                Ciudad = "Quito",
                Telefono = "0987654321",
                Email = "test1@proveedor.com",
                EstaActivo = true,
                TotalContactos = 2,
                TotalOrdenesCompra = 5,
                MontoTotalCompras = 15000.00m
            }
        };
        var paginacion = new PaginatedList<ProveedorDto>
        {
            Items = proveedores,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 1,
            TotalPages = 1
        };
        var estadisticas = new ProveedorEstadisticasDto();

        _proveedoresApiMock.Setup(x => x.GetProveedoresPaginados(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProveedorFiltrosDto>()))
            .ReturnsAsync(paginacion);
        _proveedoresApiMock.Setup(x => x.GetProveedorEstadisticas())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Proveedores>();
        component.WaitForAssertion(() => component.FindAll(".card").Count.Should().BeGreaterThan(0));

        // Assert
        component.FindAll(".card").Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task ProveedoresPage_ShouldDisplayProveedores()
    {
        // Arrange
        var proveedores = new List<ProveedorDto>
        {
            new ProveedorDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Proveedor Test 1",
                Ruc = "12345678901",
                Ciudad = "Quito",
                Telefono = "0987654321",
                Email = "test1@proveedor.com",
                EstaActivo = true,
                TotalContactos = 2,
                TotalOrdenesCompra = 5,
                MontoTotalCompras = 15000.00m
            },
            new ProveedorDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Proveedor Test 2",
                Ruc = "98765432109",
                Ciudad = "Guayaquil",
                Telefono = "0912345678",
                Email = "test2@proveedor.com",
                EstaActivo = false,
                TotalContactos = 1,
                TotalOrdenesCompra = 3,
                MontoTotalCompras = 8500.00m
            }
        };
        var paginacion = new PaginatedList<ProveedorDto>
        {
            Items = proveedores,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 2,
            TotalPages = 1
        };
        var estadisticas = new ProveedorEstadisticasDto();

        _proveedoresApiMock.Setup(x => x.GetProveedoresPaginados(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProveedorFiltrosDto>()))
            .ReturnsAsync(paginacion);
        _proveedoresApiMock.Setup(x => x.GetProveedorEstadisticas())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Proveedores>();
        component.WaitForAssertion(() => component.FindAll(".card").Count.Should().BeGreaterThan(0));

        // Assert
        component.FindAll(".card").Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void ProveedoresPage_ShouldHaveActionButtons()
    {
        // Arrange
        var proveedores = new List<ProveedorDto>();
        var paginacion = new PaginatedList<ProveedorDto>
        {
            Items = proveedores,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var estadisticas = new ProveedorEstadisticasDto();

        _proveedoresApiMock.Setup(x => x.GetProveedoresPaginados(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProveedorFiltrosDto>()))
            .ReturnsAsync(paginacion);
        _proveedoresApiMock.Setup(x => x.GetProveedorEstadisticas())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Proveedores>();

        // Assert - Buscar por texto en lugar de onclick
        component.FindAll("button").Should().HaveCountGreaterThan(0);
        var buttons = component.FindAll("button");
        buttons.Should().Contain(b => b.TextContent.Contains("Nuevo Proveedor"));
        buttons.Should().Contain(b => b.TextContent.Contains("Exportar Excel"));
        buttons.Should().Contain(b => b.TextContent.Contains("Actualizar"));
    }

    [Fact]
    public void ProveedoresPage_ShouldDisplayStatistics()
    {
        // Arrange
        var proveedores = new List<ProveedorDto>();
        var paginacion = new PaginatedList<ProveedorDto>
        {
            Items = proveedores,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var estadisticas = new ProveedorEstadisticasDto
        {
            TotalProveedores = 25,
            ProveedoresActivos = 20,
            TotalContactos = 45,
            TotalOrdenesCompra = 120,
            MontoTotalCompras = 150000.00m,
            MontoPromedioCompras = 1250.00m
        };

        _proveedoresApiMock.Setup(x => x.GetProveedoresPaginados(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProveedorFiltrosDto>()))
            .ReturnsAsync(paginacion);
        _proveedoresApiMock.Setup(x => x.GetProveedorEstadisticas())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Proveedores>();

        // Assert
        component.FindAll(".col-md-2").Should().HaveCount(6); // 6 tarjetas de estadísticas
    }

    [Fact]
    public void ProveedoresPage_ShouldHaveCorrectPageTitle()
    {
        // Arrange
        var proveedores = new List<ProveedorDto>();
        var paginacion = new PaginatedList<ProveedorDto>
        {
            Items = proveedores,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var estadisticas = new ProveedorEstadisticasDto();

        _proveedoresApiMock.Setup(x => x.GetProveedoresPaginados(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProveedorFiltrosDto>()))
            .ReturnsAsync(paginacion);
        _proveedoresApiMock.Setup(x => x.GetProveedorEstadisticas())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Proveedores>();

        // Assert - Buscar el título en el h2
        component.Find("h2").TextContent.Trim().Should().Be("Gestión de Proveedores");
    }

    [Fact]
    public void ProveedoresPage_ShouldHaveViewToggleButtons()
    {
        // Arrange
        var proveedores = new List<ProveedorDto>();
        var paginacion = new PaginatedList<ProveedorDto>
        {
            Items = proveedores,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var estadisticas = new ProveedorEstadisticasDto();

        _proveedoresApiMock.Setup(x => x.GetProveedoresPaginados(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProveedorFiltrosDto>()))
            .ReturnsAsync(paginacion);
        _proveedoresApiMock.Setup(x => x.GetProveedorEstadisticas())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Proveedores>();

        // Assert - Buscar por texto en lugar de onclick
        var buttons = component.FindAll("button");
        var tablaButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Tabla"));
        var tarjetasButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Tarjetas"));
        
        tablaButton.Should().NotBeNull();
        tarjetasButton.Should().NotBeNull();
    }

    [Fact]
    public void ProveedoresPage_ShouldHaveExportButton()
    {
        // Arrange
        var proveedores = new List<ProveedorDto>();
        var paginacion = new PaginatedList<ProveedorDto>
        {
            Items = proveedores,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var estadisticas = new ProveedorEstadisticasDto();

        _proveedoresApiMock.Setup(x => x.GetProveedoresPaginados(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProveedorFiltrosDto>()))
            .ReturnsAsync(paginacion);
        _proveedoresApiMock.Setup(x => x.GetProveedorEstadisticas())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Proveedores>();

        // Assert - Buscar por texto en lugar de onclick
        var buttons = component.FindAll("button");
        var exportButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Exportar Excel"));
        exportButton.Should().NotBeNull();
        exportButton!.TextContent.Should().Contain("Exportar Excel");
    }

    [Fact]
    public void ProveedoresPage_ShouldHaveRefreshButton()
    {
        // Arrange
        var proveedores = new List<ProveedorDto>();
        var paginacion = new PaginatedList<ProveedorDto>
        {
            Items = proveedores,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var estadisticas = new ProveedorEstadisticasDto();

        _proveedoresApiMock.Setup(x => x.GetProveedoresPaginados(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProveedorFiltrosDto>()))
            .ReturnsAsync(paginacion);
        _proveedoresApiMock.Setup(x => x.GetProveedorEstadisticas())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Proveedores>();

        // Assert - Buscar por texto en lugar de onclick
        var buttons = component.FindAll("button");
        var refreshButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Actualizar"));
        refreshButton.Should().NotBeNull();
        refreshButton!.TextContent.Should().Contain("Actualizar");
    }

    [Fact]
    public void ProveedoresPage_ShouldHandleEmptyState()
    {
        // Arrange
        var proveedores = new List<ProveedorDto>();
        var paginacion = new PaginatedList<ProveedorDto>
        {
            Items = proveedores,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var estadisticas = new ProveedorEstadisticasDto();

        _proveedoresApiMock.Setup(x => x.GetProveedoresPaginados(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProveedorFiltrosDto>()))
            .ReturnsAsync(paginacion);
        _proveedoresApiMock.Setup(x => x.GetProveedorEstadisticas())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Proveedores>();

        // Assert
        component.FindAll(".card").Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void ProveedoresPage_ShouldHandleErrorState()
    {
        // Arrange
        _proveedoresApiMock.Setup(x => x.GetProveedoresPaginados(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProveedorFiltrosDto>()))
            .ThrowsAsync(new Exception("Error de red"));
        _proveedoresApiMock.Setup(x => x.GetProveedorEstadisticas())
            .ThrowsAsync(new Exception("Error de red"));

        // Act
        var component = RenderComponent<Proveedores>();

        // Assert
        component.Should().NotBeNull();
    }

    [Fact]
    public void ProveedoresPage_ShouldHaveStatisticsCards()
    {
        // Arrange
        var proveedores = new List<ProveedorDto>();
        var paginacion = new PaginatedList<ProveedorDto>
        {
            Items = proveedores,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var estadisticas = new ProveedorEstadisticasDto
        {
            TotalProveedores = 25,
            ProveedoresActivos = 20,
            TotalContactos = 45,
            TotalOrdenesCompra = 120,
            MontoTotalCompras = 150000.00m,
            MontoPromedioCompras = 1250.00m
        };

        _proveedoresApiMock.Setup(x => x.GetProveedoresPaginados(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProveedorFiltrosDto>()))
            .ReturnsAsync(paginacion);
        _proveedoresApiMock.Setup(x => x.GetProveedorEstadisticas())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Proveedores>();

        // Assert
        component.FindAll(".col-md-2").Should().HaveCount(6); // 6 tarjetas de estadísticas
    }
}
