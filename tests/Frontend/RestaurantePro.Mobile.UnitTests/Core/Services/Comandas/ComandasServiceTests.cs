using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Comandas;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Core.Services.Comandas;

public class ComandasServiceTests
{
    private readonly Mock<IApiService> _apiServiceMock;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly ComandasService _comandasService;

    public ComandasServiceTests()
    {
        _apiServiceMock = new Mock<IApiService>();
        _mockAuthService = new Mock<IAuthService>();
        _comandasService = new ComandasService(_apiServiceMock.Object, _mockAuthService.Object);
    }

    #region ObtenerComandasActivasAsync Tests

    [Fact]
    public async Task ObtenerComandasActivasAsync_ShouldReturnSuccess_WhenApiCallSucceeds()
    {
        // Arrange
        var expectedComandas = new List<ComandaDto>
        {
            new ComandaDto { Id = Guid.NewGuid(), Numero = "C001", Estado = "pendiente" },
            new ComandaDto { Id = Guid.NewGuid(), Numero = "C002", Estado = "en_preparacion" }
        };
        var expectedResponse = ApiResponse<List<ComandaDto>>.SuccessResponse(expectedComandas);

        _apiServiceMock.Setup(x => x.GetAsync<List<ComandaDto>>("api/comandas/activas", It.IsAny<string?>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _comandasService.ObtenerComandasActivasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedComandas);
    }

    [Fact]
    public async Task ObtenerComandasActivasAsync_ShouldReturnError_WhenApiCallFails()
    {
        // Arrange
        _apiServiceMock.Setup(x => x.GetAsync<List<ComandaDto>>("api/comandas/activas", It.IsAny<string?>()))
            .ThrowsAsync(new Exception("API error"));

        // Act
        var result = await _comandasService.ObtenerComandasActivasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Error al obtener comandas activas");
    }

    #endregion

    #region ObtenerComandasPorMesaAsync Tests

    [Fact]
    public async Task ObtenerComandasPorMesaAsync_ShouldReturnSuccess_WhenApiCallSucceeds()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var expectedComandas = new List<ComandaDto>
        {
            new ComandaDto { Id = Guid.NewGuid(), MesaId = mesaId, Numero = "C001" }
        };
        var expectedResponse = ApiResponse<List<ComandaDto>>.SuccessResponse(expectedComandas);

        _apiServiceMock.Setup(x => x.GetAsync<List<ComandaDto>>($"api/comandas/mesa/{mesaId}", It.IsAny<string?>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _comandasService.ObtenerComandasPorMesaAsync(mesaId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedComandas);
    }

    #endregion

    #region CrearComandaAsync Tests

    [Fact]
    public async Task CrearComandaAsync_ShouldReturnSuccess_WhenValidRequest()
    {
        // Arrange
        var request = new CrearComandaRequest
        {
            MesaId = Guid.NewGuid(),
            ClienteNombre = "Juan Pérez",
            Observaciones = "Sin cebolla"
        };

        var expectedComanda = new ComandaDto
        {
            Id = Guid.NewGuid(),
            MesaId = request.MesaId,
            ClienteNombre = request.ClienteNombre,
            Numero = "C001"
        };

        var expectedResponse = ApiResponse<ComandaDto>.SuccessResponse(expectedComanda);

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>("api/comandas", request, It.IsAny<string?>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _comandasService.CrearComandaAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedComanda);
    }

    [Fact]
    public async Task CrearComandaAsync_ShouldReturnError_WhenMesaIdIsEmpty()
    {
        // Arrange
        var request = new CrearComandaRequest
        {
            MesaId = Guid.Empty,
            ClienteNombre = "Juan Pérez"
        };

        // Act
        var result = await _comandasService.CrearComandaAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("La mesa es requerida");
    }

    #endregion

    #region AgregarProductosAsync Tests

    [Fact]
    public async Task AgregarProductosAsync_ShouldReturnSuccess_WhenValidRequest()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productos = new List<ComandaProductoRequest>
        {
            new ComandaProductoRequest { ProductoId = Guid.NewGuid(), Cantidad = 2 }
        };

        var expectedComanda = new ComandaDto { Id = comandaId };
        var expectedResponse = ApiResponse<ComandaDto>.SuccessResponse(expectedComanda);

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>($"api/comandas/{comandaId}/productos", productos, It.IsAny<string?>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _comandasService.AgregarProductosAsync(comandaId, productos);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedComanda);
    }

    [Fact]
    public async Task AgregarProductosAsync_ShouldReturnError_WhenComandaIdIsEmpty()
    {
        // Arrange
        var productos = new List<ComandaProductoRequest>
        {
            new ComandaProductoRequest { ProductoId = Guid.NewGuid(), Cantidad = 2 }
        };

        // Act
        var result = await _comandasService.AgregarProductosAsync(Guid.Empty, productos);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("ID de comanda es requerido");
    }

    [Fact]
    public async Task AgregarProductosAsync_ShouldReturnError_WhenProductosIsEmpty()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productos = new List<ComandaProductoRequest>();

        // Act
        var result = await _comandasService.AgregarProductosAsync(comandaId, productos);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Se requiere al menos un producto");
    }

    #endregion

    #region ActualizarCantidadProductoAsync Tests

    [Fact]
    public async Task ActualizarCantidadProductoAsync_ShouldReturnSuccess_WhenValidRequest()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var nuevaCantidad = 3;

        var expectedComanda = new ComandaDto { Id = comandaId };
        var expectedResponse = ApiResponse<ComandaDto>.SuccessResponse(expectedComanda);

        _apiServiceMock.Setup(x => x.PutAsync<ComandaDto>($"api/comandas/{comandaId}/productos/{productoId}/cantidad", It.IsAny<object>(), It.IsAny<string?>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _comandasService.ActualizarCantidadProductoAsync(comandaId, productoId, nuevaCantidad);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedComanda);
    }

    [Fact]
    public async Task ActualizarCantidadProductoAsync_ShouldReturnError_WhenCantidadIsZero()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var nuevaCantidad = 0;

        // Act
        var result = await _comandasService.ActualizarCantidadProductoAsync(comandaId, productoId, nuevaCantidad);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("La cantidad debe ser mayor a 0");
    }

    #endregion

    #region CambiarEstadoComandaAsync Tests

    [Fact]
    public async Task CambiarEstadoComandaAsync_ShouldReturnSuccess_WhenValidRequest()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var nuevoEstado = "en_preparacion";
        var observaciones = "Iniciando preparación";

        var expectedComanda = new ComandaDto { Id = comandaId, Estado = nuevoEstado };
        var expectedResponse = ApiResponse<ComandaDto>.SuccessResponse(expectedComanda);

        _apiServiceMock.Setup(x => x.PutAsync<ComandaDto>($"api/comandas/{comandaId}/estado", It.IsAny<object>(), It.IsAny<string?>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _comandasService.CambiarEstadoComandaAsync(comandaId, nuevoEstado, observaciones);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedComanda);
    }

    [Fact]
    public async Task CambiarEstadoComandaAsync_ShouldReturnError_WhenEstadoIsEmpty()
    {
        // Arrange
        var comandaId = Guid.NewGuid();

        // Act
        var result = await _comandasService.CambiarEstadoComandaAsync(comandaId, "", null);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("El nuevo estado es requerido");
    }

    #endregion

    #region FinalizarComandaAsync Tests

    [Fact]
    public async Task FinalizarComandaAsync_ShouldReturnSuccess_WhenValidRequest()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var metodoPago = "efectivo";
        var observaciones = "Pago completado";

        var expectedComanda = new ComandaDto { Id = comandaId, Estado = "finalizada" };
        var expectedResponse = ApiResponse<ComandaDto>.SuccessResponse(expectedComanda);

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>($"api/comandas/{comandaId}/finalizar", It.IsAny<object>(), It.IsAny<string?>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _comandasService.FinalizarComandaAsync(comandaId, metodoPago, observaciones);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedComanda);
    }

    [Fact]
    public async Task FinalizarComandaAsync_ShouldReturnError_WhenMetodoPagoIsEmpty()
    {
        // Arrange
        var comandaId = Guid.NewGuid();

        // Act
        var result = await _comandasService.FinalizarComandaAsync(comandaId, "", null);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("El método de pago es requerido");
    }

    #endregion

    #region CancelarComandaAsync Tests

    [Fact]
    public async Task CancelarComandaAsync_ShouldReturnSuccess_WhenValidRequest()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var motivo = "Cliente canceló la orden";

        var expectedComanda = new ComandaDto { Id = comandaId, Estado = "cancelada" };
        var expectedResponse = ApiResponse<ComandaDto>.SuccessResponse(expectedComanda);

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>($"api/comandas/{comandaId}/cancelar", It.IsAny<object>(), It.IsAny<string?>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _comandasService.CancelarComandaAsync(comandaId, motivo);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedComanda);
    }

    [Fact]
    public async Task CancelarComandaAsync_ShouldReturnError_WhenMotivoIsEmpty()
    {
        // Arrange
        var comandaId = Guid.NewGuid();

        // Act
        var result = await _comandasService.CancelarComandaAsync(comandaId, "");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("El motivo de cancelación es requerido");
    }

    #endregion

    #region BuscarComandasAsync Tests

    [Fact]
    public async Task BuscarComandasAsync_ShouldReturnSuccess_WhenCalledWithFilters()
    {
        // Arrange
        var estado = "pendiente";
        var mesaId = Guid.NewGuid();
        var expectedComandas = new List<ComandaDto>
        {
            new ComandaDto { Id = Guid.NewGuid(), Estado = estado, MesaId = mesaId }
        };
        var expectedResponse = ApiResponse<List<ComandaDto>>.SuccessResponse(expectedComandas);

        _apiServiceMock.Setup(x => x.GetAsync<List<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _comandasService.BuscarComandasAsync(estado, mesaId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedComandas);
    }

    [Fact]
    public async Task BuscarComandasAsync_ShouldReturnSuccess_WhenCalledWithoutFilters()
    {
        // Arrange
        var expectedComandas = new List<ComandaDto>();
        var expectedResponse = ApiResponse<List<ComandaDto>>.SuccessResponse(expectedComandas);

        _apiServiceMock.Setup(x => x.GetAsync<List<ComandaDto>>("api/comandas/buscar", It.IsAny<string?>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _comandasService.BuscarComandasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedComandas);
    }

    #endregion

    #region ObtenerEstadisticasAsync Tests

    [Fact]
    public async Task ObtenerEstadisticasAsync_ShouldReturnSuccess_WhenApiCallSucceeds()
    {
        // Arrange
        var expectedEstadisticas = new EstadisticasComandasDto
        {
            TotalComandasActivas = 5,
            ComandasPendientes = 2,
            ComandasEnPreparacion = 3,
            VentasTotalDia = 1500.50m
        };
        var expectedResponse = ApiResponse<EstadisticasComandasDto>.SuccessResponse(expectedEstadisticas);

        _apiServiceMock.Setup(x => x.GetAsync<EstadisticasComandasDto>("api/comandas/estadisticas", It.IsAny<string?>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _comandasService.ObtenerEstadisticasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedEstadisticas);
    }

    #endregion
} 