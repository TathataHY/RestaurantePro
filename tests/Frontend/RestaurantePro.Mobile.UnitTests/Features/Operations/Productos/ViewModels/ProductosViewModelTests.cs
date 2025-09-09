using AutoFixture;
using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Productos;

namespace RestaurantePro.Mobile.UnitTests.Features.Operations.Productos.ViewModels;

public class ProductosViewModelTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IProductosService> _mockProductosService = new();
    private readonly Mock<IDialogService> _mockDialog = new();
    private readonly Mock<INavigationService> _mockNav = new();

    [Fact]
    public void Constructor_InitializesProperties()
    {
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        
        vm.Title.Should().Be("Productos");
        vm.Productos.Should().NotBeNull();
        vm.Categorias.Should().NotBeNull();
        vm.TextoBusqueda.Should().BeEmpty();
        vm.MostrarSoloDisponibles.Should().BeTrue();
        vm.MostrarFiltros.Should().BeFalse();
        vm.TotalProductos.Should().Be(0);
        vm.ProductosDisponibles.Should().Be(0);
        vm.ProductosAgotados.Should().Be(0);
    }

    [Fact]
    public async Task LoadProductosAsync_SuccessfulLoad_PopulatesProductos()
    {
        var productos = new List<ProductoDto>
        {
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Producto 1", Precio = 10.0m, Activo = true, CantidadDisponible = 5 },
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Producto 2", Precio = 15.0m, Activo = true, CantidadDisponible = 3 }
        };
        _mockProductosService.Setup(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(productos));

        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        await vm.LoadProductosCommand.ExecuteAsync(null);

        vm.Productos.Should().HaveCount(2);
        vm.TieneProductos.Should().BeTrue();
        vm.TotalProductos.Should().Be(2);
        vm.ProductosDisponibles.Should().Be(2);
        vm.ProductosAgotados.Should().Be(0);
        vm.PorcentajeDisponibilidad.Should().Be(100.0);
    }

    [Fact]
    public async Task LoadProductosAsync_ErrorResponse_SetsErrorMessage()
    {
        _mockProductosService.Setup(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.ErrorResponse(new List<string> { "Error de servicio" }, "Error", 500));

        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        await vm.LoadProductosCommand.ExecuteAsync(null);

        vm.HasError.Should().BeTrue();
        vm.ErrorMessage.Should().Contain("Error");
        _mockDialog.Verify(x => x.ShowErrorAsync(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task LoadCategoriasAsync_SuccessfulLoad_PopulatesCategorias()
    {
        var categorias = new List<CategoriaProductoDto>
        {
            new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Categoría 1" },
            new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Categoría 2" }
        };
        _mockProductosService.Setup(x => x.ObtenerCategoriasAsync())
            .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias));

        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        await vm.LoadCategoriasCommand.ExecuteAsync(null);

        vm.Categorias.Should().HaveCount(2);
        vm.TieneCategorias.Should().BeTrue();
    }

    [Fact]
    public async Task BuscarProductosAsync_WithText_ShouldSearchProducts()
    {
        var productos = new List<ProductoDto>
        {
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Pizza", Precio = 10.0m, Activo = true, CantidadDisponible = 5 }
        };
        _mockProductosService.Setup(x => x.BuscarProductosAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(productos));

        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        vm.TextoBusqueda = "Pizza";
        await vm.BuscarProductosCommand.ExecuteAsync(null);

        vm.Productos.Should().HaveCount(1);
        vm.Productos.First().Nombre.Should().Be("Pizza");
        _mockProductosService.Verify(x => x.BuscarProductosAsync("Pizza", true), Times.Once);
    }

    [Fact]
    public async Task FiltrarPorCategoriaAsync_WithCategoria_ShouldFilterProducts()
    {
        var categoria = new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Bebidas" };
        var productos = new List<ProductoDto>
        {
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Coca Cola", CategoriaId = categoria.Id, Activo = true, CantidadDisponible = 10 }
        };
        _mockProductosService.Setup(x => x.ObtenerProductosPorCategoriaAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(productos));

        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        await vm.FiltrarPorCategoriaCommand.ExecuteAsync(categoria);

        vm.SelectedCategoria.Should().Be(categoria);
        vm.Productos.Should().HaveCount(1);
        _mockProductosService.Verify(x => x.ObtenerProductosPorCategoriaAsync(categoria.Id, true), Times.Once);
    }

    [Fact]
    public async Task FiltrarPorCategoriaAsync_WithNullCategoria_ShouldClearFilter()
    {
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        vm.SelectedCategoria = new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Test" };
        
        await vm.FiltrarPorCategoriaCommand.ExecuteAsync(null);

        vm.SelectedCategoria.Should().BeNull();
    }

    [Fact]
    public async Task ToggleSoloDisponiblesAsync_ShouldToggleFilter()
    {
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        vm.MostrarSoloDisponibles = true;

        await vm.ToggleSoloDisponiblesCommand.ExecuteAsync(null);

        vm.MostrarSoloDisponibles.Should().BeFalse();
    }

    [Fact]
    public async Task LimpiarBusquedaAsync_ShouldClearSearch()
    {
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        vm.TextoBusqueda = "test";
        vm.SelectedCategoria = new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Test" };

        await vm.LimpiarBusquedaCommand.ExecuteAsync(null);

        vm.TextoBusqueda.Should().BeEmpty();
        vm.SelectedCategoria.Should().BeNull();
    }

    [Fact]
    public async Task VerDetalleProductoAsync_WithProducto_ShouldNavigate()
    {
        var producto = new ProductoDto { Id = Guid.NewGuid(), Nombre = "Test Product" };
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);

        await vm.VerDetalleProductoCommand.ExecuteAsync(producto);

        _mockNav.Verify(x => x.NavigateToAsync("producto-detalle", It.Is<Dictionary<string, object>>(p => p.ContainsKey("productoId"))), Times.Once);
    }

    [Fact]
    public async Task VerDetalleProductoAsync_WithNullProducto_ShouldNotNavigate()
    {
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);

        await vm.VerDetalleProductoCommand.ExecuteAsync(null);

        _mockNav.Verify(x => x.NavigateToAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
    }

    [Fact]
    public async Task AgregarAComandaAsync_WithProducto_ShouldNavigate()
    {
        var producto = new ProductoDto { Id = Guid.NewGuid(), Nombre = "Test Product", Activo = true, CantidadDisponible = 5 };
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);

        await vm.AgregarAComandaCommand.ExecuteAsync(producto);

        _mockNav.Verify(x => x.NavigateToAsync("comandas", It.Is<Dictionary<string, object>>(p => p.ContainsKey("productoId"))), Times.Once);
    }

    [Fact]
    public async Task AgregarAComandaAsync_WithNullProducto_ShouldNotNavigate()
    {
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);

        await vm.AgregarAComandaCommand.ExecuteAsync(null);

        _mockNav.Verify(x => x.NavigateToAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
    }

    [Fact]
    public void MostrarFiltrosCommand_ShouldToggleFiltros()
    {
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        vm.MostrarFiltros = false;

        vm.MostrarFiltrosCommandCommand.Execute(null);

        vm.MostrarFiltros.Should().BeTrue();
    }

    [Fact]
    public async Task LoadProductosPopularesAsync_ShouldLoadPopularProducts()
    {
        var productos = new List<ProductoDto>
        {
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Popular 1", Activo = true, CantidadDisponible = 5 },
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Popular 2", Activo = true, CantidadDisponible = 3 }
        };
        _mockProductosService.Setup(x => x.ObtenerProductosPopularesAsync(It.IsAny<int>()))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(productos));

        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        await vm.LoadProductosPopularesCommand.ExecuteAsync(null);

        vm.Productos.Should().HaveCount(2);
        _mockProductosService.Verify(x => x.ObtenerProductosPopularesAsync(10), Times.Once);
    }

    [Theory]
    [InlineData("", null, "No hay productos disponibles")]
    [InlineData("test", null, "No se encontraron productos para 'test'")]
    [InlineData("", "Categoría Test", "No hay productos en la categoría 'Categoría Test'")]
    public void MensajeSinProductos_ShouldReturnCorrectMessage(string busqueda, string? categoriaNombre, string expectedMessage)
    {
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        vm.TextoBusqueda = busqueda;
        vm.SelectedCategoria = categoriaNombre != null ? new CategoriaProductoDto { Nombre = categoriaNombre } : null;

        vm.MensajeSinProductos.Should().Be(expectedMessage);
    }

    [Fact]
    public void PorcentajeDisponibilidad_WithProducts_ShouldCalculateCorrectly()
    {
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        vm.TotalProductos = 10;
        vm.ProductosDisponibles = 7;

        vm.PorcentajeDisponibilidad.Should().Be(70.0);
    }

    [Fact]
    public void PorcentajeDisponibilidad_WithZeroTotal_ShouldReturnZero()
    {
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        vm.TotalProductos = 0;
        vm.ProductosDisponibles = 0;

        vm.PorcentajeDisponibilidad.Should().Be(0.0);
    }

    [Fact]
    public async Task InitializeAsync_ShouldLoadData()
    {
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);

        await vm.InitializeAsync();

        _mockProductosService.Verify(x => x.ObtenerCategoriasAsync(), Times.Exactly(2));
        _mockProductosService.Verify(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public void Cleanup_ShouldClearCollections()
    {
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        vm.Productos.Add(new ProductoDto { Id = Guid.NewGuid(), Nombre = "Test" });
        vm.Categorias.Add(new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Test" });

        vm.Cleanup();

        vm.Productos.Should().BeEmpty();
        vm.Categorias.Should().BeEmpty();
    }

    [Fact]
    public void SelectedProducto_WhenSet_ShouldUpdateProperty()
    {
        var producto = new ProductoDto { Id = Guid.NewGuid(), Nombre = "Test Product" };
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        
        vm.SelectedProducto = producto;
        
        vm.SelectedProducto.Should().Be(producto);
    }

    [Fact]
    public void TextoBusqueda_WhenSet_ShouldUpdateProperty()
    {
        var busqueda = "test search";
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        
        vm.TextoBusqueda = busqueda;
        
        vm.TextoBusqueda.Should().Be(busqueda);
    }

    [Fact]
    public void MostrarSoloDisponibles_WhenSet_ShouldUpdateProperty()
    {
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        
        vm.MostrarSoloDisponibles = false;
        
        vm.MostrarSoloDisponibles.Should().BeFalse();
    }

    [Fact]
    public async Task LoadProductosAsync_WhenIsBusy_ShouldNotCallService()
    {
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);

        vm.IsBusy = true;

        await vm.LoadProductosCommand.ExecuteAsync(null);

        _mockProductosService.Verify(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>()), Times.Never);
        _mockProductosService.Verify(x => x.ObtenerCategoriasAsync(), Times.Never);
    }

    [Fact]
    public async Task PropertyChanged_ComputedProperties_ShouldRaiseAfterSuccessfulLoad()
    {
        var productos = new List<ProductoDto>
        {
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "P1", Activo = true, CantidadDisponible = 1, Precio = 10 },
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "P2", Activo = false, CantidadDisponible = 0, Precio = 20 }
        };
        _mockProductosService
            .Setup(x => x.ObtenerCategoriasAsync())
            .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(new List<CategoriaProductoDto>()));
        _mockProductosService
            .Setup(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(productos));

        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        var raised = new List<string>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

        await vm.LoadProductosCommand.ExecuteAsync(null);

        raised.Should().Contain(nameof(ProductosViewModel.TieneProductos));
        raised.Should().Contain(nameof(ProductosViewModel.PorcentajeDisponibilidad));
        raised.Should().Contain(nameof(ProductosViewModel.MensajeSinProductos));
        raised.Should().Contain(nameof(ProductosViewModel.Estadisticas));
    }

    [Fact]
    public void PropertyChanged_TextoBusqueda_ShouldRaise()
    {
        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        var raised = new List<string>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

        vm.TextoBusqueda = "pizza";

        raised.Should().Contain(nameof(ProductosViewModel.TextoBusqueda));
    }

    [Fact]
    public async Task LoadProductosAsync_WithEmptyResult_ShouldSetEmptyState()
    {
        _mockProductosService
            .Setup(x => x.ObtenerCategoriasAsync())
            .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(new List<CategoriaProductoDto>()));
        _mockProductosService
            .Setup(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(new List<ProductoDto>()));

        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        await vm.LoadProductosCommand.ExecuteAsync(null);

        vm.Productos.Should().BeEmpty();
        vm.TieneProductos.Should().BeFalse();
        vm.MensajeSinProductos.Should().Be("No hay productos disponibles");
    }

    [Fact]
    public async Task LoadProductosAsync_ShouldCallGetPagedWithDefaultPaging_WhenNoFilters()
    {
        _mockProductosService
            .Setup(x => x.ObtenerCategoriasAsync())
            .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(new List<CategoriaProductoDto>()));
        _mockProductosService
            .Setup(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(new List<ProductoDto>()));

        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);
        await vm.LoadProductosCommand.ExecuteAsync(null);

        _mockProductosService.Verify(x => x.ObtenerProductosPaginadosAsync(
            It.Is<int>(n => n == 1),
            It.Is<int>(s => s == 100),
            It.Is<string?>(f => f == null),
            It.Is<bool>(a => a == true)
        ), Times.Once);
    }

    [Fact]
    public async Task BuscarProductosAsync_ShouldDebounceRapidCalls_OnlyOneServiceCall()
    {
        _mockProductosService
            .Setup(x => x.ObtenerCategoriasAsync())
            .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(new List<CategoriaProductoDto>()));
        _mockProductosService
            .Setup(x => x.BuscarProductosAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(new List<ProductoDto>()));

        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object)
        {
            DebounceDelayMs = 50,
            TextoBusqueda = "p"
        };

        // Lanzar varias llamadas rápidas
        var t1 = vm.BuscarProductosCommand.ExecuteAsync(null);
        vm.TextoBusqueda = "pi";
        var t2 = vm.BuscarProductosCommand.ExecuteAsync(null);
        vm.TextoBusqueda = "piz";
        var t3 = vm.BuscarProductosCommand.ExecuteAsync(null);
        vm.TextoBusqueda = "pizza";
        var t4 = vm.BuscarProductosCommand.ExecuteAsync(null);

        await Task.WhenAll(t1, t2, t3, t4);
        // Esperar un poco más del debounce para asegurar ejecución
        await Task.Delay(100);

        _mockProductosService.Verify(x => x.BuscarProductosAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task LoadProductosPopularesAsync_ErrorResponse_ShouldShowError()
    {
        _mockProductosService
            .Setup(x => x.ObtenerProductosPopularesAsync(It.IsAny<int>()))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.ErrorResponse(new List<string>{"err"}, "Error al cargar", 500));

        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);

        await vm.LoadProductosPopularesCommand.ExecuteAsync(null);

        _mockDialog.Verify(x => x.ShowErrorAsync("Error al cargar productos populares"), Times.Once);
    }

    [Fact]
    public async Task LoadProductosPopularesAsync_WhenException_ShouldShowUnexpectedError()
    {
        _mockProductosService
            .Setup(x => x.ObtenerProductosPopularesAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("boom"));

        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);

        await vm.LoadProductosPopularesCommand.ExecuteAsync(null);

        _mockDialog.Verify(x => x.ShowErrorAsync(It.Is<string>(s => s.Contains("Error inesperado"))), Times.Once);
    }

    [Fact]
    public async Task LoadCategoriasAsync_WhenException_ShouldShowError()
    {
        _mockProductosService
            .Setup(x => x.ObtenerCategoriasAsync())
            .ThrowsAsync(new Exception("net"));

        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object);

        await vm.LoadCategoriasCommand.ExecuteAsync(null);

        _mockDialog.Verify(x => x.ShowErrorAsync("Error al cargar categorías"), Times.Once);
    }

    [Fact]
    public async Task LoadProductosAsync_FilterByCategoria_Error_ShouldShowErrorAndSetHasError()
    {
        var categoria = new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Bebidas" };
        _mockProductosService
            .Setup(x => x.ObtenerCategoriasAsync())
            .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(new List<CategoriaProductoDto>()));
        _mockProductosService
            .Setup(x => x.ObtenerProductosPorCategoriaAsync(categoria.Id, It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.ErrorResponse(new List<string>{"E"}, "Error", 500));

        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object)
        {
            SelectedCategoria = categoria
        };

        await vm.LoadProductosCommand.ExecuteAsync(null);

        vm.HasError.Should().BeTrue();
        _mockDialog.Verify(x => x.ShowErrorAsync("Error"), Times.Once);
    }

    [Fact]
    public async Task BuscarProductosAsync_ErrorResponse_ShouldShowErrorAndSetHasError()
    {
        _mockProductosService
            .Setup(x => x.ObtenerCategoriasAsync())
            .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(new List<CategoriaProductoDto>()));
        _mockProductosService
            .Setup(x => x.BuscarProductosAsync("pizza", It.IsAny<bool>()))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.ErrorResponse(new List<string>{"E"}, "Error", 500));

        var vm = new ProductosViewModel(_mockProductosService.Object, _mockDialog.Object, _mockNav.Object)
        {
            TextoBusqueda = "pizza"
        };

        await vm.BuscarProductosCommand.ExecuteAsync(null);

        vm.HasError.Should().BeTrue();
        _mockDialog.Verify(x => x.ShowErrorAsync("Error"), Times.Once);
    }
} 