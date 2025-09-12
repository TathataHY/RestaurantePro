using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Models.Common;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Features.Operations.Productos.ViewModels;

public class ProductoEditorViewModelTests
{
    private readonly Mock<IProductosService> _mockProductosService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly ProductoEditorViewModel _viewModel;

    public ProductoEditorViewModelTests()
    {
        _mockProductosService = new Mock<IProductosService>();
        _mockNavigationService = new Mock<INavigationService>();
        _mockDialogService = new Mock<IDialogService>();
        _viewModel = new ProductoEditorViewModel(_mockProductosService.Object, _mockDialogService.Object, _mockNavigationService.Object);
    }

    [Fact]
    public void Constructor_WithValidServices_ShouldInitializeProperties()
    {
        // Act & Assert
        _viewModel.Id.Should().Be(Guid.Empty);
        _viewModel.Nombre.Should().BeEmpty();
        _viewModel.Descripcion.Should().BeEmpty();
        _viewModel.Precio.Should().Be(0);
        _viewModel.CategoriaId.Should().Be(Guid.Empty);
        _viewModel.Activo.Should().BeTrue();
        _viewModel.EsEdicion.Should().BeFalse();
        _viewModel.PrimaryButtonText.Should().Be("Crear");
        _viewModel.Categorias.Should().BeEmpty();
        _viewModel.IsBusy.Should().BeFalse();
        _viewModel.Title.Should().Be("Producto");
    }

    [Fact]
    public void Constructor_WithNullServices_ShouldNotThrowException()
    {
        // Act & Assert - El constructor no valida parámetros nulos, por lo que no lanza excepciones
        var viewModel1 = new ProductoEditorViewModel(null!, _mockDialogService.Object, _mockNavigationService.Object);
        var viewModel2 = new ProductoEditorViewModel(_mockProductosService.Object, null!, _mockNavigationService.Object);
        var viewModel3 = new ProductoEditorViewModel(_mockProductosService.Object, _mockDialogService.Object, null!);
        
        Assert.NotNull(viewModel1);
        Assert.NotNull(viewModel2);
        Assert.NotNull(viewModel3);
    }

    [Fact]
    public async Task CargarCategoriasAsync_WhenServiceReturnsSuccess_ShouldPopulateCategorias()
    {
        // Arrange
        var categorias = new List<CategoriaProductoDto>
        {
            new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Bebidas" },
            new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Platos Principales" }
        };
        var apiResponse = ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias);

        _mockProductosService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.CargarCategoriasCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.ObtenerCategoriasAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(2, _viewModel.Categorias.Count);
        Assert.Equal("Bebidas", _viewModel.Categorias.First().Nombre);
        Assert.Equal("Platos Principales", _viewModel.Categorias.Last().Nombre);
    }

    [Fact]
    public async Task CargarCategoriasAsync_WhenServiceFails_ShouldNotThrow()
    {
        // Arrange
        var apiResponse = ApiResponse<List<CategoriaProductoDto>>.Failure("Error de API");

        _mockProductosService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act & Assert
        await _viewModel.CargarCategoriasCommand.ExecuteAsync(null);
        _mockProductosService.Verify(x => x.ObtenerCategoriasAsync(It.IsAny<CancellationToken>()), Times.Once);
        _viewModel.Categorias.Should().BeEmpty();
    }

    [Fact]
    public async Task GuardarAsync_WhenCreatingNewProduct_WithValidData_ShouldCallService()
    {
        // Arrange
        _viewModel.Nombre = "Pizza Margherita";
        _viewModel.Descripcion = "Deliciosa pizza con tomate y mozzarella";
        _viewModel.Precio = 15.99m;
        _viewModel.CategoriaId = Guid.NewGuid();

        var createdProduct = new ProductoDto { Id = Guid.NewGuid(), Nombre = "Pizza Margherita" };
        var apiResponse = ApiResponse<ProductoDto>.SuccessResponse(createdProduct);

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockDialogService.Verify(x => x.ShowSuccessAsync("Producto guardado"), Times.Once);
        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Once);
    }

    [Fact]
    public async Task GuardarAsync_WhenUpdatingExistingProduct_WithValidData_ShouldCallService()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        _viewModel.Id = productoId;
        _viewModel.EsEdicion = true;
        _viewModel.Nombre = "Pizza Margherita Actualizada";
        _viewModel.Descripcion = "Descripción actualizada";
        _viewModel.Precio = 18.99m;
        _viewModel.CategoriaId = Guid.NewGuid();

        var updatedProduct = new ProductoDto { Id = productoId, Nombre = "Pizza Margherita Actualizada" };
        var apiResponse = ApiResponse<ProductoDto>.SuccessResponse(updatedProduct);

        _mockProductosService.Setup(x => x.ActualizarProductoAsync(It.IsAny<ActualizarProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.ActualizarProductoAsync(It.IsAny<ActualizarProductoRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockDialogService.Verify(x => x.ShowSuccessAsync("Producto guardado"), Times.Once);
        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Once);
    }

    [Fact]
    public async Task GuardarAsync_WhenServiceFails_ShouldShowError()
    {
        // Arrange
        _viewModel.Nombre = "Pizza";
        _viewModel.Precio = 15.99m;
        _viewModel.CategoriaId = Guid.NewGuid();

        var apiResponse = ApiResponse<ProductoDto>.Failure("Error al guardar producto");

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockDialogService.Verify(x => x.ShowErrorAsync("Error en la operación"), Times.Once);
        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Never);
    }

    [Fact]
    public async Task CargarParaEdicionAsync_WhenProductExists_ShouldPopulateViewModel()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var categoriaId = Guid.NewGuid();
        
        var producto = new ProductoDto
        {
            Id = productoId,
            Nombre = "Pizza Margherita",
            Descripcion = "Deliciosa pizza",
            Precio = 15.99m,
            CategoriaId = categoriaId,
            Activo = true
        };
        
        var apiResponse = ApiResponse<ProductoDto>.SuccessResponse(producto);

        _mockProductosService.Setup(x => x.ObtenerProductoPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.CargarParaEdicionCommand.ExecuteAsync(productoId);

        // Assert
        _mockProductosService.Verify(x => x.ObtenerProductoPorIdAsync(productoId, It.IsAny<CancellationToken>()), Times.Once);
        _viewModel.Id.Should().Be(productoId);
        _viewModel.Nombre.Should().Be("Pizza Margherita");
        _viewModel.Descripcion.Should().Be("Deliciosa pizza");
        _viewModel.Precio.Should().Be(15.99m);
        _viewModel.CategoriaId.Should().Be(categoriaId);
        _viewModel.Activo.Should().BeTrue();
        _viewModel.EsEdicion.Should().BeTrue();
        _viewModel.Title.Should().Be("Editar producto");
        _viewModel.PrimaryButtonText.Should().Be("Guardar");
    }

    [Fact]
    public async Task CargarParaEdicionAsync_WhenProductNotFound_ShouldShowError()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var apiResponse = ApiResponse<ProductoDto>.Failure("Producto no encontrado");

        _mockProductosService.Setup(x => x.ObtenerProductoPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.CargarParaEdicionCommand.ExecuteAsync(productoId);

        // Assert
        _mockProductosService.Verify(x => x.ObtenerProductoPorIdAsync(productoId, It.IsAny<CancellationToken>()), Times.Once);
        _mockDialogService.Verify(x => x.ShowErrorAsync("Error en la operación"), Times.Once);
        _viewModel.EsEdicion.Should().BeFalse();
    }

    [Fact]
    public async Task CargarParaEdicionAsync_WithEmptyGuid_ShouldNotCallService()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        await _viewModel.CargarParaEdicionCommand.ExecuteAsync(emptyGuid);

        // Assert
        _mockProductosService.Verify(x => x.ObtenerProductoPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelarAsync_ShouldNavigateBack()
    {
        // Act
        await _viewModel.CancelarCommand.ExecuteAsync(null);

        // Assert
        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Once);
    }

    [Fact]
    public async Task GuardarAsync_WithEmptyNombre_ShouldShowValidationError()
    {
        // Arrange
        _viewModel.Nombre = string.Empty;
        _viewModel.Precio = 15.99m;
        _viewModel.CategoriaId = Guid.NewGuid();

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Validación", "El nombre es obligatorio", "OK"), Times.Once);
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GuardarAsync_WithZeroPrecio_ShouldShowValidationError()
    {
        // Arrange
        _viewModel.Nombre = "Pizza";
        _viewModel.Precio = 0;
        _viewModel.CategoriaId = Guid.NewGuid();

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Validación", "El precio debe ser mayor a 0", "OK"), Times.Once);
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GuardarAsync_WithEmptyCategoriaId_ShouldShowValidationError()
    {
        // Arrange
        _viewModel.Nombre = "Pizza";
        _viewModel.Precio = 15.99m;
        _viewModel.CategoriaId = Guid.Empty;

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Validación", "Selecciona una categoría", "OK"), Times.Once);
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GuardarAsync_ShouldSetIsBusy()
    {
        // Arrange
        _viewModel.Nombre = "Pizza";
        _viewModel.Precio = 15.99m;
        _viewModel.CategoriaId = Guid.NewGuid();

        var apiResponse = ApiResponse<ProductoDto>.SuccessResponse(new ProductoDto { Id = Guid.NewGuid() });

        _mockProductosService.Setup(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.GuardarCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.CrearProductoAsync(It.IsAny<CrearProductoRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _viewModel.IsBusy.Should().BeFalse(); // Should be reset after completion
    }

    [Fact]
    public async Task CargarParaEdicionAsync_ShouldSetIsBusy()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var producto = new ProductoDto { Id = productoId, Nombre = "Test" };
        var apiResponse = ApiResponse<ProductoDto>.SuccessResponse(producto);

        _mockProductosService.Setup(x => x.ObtenerProductoPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.CargarParaEdicionCommand.ExecuteAsync(productoId);

        // Assert
        _viewModel.IsBusy.Should().BeFalse(); // Should be reset after completion
    }
}