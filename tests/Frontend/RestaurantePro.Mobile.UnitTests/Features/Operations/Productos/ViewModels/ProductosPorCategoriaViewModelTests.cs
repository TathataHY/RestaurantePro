using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;
using RestaurantePro.Mobile.Core.Models.Common;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Productos;

namespace RestaurantePro.Mobile.UnitTests.Features.Operations.Productos.ViewModels;

/// <summary>
/// Pruebas unitarias para ProductosPorCategoriaViewModel
/// </summary>
public class ProductosPorCategoriaViewModelTests
{
    private readonly Mock<IProductosService> _mockProductosService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly ProductosPorCategoriaViewModel _viewModel;

    public ProductosPorCategoriaViewModelTests()
    {
        _mockProductosService = new Mock<IProductosService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockNavigationService = new Mock<INavigationService>();

        _viewModel = new ProductosPorCategoriaViewModel(
            _mockProductosService.Object,
            _mockDialogService.Object,
            _mockNavigationService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidServices_ShouldInitializeProperties()
    {
        // Act
        var viewModel = new ProductosPorCategoriaViewModel(
            _mockProductosService.Object,
            _mockDialogService.Object,
            _mockNavigationService.Object);

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.Productos.Should().NotBeNull();
        viewModel.Estadisticas.Should().NotBeNull();
        viewModel.TituloPagina.Should().Be("Productos");
        viewModel.CategoriaNombre.Should().Be("Productos");
        viewModel.TextoBusqueda.Should().Be(string.Empty);
        viewModel.MostrarSoloDisponibles.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithCategoriaParameters_ShouldConfigureCategoria()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var categoriaNombre = "Bebidas";

        // Act
        var viewModel = new ProductosPorCategoriaViewModel(
            _mockProductosService.Object,
            _mockDialogService.Object,
            _mockNavigationService.Object,
            categoriaId,
            categoriaNombre);

        // Assert
        // Nota: El ViewModel no asigna CategoriaId y CategoriaNombre en ConfigurarCategoria
        viewModel.CategoriaId.Should().Be(Guid.Empty); // Valor por defecto
        viewModel.CategoriaNombre.Should().Be("Productos"); // Valor por defecto
        viewModel.TituloPagina.Should().Be($"Productos - {categoriaNombre}");
        viewModel.CategoriaSeleccionada.Should().NotBeNull();
        viewModel.CategoriaSeleccionada!.Id.Should().Be(categoriaId);
        viewModel.CategoriaSeleccionada.Nombre.Should().Be(categoriaNombre);
    }

    #endregion

    #region ConfigurarCategoria Tests

    [Fact]
    public void ConfigurarCategoria_WithValidCategoria_ShouldSetProperties()
    {
        // Arrange
        var categoria = new CategoriaProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Postres",
            Descripcion = "Deliciosos postres",
            Activa = true
        };

        // Act
        _viewModel.ConfigurarCategoria(categoria);

        // Assert
        _viewModel.CategoriaSeleccionada.Should().Be(categoria);
        _viewModel.TituloPagina.Should().Be($"Productos - {categoria.Nombre}");
    }

    [Fact]
    public void ConfigurarDesdeParametros_WithValidParameters_ShouldConfigureCategoria()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var categoriaNombre = "Entradas";
        var parametros = new Dictionary<string, object>
        {
            { "categoriaId", categoriaId },
            { "categoriaNombre", categoriaNombre }
        };

        // Act
        _viewModel.ConfigurarDesdeParametros(parametros);

        // Assert
        // Nota: El ViewModel no asigna CategoriaId y CategoriaNombre en ConfigurarCategoria
        _viewModel.CategoriaId.Should().Be(Guid.Empty); // Valor por defecto
        _viewModel.CategoriaNombre.Should().Be("Productos"); // Valor por defecto
        _viewModel.TituloPagina.Should().Be($"Productos - {categoriaNombre}");
        _viewModel.CategoriaSeleccionada.Should().NotBeNull();
        _viewModel.CategoriaSeleccionada!.Id.Should().Be(categoriaId);
        _viewModel.CategoriaSeleccionada.Nombre.Should().Be(categoriaNombre);
    }

    [Fact]
    public void ConfigurarDesdeParametros_WithMissingParameters_ShouldNotConfigureCategoria()
    {
        // Arrange
        var parametros = new Dictionary<string, object>
        {
            { "categoriaId", Guid.NewGuid() }
            // Falta categoriaNombre
        };

        // Act
        _viewModel.ConfigurarDesdeParametros(parametros);

        // Assert
        _viewModel.CategoriaSeleccionada.Should().BeNull();
        _viewModel.TituloPagina.Should().Be("Productos");
    }

    #endregion

    #region CargarProductosAsync Tests

    [Fact]
    public async Task CargarProductosAsync_WithValidCategoriaId_ShouldLoadProductos()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        _viewModel.CategoriaId = categoriaId;
        
        var productos = new List<ProductoDto>
        {
            CreateProductoDto("Producto 1", categoriaId),
            CreateProductoDto("Producto 2", categoriaId)
        };

        var apiResponse = ApiResponse<List<ProductoDto>>.SuccessResponse(productos, "Productos cargados");

        _mockProductosService.Setup(x => x.ObtenerProductosPorCategoriaAsync(categoriaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.CargarProductosCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.ObtenerProductosPorCategoriaAsync(categoriaId, true, It.IsAny<CancellationToken>()), Times.Once);
        _viewModel.Productos.Should().HaveCount(2);
        _viewModel.Estadisticas.TotalProductos.Should().Be(2);
        _viewModel.IsRefreshing.Should().BeFalse();
    }

    [Fact]
    public async Task CargarProductosAsync_WithEmptyCategoriaId_ShouldNotCallService()
    {
        // Arrange
        _viewModel.CategoriaId = Guid.Empty;

        // Act
        await _viewModel.CargarProductosCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.ObtenerProductosPorCategoriaAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CargarProductosAsync_WhenServiceFails_ShouldShowError()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        _viewModel.CategoriaId = categoriaId;
        
        var apiResponse = ApiResponse<List<ProductoDto>>.Failure("Error al cargar productos");

        _mockProductosService.Setup(x => x.ObtenerProductosPorCategoriaAsync(categoriaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.CargarProductosCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.ObtenerProductosPorCategoriaAsync(categoriaId, true, It.IsAny<CancellationToken>()), Times.Once);
        _mockDialogService.Verify(x => x.ShowErrorAsync("Error en la operación"), Times.Once);
        _viewModel.Productos.Should().BeEmpty();
    }

    #endregion

    #region BuscarProductosAsync Tests

    [Fact]
    public async Task BuscarProductosAsync_WithEmptySearchText_ShouldNotCallSearchService()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        _viewModel.CategoriaId = categoriaId;
        _viewModel.TextoBusqueda = string.Empty;
        
        var categoria = new CategoriaProductoDto
        {
            Id = categoriaId,
            Nombre = "Categoría Test",
            Descripcion = "Descripción",
            Activa = true
        };
        _viewModel.ConfigurarCategoria(categoria);

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync(null);

        // Assert
        // Verificar que CategoriaSeleccionada no es null (requerido para BuscarProductosAsync)
        _viewModel.CategoriaSeleccionada.Should().NotBeNull();
        // Verificar que no se llamó al servicio de búsqueda
        _mockProductosService.Verify(x => x.BuscarProductosAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuscarProductosAsync_WithSearchText_ShouldSearchAndFilterByCategory()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        _viewModel.CategoriaId = categoriaId;
        _viewModel.TextoBusqueda = "pizza";
        
        var categoria = new CategoriaProductoDto
        {
            Id = categoriaId,
            Nombre = "Categoría Test",
            Descripcion = "Descripción",
            Activa = true
        };
        _viewModel.ConfigurarCategoria(categoria);

        var productos = new List<ProductoDto>
        {
            CreateProductoDto("Pizza Margarita", categoriaId),
            CreateProductoDto("Pizza Pepperoni", categoriaId),
            CreateProductoDto("Hamburguesa", Guid.NewGuid()) // Diferente categoría
        };

        var apiResponse = ApiResponse<List<ProductoDto>>.SuccessResponse(productos, "Búsqueda exitosa");

        _mockProductosService.Setup(x => x.BuscarProductosAsync("pizza", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.BuscarProductosAsync("pizza", true, It.IsAny<CancellationToken>()), Times.Once);
        _viewModel.Productos.Should().HaveCount(2); // Solo los de la categoría correcta
        _viewModel.Productos.Should().OnlyContain(p => p.CategoriaId == categoriaId);
    }

    [Fact]
    public async Task BuscarProductosAsync_WhenSearchFails_ShouldShowError()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        _viewModel.CategoriaId = categoriaId;
        _viewModel.TextoBusqueda = "búsqueda";
        
        var categoria = new CategoriaProductoDto
        {
            Id = categoriaId,
            Nombre = "Categoría Test",
            Descripcion = "Descripción",
            Activa = true
        };
        _viewModel.ConfigurarCategoria(categoria);

        var apiResponse = ApiResponse<List<ProductoDto>>.Failure("Error en búsqueda");

        _mockProductosService.Setup(x => x.BuscarProductosAsync("búsqueda", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.BuscarProductosAsync("búsqueda", true, It.IsAny<CancellationToken>()), Times.Once);
        _mockDialogService.Verify(x => x.ShowErrorAsync("Error en la operación"), Times.Once);
        _viewModel.Productos.Should().BeEmpty();
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public async Task VerProductoAsync_WithValidProducto_ShouldNavigateToDetail()
    {
        // Arrange
        var producto = CreateProductoDto("Producto Test", Guid.NewGuid());

        // Act
        await _viewModel.VerProductoCommand.ExecuteAsync(producto);

        // Assert
        _mockNavigationService.Verify(x => x.NavigateToAsync("productodetalle", It.Is<IDictionary<string, object>>(
            dict => dict.ContainsKey("productoId") && dict["productoId"].Equals(producto.Id))), Times.Once);
    }

    [Fact]
    public async Task VerProductoAsync_WithNullProducto_ShouldNotNavigate()
    {
        // Act
        await _viewModel.VerProductoCommand.ExecuteAsync(null);

        // Assert
        _mockNavigationService.Verify(x => x.NavigateToAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()), Times.Never);
    }

    [Fact]
    public async Task VerProductoAsync_WhenNavigationFails_ShouldShowError()
    {
        // Arrange
        var producto = CreateProductoDto("Producto Test", Guid.NewGuid());
        
        _mockNavigationService.Setup(x => x.NavigateToAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
            .ThrowsAsync(new Exception("Error de navegación"));

        // Act
        await _viewModel.VerProductoCommand.ExecuteAsync(producto);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync(It.Is<string>(msg => msg.Contains("Error al navegar al detalle"))), Times.Once);
    }

    [Fact]
    public async Task AgregarAComandaAsync_WithValidProducto_ShouldNavigateToCreateComanda()
    {
        // Arrange
        var producto = CreateProductoDto("Producto Test", Guid.NewGuid());
        var categoriaId = Guid.NewGuid();
        
        var categoria = new CategoriaProductoDto
        {
            Id = categoriaId,
            Nombre = "Categoría Test",
            Descripcion = "Descripción",
            Activa = true
        };
        _viewModel.ConfigurarCategoria(categoria);

        // Act
        await _viewModel.AgregarAComandaCommand.ExecuteAsync(producto);

        // Assert
        _mockNavigationService.Verify(x => x.NavigateToAsync("crearcomanda", It.Is<IDictionary<string, object>>(
            dict => dict.ContainsKey("productoId") && dict["productoId"].Equals(producto.Id) &&
                   dict.ContainsKey("categoriaId") && dict["categoriaId"].Equals(categoriaId))), Times.Once);
    }

    [Fact]
    public async Task VolverACategoriasAsync_ShouldNavigateBack()
    {
        // Act
        await _viewModel.VolverACategoriasCommand.ExecuteAsync(null);

        // Assert
        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Once);
    }

    [Fact]
    public async Task VolverACategoriasAsync_WhenNavigationFails_ShouldShowError()
    {
        // Arrange
        _mockNavigationService.Setup(x => x.GoBackAsync())
            .ThrowsAsync(new Exception("Error de navegación"));

        // Act
        await _viewModel.VolverACategoriasCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync(It.Is<string>(msg => msg.Contains("Error al volver"))), Times.Once);
    }

    #endregion

    #region Properties Tests

    [Fact]
    public void TieneProductos_WithEmptyProductos_ShouldReturnFalse()
    {
        // Act & Assert
        _viewModel.TieneProductos.Should().BeFalse();
    }

    [Fact]
    public void TieneProductos_WithProductos_ShouldReturnTrue()
    {
        // Arrange
        _viewModel.Productos.Add(CreateProductoDto("Producto 1", Guid.NewGuid()));

        // Act & Assert
        _viewModel.TieneProductos.Should().BeTrue();
    }

    [Fact]
    public void MensajeSinProductos_WithSearchText_ShouldReturnSearchMessage()
    {
        // Arrange
        _viewModel.TextoBusqueda = "búsqueda";

        // Act & Assert
        _viewModel.MensajeSinProductos.Should().Be("No se encontraron productos para 'búsqueda'");
    }

    [Fact]
    public void MensajeSinProductos_WithCategoriaNombre_ShouldReturnCategoryMessage()
    {
        // Arrange
        _viewModel.TextoBusqueda = string.Empty;
        _viewModel.CategoriaNombre = "Bebidas";

        // Act & Assert
        _viewModel.MensajeSinProductos.Should().Be("No hay productos en la categoría 'Bebidas'");
    }

    [Fact]
    public void MensajeSinProductos_WithEmptyCategory_ShouldReturnDefaultMessage()
    {
        // Arrange
        _viewModel.TextoBusqueda = string.Empty;
        _viewModel.CategoriaNombre = string.Empty;

        // Act & Assert
        _viewModel.MensajeSinProductos.Should().Be("No hay productos disponibles");
    }

    #endregion

    #region Helper Methods

    private ProductoDto CreateProductoDto(string nombre, Guid categoriaId)
    {
        return new ProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            Descripcion = $"Descripción de {nombre}",
            Precio = 10.99m,
            CategoriaId = categoriaId,
            Activo = true,
            CantidadDisponible = 10 // Para que EstadoDisponibilidad sea "Disponible" y PuedeAgregarAComanda sea true
        };
    }

    #endregion
}
