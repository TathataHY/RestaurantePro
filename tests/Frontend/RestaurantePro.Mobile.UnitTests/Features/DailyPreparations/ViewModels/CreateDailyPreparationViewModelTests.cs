using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;
using RestaurantePro.Mobile.Core.Models.Common;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Productos;

namespace RestaurantePro.Mobile.UnitTests.Features.DailyPreparations.ViewModels;

/// <summary>
/// Pruebas unitarias para CreateDailyPreparationViewModel
/// </summary>
public class CreateDailyPreparationViewModelTests
{
    private readonly Mock<IDailyPreparationsService> _mockDailyPreparationsService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly Mock<IProductosService> _mockProductosService;
    private readonly CreateDailyPreparationViewModel _viewModel;

    public CreateDailyPreparationViewModelTests()
    {
        _mockDailyPreparationsService = new Mock<IDailyPreparationsService>();
        _mockAuthService = new Mock<IAuthService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockNavigationService = new Mock<INavigationService>();
        _mockProductosService = new Mock<IProductosService>();

        _viewModel = new CreateDailyPreparationViewModel(
            _mockDailyPreparationsService.Object,
            _mockAuthService.Object,
            _mockDialogService.Object,
            _mockNavigationService.Object,
            _mockProductosService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidServices_ShouldInitializeProperties()
    {
        // Act
        var viewModel = new CreateDailyPreparationViewModel(
            _mockDailyPreparationsService.Object,
            _mockAuthService.Object,
            _mockDialogService.Object,
            _mockNavigationService.Object,
            _mockProductosService.Object);

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.Productos.Should().NotBeNull();
        viewModel.Categorias.Should().NotBeNull();
        viewModel.ProductoBusqueda.Should().Be(string.Empty);
        viewModel.ProductoIdText.Should().Be(string.Empty);
        viewModel.Cantidad.Should().Be(1);
        viewModel.FechaVencimiento.Should().BeCloseTo(DateTime.Today.AddHours(8), TimeSpan.FromMinutes(1));
        viewModel.Observaciones.Should().Be(string.Empty);
        viewModel.IsBusy.Should().BeFalse();
    }

    #endregion

    #region Property Change Tests

    [Fact]
    public void OnProductoSeleccionadoChanged_WithValidProducto_ShouldUpdateProductoIdText()
    {
        // Arrange
        var producto = CreateProductoDto("Producto Test", Guid.NewGuid());

        // Act
        _viewModel.ProductoSeleccionado = producto;

        // Assert
        _viewModel.ProductoIdText.Should().Be(producto.Id.ToString());
    }

    [Fact]
    public void OnProductoSeleccionadoChanged_WithNullProducto_ShouldNotUpdateProductoIdText()
    {
        // Arrange
        _viewModel.ProductoIdText = "Test Value";

        // Act
        _viewModel.ProductoSeleccionado = null;

        // Assert
        _viewModel.ProductoIdText.Should().Be("Test Value");
    }

    [Fact]
    public void OnCategoriaSeleccionadaChanged_WithValidCategoria_ShouldTriggerBuscarProductosAsync()
    {
        // Arrange
        var categoria = new CategoriaProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Categoría Test",
            Descripcion = "Descripción",
            Activa = true
        };

        var productos = new List<ProductoDto>
        {
            CreateProductoDto("Producto 1", Guid.NewGuid())
        };

        var apiResponse = ApiResponse<List<ProductoDto>>.SuccessResponse(productos, "Productos cargados");

        _mockProductosService.Setup(x => x.ObtenerProductosPorCategoriaAsync(categoria.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        _viewModel.CategoriaSeleccionada = categoria;

        // Assert
        // Verificar que se llamó al servicio de productos
        _mockProductosService.Verify(x => x.ObtenerProductosPorCategoriaAsync(categoria.Id, true, It.IsAny<CancellationToken>()), Times.Once);
        _viewModel.Productos.Should().HaveCount(1);
    }

    #endregion

    #region CrearPreparacionAsync Tests

    [Fact]
    public async Task CrearPreparacionAsync_WithValidData_ShouldCreatePreparacion()
    {
        // Arrange
        var producto = CreateProductoDto("Producto Test", Guid.NewGuid());
        _viewModel.ProductoSeleccionado = producto;
        _viewModel.Cantidad = 5;
        _viewModel.FechaVencimiento = DateTime.Today.AddDays(1);
        _viewModel.Observaciones = "Observaciones de prueba";

        var userId = Guid.NewGuid();
        _mockAuthService.Setup(x => x.GetUserIdAsync()).ReturnsAsync(userId.ToString());

        var result = Result<PreparacionDiariaDto>.Success(CreatePreparacionDiariaDto());
        _mockDailyPreparationsService.Setup(x => x.CrearPreparacionDiariaAsync(It.IsAny<CrearPreparacionDiariaCommand>()))
            .ReturnsAsync(result);

        // Act
        await _viewModel.CrearPreparacionCommand.ExecuteAsync(null);

        // Assert
        _mockAuthService.Verify(x => x.GetUserIdAsync(), Times.Once);
        _mockDailyPreparationsService.Verify(x => x.CrearPreparacionDiariaAsync(It.Is<CrearPreparacionDiariaCommand>(cmd =>
            cmd.ProductoId == producto.Id &&
            cmd.Cantidad == 5 &&
            cmd.ChefId == userId &&
            cmd.FechaVencimiento == _viewModel.FechaVencimiento &&
            cmd.Observaciones == "Observaciones de prueba")), Times.Once);
        _mockDialogService.Verify(x => x.ShowSuccessAsync("Preparación creada correctamente."), Times.Once);
        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Once);
    }

    [Fact]
    public async Task CrearPreparacionAsync_WithInvalidProductoIdText_ShouldShowError()
    {
        // Arrange
        _viewModel.ProductoSeleccionado = null;
        _viewModel.ProductoIdText = "invalid-guid";
        _viewModel.Cantidad = 5;
        _viewModel.FechaVencimiento = DateTime.Today.AddDays(1);

        // Act
        await _viewModel.CrearPreparacionCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync("ProductoId inválido. Usa un GUID válido."), Times.Once);
        _mockDailyPreparationsService.Verify(x => x.CrearPreparacionDiariaAsync(It.IsAny<CrearPreparacionDiariaCommand>()), Times.Never);
    }

    [Fact]
    public async Task CrearPreparacionAsync_WithZeroCantidad_ShouldShowError()
    {
        // Arrange
        var producto = CreateProductoDto("Producto Test", Guid.NewGuid());
        _viewModel.ProductoSeleccionado = producto;
        _viewModel.Cantidad = 0;
        _viewModel.FechaVencimiento = DateTime.Today.AddDays(1);

        // Act
        await _viewModel.CrearPreparacionCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync("La cantidad debe ser mayor que 0."), Times.Once);
        _mockDailyPreparationsService.Verify(x => x.CrearPreparacionDiariaAsync(It.IsAny<CrearPreparacionDiariaCommand>()), Times.Never);
    }

    [Fact]
    public async Task CrearPreparacionAsync_WithPastFechaVencimiento_ShouldShowError()
    {
        // Arrange
        var producto = CreateProductoDto("Producto Test", Guid.NewGuid());
        _viewModel.ProductoSeleccionado = producto;
        _viewModel.Cantidad = 5;
        _viewModel.FechaVencimiento = DateTime.Today; // Fecha actual (no futura)

        // Act
        await _viewModel.CrearPreparacionCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync("La fecha de vencimiento debe ser futura (desde mañana)."), Times.Once);
        _mockDailyPreparationsService.Verify(x => x.CrearPreparacionDiariaAsync(It.IsAny<CrearPreparacionDiariaCommand>()), Times.Never);
    }

    [Fact]
    public async Task CrearPreparacionAsync_WhenServiceFails_ShouldShowError()
    {
        // Arrange
        var producto = CreateProductoDto("Producto Test", Guid.NewGuid());
        _viewModel.ProductoSeleccionado = producto;
        _viewModel.Cantidad = 5;
        _viewModel.FechaVencimiento = DateTime.Today.AddDays(1);

        var userId = Guid.NewGuid();
        _mockAuthService.Setup(x => x.GetUserIdAsync()).ReturnsAsync(userId.ToString());

        var result = Result<PreparacionDiariaDto>.Failure("Error al crear preparación");
        _mockDailyPreparationsService.Setup(x => x.CrearPreparacionDiariaAsync(It.IsAny<CrearPreparacionDiariaCommand>()))
            .ReturnsAsync(result);

        // Act
        await _viewModel.CrearPreparacionCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync("Error al crear preparación"), Times.Once);
        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Never);
    }

    [Fact]
    public async Task CrearPreparacionAsync_WhenExceptionThrown_ShouldShowError()
    {
        // Arrange
        var producto = CreateProductoDto("Producto Test", Guid.NewGuid());
        _viewModel.ProductoSeleccionado = producto;
        _viewModel.Cantidad = 5;
        _viewModel.FechaVencimiento = DateTime.Today.AddDays(1);

        _mockAuthService.Setup(x => x.GetUserIdAsync()).ThrowsAsync(new Exception("Error de autenticación"));

        // Act
        await _viewModel.CrearPreparacionCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync(It.Is<string>(msg => msg.Contains("Error al crear preparación"))), Times.Once);
        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Never);
    }

    #endregion

    #region BuscarProductosAsync Tests

    [Fact]
    public async Task BuscarProductosAsync_WithCategoriaSeleccionada_ShouldSearchByCategoria()
    {
        // Arrange
        var categoria = new CategoriaProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Categoría Test",
            Descripcion = "Descripción",
            Activa = true
        };
        _viewModel.CategoriaSeleccionada = categoria;
        _viewModel.ProductoBusqueda = string.Empty;

        var productos = new List<ProductoDto>
        {
            CreateProductoDto("Producto 1", Guid.NewGuid())
        };

        var apiResponse = ApiResponse<List<ProductoDto>>.SuccessResponse(productos, "Productos cargados");

        _mockProductosService.Setup(x => x.ObtenerProductosPorCategoriaAsync(categoria.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync(null);

        // Assert
        // Se llama 2 veces: una por OnCategoriaSeleccionadaChanged y otra por el comando
        _mockProductosService.Verify(x => x.ObtenerProductosPorCategoriaAsync(categoria.Id, true, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _viewModel.Productos.Should().HaveCount(1);
    }

    [Fact]
    public async Task BuscarProductosAsync_WithSearchTerm_ShouldSearchByTerm()
    {
        // Arrange
        _viewModel.ProductoBusqueda = "pizza";

        var productos = new List<ProductoDto>
        {
            CreateProductoDto("Pizza Margarita", Guid.NewGuid())
        };

        var apiResponse = ApiResponse<List<ProductoDto>>.SuccessResponse(productos, "Búsqueda exitosa");

        _mockProductosService.Setup(x => x.BuscarProductosAsync("pizza", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.BuscarProductosAsync("pizza", true, It.IsAny<CancellationToken>()), Times.Once);
        _viewModel.Productos.Should().HaveCount(1);
    }

    [Fact]
    public async Task BuscarProductosAsync_WithNoSearchTerm_ShouldGetPaginados()
    {
        // Arrange
        _viewModel.ProductoBusqueda = string.Empty;
        _viewModel.CategoriaSeleccionada = null;

        var productos = new List<ProductoDto>
        {
            CreateProductoDto("Producto 1", Guid.NewGuid()),
            CreateProductoDto("Producto 2", Guid.NewGuid())
        };

        var apiResponse = ApiResponse<List<ProductoDto>>.SuccessResponse(productos, "Productos cargados");

        _mockProductosService.Setup(x => x.ObtenerProductosPaginadosAsync(1, 10, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.ObtenerProductosPaginadosAsync(1, 10, null, true, It.IsAny<CancellationToken>()), Times.Once);
        _viewModel.Productos.Should().HaveCount(2);
    }

    [Fact]
    public async Task BuscarProductosAsync_WhenServiceFails_ShouldShowError()
    {
        // Arrange
        _viewModel.ProductoBusqueda = "test";

        _mockProductosService.Setup(x => x.BuscarProductosAsync("test", true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de conexión"));

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync("Error al buscar productos: Error de conexión"), Times.Once);
        _viewModel.Productos.Should().BeEmpty();
    }

    #endregion

    #region CargarCategoriasAsync Tests

    [Fact]
    public async Task CargarCategoriasAsync_WithValidData_ShouldLoadCategorias()
    {
        // Arrange
        var categorias = new List<CategoriaProductoDto>
        {
            new CategoriaProductoDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Categoría 1",
                Descripcion = "Descripción 1",
                Activa = true
            },
            new CategoriaProductoDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Categoría 2",
                Descripcion = "Descripción 2",
                Activa = true
            }
        };

        var apiResponse = ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias, "Categorías cargadas");

        _mockProductosService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.CargarCategoriasCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.ObtenerCategoriasAsync(It.IsAny<CancellationToken>()), Times.Once);
        _viewModel.Categorias.Should().HaveCount(2);
    }

    [Fact]
    public async Task CargarCategoriasAsync_WhenServiceFails_ShouldShowError()
    {
        // Arrange
        _mockProductosService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de conexión"));

        // Act
        await _viewModel.CargarCategoriasCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync("Error al cargar categorías: Error de conexión"), Times.Once);
        _viewModel.Categorias.Should().BeEmpty();
    }

    #endregion

    #region CancelarAsync Tests

    [Fact]
    public async Task CancelarAsync_ShouldNavigateBack()
    {
        // Act
        await _viewModel.CancelarCommand.ExecuteAsync(null);

        // Assert
        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Once);
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
            CantidadDisponible = 10
        };
    }

    private PreparacionDiariaDto CreatePreparacionDiariaDto()
    {
        return new PreparacionDiariaDto
        {
            Id = Guid.NewGuid(),
            ProductoId = Guid.NewGuid(),
            CantidadPreparada = 5,
            CantidadDisponible = 5,
            FechaVencimiento = DateTime.Today.AddDays(1),
            Observaciones = "Observaciones de prueba",
            ChefId = Guid.NewGuid()
        };
    }

    #endregion
}
