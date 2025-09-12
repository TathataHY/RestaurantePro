using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Services.Categorias;

namespace RestaurantePro.Mobile.UnitTests.Features.DailyPreparations.ViewModels;

public class EditDailyPreparationViewModelTests
{
    private readonly Mock<IDailyPreparationsService> _mockDailyPreparationsService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly Mock<IProductosService> _mockProductosService;
    private readonly Mock<ICategoriasService> _mockCategoriasService;
    private readonly EditDailyPreparationViewModel _viewModel;

    public EditDailyPreparationViewModelTests()
    {
        _mockDailyPreparationsService = new Mock<IDailyPreparationsService>();
        _mockAuthService = new Mock<IAuthService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockNavigationService = new Mock<INavigationService>();
        _mockProductosService = new Mock<IProductosService>();
        _mockCategoriasService = new Mock<ICategoriasService>();

        _viewModel = new EditDailyPreparationViewModel(
            _mockDailyPreparationsService.Object,
            _mockAuthService.Object,
            _mockDialogService.Object,
            _mockNavigationService.Object,
            _mockProductosService.Object,
            _mockCategoriasService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        Assert.Equal("Editar Preparación", _viewModel.Titulo);
        Assert.NotNull(_viewModel.Categorias);
        Assert.NotNull(_viewModel.Productos);
        Assert.Equal(0, _viewModel.CantidadPreparada);
        Assert.Equal(0, _viewModel.CantidadDisponible);
        Assert.Equal(DateTime.Today.AddDays(1), _viewModel.FechaVencimiento);
        Assert.Equal(string.Empty, _viewModel.Observaciones);
        Assert.False(_viewModel.IsBusy);
    }

    #endregion

    #region InitializeAsync Tests

    [Fact]
    public async Task InitializeAsync_ShouldLoadCategoriasAndPreparacion()
    {
        // Arrange
        var preparacionId = Guid.NewGuid();
        var preparacion = CreatePreparacionDiariaDto();
        var categorias = CreateCategoriasList();

        _mockCategoriasService.Setup(x => x.ObtenerCategoriasAsync(true, CancellationToken.None))
            .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias));
        _mockDailyPreparationsService.Setup(x => x.GetPreparacionDiariaAsync(preparacionId))
            .ReturnsAsync(Result<PreparacionDiariaDto>.Success(preparacion));
        _mockProductosService.Setup(x => x.ObtenerProductosPaginadosAsync(1, 10, preparacion.NombreProducto, true, CancellationToken.None))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(new List<ProductoDto> { CreateProductoDto() }));

        // Act
        await _viewModel.InitializeAsync(preparacionId);

        // Assert
        Assert.Single(_viewModel.Categorias);
        Assert.Single(_viewModel.Productos);
        Assert.Equal(preparacion.CantidadPreparada, _viewModel.CantidadPreparada);
        Assert.Equal(preparacion.CantidadDisponible, _viewModel.CantidadDisponible);
        Assert.Equal(preparacion.FechaVencimiento, _viewModel.FechaVencimiento);
        Assert.Equal(preparacion.Observaciones, _viewModel.Observaciones);
    }

    [Fact]
    public async Task InitializeAsync_WhenPreparacionNotFound_ShouldShowError()
    {
        // Arrange
        var preparacionId = Guid.NewGuid();
        var categorias = CreateCategoriasList();

        _mockCategoriasService.Setup(x => x.ObtenerCategoriasAsync(true, CancellationToken.None))
            .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias));
        _mockDailyPreparationsService.Setup(x => x.GetPreparacionDiariaAsync(preparacionId))
            .ReturnsAsync(Result<PreparacionDiariaDto>.Failure("No encontrado"));

        // Act
        await _viewModel.InitializeAsync(preparacionId);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync("No encontrado"), Times.Once);
    }

    #endregion

    #region CargarCategoriasAsync Tests

    [Fact]
    public async Task CargarCategoriasAsync_ShouldLoadCategorias()
    {
        // Arrange
        var categorias = CreateCategoriasList();
        _mockCategoriasService.Setup(x => x.ObtenerCategoriasAsync(true, CancellationToken.None))
            .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias));

        // Act
        await _viewModel.CargarCategoriasCommand.ExecuteAsync(null);

        // Assert
        Assert.Single(_viewModel.Categorias);
        Assert.Equal(categorias[0].Id, _viewModel.Categorias[0].Id);
    }

    [Fact]
    public async Task CargarCategoriasAsync_WhenServiceFails_ShouldShowError()
    {
        // Arrange
        _mockCategoriasService.Setup(x => x.ObtenerCategoriasAsync(true, CancellationToken.None))
            .ThrowsAsync(new Exception("Error de servicio"));

        // Act
        await _viewModel.CargarCategoriasCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync("Error al cargar categorías: Error de servicio"), Times.Once);
    }

    #endregion

    #region BuscarProductosAsync Tests

    [Fact]
    public async Task BuscarProductosAsync_WithSearchTerm_ShouldSearchProducts()
    {
        // Arrange
        var productos = CreateProductosList();
        _mockProductosService.Setup(x => x.ObtenerProductosPaginadosAsync(1, 10, "test", true, CancellationToken.None))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(productos));

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync("test");

        // Assert
        Assert.Single(_viewModel.Productos);
        Assert.Equal(productos[0].Id, _viewModel.Productos[0].Id);
    }

    [Fact]
    public async Task BuscarProductosAsync_WithCategoriaSeleccionada_ShouldSearchByCategory()
    {
        // Arrange
        var categoria = CreateCategoriaDto();
        var productos = CreateProductosList();
        _viewModel.CategoriaSeleccionada = categoria;
        _mockProductosService.Setup(x => x.ObtenerProductosPorCategoriaAsync(categoria.Id, true, CancellationToken.None))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(productos));

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync(null);

        // Assert
        Assert.Single(_viewModel.Productos);
        _mockProductosService.Verify(x => x.ObtenerProductosPorCategoriaAsync(categoria.Id, true, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task BuscarProductosAsync_WhenIsBusy_ShouldReturnEarly()
    {
        // Arrange
        _viewModel.IsBusy = true;

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync("test");

        // Assert
        _mockProductosService.Verify(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuscarProductosAsync_WhenServiceFails_ShouldShowError()
    {
        // Arrange
        _mockProductosService.Setup(x => x.ObtenerProductosPaginadosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de servicio"));

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync("test");

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync("Error al buscar productos: Error de servicio"), Times.Once);
    }

    #endregion

    #region GuardarCambiosAsync Tests

    [Fact]
    public async Task GuardarCambiosAsync_WithValidData_ShouldUpdatePreparacion()
    {
        // Arrange
        var producto = CreateProductoDto();
        var chefId = Guid.NewGuid();
        _viewModel.ProductoSeleccionado = producto;
        _viewModel.CantidadPreparada = 10;
        _viewModel.CantidadDisponible = 8;
        _viewModel.FechaVencimiento = DateTime.Today.AddDays(2);
        _viewModel.Observaciones = "Test observaciones";

        _mockAuthService.Setup(x => x.GetUserIdAsync()).ReturnsAsync(chefId.ToString());
        var preparacionId = Guid.NewGuid();
        var command = new ActualizarPreparacionDiariaCommand();
        _mockDailyPreparationsService.Setup(x => x.ActualizarPreparacionDiariaAsync(It.IsAny<Guid>(), It.IsAny<ActualizarPreparacionDiariaCommand>()))
            .ReturnsAsync(Result<PreparacionDiariaDto>.Success(CreatePreparacionDiariaDto()));

        // Act
        await _viewModel.GuardarCambiosCommand.ExecuteAsync(null);

        // Assert
        _mockDailyPreparationsService.Verify(x => x.ActualizarPreparacionDiariaAsync(It.IsAny<Guid>(), It.IsAny<ActualizarPreparacionDiariaCommand>()), Times.Once);
        _mockDialogService.Verify(x => x.ShowSuccessAsync("Preparación actualizada correctamente"), Times.Once);
        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Once);
    }

    [Fact]
    public async Task GuardarCambiosAsync_WhenNoProductSelected_ShouldShowError()
    {
        // Arrange
        _viewModel.ProductoSeleccionado = null;

        // Act
        await _viewModel.GuardarCambiosCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync("Seleccione un producto."), Times.Once);
    }

    [Fact]
    public async Task GuardarCambiosAsync_WhenCantidadPreparadaIsZero_ShouldShowError()
    {
        // Arrange
        _viewModel.ProductoSeleccionado = CreateProductoDto();
        _viewModel.CantidadPreparada = 0;

        // Act
        await _viewModel.GuardarCambiosCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync("La cantidad preparada debe ser mayor que 0."), Times.Once);
    }

    [Fact]
    public async Task GuardarCambiosAsync_WhenCantidadDisponibleInvalid_ShouldShowError()
    {
        // Arrange
        _viewModel.ProductoSeleccionado = CreateProductoDto();
        _viewModel.CantidadPreparada = 10;
        _viewModel.CantidadDisponible = 15; // Mayor que la preparada

        // Act
        await _viewModel.GuardarCambiosCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync("La disponible debe estar entre 0 y la preparada."), Times.Once);
    }

    [Fact]
    public async Task GuardarCambiosAsync_WhenFechaVencimientoIsToday_ShouldShowError()
    {
        // Arrange
        _viewModel.ProductoSeleccionado = CreateProductoDto();
        _viewModel.CantidadPreparada = 10;
        _viewModel.CantidadDisponible = 8;
        _viewModel.FechaVencimiento = DateTime.Today;

        // Act
        await _viewModel.GuardarCambiosCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync("La fecha de vencimiento debe ser futura (desde mañana)."), Times.Once);
    }

    [Fact]
    public async Task GuardarCambiosAsync_WhenServiceFails_ShouldShowError()
    {
        // Arrange
        var producto = CreateProductoDto();
        var chefId = Guid.NewGuid();
        _viewModel.ProductoSeleccionado = producto;
        _viewModel.CantidadPreparada = 10;
        _viewModel.CantidadDisponible = 8;
        _viewModel.FechaVencimiento = DateTime.Today.AddDays(2);

        _mockAuthService.Setup(x => x.GetUserIdAsync()).ReturnsAsync(chefId.ToString());
        var preparacionId = Guid.NewGuid();
        var command = new ActualizarPreparacionDiariaCommand();
        _mockDailyPreparationsService.Setup(x => x.ActualizarPreparacionDiariaAsync(It.IsAny<Guid>(), It.IsAny<ActualizarPreparacionDiariaCommand>()))
            .ReturnsAsync(Result<PreparacionDiariaDto>.Failure("Error al actualizar"));

        // Act
        await _viewModel.GuardarCambiosCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GuardarCambiosAsync_WhenIsBusy_ShouldReturnEarly()
    {
        // Arrange
        _viewModel.IsBusy = true;

        // Act
        await _viewModel.GuardarCambiosCommand.ExecuteAsync(null);

        // Assert
        _mockDailyPreparationsService.Verify(x => x.ActualizarPreparacionDiariaAsync(It.IsAny<Guid>(), It.IsAny<ActualizarPreparacionDiariaCommand>()), Times.Never);
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

    private static CategoriaProductoDto CreateCategoriaDto()
    {
        return new CategoriaProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Test Categoria"
        };
    }

    private static List<CategoriaProductoDto> CreateCategoriasList()
    {
        return new List<CategoriaProductoDto> { CreateCategoriaDto() };
    }

    private static ProductoDto CreateProductoDto()
    {
        return new ProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Test Producto",
            CantidadDisponible = 10
        };
    }

    private static List<ProductoDto> CreateProductosList()
    {
        return new List<ProductoDto> { CreateProductoDto() };
    }

    private static PreparacionDiariaDto CreatePreparacionDiariaDto()
    {
        return new PreparacionDiariaDto
        {
            Id = Guid.NewGuid(),
            ProductoId = Guid.NewGuid(),
            NombreProducto = "Test Producto",
            CantidadPreparada = 10,
            CantidadDisponible = 8,
            FechaVencimiento = DateTime.Today.AddDays(2),
            Observaciones = "Test observaciones"
        };
    }

    #endregion
}
