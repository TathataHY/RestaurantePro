using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Productos;

namespace RestaurantePro.Mobile.UnitTests.Features.Operations.Productos.ViewModels;

public class ProductoEditorViewModelTests
{
    private readonly Mock<IProductosService> _mockProductosService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly ProductoEditorViewModel _viewModel;

    public ProductoEditorViewModelTests()
    {
        _mockProductosService = new Mock<IProductosService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockNavigationService = new Mock<INavigationService>();

        _viewModel = new ProductoEditorViewModel(
            _mockProductosService.Object,
            _mockDialogService.Object,
            _mockNavigationService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        Assert.Equal(Guid.Empty, _viewModel.Id);
        Assert.Equal(string.Empty, _viewModel.Nombre);
        Assert.Equal(string.Empty, _viewModel.Descripcion);
        Assert.Equal(0m, _viewModel.Precio);
        Assert.Equal(Guid.Empty, _viewModel.CategoriaId);
        Assert.True(_viewModel.Activo);
        Assert.False(_viewModel.EsEdicion);
        Assert.Equal("Crear", _viewModel.PrimaryButtonText);
        Assert.NotNull(_viewModel.Categorias);
        Assert.Equal("Producto", _viewModel.Title);
    }

    #endregion

    #region CargarCategoriasAsync Tests

    [Fact]
    public async Task CargarCategoriasAsync_WithValidData_ShouldLoadCategorias()
    {
        // Arrange
        var categorias = new List<CategoriaProductoDto>
        {
            CreateCategoriaDto("11111111-1111-1111-1111-111111111111", "Bebidas"),
            CreateCategoriaDto("22222222-2222-2222-2222-222222222222", "Comidas")
        };

        _mockProductosService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias));

        // Act
        await _viewModel.CargarCategoriasCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(2, _viewModel.Categorias.Count);
        _mockProductosService.Verify(x => x.ObtenerCategoriasAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CargarCategoriasAsync_WithServiceError_ShouldNotThrow()
    {
        // Arrange
        _mockProductosService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.Failure("Error del servicio"));

        // Act & Assert
        await _viewModel.CargarCategoriasCommand.ExecuteAsync(null);
        // Should not throw exception
    }

    #endregion

    #region CargarParaEdicionAsync Tests

    [Fact]
    public async Task CargarParaEdicionAsync_WithValidProductId_ShouldLoadProduct()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var producto = CreateProductoDto("11111111-1111-1111-1111-111111111111", "Pizza", 25.50m, "22222222-2222-2222-2222-222222222222");

        _mockProductosService.Setup(x => x.ObtenerProductoPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(producto));

        // Act
        await _viewModel.CargarParaEdicionCommand.ExecuteAsync(productoId);

        // Assert
        Assert.Equal(producto.Id, _viewModel.Id);
        Assert.Equal(producto.Nombre, _viewModel.Nombre);
        Assert.Equal(producto.Descripcion, _viewModel.Descripcion);
        Assert.Equal(producto.Precio, _viewModel.Precio);
        Assert.Equal(producto.CategoriaId, _viewModel.CategoriaId);
        Assert.Equal(producto.Activo, _viewModel.Activo);
        Assert.True(_viewModel.EsEdicion);
        Assert.Equal("Editar producto", _viewModel.Title);
        Assert.Equal("Guardar", _viewModel.PrimaryButtonText);
    }

    [Fact]
    public async Task CargarParaEdicionAsync_WithEmptyProductId_ShouldNotLoad()
    {
        // Act
        await _viewModel.CargarParaEdicionCommand.ExecuteAsync(Guid.Empty);

        // Assert
        Assert.Equal(Guid.Empty, _viewModel.Id);
        Assert.False(_viewModel.EsEdicion);
    }

    [Fact]
    public async Task CargarParaEdicionAsync_WithServiceError_ShouldShowError()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        _mockProductosService.Setup(x => x.ObtenerProductoPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.Failure("Error del servicio"));

        // Act
        await _viewModel.CargarParaEdicionCommand.ExecuteAsync(productoId);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync("Error en la operación"), Times.Once);
    }

    #endregion

    #region Validar Tests (Indirect through GuardarAsync)

    [Fact]
    public async Task GuardarAsync_WithEmptyNombre_ShouldShowValidationError()
    {
        // Arrange
        _viewModel.Nombre = ""; // Invalid
        _viewModel.Precio = 25.50m;
        _viewModel.CategoriaId = Guid.NewGuid();

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Validación", "El nombre es obligatorio", It.IsAny<string>()), Times.Once);
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GuardarAsync_WithZeroPrecio_ShouldShowValidationError()
    {
        // Arrange
        _viewModel.Nombre = "Pizza";
        _viewModel.Precio = 0; // Invalid
        _viewModel.CategoriaId = Guid.NewGuid();

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Validación", "El precio debe ser mayor a 0", It.IsAny<string>()), Times.Once);
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GuardarAsync_WithEmptyCategoriaId_ShouldShowValidationError()
    {
        // Arrange
        _viewModel.Nombre = "Pizza";
        _viewModel.Precio = 25.50m;
        _viewModel.CategoriaId = Guid.Empty; // Invalid

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Validación", "Selecciona una categoría", It.IsAny<string>()), Times.Once);
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region GuardarAsync Tests

    [Fact]
    public async Task GuardarAsync_WithInvalidData_ShouldNotSave()
    {
        // Arrange
        _viewModel.Nombre = ""; // Invalid

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockProductosService.Verify(x => x.ActualizarProductoAsync(It.IsAny<ActualizarProductoRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GuardarAsync_WithValidData_ShouldCreateProduct()
    {
        // Arrange
        _viewModel.Nombre = "Pizza";
        _viewModel.Descripcion = "Pizza deliciosa";
        _viewModel.Precio = 25.50m;
        _viewModel.CategoriaId = Guid.NewGuid();
        _viewModel.Activo = true;

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(CreateProductoDto("11111111-1111-1111-1111-111111111111", "Pizza", 25.50m, "22222222-2222-2222-2222-222222222222")));

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockDialogService.Verify(x => x.ShowSuccessAsync("Producto guardado"), Times.Once);
        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Once);
    }

    [Fact]
    public async Task GuardarAsync_WithValidDataInEditMode_ShouldUpdateProduct()
    {
        // Arrange
        _viewModel.EsEdicion = true;
        _viewModel.Id = Guid.NewGuid();
        _viewModel.Nombre = "Pizza";
        _viewModel.Descripcion = "Pizza deliciosa";
        _viewModel.Precio = 25.50m;
        _viewModel.CategoriaId = Guid.NewGuid();
        _viewModel.Activo = true;

        _mockProductosService.Setup(x => x.ActualizarProductoAsync(It.IsAny<ActualizarProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(CreateProductoDto("11111111-1111-1111-1111-111111111111", "Pizza", 25.50m, "22222222-2222-2222-2222-222222222222")));

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.ActualizarProductoAsync(It.IsAny<ActualizarProductoRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockDialogService.Verify(x => x.ShowSuccessAsync("Producto guardado"), Times.Once);
        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Once);
    }

    [Fact]
    public async Task GuardarAsync_WithServiceError_ShouldShowError()
    {
        // Arrange
        _viewModel.Nombre = "Pizza";
        _viewModel.Precio = 25.50m;
        _viewModel.CategoriaId = Guid.NewGuid();

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.Failure("Error del servicio"));

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync("Error en la operación"), Times.Once);
        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Never);
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

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Validaciones de Formulario Avanzadas

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GuardarAsync_WithInvalidNombre_ShouldShowValidationError(string nombre)
    {
        // Arrange
        _viewModel.Nombre = nombre;
        _viewModel.Precio = 25.50m;
        _viewModel.CategoriaId = Guid.NewGuid();

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Validación", "El nombre es obligatorio", It.IsAny<string>()), Times.Once);
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    [InlineData(0)]
    public async Task GuardarAsync_WithInvalidPrecio_ShouldShowValidationError(decimal precio)
    {
        // Arrange
        _viewModel.Nombre = "Pizza";
        _viewModel.Precio = precio;
        _viewModel.CategoriaId = Guid.NewGuid();

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Validación", "El precio debe ser mayor a 0", It.IsAny<string>()), Times.Once);
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GuardarAsync_WithVeryLongNombre_ShouldHandleGracefully()
    {
        // Arrange
        var longNombre = new string('a', 1000); // 1000 caracteres
        _viewModel.Nombre = longNombre;
        _viewModel.Precio = 25.50m;
        _viewModel.CategoriaId = Guid.NewGuid();

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(CreateProductoDto("11111111-1111-1111-1111-111111111111", longNombre, 25.50m, "22222222-2222-2222-2222-222222222222")));

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.Is<CrearProductoRequest>(r => r.Nombre == longNombre), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GuardarAsync_WithVeryLongDescripcion_ShouldHandleGracefully()
    {
        // Arrange
        var longDescripcion = new string('a', 5000); // 5000 caracteres
        _viewModel.Nombre = "Pizza";
        _viewModel.Descripcion = longDescripcion;
        _viewModel.Precio = 25.50m;
        _viewModel.CategoriaId = Guid.NewGuid();

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(CreateProductoDto("11111111-1111-1111-1111-111111111111", "Pizza", 25.50m, "22222222-2222-2222-2222-222222222222")));

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.Is<CrearProductoRequest>(r => r.Descripcion == longDescripcion), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(999999.99)]
    [InlineData(1000000)]
    public async Task GuardarAsync_WithExtremePrecios_ShouldHandleGracefully(decimal precio)
    {
        // Arrange
        _viewModel.Nombre = "Pizza";
        _viewModel.Precio = precio;
        _viewModel.CategoriaId = Guid.NewGuid();

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(CreateProductoDto("11111111-1111-1111-1111-111111111111", "Pizza", precio, "22222222-2222-2222-2222-222222222222")));

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.Is<CrearProductoRequest>(r => r.Precio == precio), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Pizza")]
    [InlineData("Pizza con Queso")]
    [InlineData("Pizza con Queso y Pepperoni")]
    [InlineData("Pizza con Queso, Pepperoni y Champiñones")]
    public async Task GuardarAsync_WithValidNombres_ShouldCreateProduct(string nombre)
    {
        // Arrange
        _viewModel.Nombre = nombre;
        _viewModel.Precio = 25.50m;
        _viewModel.CategoriaId = Guid.NewGuid();

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(CreateProductoDto("11111111-1111-1111-1111-111111111111", nombre, 25.50m, "22222222-2222-2222-2222-222222222222")));

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.Is<CrearProductoRequest>(r => r.Nombre == nombre), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Manejo de Errores de Red y Servicios

    [Fact]
    public async Task CargarCategoriasAsync_WithNetworkTimeout_ShouldHandleGracefully()
    {
        // Arrange
        _mockProductosService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act & Assert
        // Should not throw exception
        await _viewModel.CargarCategoriasCommand.ExecuteAsync(null);
    }

    [Fact]
    public async Task CargarCategoriasAsync_WithHttpException_ShouldHandleGracefully()
    {
        // Arrange
        _mockProductosService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        // Should not throw exception
        await _viewModel.CargarCategoriasCommand.ExecuteAsync(null);
    }

    [Fact]
    public async Task CargarParaEdicionAsync_WithNetworkTimeout_ShouldShowError()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        _mockProductosService.Setup(x => x.ObtenerProductoPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        await _viewModel.CargarParaEdicionCommand.ExecuteAsync(productoId);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync(It.Is<string>(s => s.Contains("timeout"))), Times.Once);
    }

    [Fact]
    public async Task CargarParaEdicionAsync_WithHttpException_ShouldShowError()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        _mockProductosService.Setup(x => x.ObtenerProductoPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        await _viewModel.CargarParaEdicionCommand.ExecuteAsync(productoId);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync(It.Is<string>(s => s.Contains("Network error"))), Times.Once);
    }

    [Fact]
    public async Task GuardarAsync_WithNetworkTimeout_ShouldShowError()
    {
        // Arrange
        _viewModel.Nombre = "Pizza";
        _viewModel.Precio = 25.50m;
        _viewModel.CategoriaId = Guid.NewGuid();

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync(It.Is<string>(s => s.Contains("timeout"))), Times.Once);
    }

    [Fact]
    public async Task GuardarAsync_WithHttpException_ShouldShowError()
    {
        // Arrange
        _viewModel.Nombre = "Pizza";
        _viewModel.Precio = 25.50m;
        _viewModel.CategoriaId = Guid.NewGuid();

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync(It.Is<string>(s => s.Contains("Network error"))), Times.Once);
    }

    [Fact]
    public async Task GuardarAsync_WithServiceFailure_ShouldShowError()
    {
        // Arrange
        _viewModel.Nombre = "Pizza";
        _viewModel.Precio = 25.50m;
        _viewModel.CategoriaId = Guid.NewGuid();

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.Failure("Service unavailable"));

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync("Error en la operación"), Times.Once);
    }

    #endregion

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Casos Edge y Límites

    [Fact]
    public async Task GuardarAsync_WithNullData_ShouldHandleGracefully()
    {
        // Arrange
        _viewModel.Nombre = "Pizza";
        _viewModel.Precio = 25.50m;
        _viewModel.CategoriaId = Guid.NewGuid();

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(null!));

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockDialogService.Verify(x => x.ShowSuccessAsync("Producto guardado"), Times.Once);
    }

    [Fact]
    public async Task GuardarAsync_WithEmptyCategorias_ShouldStillWork()
    {
        // Arrange
        _viewModel.Nombre = "Pizza";
        _viewModel.Precio = 25.50m;
        _viewModel.CategoriaId = Guid.NewGuid();

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(CreateProductoDto("11111111-1111-1111-1111-111111111111", "Pizza", 25.50m, "22222222-2222-2222-2222-222222222222")));

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GuardarAsync_WithVeryLargePrecio_ShouldHandleGracefully()
    {
        // Arrange
        _viewModel.Nombre = "Pizza";
        _viewModel.Precio = 999999999.99m; // Very large price
        _viewModel.CategoriaId = Guid.NewGuid();

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(CreateProductoDto("11111111-1111-1111-1111-111111111111", "Pizza", 999999999.99m, "22222222-2222-2222-2222-222222222222")));

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.Is<CrearProductoRequest>(r => r.Precio == 999999999.99m), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GuardarAsync_WithVerySmallPrecio_ShouldHandleGracefully()
    {
        // Arrange
        _viewModel.Nombre = "Pizza";
        _viewModel.Precio = 0.01m; // Very small price
        _viewModel.CategoriaId = Guid.NewGuid();

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(CreateProductoDto("11111111-1111-1111-1111-111111111111", "Pizza", 0.01m, "22222222-2222-2222-2222-222222222222")));

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.Is<CrearProductoRequest>(r => r.Precio == 0.01m), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Concurrencia y Threading

    [Fact]
    public async Task GuardarAsync_WithConcurrentCalls_ShouldHandleGracefully()
    {
        // Arrange
        _viewModel.Nombre = "Pizza";
        _viewModel.Precio = 25.50m;
        _viewModel.CategoriaId = Guid.NewGuid();

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(CreateProductoDto("11111111-1111-1111-1111-111111111111", "Pizza", 25.50m, "22222222-2222-2222-2222-222222222222")));

        // Act
        var task1 = _viewModel.GuardarCommand.ExecuteAsync(null);
        var task2 = _viewModel.GuardarCommand.ExecuteAsync(null);
        var task3 = _viewModel.GuardarCommand.ExecuteAsync(null);

        await Task.WhenAll(task1, task2, task3);

        // Assert
        // Should not throw exception and should handle concurrent calls
        Assert.True(true);
    }

    [Fact]
    public async Task CargarCategoriasAsync_WithConcurrentCalls_ShouldHandleGracefully()
    {
        // Arrange
        var categorias = new List<CategoriaProductoDto>
        {
            CreateCategoriaDto("11111111-1111-1111-1111-111111111111", "Bebidas"),
            CreateCategoriaDto("22222222-2222-2222-2222-222222222222", "Comidas")
        };

        _mockProductosService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias));

        // Act
        var task1 = _viewModel.CargarCategoriasCommand.ExecuteAsync(null);
        var task2 = _viewModel.CargarCategoriasCommand.ExecuteAsync(null);
        var task3 = _viewModel.CargarCategoriasCommand.ExecuteAsync(null);

        await Task.WhenAll(task1, task2, task3);

        // Assert
        // Should not throw exception and should handle concurrent calls
        Assert.True(true);
    }

    #endregion

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Validaciones de Caracteres Especiales

    [Theory]
    [InlineData("Pizza con Ñoquis")]
    [InlineData("Pizza con Año")]
    [InlineData("Pizza con Açúcar")]
    [InlineData("Pizza con Café")]
    [InlineData("Pizza con Año 2024")]
    public async Task GuardarAsync_WithSpecialCharacters_ShouldHandleGracefully(string nombre)
    {
        // Arrange
        _viewModel.Nombre = nombre;
        _viewModel.Precio = 25.50m;
        _viewModel.CategoriaId = Guid.NewGuid();

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(CreateProductoDto("11111111-1111-1111-1111-111111111111", nombre, 25.50m, "22222222-2222-2222-2222-222222222222")));

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.Is<CrearProductoRequest>(r => r.Nombre == nombre), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Pizza con <script>alert('xss')</script>")]
    [InlineData("Pizza con &lt;script&gt;")]
    [InlineData("Pizza con &amp; &quot; &apos;")]
    public async Task GuardarAsync_WithHtmlCharacters_ShouldHandleGracefully(string nombre)
    {
        // Arrange
        _viewModel.Nombre = nombre;
        _viewModel.Precio = 25.50m;
        _viewModel.CategoriaId = Guid.NewGuid();

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(CreateProductoDto("11111111-1111-1111-1111-111111111111", nombre, 25.50m, "22222222-2222-2222-2222-222222222222")));

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.Is<CrearProductoRequest>(r => r.Nombre == nombre), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Helper Methods

    private CategoriaProductoDto CreateCategoriaDto(string id, string nombre)
    {
        return new CategoriaProductoDto
        {
            Id = new Guid(id),
            Nombre = nombre
        };
    }

    private ProductoDto CreateProductoDto(string id, string nombre, decimal precio, string categoriaId)
    {
        return new ProductoDto
        {
            Id = new Guid(id),
            Nombre = nombre,
            Precio = precio,
            CategoriaId = new Guid(categoriaId),
            Activo = true
        };
    }

    #endregion
}