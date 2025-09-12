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