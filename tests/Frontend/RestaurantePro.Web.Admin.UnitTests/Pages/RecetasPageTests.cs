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

public class RecetasPageTests : TestContext
{
    private readonly Mock<IRecetasApiService> _recetasApiMock;
    private readonly Mock<IProductosApiService> _productosApiMock;
    private readonly Mock<IInventarioApiService> _inventarioApiMock;

    public RecetasPageTests()
    {
        _recetasApiMock = new Mock<IRecetasApiService>();
        _productosApiMock = new Mock<IProductosApiService>();
        _inventarioApiMock = new Mock<IInventarioApiService>();
        
        Services.AddSingleton(_recetasApiMock.Object);
        Services.AddSingleton<IRecetasApiService>(_recetasApiMock.Object);
        Services.AddSingleton(_productosApiMock.Object);
        Services.AddSingleton<IProductosApiService>(_productosApiMock.Object);
        Services.AddSingleton(_inventarioApiMock.Object);
        Services.AddSingleton<IInventarioApiService>(_inventarioApiMock.Object);
        Services.AddSingleton<TestNavigationManager>();
        
        // Configurar JSInterop para manejar llamadas JavaScript
        JSInterop.SetupVoid("console.error", _ => true);
        JSInterop.SetupVoid("alert", _ => true);
        JSInterop.SetupVoid("window.print", _ => true);
    }

    [Fact]
    public void RecetasPage_ShouldRender()
    {
        // Arrange
        var recetas = new PaginatedList<RecetaDto>
        {
            Items = new List<RecetaDto>(),
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var productos = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };
        var ingredientes = new PaginatedList<IngredienteDto>
        {
            Items = new List<IngredienteDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };

        _recetasApiMock.Setup(x => x.ObtenerRecetasPaginadasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(recetas);
        _productosApiMock.Setup(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(productos);
        _inventarioApiMock.Setup(x => x.ObtenerIngredientesPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InventarioFiltrosDto>()))
            .ReturnsAsync(ingredientes);

        // Act
        var component = RenderComponent<Recetas>();

        // Assert
        component.Should().NotBeNull();
    }

    [Fact]
    public async Task RecetasPage_ShouldLoadRecetas()
    {
        // Arrange
        var recetas = new List<RecetaDto>
        {
            new RecetaDto
            {
                Id = Guid.NewGuid(),
                ProductoId = Guid.NewGuid(),
                NombreProducto = "Pizza Margherita",
                Preparacion = "Preparar la masa, agregar ingredientes y hornear",
                TiempoPreparacionMinutos = 30,
                CostoTotal = 15.50m,
                EstaActiva = true,
                FechaCreacion = DateTime.Now,
                Ingredientes = new List<IngredienteRecetaDto>()
            }
        };
        var paginacion = new PaginatedList<RecetaDto>
        {
            Items = recetas,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 1,
            TotalPages = 1
        };
        var productos = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };
        var ingredientes = new PaginatedList<IngredienteDto>
        {
            Items = new List<IngredienteDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };

        _recetasApiMock.Setup(x => x.ObtenerRecetasPaginadasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(paginacion);
        _productosApiMock.Setup(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(productos);
        _inventarioApiMock.Setup(x => x.ObtenerIngredientesPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InventarioFiltrosDto>()))
            .ReturnsAsync(ingredientes);

        // Act
        var component = RenderComponent<Recetas>();
        component.WaitForAssertion(() => component.FindAll("table tbody tr").Count.Should().BeGreaterThan(0));

        // Assert
        component.FindAll("table tbody tr").Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task RecetasPage_ShouldDisplayRecetas()
    {
        // Arrange
        var recetas = new List<RecetaDto>
        {
            new RecetaDto
            {
                Id = Guid.NewGuid(),
                ProductoId = Guid.NewGuid(),
                NombreProducto = "Pizza Margherita",
                Preparacion = "Preparar la masa, agregar ingredientes y hornear",
                TiempoPreparacionMinutos = 30,
                CostoTotal = 15.50m,
                EstaActiva = true,
                FechaCreacion = DateTime.Now,
                Ingredientes = new List<IngredienteRecetaDto>()
            },
            new RecetaDto
            {
                Id = Guid.NewGuid(),
                ProductoId = Guid.NewGuid(),
                NombreProducto = "Pasta Carbonara",
                Preparacion = "Cocer pasta, preparar salsa y mezclar",
                TiempoPreparacionMinutos = 25,
                CostoTotal = 12.75m,
                EstaActiva = false,
                FechaCreacion = DateTime.Now.AddDays(-1),
                Ingredientes = new List<IngredienteRecetaDto>()
            }
        };
        var paginacion = new PaginatedList<RecetaDto>
        {
            Items = recetas,
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 2,
            TotalPages = 1
        };
        var productos = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };
        var ingredientes = new PaginatedList<IngredienteDto>
        {
            Items = new List<IngredienteDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };

        _recetasApiMock.Setup(x => x.ObtenerRecetasPaginadasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(paginacion);
        _productosApiMock.Setup(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(productos);
        _inventarioApiMock.Setup(x => x.ObtenerIngredientesPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InventarioFiltrosDto>()))
            .ReturnsAsync(ingredientes);

        // Act
        var component = RenderComponent<Recetas>();
        component.WaitForAssertion(() => component.FindAll("table tbody tr").Count.Should().BeGreaterThan(0));

        // Assert
        component.FindAll("table tbody tr").Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void RecetasPage_ShouldHaveActionButtons()
    {
        // Arrange
        var recetas = new PaginatedList<RecetaDto>
        {
            Items = new List<RecetaDto>(),
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var productos = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };
        var ingredientes = new PaginatedList<IngredienteDto>
        {
            Items = new List<IngredienteDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };

        _recetasApiMock.Setup(x => x.ObtenerRecetasPaginadasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(recetas);
        _productosApiMock.Setup(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(productos);
        _inventarioApiMock.Setup(x => x.ObtenerIngredientesPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InventarioFiltrosDto>()))
            .ReturnsAsync(ingredientes);

        // Act
        var component = RenderComponent<Recetas>();

        // Assert - Buscar por texto en lugar de onclick
        component.FindAll("button").Should().HaveCountGreaterThan(0);
        var buttons = component.FindAll("button");
        buttons.Should().Contain(b => b.TextContent.Contains("Nueva Receta"));
        buttons.Should().Contain(b => b.TextContent.Contains("Aplicar Filtros"));
    }

    [Fact]
    public void RecetasPage_ShouldDisplayStatistics()
    {
        // Arrange
        var recetas = new PaginatedList<RecetaDto>
        {
            Items = new List<RecetaDto>(),
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var productos = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };
        var ingredientes = new PaginatedList<IngredienteDto>
        {
            Items = new List<IngredienteDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };

        _recetasApiMock.Setup(x => x.ObtenerRecetasPaginadasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(recetas);
        _productosApiMock.Setup(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(productos);
        _inventarioApiMock.Setup(x => x.ObtenerIngredientesPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InventarioFiltrosDto>()))
            .ReturnsAsync(ingredientes);

        // Act
        var component = RenderComponent<Recetas>();

        // Assert
        component.FindAll(".grid.grid-cols-1.sm\\:grid-cols-2.lg\\:grid-cols-4").Should().HaveCount(1);
        component.FindAll(".grid.grid-cols-1.sm\\:grid-cols-2.lg\\:grid-cols-4 .flex.flex-col").Should().HaveCount(4); // 4 tarjetas de estadísticas
    }

    [Fact]
    public void RecetasPage_ShouldHaveCorrectPageTitle()
    {
        // Arrange
        var recetas = new PaginatedList<RecetaDto>
        {
            Items = new List<RecetaDto>(),
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var productos = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };
        var ingredientes = new PaginatedList<IngredienteDto>
        {
            Items = new List<IngredienteDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };

        _recetasApiMock.Setup(x => x.ObtenerRecetasPaginadasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(recetas);
        _productosApiMock.Setup(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(productos);
        _inventarioApiMock.Setup(x => x.ObtenerIngredientesPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InventarioFiltrosDto>()))
            .ReturnsAsync(ingredientes);

        // Act
        var component = RenderComponent<Recetas>();

        // Assert - Buscar el título en el h1
        component.Find("h1").TextContent.Trim().Should().Be("Gestión de Recetas");
    }

    [Fact]
    public void RecetasPage_ShouldHaveFilterControls()
    {
        // Arrange
        var recetas = new PaginatedList<RecetaDto>
        {
            Items = new List<RecetaDto>(),
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var productos = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };
        var ingredientes = new PaginatedList<IngredienteDto>
        {
            Items = new List<IngredienteDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };

        _recetasApiMock.Setup(x => x.ObtenerRecetasPaginadasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(recetas);
        _productosApiMock.Setup(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(productos);
        _inventarioApiMock.Setup(x => x.ObtenerIngredientesPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InventarioFiltrosDto>()))
            .ReturnsAsync(ingredientes);

        // Act
        var component = RenderComponent<Recetas>();

        // Assert
        component.Find("input[placeholder='Nombre de la receta']").Should().NotBeNull();
        component.Find("select").Should().NotBeNull();
        component.FindAll("select").Should().HaveCount(3); // 3 selectores de filtro
    }

    [Fact]
    public void RecetasPage_ShouldHaveNewRecipeButton()
    {
        // Arrange
        var recetas = new PaginatedList<RecetaDto>
        {
            Items = new List<RecetaDto>(),
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var productos = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };
        var ingredientes = new PaginatedList<IngredienteDto>
        {
            Items = new List<IngredienteDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };

        _recetasApiMock.Setup(x => x.ObtenerRecetasPaginadasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(recetas);
        _productosApiMock.Setup(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(productos);
        _inventarioApiMock.Setup(x => x.ObtenerIngredientesPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InventarioFiltrosDto>()))
            .ReturnsAsync(ingredientes);

        // Act
        var component = RenderComponent<Recetas>();

        // Assert - Buscar por texto en lugar de onclick
        var buttons = component.FindAll("button");
        var newButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Nueva Receta"));
        newButton.Should().NotBeNull();
        newButton!.TextContent.Should().Contain("Nueva Receta");
    }

    [Fact]
    public void RecetasPage_ShouldHaveFilterButton()
    {
        // Arrange
        var recetas = new PaginatedList<RecetaDto>
        {
            Items = new List<RecetaDto>(),
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var productos = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };
        var ingredientes = new PaginatedList<IngredienteDto>
        {
            Items = new List<IngredienteDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };

        _recetasApiMock.Setup(x => x.ObtenerRecetasPaginadasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(recetas);
        _productosApiMock.Setup(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(productos);
        _inventarioApiMock.Setup(x => x.ObtenerIngredientesPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InventarioFiltrosDto>()))
            .ReturnsAsync(ingredientes);

        // Act
        var component = RenderComponent<Recetas>();

        // Assert - Buscar por texto en lugar de onclick
        var buttons = component.FindAll("button");
        var filterButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Aplicar Filtros"));
        filterButton.Should().NotBeNull();
        filterButton!.TextContent.Should().Contain("Aplicar Filtros");
    }

    [Fact]
    public void RecetasPage_ShouldHandleEmptyState()
    {
        // Arrange
        var recetas = new PaginatedList<RecetaDto>
        {
            Items = new List<RecetaDto>(),
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var productos = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };
        var ingredientes = new PaginatedList<IngredienteDto>
        {
            Items = new List<IngredienteDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };

        _recetasApiMock.Setup(x => x.ObtenerRecetasPaginadasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(recetas);
        _productosApiMock.Setup(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(productos);
        _inventarioApiMock.Setup(x => x.ObtenerIngredientesPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InventarioFiltrosDto>()))
            .ReturnsAsync(ingredientes);

        // Act
        var component = RenderComponent<Recetas>();

        // Assert
        component.FindAll("table tbody tr").Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void RecetasPage_ShouldHandleErrorState()
    {
        // Arrange
        _recetasApiMock.Setup(x => x.ObtenerRecetasPaginadasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ThrowsAsync(new Exception("Error de red"));
        _productosApiMock.Setup(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Error de red"));
        _inventarioApiMock.Setup(x => x.ObtenerIngredientesPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InventarioFiltrosDto>()))
            .ThrowsAsync(new Exception("Error de red"));

        // Act
        var component = RenderComponent<Recetas>();

        // Assert
        component.Should().NotBeNull();
    }

    [Fact]
    public void RecetasPage_ShouldHaveStatisticsCards()
    {
        // Arrange
        var recetas = new PaginatedList<RecetaDto>
        {
            Items = new List<RecetaDto>(),
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };
        var productos = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };
        var ingredientes = new PaginatedList<IngredienteDto>
        {
            Items = new List<IngredienteDto>(),
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 0,
            TotalPages = 0
        };

        _recetasApiMock.Setup(x => x.ObtenerRecetasPaginadasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(recetas);
        _productosApiMock.Setup(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(productos);
        _inventarioApiMock.Setup(x => x.ObtenerIngredientesPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InventarioFiltrosDto>()))
            .ReturnsAsync(ingredientes);

        // Act
        var component = RenderComponent<Recetas>();

        // Assert
        component.FindAll(".grid.grid-cols-1.sm\\:grid-cols-2.lg\\:grid-cols-4").Should().HaveCount(1);
        component.FindAll(".grid.grid-cols-1.sm\\:grid-cols-2.lg\\:grid-cols-4 .flex.flex-col").Should().HaveCount(4); // 4 tarjetas de estadísticas
    }
}
