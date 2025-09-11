using Xunit;
using Moq;
using RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;
using RestaurantePro.Mobile.Core.Features.Operations.Comandas.Models;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using ComandaModels = RestaurantePro.Mobile.Core.Features.Operations.Comandas.Models;

namespace RestaurantePro.Mobile.UnitTests.Features.Operations.Comandas.ViewModels;

/// <summary>
/// Pruebas unitarias para CrearComandaViewModel - Flujo principal del negocio
/// </summary>
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

    #region Constructor e Inicialización

    [Fact]
    public void Constructor_ShouldInitializeCollectionsAndProperties()
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
    public void TituloPagina_WhenEsEdicionFalse_ShouldReturnNuevaComanda()
    {
        // Arrange
        _viewModel.EsEdicion = false;

        // Act & Assert
        Assert.Equal("Nueva Comanda", _viewModel.TituloPagina);
    }

    [Fact]
    public void TituloPagina_WhenEsEdicionTrue_ShouldReturnEditandoComanda()
    {
        // Arrange
        _viewModel.EsEdicion = true;

        // Act & Assert
        Assert.Equal("Editando Comanda", _viewModel.TituloPagina);
    }

    [Fact]
    public void TextoBotonPrimario_WhenEsEdicionFalse_ShouldReturnCrear()
    {
        // Arrange
        _viewModel.EsEdicion = false;

        // Act & Assert
        Assert.Equal("Crear", _viewModel.TextoBotonPrimario);
    }

    [Fact]
    public void TextoBotonPrimario_WhenEsEdicionTrue_ShouldReturnGuardarCambios()
    {
        // Arrange
        _viewModel.EsEdicion = true;

        // Act & Assert
        Assert.Equal("Guardar Cambios", _viewModel.TextoBotonPrimario);
    }

    #endregion

    #region Propiedades Calculadas

    [Fact]
    public void MesaInfo_WhenMesaIsDefault_ShouldReturnFormattedInfo()
    {
        // Arrange
        _viewModel.Mesa = new MesaDto(); // Usar objeto por defecto

        // Act & Assert
        Assert.Equal("Mesa 0 -  (Capacidad: 0)", _viewModel.MesaInfo);
    }

    [Fact]
    public void MesaInfo_WhenMesaIsValid_ShouldReturnFormattedInfo()
    {
        // Arrange
        _viewModel.Mesa = new MesaDto
        {
            Id = Guid.NewGuid(),
            Numero = "5",
            Ubicacion = "Salón Principal",
            Capacidad = 4
        };

        // Act & Assert
        Assert.Equal("Mesa 5 - Salón Principal (Capacidad: 4)", _viewModel.MesaInfo);
    }

    [Fact]
    public void PuedeCrearComanda_WhenCarritoEmpty_ShouldReturnFalse()
    {
        // Arrange
        _viewModel.ProductosCarrito.Clear();
        _viewModel.IsLoading = false;

        // Act & Assert
        Assert.False(_viewModel.PuedeCrearComanda);
    }

    [Fact]
    public void PuedeCrearComanda_WhenCarritoHasItemsAndNotLoading_ShouldReturnTrue()
    {
        // Arrange
        _viewModel.ProductosCarrito.Add(new ProductoCarritoDto
        {
            Id = "1",
            Nombre = "Pizza",
            Precio = 15.99m,
            Cantidad = 1
        });
        _viewModel.IsLoading = false;

        // Act & Assert
        Assert.True(_viewModel.PuedeCrearComanda);
    }

    [Fact]
    public void TotalCarrito_WhenEmpty_ShouldReturnZero()
    {
        // Arrange
        _viewModel.ProductosCarrito.Clear();

        // Act & Assert
        Assert.Equal(0, _viewModel.TotalCarrito);
    }

    [Fact]
    public void TotalCarrito_WhenHasItems_ShouldReturnCorrectSum()
    {
        // Arrange
        _viewModel.ProductosCarrito.Clear();
        _viewModel.ProductosCarrito.Add(new ProductoCarritoDto
        {
            Id = "1",
            Nombre = "Pizza",
            Precio = 15.99m,
            Cantidad = 2
        });
        _viewModel.ProductosCarrito.Add(new ProductoCarritoDto
        {
            Id = "2",
            Nombre = "Bebida",
            Precio = 3.50m,
            Cantidad = 1
        });

        // Act & Assert
        Assert.Equal(35.48m, _viewModel.TotalCarrito); // (15.99 * 2) + (3.50 * 1)
    }

    #endregion

    #region BuscarProductosAsync

    [Fact]
    public async Task BuscarProductosAsync_WithSearchText_ShouldCallBuscarProductosAsync()
    {
        // Arrange
        _viewModel.TextoBusqueda = "pizza";
        var productos = new List<ProductoDto>
        {
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Pizza Margherita", Precio = 15.99m }
        };
        var apiResponse = ApiResponse<List<ProductoDto>>.SuccessResponse(productos);

        _mockProductosService.Setup(x => x.BuscarProductosAsync("pizza"))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync(null);

        // Assert
        _mockProductosService.Verify(x => x.BuscarProductosAsync("pizza"), Times.Once);
        Assert.Single(_viewModel.ProductosDisponibles);
        Assert.Equal("Pizza Margherita", _viewModel.ProductosDisponibles.First().Nombre);
    }

    [Fact]
    public async Task BuscarProductosAsync_WhenApiFails_ShouldShowError()
    {
        // Arrange
        _viewModel.TextoBusqueda = "pizza";
        var apiResponse = ApiResponse<List<ProductoDto>>.Failure("Error de API");

        _mockProductosService.Setup(x => x.BuscarProductosAsync("pizza"))
            .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.BuscarProductosCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Error", It.Is<string>(msg => msg.Contains("Error al buscar productos"))), Times.Once);
        Assert.Empty(_viewModel.ProductosDisponibles);
    }

    #endregion

    #region AgregarAlCarrito

    [Fact]
    public void AgregarAlCarrito_WithValidProduct_ShouldAddToCarrito()
    {
        // Arrange
        var producto = new ProductoCarritoDto
        {
            Id = "1",
            Nombre = "Pizza",
            Precio = 15.99m,
            Cantidad = 2
        };
        _viewModel.ProductosCarrito.Clear();

        // Act
        _viewModel.AgregarAlCarritoCommand.Execute(producto);

        // Assert
        Assert.Single(_viewModel.ProductosCarrito);
        Assert.Equal("Pizza", _viewModel.ProductosCarrito.First().Nombre);
        Assert.Equal(2, _viewModel.ProductosCarrito.First().Cantidad);
        Assert.Equal(0, producto.Cantidad); // Se resetea la cantidad en el producto original
    }

    [Fact]
    public void AgregarAlCarrito_WithExistingProduct_ShouldIncrementQuantity()
    {
        // Arrange
        var productoExistente = new ProductoCarritoDto
        {
            Id = "1",
            Nombre = "Pizza",
            Precio = 15.99m,
            Cantidad = 1
        };
        _viewModel.ProductosCarrito.Add(productoExistente);

        var productoNuevo = new ProductoCarritoDto
        {
            Id = "1",
            Nombre = "Pizza",
            Precio = 15.99m,
            Cantidad = 2
        };

        // Act
        _viewModel.AgregarAlCarritoCommand.Execute(productoNuevo);

        // Assert
        Assert.Single(_viewModel.ProductosCarrito);
        Assert.Equal(3, _viewModel.ProductosCarrito.First().Cantidad);
    }

    [Fact]
    public void AgregarAlCarrito_WithZeroQuantity_ShouldNotAddToCarrito()
    {
        // Arrange
        var producto = new ProductoCarritoDto
        {
            Id = "1",
            Nombre = "Pizza",
            Precio = 15.99m,
            Cantidad = 0
        };
        _viewModel.ProductosCarrito.Clear();

        // Act
        _viewModel.AgregarAlCarritoCommand.Execute(producto);

        // Assert
        Assert.Empty(_viewModel.ProductosCarrito);
    }

    #endregion

    #region IncrementarCantidad

    [Fact]
    public void IncrementarCantidad_WithValidProduct_ShouldIncrementQuantity()
    {
        // Arrange
        var producto = new ProductoCarritoDto
        {
            Id = "1",
            Nombre = "Pizza",
            Precio = 15.99m,
            Cantidad = 1
        };

        _mockDailyPreparationsService.Setup(x => x.GetPreparacionesDiariasPorProductoAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Result<List<PreparacionDiariaDto>>.Success(new List<PreparacionDiariaDto>()));

        // Act
        _viewModel.IncrementarCantidadCommand.Execute(producto);

        // Assert
        Assert.Equal(2, producto.Cantidad);
    }

    #endregion

    #region DecrementarCantidad

    [Fact]
    public void DecrementarCantidad_WithValidProduct_ShouldDecrementQuantity()
    {
        // Arrange
        var producto = new ProductoCarritoDto
        {
            Id = "1",
            Nombre = "Pizza",
            Precio = 15.99m,
            Cantidad = 2
        };

        // Act
        _viewModel.DecrementarCantidadCommand.Execute(producto);

        // Assert
        Assert.Equal(1, producto.Cantidad);
    }

    [Fact]
    public void DecrementarCantidad_WithZeroQuantity_ShouldNotDecrement()
    {
        // Arrange
        var producto = new ProductoCarritoDto
        {
            Id = "1",
            Nombre = "Pizza",
            Precio = 15.99m,
            Cantidad = 0
        };

        // Act
        _viewModel.DecrementarCantidadCommand.Execute(producto);

        // Assert
        Assert.Equal(0, producto.Cantidad);
    }

    #endregion

    #region EliminarDelCarrito

    [Fact]
    public void EliminarDelCarrito_WithValidProduct_ShouldRemoveFromCarrito()
    {
        // Arrange
        var producto = new ProductoCarritoDto
        {
            Id = "1",
            Nombre = "Pizza",
            Precio = 15.99m,
            Cantidad = 1
        };
        _viewModel.ProductosCarrito.Add(producto);

        // Act
        _viewModel.EliminarDelCarritoCommand.Execute(producto);

        // Assert
        Assert.Empty(_viewModel.ProductosCarrito);
    }

    #endregion

    #region CrearComandaAsync

    [Fact]
    public async Task CrearComandaAsync_WhenCarritoEmpty_ShouldShowError()
    {
        // Arrange
        _viewModel.ProductosCarrito.Clear();
        _viewModel.EsEdicion = false;

        // Act
        await _viewModel.CrearComandaCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowAlertAsync("Error", "Debe seleccionar al menos un producto"), Times.Once);
        _mockComandasService.Verify(x => x.CrearComandaAsync(It.IsAny<ComandaModels.CrearComandaRequest>()), Times.Never);
    }

    [Fact]
    public async Task CrearComandaAsync_WhenUserCancels_ShouldNotCreateComanda()
    {
        // Arrange
        _viewModel.ProductosCarrito.Add(new ProductoCarritoDto
        {
            Id = "1",
            Nombre = "Pizza",
            Precio = 15.99m,
            Cantidad = 1
        });
        _viewModel.Mesa = new MesaDto { Id = Guid.NewGuid(), Numero = "1" };

        _mockDialogService.Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        await _viewModel.CrearComandaCommand.ExecuteAsync(null);

        // Assert
        _mockComandasService.Verify(x => x.CrearComandaAsync(It.IsAny<ComandaModels.CrearComandaRequest>()), Times.Never);
    }

    [Fact]
    public async Task CrearComandaAsync_WithValidData_ShouldCreateComanda()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        _viewModel.Mesa = new MesaDto { Id = mesaId, Numero = "1" };
        _viewModel.Observaciones = "Sin cebolla";
        
        _viewModel.ProductosCarrito.Add(new ProductoCarritoDto
        {
            Id = "1",
            Nombre = "Pizza",
            Precio = 15.99m,
            Cantidad = 2
        });

        var comandaCreada = new ComandaDto
        {
            Id = Guid.NewGuid(),
            Numero = "001",
            MesaId = mesaId
        };

        _mockDialogService.Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _mockComandasService.Setup(x => x.CrearComandaAsync(It.IsAny<ComandaModels.CrearComandaRequest>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(comandaCreada));

        // Act
        await _viewModel.CrearComandaCommand.ExecuteAsync(null);

        // Assert
        _mockComandasService.Verify(x => x.CrearComandaAsync(It.Is<ComandaModels.CrearComandaRequest>(req => 
            req.MesaId == mesaId.ToString() && 
            req.Observaciones == "Sin cebolla" &&
            req.Items.Count == 1)), Times.Once);

        _mockDialogService.Verify(x => x.ShowAlertAsync("Éxito", "Comanda creada exitosamente"), Times.Once);
        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Once);
    }

    #endregion

    #region CancelarAsync

    [Fact]
    public async Task CancelarAsync_WhenUserConfirms_ShouldNavigateBack()
    {
        // Arrange
        _mockDialogService.Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _viewModel.CancelarCommand.ExecuteAsync(null);

        // Assert
        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelarAsync_WhenUserCancels_ShouldNotNavigate()
    {
        // Arrange
        _mockDialogService.Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        await _viewModel.CancelarCommand.ExecuteAsync(null);

        // Assert
        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Never);
    }

    #endregion

    #region InitializeAsync

    [Fact]
    public async Task InitializeAsync_WithValidMesaId_ShouldLoadMesaAndProducts()
    {
        // Arrange
        var mesaId = Guid.NewGuid().ToString();
        var mesa = new MesaDto { Id = Guid.Parse(mesaId), Numero = "5", Ubicacion = "Salón", Capacidad = 4 };
        var productos = new List<ProductoDto>
        {
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Pizza", Precio = 15.99m }
        };

        _mockMesasService.Setup(x => x.ObtenerMesaAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ApiResponse<MesaDto>.SuccessResponse(mesa));

        _mockProductosService.Setup(x => x.ObtenerProductosPaginadosAsync(1, 100, null, true))
            .ReturnsAsync(ApiResponse<List<ProductoDto>>.SuccessResponse(productos));

        // Act
        await _viewModel.InitializeAsync(mesaId);

        // Assert
        _mockMesasService.Verify(x => x.ObtenerMesaAsync(Guid.Parse(mesaId)), Times.Once);
        _mockProductosService.Verify(x => x.ObtenerProductosPaginadosAsync(1, 100, null, true), Times.Once);
        Assert.Equal(mesa.Numero, _viewModel.Mesa.Numero);
        Assert.Single(_viewModel.ProductosDisponibles);
    }

    #endregion

    #region LoadPreparacionesDiaAsync

    [Fact]
    public async Task LoadPreparacionesDiaAsync_WithValidData_ShouldLoadPreparaciones()
    {
        // Arrange
        var preparaciones = new List<PreparacionDiariaDto>
        {
            new PreparacionDiariaDto
            {
                Id = Guid.NewGuid(),
                NombreProducto = "Pizza",
                CantidadDisponible = 5
            }
        };

        _mockDailyPreparationsService.Setup(x => x.GetPreparacionesDiariasAsync())
            .ReturnsAsync(Result<List<PreparacionDiariaDto>>.Success(preparaciones));

        // Act
        await _viewModel.LoadPreparacionesDiaCommand.ExecuteAsync(null);

        // Assert
        Assert.Single(_viewModel.PreparacionesDelDia);
    }

    #endregion
}
