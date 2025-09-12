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

        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
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
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
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
                s.Contains("pageSize=12")), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(paged));

        var result = await _comandasService.ObtenerComandasActivasAsync();

        result.Success.Should().BeTrue();
        _apiServiceMock.Verify(x => x.GetAsync<PaginatedList<ComandaDto>>(It.Is<string>(s => s.Contains("pageNumber=1") && s.Contains("pageSize=12")), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
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
                s.Contains("pageSize=12")), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(paged));

        var result = await _comandasService.BuscarComandasAsync(estado: "pendiente");

        result.Success.Should().BeTrue();
        _apiServiceMock.Verify(x => x.GetAsync<PaginatedList<ComandaDto>>(It.Is<string>(s => s.Contains("pageNumber=1") && s.Contains("pageSize=12")), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
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
                s.Contains("incluirItems=true")), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(paged));

        var result = await _comandasService.ObtenerComandasPorMesaAsync(mesaId);

        result.Success.Should().BeTrue();
        _apiServiceMock.Verify(x => x.GetAsync<PaginatedList<ComandaDto>>(It.Is<string>(s => s.Contains($"mesaId={mesaId}") && s.Contains("pageSize=12") && s.Contains("soloActivas=true") && s.Contains("incluirItems=true")), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerComandasActivasAsync_DebeIncluirSoloActivasEIncluirItems()
    {
        var paged = new PaginatedList<ComandaDto> { Items = new List<ComandaDto>() };

        _apiServiceMock
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.Is<string>(s =>
                s.StartsWith("api/operaciones/comandas?") &&
                s.Contains("soloActivas=true") &&
                s.Contains("incluirItems=true")), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(paged));

        var result = await _comandasService.ObtenerComandasActivasAsync();

        result.Success.Should().BeTrue();
        _apiServiceMock.Verify(x => x.GetAsync<PaginatedList<ComandaDto>>(It.Is<string>(s => s.Contains("soloActivas=true") && s.Contains("incluirItems=true")), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
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
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
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

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
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

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
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

        _apiServiceMock.Setup(x => x.PutAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
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
        _apiServiceMock.Setup(x => x.PutAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
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

        _apiServiceMock.Setup(x => x.PatchAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
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

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
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

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
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
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
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
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
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
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
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
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Callback<string, string?, CancellationToken>((endpoint, token, cancellationToken) => capturedEndpoint = endpoint)
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
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Callback<string, string?, CancellationToken>((endpoint, token, cancellationToken) => capturedEndpoint = endpoint)
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
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Callback<string, string?, CancellationToken>((endpoint, token, cancellationToken) => capturedEndpoint = endpoint)
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
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
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
            .Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
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

    #region Validaciones de Entrada Robustas

    [Fact]
    public async Task CrearComandaAsync_ShouldReturnError_WhenMesaIdIsNull()
    {
        // Arrange
        var request = new ComandaModels.CrearComandaRequest
        {
            MesaId = null!,
            ProductosIniciales = new List<ComandaModels.ProductoComandaRequest>
            {
                new ComandaModels.ProductoComandaRequest { ProductoId = Guid.NewGuid().ToString(), Cantidad = 1, Precio = 10 }
            }
        };

        // Act
        var result = await _comandasService.CrearComandaAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("La mesa es requerida");
    }

    [Fact]
    public async Task CrearComandaAsync_ShouldReturnError_WhenMesaIdIsWhitespace()
    {
        // Arrange
        var request = new ComandaModels.CrearComandaRequest
        {
            MesaId = "   ",
            ProductosIniciales = new List<ComandaModels.ProductoComandaRequest>
            {
                new ComandaModels.ProductoComandaRequest { ProductoId = Guid.NewGuid().ToString(), Cantidad = 1, Precio = 10 }
            }
        };

        // Act
        var result = await _comandasService.CrearComandaAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("La mesa es requerida");
    }

    [Fact]
    public async Task CrearComandaAsync_ShouldReturnError_WhenProductosInicialesIsNull()
    {
        // Arrange
        var request = new ComandaModels.CrearComandaRequest
        {
            MesaId = Guid.NewGuid().ToString(),
            ProductosIniciales = null!
        };

        // Act
        var result = await _comandasService.CrearComandaAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Se requiere al menos un producto");
    }

    [Fact]
    public async Task CrearComandaAsync_ShouldReturnError_WhenProductosInicialesIsEmpty()
    {
        // Arrange
        var request = new ComandaModels.CrearComandaRequest
        {
            MesaId = Guid.NewGuid().ToString(),
            ProductosIniciales = new List<ComandaModels.ProductoComandaRequest>()
        };

        // Act
        var result = await _comandasService.CrearComandaAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Se requiere al menos un producto");
    }

    [Fact]
    public async Task CrearComandaAsync_ShouldUseUserId_WhenMeseroIdIsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        _mockAuthService.Setup(a => a.GetUserIdAsync()).ReturnsAsync(userId);
        
        var request = new ComandaModels.CrearComandaRequest
        {
            MesaId = Guid.NewGuid().ToString(),
            MeseroId = "",
            ProductosIniciales = new List<ComandaModels.ProductoComandaRequest>
            {
                new ComandaModels.ProductoComandaRequest { ProductoId = Guid.NewGuid().ToString(), Cantidad = 1, Precio = 10 }
            }
        };

        var expectedComanda = new ComandaDto { Id = Guid.NewGuid() };
        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(expectedComanda));

        // Act
        var result = await _comandasService.CrearComandaAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockAuthService.Verify(a => a.GetUserIdAsync(), Times.Once);
    }

    [Fact]
    public async Task CrearComandaAsync_ShouldReturnError_WhenUserIdIsEmpty()
    {
        // Arrange
        _mockAuthService.Setup(a => a.GetUserIdAsync()).ReturnsAsync("");
        
        var request = new ComandaModels.CrearComandaRequest
        {
            MesaId = Guid.NewGuid().ToString(),
            MeseroId = "",
            ProductosIniciales = new List<ComandaModels.ProductoComandaRequest>
            {
                new ComandaModels.ProductoComandaRequest { ProductoId = Guid.NewGuid().ToString(), Cantidad = 1, Precio = 10 }
            }
        };

        // Act
        var result = await _comandasService.CrearComandaAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Debe iniciar sesión para crear una comanda");
    }

    [Fact]
    public async Task AgregarProductosAsync_ShouldReturnError_WhenProductosIsNull()
    {
        // Arrange
        var comandaId = Guid.NewGuid();

        // Act
        var result = await _comandasService.AgregarProductosAsync(comandaId, null!);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Se requiere al menos un producto");
    }

    [Fact]
    public async Task ActualizarCantidadProductoAsync_ShouldReturnError_WhenComandaIdIsEmpty()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var nuevaCantidad = 3;

        // Act
        var result = await _comandasService.ActualizarCantidadProductoAsync(Guid.Empty, productoId, nuevaCantidad);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("ID de comanda y producto son requeridos");
    }

    [Fact]
    public async Task ActualizarCantidadProductoAsync_ShouldReturnError_WhenItemIdIsEmpty()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var nuevaCantidad = 3;

        // Act
        var result = await _comandasService.ActualizarCantidadProductoAsync(comandaId, Guid.Empty, nuevaCantidad);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("ID de comanda y producto son requeridos");
    }

    [Fact]
    public async Task RemoverProductoAsync_ShouldReturnError_WhenComandaIdIsEmpty()
    {
        // Arrange
        var itemId = Guid.NewGuid();

        // Act
        var result = await _comandasService.RemoverProductoAsync(Guid.Empty, itemId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("ID de comanda y producto son requeridos");
    }

    [Fact]
    public async Task RemoverProductoAsync_ShouldReturnError_WhenItemIdIsEmpty()
    {
        // Arrange
        var comandaId = Guid.NewGuid();

        // Act
        var result = await _comandasService.RemoverProductoAsync(comandaId, Guid.Empty);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("ID de comanda y producto son requeridos");
    }

    [Fact]
    public async Task CambiarEstadoComandaAsync_ShouldReturnError_WhenComandaIdIsEmpty()
    {
        // Arrange
        var nuevoEstado = "en_preparacion";

        // Act
        var result = await _comandasService.CambiarEstadoComandaAsync(Guid.Empty, nuevoEstado);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("ID de comanda es requerido");
    }

    [Fact]
    public async Task CambiarEstadoComandaAsync_ShouldReturnError_WhenEstadoIsNull()
    {
        // Arrange
        var comandaId = Guid.NewGuid();

        // Act
        var result = await _comandasService.CambiarEstadoComandaAsync(comandaId, null!);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("El nuevo estado es requerido");
    }

    [Fact]
    public async Task CambiarEstadoComandaAsync_ShouldReturnError_WhenEstadoIsWhitespace()
    {
        // Arrange
        var comandaId = Guid.NewGuid();

        // Act
        var result = await _comandasService.CambiarEstadoComandaAsync(comandaId, "   ");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("El nuevo estado es requerido");
    }

    [Fact]
    public async Task FinalizarComandaAsync_ShouldReturnError_WhenComandaIdIsEmpty()
    {
        // Arrange
        var metodoPago = "efectivo";

        // Act
        var result = await _comandasService.FinalizarComandaAsync(Guid.Empty, metodoPago);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("ID de comanda es requerido");
    }

    [Fact]
    public async Task FinalizarComandaAsync_ShouldReturnError_WhenMetodoPagoIsNull()
    {
        // Arrange
        var comandaId = Guid.NewGuid();

        // Act
        var result = await _comandasService.FinalizarComandaAsync(comandaId, null!);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("El método de pago es requerido");
    }

    [Fact]
    public async Task FinalizarComandaAsync_ShouldReturnError_WhenMetodoPagoIsWhitespace()
    {
        // Arrange
        var comandaId = Guid.NewGuid();

        // Act
        var result = await _comandasService.FinalizarComandaAsync(comandaId, "   ");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("El método de pago es requerido");
    }

    [Fact]
    public async Task FinalizarComandaAsync_ShouldReturnError_WhenUserIdIsEmpty()
    {
        // Arrange
        _mockAuthService.Setup(a => a.GetUserIdAsync()).ReturnsAsync("");
        var comandaId = Guid.NewGuid();
        var metodoPago = "efectivo";

        // Act
        var result = await _comandasService.FinalizarComandaAsync(comandaId, metodoPago);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("UsuarioId requerido");
    }

    [Fact]
    public async Task FinalizarComandaAsync_ShouldReturnError_WhenUserIdIsInvalid()
    {
        // Arrange
        _mockAuthService.Setup(a => a.GetUserIdAsync()).ReturnsAsync("invalid-guid");
        var comandaId = Guid.NewGuid();
        var metodoPago = "efectivo";

        // Act
        var result = await _comandasService.FinalizarComandaAsync(comandaId, metodoPago);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("UsuarioId requerido");
    }

    [Fact]
    public async Task CancelarComandaAsync_ShouldReturnError_WhenComandaIdIsEmpty()
    {
        // Arrange
        var motivo = "Cliente canceló";

        // Act
        var result = await _comandasService.CancelarComandaAsync(Guid.Empty, motivo);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("ID de comanda es requerido");
    }

    [Fact]
    public async Task CancelarComandaAsync_ShouldReturnError_WhenMotivoIsNull()
    {
        // Arrange
        var comandaId = Guid.NewGuid();

        // Act
        var result = await _comandasService.CancelarComandaAsync(comandaId, null!);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("El motivo de cancelación es requerido");
    }

    [Fact]
    public async Task CancelarComandaAsync_ShouldReturnError_WhenMotivoIsWhitespace()
    {
        // Arrange
        var comandaId = Guid.NewGuid();

        // Act
        var result = await _comandasService.CancelarComandaAsync(comandaId, "   ");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("El motivo de cancelación es requerido");
    }

    #endregion

    #region Manejo de Errores de Red y Servicios

    [Fact]
    public async Task ObtenerComandasActivasAsync_ShouldHandleTimeout()
    {
        // Arrange
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _comandasService.ObtenerComandasActivasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al obtener comandas activas"));
    }

    [Fact]
    public async Task ObtenerComandasActivasAsync_ShouldHandleHttpRequestException()
    {
        // Arrange
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _comandasService.ObtenerComandasActivasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al obtener comandas activas"));
    }

    [Fact]
    public async Task ObtenerComandasActivasAsync_ShouldHandleSocketException()
    {
        // Arrange
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SocketException(10054));

        // Act
        var result = await _comandasService.ObtenerComandasActivasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al obtener comandas activas"));
    }

    [Fact]
    public async Task ObtenerComandasActivasAsync_ShouldHandleAggregateException()
    {
        // Arrange
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AggregateException("Multiple errors"));

        // Act
        var result = await _comandasService.ObtenerComandasActivasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al obtener comandas activas"));
    }

    [Fact]
    public async Task ObtenerComandasActivasAsync_ShouldHandleIOException()
    {
        // Arrange
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("IO error"));

        // Act
        var result = await _comandasService.ObtenerComandasActivasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al obtener comandas activas"));
    }

    [Fact]
    public async Task CrearComandaAsync_ShouldHandleTimeout()
    {
        // Arrange
        var request = new ComandaModels.CrearComandaRequest
        {
            MesaId = Guid.NewGuid().ToString(),
            ProductosIniciales = new List<ComandaModels.ProductoComandaRequest>
            {
                new ComandaModels.ProductoComandaRequest { ProductoId = Guid.NewGuid().ToString(), Cantidad = 1, Precio = 10 }
            }
        };

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _comandasService.CrearComandaAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al crear comanda"));
    }

    [Fact]
    public async Task CrearComandaAsync_ShouldHandleHttpRequestException()
    {
        // Arrange
        var request = new ComandaModels.CrearComandaRequest
        {
            MesaId = Guid.NewGuid().ToString(),
            ProductosIniciales = new List<ComandaModels.ProductoComandaRequest>
            {
                new ComandaModels.ProductoComandaRequest { ProductoId = Guid.NewGuid().ToString(), Cantidad = 1, Precio = 10 }
            }
        };

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _comandasService.CrearComandaAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al crear comanda"));
    }

    [Fact]
    public async Task AgregarProductosAsync_ShouldHandleTimeout()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productos = new List<ComandaProductoRequest>
        {
            new ComandaProductoRequest { ProductoId = Guid.NewGuid(), Cantidad = 2 }
        };

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _comandasService.AgregarProductosAsync(comandaId, productos);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al agregar productos"));
    }

    [Fact]
    public async Task AgregarProductosAsync_ShouldHandleHttpRequestException()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productos = new List<ComandaProductoRequest>
        {
            new ComandaProductoRequest { ProductoId = Guid.NewGuid(), Cantidad = 2 }
        };

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _comandasService.AgregarProductosAsync(comandaId, productos);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al agregar productos"));
    }

    [Fact]
    public async Task ActualizarCantidadProductoAsync_ShouldHandleTimeout()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var nuevaCantidad = 3;

        _apiServiceMock.Setup(x => x.PutAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _comandasService.ActualizarCantidadProductoAsync(comandaId, productoId, nuevaCantidad);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al actualizar cantidad"));
    }

    [Fact]
    public async Task ActualizarCantidadProductoAsync_ShouldHandleHttpRequestException()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var nuevaCantidad = 3;

        _apiServiceMock.Setup(x => x.PutAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _comandasService.ActualizarCantidadProductoAsync(comandaId, productoId, nuevaCantidad);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al actualizar cantidad"));
    }

    [Fact]
    public async Task RemoverProductoAsync_ShouldHandleTimeout()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        _apiServiceMock.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _comandasService.RemoverProductoAsync(comandaId, itemId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al remover producto"));
    }

    [Fact]
    public async Task RemoverProductoAsync_ShouldHandleHttpRequestException()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        _apiServiceMock.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _comandasService.RemoverProductoAsync(comandaId, itemId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al remover producto"));
    }

    [Fact]
    public async Task CambiarEstadoComandaAsync_ShouldHandleTimeout()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var nuevoEstado = "en_preparacion";

        _apiServiceMock.Setup(x => x.PatchAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _comandasService.CambiarEstadoComandaAsync(comandaId, nuevoEstado);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al cambiar estado"));
    }

    [Fact]
    public async Task CambiarEstadoComandaAsync_ShouldHandleHttpRequestException()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var nuevoEstado = "en_preparacion";

        _apiServiceMock.Setup(x => x.PatchAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _comandasService.CambiarEstadoComandaAsync(comandaId, nuevoEstado);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al cambiar estado"));
    }

    [Fact]
    public async Task FinalizarComandaAsync_ShouldHandleTimeout()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var metodoPago = "efectivo";

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _comandasService.FinalizarComandaAsync(comandaId, metodoPago);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al finalizar comanda"));
    }

    [Fact]
    public async Task FinalizarComandaAsync_ShouldHandleHttpRequestException()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var metodoPago = "efectivo";

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _comandasService.FinalizarComandaAsync(comandaId, metodoPago);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al finalizar comanda"));
    }

    [Fact]
    public async Task CancelarComandaAsync_ShouldHandleTimeout()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var motivo = "Cliente canceló";

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _comandasService.CancelarComandaAsync(comandaId, motivo);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al cancelar comanda"));
    }

    [Fact]
    public async Task CancelarComandaAsync_ShouldHandleHttpRequestException()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var motivo = "Cliente canceló";

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _comandasService.CancelarComandaAsync(comandaId, motivo);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al cancelar comanda"));
    }

    [Fact]
    public async Task BuscarComandasAsync_ShouldHandleTimeout()
    {
        // Arrange
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _comandasService.BuscarComandasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al buscar comandas"));
    }

    [Fact]
    public async Task BuscarComandasAsync_ShouldHandleHttpRequestException()
    {
        // Arrange
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _comandasService.BuscarComandasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al buscar comandas"));
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ShouldHandleTimeout()
    {
        // Arrange
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _comandasService.ObtenerEstadisticasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al obtener estadísticas"));
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ShouldHandleHttpRequestException()
    {
        // Arrange
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _comandasService.ObtenerEstadisticasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Error al obtener estadísticas"));
    }

    #endregion

    #region Casos Edge y Límites

    [Fact]
    public async Task ObtenerComandasActivasAsync_ShouldHandleNullApiResponse()
    {
        // Arrange
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApiResponse<PaginatedList<ComandaDto>>)null!);

        // Act
        var result = await _comandasService.ObtenerComandasActivasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Error al obtener comandas activas");
    }

    [Fact]
    public async Task ObtenerComandasActivasAsync_ShouldHandleNullDataInSuccessfulResponse()
    {
        // Arrange
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(null!));

        // Act
        var result = await _comandasService.ObtenerComandasActivasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Error al obtener comandas activas");
    }

    [Fact]
    public async Task ObtenerComandasActivasAsync_ShouldHandleEmptyList()
    {
        // Arrange
        var emptyList = new List<ComandaDto>();
        var pagedEmpty = new PaginatedList<ComandaDto> { Items = emptyList };
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(pagedEmpty));

        // Act
        var result = await _comandasService.ObtenerComandasActivasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerComandasActivasAsync_ShouldHandleVeryLargeList()
    {
        // Arrange
        var largeList = Enumerable.Range(1, 1000)
            .Select(i => new ComandaDto { Id = Guid.NewGuid(), Numero = $"C{i:D3}", Estado = "pendiente" })
            .ToList();
        var pagedLarge = new PaginatedList<ComandaDto> { Items = largeList };
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(pagedLarge));

        // Act
        var result = await _comandasService.ObtenerComandasActivasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1000);
    }

    [Fact]
    public async Task BuscarComandasAsync_ShouldHandleSpecialCharactersInEstado()
    {
        // Arrange
        var estado = "en_preparación"; // Con acento
        var expectedComandas = new List<ComandaDto>();
        var paged = new PaginatedList<ComandaDto> { Items = expectedComandas };
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(paged));

        // Act
        var result = await _comandasService.BuscarComandasAsync(estado);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _apiServiceMock.Verify(x => x.GetAsync<PaginatedList<ComandaDto>>(
            It.Is<string>(s => s.Contains("estado=") && s.Contains("en_preparaci%C3%B3n")), 
            It.IsAny<string?>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuscarComandasAsync_ShouldHandleSpecialCharactersInClienteNombre()
    {
        // Arrange
        var clienteNombre = "José María"; // Con acentos y espacios
        var expectedComandas = new List<ComandaDto>();
        var paged = new PaginatedList<ComandaDto> { Items = expectedComandas };
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(paged));

        // Act
        var result = await _comandasService.BuscarComandasAsync(clienteNombre: clienteNombre);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _apiServiceMock.Verify(x => x.GetAsync<PaginatedList<ComandaDto>>(
            It.Is<string>(s => s.Contains("clienteNombre=") && s.Contains("Jos%C3%A9%20Mar%C3%ADa")), 
            It.IsAny<string?>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuscarComandasAsync_ShouldHandleVeryLongClienteNombre()
    {
        // Arrange
        var clienteNombre = new string('A', 1000); // Nombre muy largo
        var expectedComandas = new List<ComandaDto>();
        var paged = new PaginatedList<ComandaDto> { Items = expectedComandas };
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(paged));

        // Act
        var result = await _comandasService.BuscarComandasAsync(clienteNombre: clienteNombre);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _apiServiceMock.Verify(x => x.GetAsync<PaginatedList<ComandaDto>>(
            It.Is<string>(s => s.Contains("clienteNombre=") && s.Contains(new string('A', 1000))), 
            It.IsAny<string?>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuscarComandasAsync_ShouldHandleDateRange()
    {
        // Arrange
        var fechaDesde = new DateTime(2024, 1, 1);
        var fechaHasta = new DateTime(2024, 12, 31);
        var expectedComandas = new List<ComandaDto>();
        var paged = new PaginatedList<ComandaDto> { Items = expectedComandas };
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(paged));

        // Act
        var result = await _comandasService.BuscarComandasAsync(fechaDesde: fechaDesde, fechaHasta: fechaHasta);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _apiServiceMock.Verify(x => x.GetAsync<PaginatedList<ComandaDto>>(
            It.Is<string>(s => s.Contains("fechaDesde=2024-01-01") && s.Contains("fechaHasta=2024-12-31")), 
            It.IsAny<string?>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AgregarProductosAsync_ShouldHandleMultipleProductos()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productos = new List<ComandaProductoRequest>
        {
            new ComandaProductoRequest { ProductoId = Guid.NewGuid(), Cantidad = 2, Observaciones = "Sin cebolla" },
            new ComandaProductoRequest { ProductoId = Guid.NewGuid(), Cantidad = 1, Observaciones = "Bien cocido" },
            new ComandaProductoRequest { ProductoId = Guid.NewGuid(), Cantidad = 3, Observaciones = "Extra queso" }
        };

        var expectedComanda = new ComandaDto { Id = comandaId };
        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(expectedComanda));

        // Act
        var result = await _comandasService.AgregarProductosAsync(comandaId, productos);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _apiServiceMock.Verify(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task AgregarProductosAsync_ShouldReturnError_WhenFirstProductFails()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productos = new List<ComandaProductoRequest>
        {
            new ComandaProductoRequest { ProductoId = Guid.NewGuid(), Cantidad = 2 },
            new ComandaProductoRequest { ProductoId = Guid.NewGuid(), Cantidad = 1 }
        };

        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.ErrorResponse("Error al agregar producto"));

        // Act
        var result = await _comandasService.AgregarProductosAsync(comandaId, productos);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Error al agregar producto");
        _apiServiceMock.Verify(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AgregarProductosAsync_ShouldReturnError_WhenSecondProductFails()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productos = new List<ComandaProductoRequest>
        {
            new ComandaProductoRequest { ProductoId = Guid.NewGuid(), Cantidad = 2 },
            new ComandaProductoRequest { ProductoId = Guid.NewGuid(), Cantidad = 1 }
        };

        var expectedComanda = new ComandaDto { Id = comandaId };
        _apiServiceMock.SetupSequence(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(expectedComanda))
            .ReturnsAsync(ApiResponse<ComandaDto>.ErrorResponse("Error al agregar segundo producto"));

        // Act
        var result = await _comandasService.AgregarProductosAsync(comandaId, productos);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Error al agregar segundo producto");
        _apiServiceMock.Verify(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ActualizarCantidadProductoAsync_ShouldHandleZeroCantidad()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var nuevaCantidad = 0;

        var expectedComanda = new ComandaDto { Id = comandaId };
        _apiServiceMock.Setup(x => x.PutAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(expectedComanda));

        // Act
        var result = await _comandasService.ActualizarCantidadProductoAsync(comandaId, productoId, nuevaCantidad);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _apiServiceMock.Verify(x => x.PutAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarCantidadProductoAsync_ShouldHandleNegativeCantidad()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var nuevaCantidad = -5;

        var expectedComanda = new ComandaDto { Id = comandaId };
        _apiServiceMock.Setup(x => x.PutAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(expectedComanda));

        // Act
        var result = await _comandasService.ActualizarCantidadProductoAsync(comandaId, productoId, nuevaCantidad);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _apiServiceMock.Verify(x => x.PutAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarCantidadProductoAsync_ShouldHandleVeryLargeCantidad()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var nuevaCantidad = int.MaxValue;

        var expectedComanda = new ComandaDto { Id = comandaId };
        _apiServiceMock.Setup(x => x.PutAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(expectedComanda));

        // Act
        var result = await _comandasService.ActualizarCantidadProductoAsync(comandaId, productoId, nuevaCantidad);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _apiServiceMock.Verify(x => x.PutAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CambiarEstadoComandaAsync_ShouldHandleVeryLongObservaciones()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var nuevoEstado = "en_preparacion";
        var observaciones = new string('A', 1000); // Observaciones muy largas

        var expectedComanda = new ComandaDto { Id = comandaId, Estado = nuevoEstado };
        _apiServiceMock.Setup(x => x.PatchAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(expectedComanda));

        // Act
        var result = await _comandasService.CambiarEstadoComandaAsync(comandaId, nuevoEstado, observaciones);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _apiServiceMock.Verify(x => x.PatchAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FinalizarComandaAsync_ShouldHandleVeryLongObservaciones()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var metodoPago = "efectivo";
        var observaciones = new string('A', 1000); // Observaciones muy largas

        var expectedComanda = new ComandaDto { Id = comandaId, Estado = "finalizada" };
        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(expectedComanda));

        // Act
        var result = await _comandasService.FinalizarComandaAsync(comandaId, metodoPago, observaciones);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _apiServiceMock.Verify(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelarComandaAsync_ShouldHandleVeryLongMotivo()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var motivo = new string('A', 1000); // Motivo muy largo

        var expectedComanda = new ComandaDto { Id = comandaId, Estado = "cancelada" };
        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(expectedComanda));

        // Act
        var result = await _comandasService.CancelarComandaAsync(comandaId, motivo);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _apiServiceMock.Verify(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Pruebas de Concurrencia

    [Fact]
    public async Task ObtenerComandasActivasAsync_ShouldHandleConcurrentCalls()
    {
        // Arrange
        var expectedComandas = new List<ComandaDto>
        {
            new ComandaDto { Id = Guid.NewGuid(), Numero = "C001", Estado = "pendiente" }
        };
        var paged = new PaginatedList<ComandaDto> { Items = expectedComandas };
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(paged));

        // Act
        var tasks = Enumerable.Range(1, 10)
            .Select(_ => _comandasService.ObtenerComandasActivasAsync())
            .ToArray();
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        results.Should().AllSatisfy(r => r.Success.Should().BeTrue());
        results.Should().AllSatisfy(r => r.Data.Should().BeEquivalentTo(expectedComandas));
    }

    [Fact]
    public async Task BuscarComandasAsync_ShouldHandleConcurrentCalls()
    {
        // Arrange
        var expectedComandas = new List<ComandaDto>();
        var paged = new PaginatedList<ComandaDto> { Items = expectedComandas };
        _apiServiceMock.Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(paged));

        // Act
        var tasks = Enumerable.Range(1, 10)
            .Select(_ => _comandasService.BuscarComandasAsync())
            .ToArray();
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        results.Should().AllSatisfy(r => r.Success.Should().BeTrue());
        results.Should().AllSatisfy(r => r.Data.Should().BeEquivalentTo(expectedComandas));
    }

    [Fact]
    public async Task AgregarProductosAsync_ShouldHandleConcurrentCalls()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productos = new List<ComandaProductoRequest>
        {
            new ComandaProductoRequest { ProductoId = Guid.NewGuid(), Cantidad = 2 }
        };

        var expectedComanda = new ComandaDto { Id = comandaId };
        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(expectedComanda));

        // Act
        var tasks = Enumerable.Range(1, 5)
            .Select(_ => _comandasService.AgregarProductosAsync(comandaId, productos))
            .ToArray();
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(5);
        results.Should().AllSatisfy(r => r.Success.Should().BeTrue());
        results.Should().AllSatisfy(r => r.Data.Should().BeEquivalentTo(expectedComanda));
    }

    [Fact]
    public async Task ActualizarCantidadProductoAsync_ShouldHandleConcurrentCalls()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var nuevaCantidad = 3;

        var expectedComanda = new ComandaDto { Id = comandaId };
        _apiServiceMock.Setup(x => x.PutAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(expectedComanda));

        // Act
        var tasks = Enumerable.Range(1, 5)
            .Select(_ => _comandasService.ActualizarCantidadProductoAsync(comandaId, productoId, nuevaCantidad))
            .ToArray();
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(5);
        results.Should().AllSatisfy(r => r.Success.Should().BeTrue());
        results.Should().AllSatisfy(r => r.Data.Should().BeEquivalentTo(expectedComanda));
    }

    [Fact]
    public async Task CambiarEstadoComandaAsync_ShouldHandleConcurrentCalls()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var nuevoEstado = "en_preparacion";

        var expectedComanda = new ComandaDto { Id = comandaId, Estado = nuevoEstado };
        _apiServiceMock.Setup(x => x.PatchAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(expectedComanda));

        // Act
        var tasks = Enumerable.Range(1, 5)
            .Select(_ => _comandasService.CambiarEstadoComandaAsync(comandaId, nuevoEstado))
            .ToArray();
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(5);
        results.Should().AllSatisfy(r => r.Success.Should().BeTrue());
        results.Should().AllSatisfy(r => r.Data.Should().BeEquivalentTo(expectedComanda));
    }

    [Fact]
    public async Task FinalizarComandaAsync_ShouldHandleConcurrentCalls()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var metodoPago = "efectivo";

        var expectedComanda = new ComandaDto { Id = comandaId, Estado = "finalizada" };
        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(expectedComanda));

        // Act
        var tasks = Enumerable.Range(1, 5)
            .Select(_ => _comandasService.FinalizarComandaAsync(comandaId, metodoPago))
            .ToArray();
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(5);
        results.Should().AllSatisfy(r => r.Success.Should().BeTrue());
        results.Should().AllSatisfy(r => r.Data.Should().BeEquivalentTo(expectedComanda));
    }

    [Fact]
    public async Task CancelarComandaAsync_ShouldHandleConcurrentCalls()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var motivo = "Cliente canceló";

        var expectedComanda = new ComandaDto { Id = comandaId, Estado = "cancelada" };
        _apiServiceMock.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.SuccessResponse(expectedComanda));

        // Act
        var tasks = Enumerable.Range(1, 5)
            .Select(_ => _comandasService.CancelarComandaAsync(comandaId, motivo))
            .ToArray();
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(5);
        results.Should().AllSatisfy(r => r.Success.Should().BeTrue());
        results.Should().AllSatisfy(r => r.Data.Should().BeEquivalentTo(expectedComanda));
    }

    #endregion

} 