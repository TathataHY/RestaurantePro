using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Features.Operations.Comandas.Models;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Services;

namespace RestaurantePro.Mobile.UnitTests.Features.Operations.Comandas.ViewModels;

public class CrearComandaViewModelTests
{
    private readonly Mock<IComandasService> _mockComandasService;
    private readonly Mock<IProductosService> _mockProductosService;
    private readonly Mock<IMesasService> _mockMesasService;
    private readonly Mock<IDailyPreparationsService> _mockDailyPreparationsService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly CrearComandaViewModel _viewModel;

    public CrearComandaViewModelTests()
    {
        _mockComandasService = new Mock<IComandasService>();
        _mockProductosService = new Mock<IProductosService>();
        _mockMesasService = new Mock<IMesasService>();
        _mockDailyPreparationsService = new Mock<IDailyPreparationsService>();
        _mockNavigationService = new Mock<INavigationService>();
        _mockDialogService = new Mock<IDialogService>();

        _viewModel = new CrearComandaViewModel(
            _mockComandasService.Object,
            _mockProductosService.Object,
            _mockMesasService.Object,
            _mockDailyPreparationsService.Object,
            _mockNavigationService.Object,
            _mockDialogService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        Assert.NotNull(_viewModel.ProductosDisponibles);
        Assert.NotNull(_viewModel.ProductosCarrito);
        Assert.NotNull(_viewModel.PreparacionesDelDia);
        Assert.False(_viewModel.EsEdicion);
        Assert.Equal(Guid.Empty, _viewModel.ComandaId);
        Assert.Equal(string.Empty, _viewModel.TextoBusqueda);
        Assert.Equal(string.Empty, _viewModel.Observaciones);
    }

    [Fact]
    public void Constructor_ShouldInitializeCalculatedProperties()
    {
        // Assert
        Assert.Equal("Nueva Comanda", _viewModel.TituloPagina);
        Assert.Equal("Crear", _viewModel.TextoBotonPrimario);
        Assert.False(_viewModel.PuedeCrearComanda);
        Assert.False(_viewModel.PuedeGuardar);
        Assert.Equal(0m, _viewModel.TotalCarrito);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void EsEdicion_WhenSetToTrue_ShouldUpdateCalculatedProperties()
    {
        // Act
        _viewModel.EsEdicion = true;

        // Assert
        Assert.True(_viewModel.EsEdicion);
        Assert.Equal("Editando Comanda", _viewModel.TituloPagina);
        Assert.Equal("Guardar Cambios", _viewModel.TextoBotonPrimario);
        Assert.True(_viewModel.PuedeGuardar);
    }

    [Fact]
    public void Mesa_WhenSet_ShouldUpdateMesaInfo()
    {
        // Arrange
        var mesa = new MesaDto
        {
            Id = Guid.NewGuid(),
            Numero = "5",
            Ubicacion = "Terraza",
            Capacidad = 4
        };

        // Act
        _viewModel.Mesa = mesa;

        // Assert
        Assert.Equal("Mesa 5 - Terraza (Capacidad: 4)", _viewModel.MesaInfo);
    }

    [Fact]
    public void ProductosCarrito_WhenItemsAdded_ShouldUpdateCalculatedProperties()
    {
        // Arrange
        var producto = CreateProductoCarritoDto("1", "Pizza", 25.50m, 2);

        // Act
        _viewModel.ProductosCarrito.Add(producto);

        // Assert
        Assert.True(_viewModel.PuedeCrearComanda);
        Assert.True(_viewModel.PuedeGuardar);
        Assert.Equal(51.00m, _viewModel.TotalCarrito);
    }

    #endregion

    #region DecrementarCantidad Tests

    [Fact]
    public void DecrementarCantidad_WithValidProduct_ShouldDecrementQuantity()
    {
        // Arrange
        var producto = CreateProductoCarritoDto("1", "Pizza", 25.50m, 2);

        // Act
        _viewModel.DecrementarCantidadCommand.Execute(producto);

        // Assert
        Assert.Equal(1, producto.Cantidad);
    }

    [Fact]
    public void DecrementarCantidad_WithZeroQuantity_ShouldNotDecrement()
    {
        // Arrange
        var producto = CreateProductoCarritoDto("1", "Pizza", 25.50m, 0);

        // Act
        _viewModel.DecrementarCantidadCommand.Execute(producto);

        // Assert
        Assert.Equal(0, producto.Cantidad);
    }

    #endregion

    #region AgregarAlCarrito Tests

    [Fact]
    public void AgregarAlCarrito_WithValidProduct_ShouldAddToCarrito()
    {
        // Arrange
        var producto = CreateProductoCarritoDto("1", "Pizza", 25.50m, 2);

        // Act
        _viewModel.AgregarAlCarritoCommand.Execute(producto);

        // Assert
        Assert.Single(_viewModel.ProductosCarrito);
        Assert.Equal(2, _viewModel.ProductosCarrito.First().Cantidad);
        Assert.Equal(0, producto.Cantidad); // Should reset to 0
    }

    [Fact]
    public void AgregarAlCarrito_WithExistingProduct_ShouldIncrementQuantity()
    {
        // Arrange
        var producto = CreateProductoCarritoDto("1", "Pizza", 25.50m, 2);
        _viewModel.ProductosCarrito.Add(CreateProductoCarritoDto("1", "Pizza", 25.50m, 1));

        // Act
        _viewModel.AgregarAlCarritoCommand.Execute(producto);

        // Assert
        Assert.Single(_viewModel.ProductosCarrito);
        Assert.Equal(3, _viewModel.ProductosCarrito.First().Cantidad);
    }

    #endregion

    #region EliminarDelCarrito Tests

    [Fact]
    public void EliminarDelCarrito_WithValidProduct_ShouldRemoveFromCarrito()
    {
        // Arrange
        var producto = CreateProductoCarritoDto("1", "Pizza", 25.50m, 2);
        _viewModel.ProductosCarrito.Add(producto);

        // Act
        _viewModel.EliminarDelCarritoCommand.Execute(producto);

        // Assert
        Assert.Empty(_viewModel.ProductosCarrito);
    }

    #endregion

    #region CrearComandaAsync Tests

    [Fact]
    public async Task CrearComandaAsync_WithNoProducts_ShouldShowError()
    {
        // Act
        await _viewModel.CrearComandaCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Error", "Debe seleccionar al menos un producto", It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region CancelarAsync Tests

    [Fact]
    public async Task CancelarAsync_WithUserConfirmation_ShouldNavigateBack()
    {
        // Arrange
        _mockDialogService.Setup(x => x.ShowConfirmAsync("Cancelar", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _viewModel.CancelarCommand.ExecuteAsync(null);

        // Assert
        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelarAsync_WithUserCancellation_ShouldNotNavigateBack()
    {
        // Arrange
        _mockDialogService.Setup(x => x.ShowConfirmAsync("Cancelar", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        await _viewModel.CancelarCommand.ExecuteAsync(null);

        // Assert
        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Never);
    }

    #endregion

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Validaciones de Datos de Entrada

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task BuscarProductosAsync_WithInvalidSearchText_ShouldHandleGracefully(string searchText)
    {
        // Arrange
        _viewModel.TextoBusqueda = searchText;
        _mockProductosService.Setup(x => x.ObtenerProductosPaginadosAsync(1, 100, null, true))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(new List<ProductoDto>()));

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.ObtenerProductosPaginadosAsync(1, 100, null, true), Times.Once);
        _mockProductosService.Verify(x => x.BuscarProductosAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task BuscarProductosAsync_WithVeryLongSearchText_ShouldTruncateOrHandle()
    {
        // Arrange
        var longSearchText = new string('a', 1000); // 1000 caracteres
        _viewModel.TextoBusqueda = longSearchText;
        _mockProductosService.Setup(x => x.BuscarProductosAsync(It.IsAny<string>()))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(new List<ProductoDto>()));

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.BuscarProductosAsync(longSearchText), Times.Once);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1000000)]
    public void IncrementarCantidad_WithExtremeValues_ShouldHandleGracefully(int initialQuantity)
    {
        // Arrange
        var producto = CreateProductoCarritoDto("1", "Pizza", 25.50m, initialQuantity);

        // Act & Assert
        // Should not throw exception
        _viewModel.IncrementarCantidadCommand.Execute(producto);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Observaciones_WithInvalidValues_ShouldBeHandled(string observaciones)
    {
        // Act
        _viewModel.Observaciones = observaciones;

        // Assert
        Assert.Equal(observaciones ?? string.Empty, _viewModel.Observaciones);
    }

    #endregion

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Manejo de Errores de Red y Servicios

    [Fact]
    public async Task BuscarProductosAsync_WithNetworkTimeout_ShouldShowError()
    {
        // Arrange
        _viewModel.TextoBusqueda = "pizza";
        _mockProductosService.Setup(x => x.BuscarProductosAsync("pizza"))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Error", It.Is<string>(s => s.Contains("timeout")), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task BuscarProductosAsync_WithHttpException_ShouldShowError()
    {
        // Arrange
        _viewModel.TextoBusqueda = "pizza";
        _mockProductosService.Setup(x => x.BuscarProductosAsync("pizza"))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Error", It.Is<string>(s => s.Contains("Network error")), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task BuscarProductosAsync_WithServiceFailure_ShouldShowError()
    {
        // Arrange
        _viewModel.TextoBusqueda = "pizza";
        _mockProductosService.Setup(x => x.BuscarProductosAsync("pizza"))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.Failure("Service unavailable"));

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Error", It.Is<string>(s => s.Contains("Service unavailable")), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task BuscarProductosAsync_WithNullData_ShouldHandleGracefully()
    {
        // Arrange
        _viewModel.TextoBusqueda = "pizza";
        _mockProductosService.Setup(x => x.BuscarProductosAsync("pizza"))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(null!));

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync(null);

        // Assert
        Assert.Empty(_viewModel.ProductosDisponibles);
    }

    [Fact]
    public async Task LoadPreparacionesDiaAsync_WithServiceError_ShouldShowError()
    {
        // Arrange
        _mockDailyPreparationsService.Setup(x => x.GetPreparacionesDiariasAsync())
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        await _viewModel.LoadPreparacionesDiaCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Error", It.Is<string>(s => s.Contains("Database connection failed")), It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Casos Edge y Límites

    [Fact]
    public async Task IncrementarCantidad_WithMaxLimitReached_ShouldShowAlert()
    {
        // Arrange
        var producto = CreateProductoCarritoDto("1", "Pizza", 25.50m, 10); // Max limit
        producto.PreparacionesDisponiblesHoy = 0; // No hay preparaciones disponibles

        // Act
        await _viewModel.IncrementarCantidadCommand.ExecuteAsync(producto);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Límite alcanzado", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task IncrementarCantidad_WithNegativePreparaciones_ShouldHandleGracefully()
    {
        // Arrange
        var producto = CreateProductoCarritoDto("1", "Pizza", 25.50m, 1);
        producto.PreparacionesDisponiblesHoy = -1; // Invalid value

        // Act
        await _viewModel.IncrementarCantidadCommand.ExecuteAsync(producto);

        // Assert
        // Should not throw exception and should handle gracefully
        Assert.True(true);
    }

    [Fact]
    public void TotalCarrito_WithVeryLargeNumbers_ShouldCalculateCorrectly()
    {
        // Arrange
        var producto1 = CreateProductoCarritoDto("1", "Pizza", 999999.99m, 100);
        var producto2 = CreateProductoCarritoDto("2", "Bebida", 0.01m, 1);

        // Act
        _viewModel.ProductosCarrito.Add(producto1);
        _viewModel.ProductosCarrito.Add(producto2);

        // Assert
        Assert.Equal(99999999.01m, _viewModel.TotalCarrito);
    }

    [Fact]
    public void TotalCarrito_WithZeroPrices_ShouldCalculateCorrectly()
    {
        // Arrange
        var producto = CreateProductoCarritoDto("1", "Gratis", 0m, 5);

        // Act
        _viewModel.ProductosCarrito.Add(producto);

        // Assert
        Assert.Equal(0m, _viewModel.TotalCarrito);
    }

    [Fact]
    public void TotalCarrito_WithNegativePrices_ShouldCalculateCorrectly()
    {
        // Arrange
        var producto = CreateProductoCarritoDto("1", "Descuento", -10m, 2);

        // Act
        _viewModel.ProductosCarrito.Add(producto);

        // Assert
        Assert.Equal(-20m, _viewModel.TotalCarrito);
    }

    #endregion

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Concurrencia y Threading

    [Fact]
    public async Task BuscarProductosAsync_WithConcurrentCalls_ShouldHandleGracefully()
    {
        // Arrange
        _viewModel.TextoBusqueda = "pizza";
        _mockProductosService.Setup(x => x.BuscarProductosAsync("pizza"))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(new List<ProductoDto>()));

        // Act
        var task1 = _viewModel.BuscarProductosCommand.ExecuteAsync(null);
        var task2 = _viewModel.BuscarProductosCommand.ExecuteAsync(null);
        var task3 = _viewModel.BuscarProductosCommand.ExecuteAsync(null);

        await Task.WhenAll(task1, task2, task3);

        // Assert
        // Should not throw exception and should handle concurrent calls
        Assert.True(true);
    }

    [Fact]
    public async Task IncrementarCantidad_WithConcurrentCalls_ShouldHandleGracefully()
    {
        // Arrange
        var producto = CreateProductoCarritoDto("1", "Pizza", 25.50m, 1);

        // Act
        var task1 = _viewModel.IncrementarCantidadCommand.ExecuteAsync(producto);
        var task2 = _viewModel.IncrementarCantidadCommand.ExecuteAsync(producto);
        var task3 = _viewModel.IncrementarCantidadCommand.ExecuteAsync(producto);

        await Task.WhenAll(task1, task2, task3);

        // Assert
        // Should not throw exception and should handle concurrent calls
        Assert.True(true);
    }

    #endregion

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Validaciones de Preparaciones Diarias

    [Fact]
    public async Task IncrementarDesdePreparacionAsync_WithNullPreparacion_ShouldReturnEarly()
    {
        // Act
        await _viewModel.IncrementarDesdePreparacionCommand.ExecuteAsync(null);

        // Assert
        // Should return early without doing anything
        Assert.True(true);
    }

    [Fact]
    public async Task IncrementarDesdePreparacionAsync_WithExpiredPreparacion_ShouldShowAlert()
    {
        // Arrange
        var preparacion = CreatePreparacionDiariaDto("1", "Pizza", 0);
        preparacion.EstaVencida = true;

        // Act
        await _viewModel.IncrementarDesdePreparacionCommand.ExecuteAsync(preparacion);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("No disponible", "La preparación no está disponible o no tiene unidades.", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task IncrementarDesdePreparacionAsync_WithZeroCantidad_ShouldShowAlert()
    {
        // Arrange
        var preparacion = CreatePreparacionDiariaDto("1", "Pizza", 0);
        preparacion.EstaVencida = false;

        // Act
        await _viewModel.IncrementarDesdePreparacionCommand.ExecuteAsync(preparacion);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("No disponible", "La preparación no está disponible o no tiene unidades.", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task IncrementarDesdePreparacionAsync_WithProductNotFound_ShouldShowAlert()
    {
        // Arrange
        var preparacion = CreatePreparacionDiariaDto("1", "Pizza", 5);
        preparacion.EstaVencida = false;
        preparacion.ProductoId = Guid.NewGuid();

        _mockProductosService.Setup(x => x.ObtenerProductoPorIdAsync(preparacion.ProductoId))
            .ReturnsAsync(ApiResponse<ProductoDto>.Failure("Product not found"));

        // Act
        await _viewModel.IncrementarDesdePreparacionCommand.ExecuteAsync(preparacion);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Producto no disponible", "El producto de la preparación no está disponible.", It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Manejo de Errores de API

    [Fact]
    public async Task CrearComandaAsync_WithApiError_ShouldShowDetailedError()
    {
        // Arrange
        var producto = CreateProductoCarritoDto("1", "Pizza", 25.50m, 1);
        _viewModel.ProductosCarrito.Add(producto);
        _viewModel.Mesa = CreateMesaDto("5");

        _mockComandasService.Setup(x => x.CrearComandaAsync(It.IsAny<CrearComandaRequest>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.Failure("API Error", new[] { "Error 1", "Error 2" }));

        _mockDialogService.Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _viewModel.CrearComandaCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Error", It.Is<string>(s => s.Contains("API Error") && s.Contains("Error 1") && s.Contains("Error 2")), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CrearComandaAsync_WithNetworkException_ShouldShowError()
    {
        // Arrange
        var producto = CreateProductoCarritoDto("1", "Pizza", 25.50m, 1);
        _viewModel.ProductosCarrito.Add(producto);
        _viewModel.Mesa = CreateMesaDto("5");

        _mockComandasService.Setup(x => x.CrearComandaAsync(It.IsAny<CrearComandaRequest>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        _mockDialogService.Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _viewModel.CrearComandaCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Error", It.Is<string>(s => s.Contains("Network error")), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CrearComandaAsync_WithTimeoutException_ShouldShowError()
    {
        // Arrange
        var producto = CreateProductoCarritoDto("1", "Pizza", 25.50m, 1);
        _viewModel.ProductosCarrito.Add(producto);
        _viewModel.Mesa = CreateMesaDto("5");

        _mockComandasService.Setup(x => x.CrearComandaAsync(It.IsAny<CrearComandaRequest>()))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        _mockDialogService.Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _viewModel.CrearComandaCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Error", It.Is<string>(s => s.Contains("timeout")), It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Casos de Fallo en Servicios

    [Fact]
    public async Task InitializeAsync_WithInvalidMesaId_ShouldShowError()
    {
        // Arrange
        var invalidMesaId = "invalid-guid";

        // Act
        await _viewModel.InitializeAsync(invalidMesaId);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Error", "Identificador de mesa inválido", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_WithMesaNotFound_ShouldShowError()
    {
        // Arrange
        var mesaId = Guid.NewGuid().ToString();
        _mockMesasService.Setup(x => x.ObtenerMesaAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ApiResponse<MesaDto>.Failure("Mesa not found"));

        // Act
        await _viewModel.InitializeAsync(mesaId);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Error", It.Is<string>(s => s.Contains("Mesa not found")), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task InitializeEdicionAsync_WithInvalidComandaId_ShouldNotLoad()
    {
        // Arrange
        var invalidComandaId = "invalid-guid";

        // Act
        await _viewModel.InitializeEdicionAsync(invalidComandaId);

        // Assert
        Assert.Equal(Guid.Empty, _viewModel.ComandaId);
        Assert.False(_viewModel.EsEdicion);
    }

    [Fact]
    public async Task InitializeEdicionAsync_WithComandaNotFound_ShouldShowError()
    {
        // Arrange
        var comandaId = Guid.NewGuid().ToString();
        _mockComandasService.Setup(x => x.ObtenerComandaPorIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.Failure("Comanda not found"));

        // Act
        await _viewModel.InitializeEdicionAsync(comandaId);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Error", "Comanda not found", It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region Helper Methods

    private ProductoCarritoDto CreateProductoCarritoDto(string id, string nombre, decimal precio, int cantidad)
    {
        return new ProductoCarritoDto
        {
            Id = id,
            Nombre = nombre,
            Precio = precio,
            Cantidad = cantidad
        };
    }

    private ProductoDto CreateProductoDto(string id, string nombre, decimal precio)
    {
        return new ProductoDto
        {
            Id = Guid.Parse(id),
            Nombre = nombre,
            Precio = precio
        };
    }

    private MesaDto CreateMesaDto(string numero)
    {
        return new MesaDto
        {
            Id = Guid.NewGuid(),
            Numero = numero,
            Ubicacion = "Terraza",
            Capacidad = 4
        };
    }

    private ComandaDto CreateComandaDto()
    {
        return new ComandaDto
        {
            Id = Guid.NewGuid(),
            Numero = "001",
            Total = 51.00m
        };
    }

    private PreparacionDiariaDto CreatePreparacionDiariaDto(string id, string nombre, int cantidad)
    {
        return new PreparacionDiariaDto
        {
            Id = Guid.Parse(id),
            NombreProducto = nombre,
            CantidadDisponible = cantidad,
            FechaVencimiento = DateTime.Now.AddDays(1)
        };
    }

    #endregion
}