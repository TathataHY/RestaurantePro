using Xunit;
using Moq;
using FluentAssertions;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Models.DTOs;
using AutoFixture;

namespace RestaurantePro.Mobile.UnitTests.Services.Mesas;

/// <summary>
/// Tests unitarios para MesasService
/// </summary>
public class MesasServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly MesasService _mesasService;
    private readonly Fixture _fixture;

    public MesasServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _mockAuthService = new Mock<IAuthService>();
        _mesasService = new MesasService(_mockApiService.Object, _mockAuthService.Object);
        _fixture = new Fixture();
    }

    #region ObtenerMesasAsync Tests

    [Fact]
    public async Task ObtenerMesasAsync_SinFiltros_DebeUsarEndpointCorrector()
    {
        // Arrange
        var expectedMesas = _fixture.CreateMany<MesaDto>(5).ToList();
        var expectedResponse = ApiResponse<List<MesaDto>>.SuccessResponse(expectedMesas);

        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.ObtenerMesasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedMesas);
        _mockApiService.Verify(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerMesasAsync_ConFiltros_DebeUsarQueryCorrector()
    {
        // Arrange
        var expectedMesas = _fixture.CreateMany<MesaDto>(3).ToList();
        var expectedResponse = ApiResponse<List<MesaDto>>.SuccessResponse(expectedMesas);

        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas?estado=Disponible&ubicacion=Interior&capacidadMinima=4", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.ObtenerMesasAsync("Disponible", "Interior", 4);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedMesas);
        _mockApiService.Verify(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas?estado=Disponible&ubicacion=Interior&capacidadMinima=4", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ObtenerMesaAsync Tests

    [Fact]
    public async Task ObtenerMesaAsync_ConIdValido_DebeRetornarMesa()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var expectedMesa = _fixture.Create<MesaDto>();
        expectedMesa.Id = mesaId;
        var expectedResponse = ApiResponse<MesaDto>.SuccessResponse(expectedMesa);

        _mockApiService.Setup(x => x.GetAsync<MesaDto>($"api/operaciones/mesas/{mesaId}", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.ObtenerMesaAsync(mesaId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedMesa);
        _mockApiService.Verify(x => x.GetAsync<MesaDto>($"api/operaciones/mesas/{mesaId}", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ObtenerMesasDisponiblesAsync Tests

    [Fact]
    public async Task ObtenerMesasDisponiblesAsync_SinFiltros_DebeUsarEndpointCorrector()
    {
        // Arrange
        var expectedMesas = _fixture.CreateMany<MesaDto>(3).ToList();
        var paginatedList = new PaginatedList<MesaDto>
        {
            Items = expectedMesas,
            TotalCount = expectedMesas.Count,
            PageNumber = 1,
            PageSize = 10
        };
        var expectedResponse = ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(paginatedList);

        _mockApiService.Setup(x => x.GetAsync<PaginatedList<MesaDto>>(It.Is<string>(s => s.StartsWith("api/operaciones/mesas/disponibles") && s.Contains("pageNumber=1") && s.Contains("pageSize=50")), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.ObtenerMesasDisponiblesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedMesas);
        _mockApiService.Verify(x => x.GetAsync<PaginatedList<MesaDto>>(It.Is<string>(s => s.StartsWith("api/operaciones/mesas/disponibles") && s.Contains("pageNumber=1") && s.Contains("pageSize=50")), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerMesasDisponiblesAsync_ConFiltros_DebeUsarQueryCorrector()
    {
        // Arrange
        var expectedMesas = _fixture.CreateMany<MesaDto>(2).ToList();
        var paginatedList = new PaginatedList<MesaDto>
        {
            Items = expectedMesas,
            TotalCount = expectedMesas.Count,
            PageNumber = 1,
            PageSize = 10
        };
        var expectedResponse = ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(paginatedList);

        _mockApiService.Setup(x => x.GetAsync<PaginatedList<MesaDto>>(It.Is<string>(s => s.StartsWith("api/operaciones/mesas/disponibles?") && s.Contains("capacidadMinima=6") && s.Contains("ubicacion=Terraza") && s.Contains("pageNumber=1") && s.Contains("pageSize=50")), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.ObtenerMesasDisponiblesAsync(6, "Terraza");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedMesas);
        _mockApiService.Verify(x => x.GetAsync<PaginatedList<MesaDto>>(It.Is<string>(s => s.StartsWith("api/operaciones/mesas/disponibles?") && s.Contains("capacidadMinima=6") && s.Contains("ubicacion=Terraza") && s.Contains("pageNumber=1") && s.Contains("pageSize=50")), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ObtenerEstadoOcupacionAsync Tests

    [Fact]
    public async Task ObtenerEstadoOcupacionAsync_DebeRetornarEstadoMesas()
    {
        // Arrange
        var expectedEstado = _fixture.Create<EstadoMesasDto>();
        var expectedResponse = ApiResponse<EstadoMesasDto>.SuccessResponse(expectedEstado);

        _mockApiService.Setup(x => x.GetAsync<EstadoMesasDto>("api/operaciones/mesas/estado-ocupacion", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.ObtenerEstadoOcupacionAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedEstado);
        _mockApiService.Verify(x => x.GetAsync<EstadoMesasDto>("api/operaciones/mesas/estado-ocupacion", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region AsignarMesaAsync Tests

    [Fact]
    public async Task AsignarMesaAsync_ConParametrosValidos_DebeAsignarMesa()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var numeroPersonas = 4;
        var observaciones = "Mesa para cena de negocios";
        var expectedResponse = ApiResponse<object>.SuccessResponse(new { });

        _mockApiService.Setup(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.AsignarMesaAsync(mesaId, clienteId, numeroPersonas, observaciones);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region LiberarMesaAsync Tests

    [Fact]
    public async Task LiberarMesaAsync_ConParametrosValidos_DebeLiberarMesa()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var motivo = "Cliente terminó de cenar";
        var expectedMesa = _fixture.Create<MesaDto>();
        var expectedResponse = ApiResponse<MesaDto>.SuccessResponse(expectedMesa);

        _mockApiService.Setup(x => x.PostAsync<MesaDto>(
            $"api/operaciones/mesas/{mesaId}/liberar",
            It.IsAny<object>(),
            It.IsAny<string?>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.LiberarMesaAsync(mesaId, motivo);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedMesa);
        _mockApiService.Verify(x => x.PostAsync<MesaDto>(
            $"api/operaciones/mesas/{mesaId}/liberar",
            It.IsAny<object>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region CambiarEstadoMesaAsync Tests

    [Fact]
    public async Task CambiarEstadoMesaAsync_ConParametrosValidos_DebeCambiarEstado()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var nuevoEstado = "Fuera de servicio";
        var motivo = "Mesa necesita mantenimiento";
        var expectedMesa = _fixture.Create<MesaDto>();
        var expectedResponse = ApiResponse<MesaDto>.SuccessResponse(expectedMesa);

        _mockApiService.Setup(x => x.PutAsync<MesaDto>(
            $"api/operaciones/mesas/{mesaId}/estado",
            It.IsAny<object>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.CambiarEstadoMesaAsync(mesaId, nuevoEstado, motivo);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedMesa);
        _mockApiService.Verify(x => x.PutAsync<MesaDto>(
            $"api/operaciones/mesas/{mesaId}/estado",
            It.IsAny<object>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Auth/RateLimit/NoContent Errors

    [Theory]
    [InlineData(401, "Unauthorized")]
    [InlineData(403, "Forbidden")]
    [InlineData(429, "Too Many Requests")]
    public async Task ObtenerMesasAsync_ShouldPropagateStatusAndMessage_OnApiError(int status, string message)
    {
        _mockApiService
            .Setup(x => x.GetAsync<List<MesaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<MesaDto>>.ErrorResponse(new List<string> { message }, message, status));

        var result = await _mesasService.ObtenerMesasAsync();

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(status);
        result.Message.Should().Be(message);
    }

    [Fact]
    public async Task ObtenerMesasDisponiblesAsync_ShouldReturnError_OnNoContent()
    {
        _mockApiService
            .Setup(x => x.GetAsync<PaginatedList<MesaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<MesaDto>>.ErrorResponse(new List<string> { "No Content" }, "No Content", 204));

        var result = await _mesasService.ObtenerMesasDisponiblesAsync();

        result.Success.Should().BeFalse();
        result.Message.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(401, "Unauthorized")]
    [InlineData(403, "Forbidden")]
    [InlineData(429, "Too Many Requests")]
    public async Task CambiarEstadoMesaAsync_ShouldPropagateStatusAndMessage_OnApiError(int status, string message)
    {
        _mockApiService
            .Setup(x => x.PutAsync<MesaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<MesaDto>.ErrorResponse(new List<string> { message }, message, status));

        var result = await _mesasService.CambiarEstadoMesaAsync(Guid.NewGuid(), "Fuera de servicio", "mantención");

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(status);
        result.Message.Should().Be(message);
    }

    #endregion

    #region BuscarMejorMesaAsync Tests

    [Fact]
    public async Task BuscarMejorMesaAsync_ConParametrosValidos_DebeRetornarMejorMesa()
    {
        // Arrange
        var numeroPersonas = 6;
        var ubicacionPreferida = "Terraza";
        var expectedMesa = _fixture.Create<MesaDto>();
        var expectedResponse = ApiResponse<MesaDto>.SuccessResponse(expectedMesa);

        _mockApiService.Setup(x => x.GetAsync<MesaDto>($"api/operaciones/mesas/buscar-mejor?numeroPersonas={numeroPersonas}&ubicacionPreferida=Terraza", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.BuscarMejorMesaAsync(numeroPersonas, ubicacionPreferida);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedMesa);
        _mockApiService.Verify(x => x.GetAsync<MesaDto>($"api/operaciones/mesas/buscar-mejor?numeroPersonas={numeroPersonas}&ubicacionPreferida=Terraza", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuscarMejorMesaAsync_SinUbicacionPreferida_DebeUsarSoloNumeroPersonas()
    {
        // Arrange
        var numeroPersonas = 4;
        var expectedMesa = _fixture.Create<MesaDto>();
        var expectedResponse = ApiResponse<MesaDto>.SuccessResponse(expectedMesa);

        _mockApiService.Setup(x => x.GetAsync<MesaDto>($"api/operaciones/mesas/buscar-mejor?numeroPersonas={numeroPersonas}", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.BuscarMejorMesaAsync(numeroPersonas);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedMesa);
        _mockApiService.Verify(x => x.GetAsync<MesaDto>($"api/operaciones/mesas/buscar-mejor?numeroPersonas={numeroPersonas}", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ObtenerMesasAsync_CuandoApiServiceFalla_DebeRetornarErrorResponse()
    {
        // Arrange
        var errorResponse = ApiResponse<List<MesaDto>>.ErrorResponse("Error de conexión");

        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(errorResponse);

        // Act
        var result = await _mesasService.ObtenerMesasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Error de conexión");
    }

    #endregion
} 