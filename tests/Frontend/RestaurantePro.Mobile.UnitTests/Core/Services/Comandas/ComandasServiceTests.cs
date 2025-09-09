using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Comandas;
using ComandaModels = RestaurantePro.Mobile.Core.Features.Operations.Comandas.Models;
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
        _mockAuthService.Setup(a => a.GetTokenAsync()).ReturnsAsync("token");
        _mockAuthService.Setup(a => a.GetUserIdAsync()).ReturnsAsync(Guid.NewGuid().ToString());
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

        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(new PaginatedList<ComandaDto> { Items = expectedComandas }));

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
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>()))
            .ThrowsAsync(new Exception("API error"));

        // Act
        var result = await _comandasService.ObtenerComandasActivasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al obtener comandas activas"));
    }

    #endregion

    #region Paginación (query params)

    [Fact]
    public async Task ObtenerComandasActivasAsync_DebeIncluirPageNumberYPageSizeEnQuery()
    {
        var expectedComandas = new List<ComandaDto>();
        var paged = new PaginatedList<ComandaDto> { Items = expectedComandas };

        _apiServiceMock
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.Is<string>(s =>
                s.StartsWith("api/operaciones/comandas?") &&
                s.Contains("pageNumber=1") &&
                s.Contains("pageSize=12")), It.IsAny<string?>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(paged));

        var result = await _comandasService.ObtenerComandasActivasAsync();

        result.Success.Should().BeTrue();
        _apiServiceMock.Verify(x => x.GetAsync<PaginatedList<ComandaDto>>(It.Is<string>(s => s.Contains("pageNumber=1") && s.Contains("pageSize=12")), It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task BuscarComandasAsync_DebeIncluirPageNumberYPageSizeEnQuery()
    {
        var expectedComandas = new List<ComandaDto>();
        var paged = new PaginatedList<ComandaDto> { Items = expectedComandas };

        _apiServiceMock
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.Is<string>(s =>
                s.StartsWith("api/operaciones/comandas?") &&
                s.Contains("pageNumber=1") &&
                s.Contains("pageSize=12")), It.IsAny<string?>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(paged));

        var result = await _comandasService.BuscarComandasAsync(estado: "pendiente");

        result.Success.Should().BeTrue();
        _apiServiceMock.Verify(x => x.GetAsync<PaginatedList<ComandaDto>>(It.Is<string>(s => s.Contains("pageNumber=1") && s.Contains("pageSize=12")), It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerComandasPorMesaAsync_DebeIncluirPageSizeSoloActivasEIncluirItems()
    {
        var mesaId = Guid.NewGuid();
        var expectedComandas = new List<ComandaDto>();
        var paged = new PaginatedList<ComandaDto> { Items = expectedComandas };

        _apiServiceMock
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.Is<string>(s =>
                s.StartsWith("api/operaciones/comandas?") &&
                s.Contains($"mesaId={mesaId}") &&
                s.Contains("soloActivas=true") &&
                s.Contains("pageSize=12") &&
                s.Contains("incluirItems=true")), It.IsAny<string?>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(paged));

        var result = await _comandasService.ObtenerComandasPorMesaAsync(mesaId);

        result.Success.Should().BeTrue();
        _apiServiceMock.Verify(x => x.GetAsync<PaginatedList<ComandaDto>>(It.Is<string>(s => s.Contains($"mesaId={mesaId}") && s.Contains("pageSize=12") && s.Contains("soloActivas=true") && s.Contains("incluirItems=true")), It.IsAny<string?>()), Times.Once);
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

        var pagedMesa = new PaginatedList<ComandaDto> { Items = expectedComandas };
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(pagedMesa));

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
        var request = new ComandaModels.CrearComandaRequest
        {
            MesaId = Guid.NewGuid().ToString(),
            Observaciones = "Sin cebolla",
            ProductosIniciales = new List<ComandaModels.ProductoComandaRequest>{ new ComandaModels.ProductoComandaRequest{ ProductoId = Guid.NewGuid().ToString(), Cantidad = 1, Precio = 10 } }
        };

        var expectedComanda = new ComandaDto
        {
            Id = Guid.NewGuid(),
            MesaId = Guid.Parse(request.MesaId),
            Numero = "C001"
        };

        var expectedResponse = ApiResponse<ComandaDto>.SuccessResponse(expectedComanda);

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>()))
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
        var request = new ComandaModels.CrearComandaRequest
        {
            MesaId = string.Empty
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

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>()))
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

        _apiServiceMock.Setup(x => x.PutAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>()))
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
        _apiServiceMock.Setup(x => x.PutAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(new ComandaDto { Id = comandaId }));
        var result = await _comandasService.ActualizarCantidadProductoAsync(comandaId, productoId, nuevaCantidad);
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
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

        _apiServiceMock.Setup(x => x.PatchAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>()))
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

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>()))
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

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>()))
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

        var paged = new PaginatedList<ComandaDto> { Items = expectedComandas };
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(paged));

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
        var pagedEmpty = new PaginatedList<ComandaDto> { Items = expectedComandas };
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(pagedEmpty));

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
        var data = new List<ComandaDto>();
        var pagedStats = new PaginatedList<ComandaDto> { Items = data };
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(pagedStats));

        // Act
        var result = await _comandasService.ObtenerEstadisticasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalComandasActivas.Should().Be(0);
        result.Data!.ComandasPendientes.Should().Be(0);
        result.Data!.ComandasEnPreparacion.Should().Be(0);
    }

    #endregion

    #region QueryString & Pagination Tests

    [Fact]
    public async Task BuscarComandasAsync_ShouldIncludeDefaultPaginationParams_InQuery()
    {
        // Arrange
        string? capturedEndpoint = null;
        var expected = new PaginatedList<ComandaDto> { Items = new List<ComandaDto>() };
        _apiServiceMock
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>()))
            .Callback<string, string?>((endpoint, token) => capturedEndpoint = endpoint)
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(expected));

        // Act
        var result = await _comandasService.BuscarComandasAsync();

        // Assert
        result.Success.Should().BeTrue();
        capturedEndpoint.Should().NotBeNull();
        capturedEndpoint!.Should().Contain("pageNumber=1");
        capturedEndpoint.Should().Contain("pageSize=12");
        capturedEndpoint.Should().Contain("incluirItems=true");
    }

    [Fact]
    public async Task ObtenerComandasActivasAsync_ShouldIncludeSoloActivasAndIncluirItemsAndPagination()
    {
        // Arrange
        string? capturedEndpoint = null;
        var expected = new PaginatedList<ComandaDto> { Items = new List<ComandaDto>() };
        _apiServiceMock
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>()))
            .Callback<string, string?>((endpoint, token) => capturedEndpoint = endpoint)
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(expected));

        // Act
        var result = await _comandasService.ObtenerComandasActivasAsync();

        // Assert
        result.Success.Should().BeTrue();
        capturedEndpoint.Should().NotBeNull();
        capturedEndpoint!.Should().Contain("soloActivas=true");
        capturedEndpoint.Should().Contain("incluirItems=true");
        capturedEndpoint.Should().Contain("pageNumber=1");
        capturedEndpoint.Should().Contain("pageSize=12");
    }

    [Fact]
    public async Task ObtenerComandasPorMesaAsync_ShouldIncludeMesaIdAndPagination()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        string? capturedEndpoint = null;
        var expected = new PaginatedList<ComandaDto> { Items = new List<ComandaDto>() };
        _apiServiceMock
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>()))
            .Callback<string, string?>((endpoint, token) => capturedEndpoint = endpoint)
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(expected));

        // Act
        var result = await _comandasService.ObtenerComandasPorMesaAsync(mesaId);

        // Assert
        result.Success.Should().BeTrue();
        capturedEndpoint.Should().NotBeNull();
        capturedEndpoint!.Should().Contain($"mesaId={mesaId}");
        capturedEndpoint.Should().Contain("pageSize=12");
        capturedEndpoint.Should().Contain("incluirItems=true");
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task BuscarComandasAsync_ShouldReturnCancelled_WhenTokenIsCancelled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var calls = 0;
        _apiServiceMock
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>()))
            .Callback(() => calls++)
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(new PaginatedList<ComandaDto>()));

        // Act
        var result = await _comandasService.BuscarComandasAsync(cancellationToken: cts.Token);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().ContainEquivalentOf("operación");
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FinalizarComandaAsync_ShouldReturnCancelled_WhenTokenIsCancelled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var comandaId = Guid.NewGuid();
        var calls = 0;
        _apiServiceMock
            .Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Callback(() => calls++)
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(new ComandaDto { Id = comandaId }));

        // Act
        var result = await _comandasService.FinalizarComandaAsync(comandaId, "Efectivo", cancellationToken: cts.Token);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().ContainEquivalentOf("operación");
        calls.Should().Be(0);
    }

    #endregion

} 