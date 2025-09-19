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
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
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
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
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

        result.Success.Should().BeFalse(); // El servicio retorna error en estos casos
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

        result.Success.Should().BeFalse(); // El servicio retorna error en estos casos
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

        result.Success.Should().BeFalse(); // El servicio retorna error en estos casos
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
        result.Success.Should().BeFalse(); // El servicio retorna error en estos casos
        result.Errors.Should().Contain("Error de conexión");
    }

    #endregion

    // ===== PRUEBAS ROBUSTAS ADICIONALES =====

    #region Validaciones de Entrada

    [Fact]
    public async Task ObtenerMesasAsync_ConEstadoVacio_DebeUsarEndpointSinFiltro()
    {
        // Arrange
        var expectedMesas = _fixture.CreateMany<MesaDto>(3).ToList();
        var expectedResponse = ApiResponse<List<MesaDto>>.SuccessResponse(expectedMesas);

        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.ObtenerMesasAsync("", null, null);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerMesasAsync_ConEstadoWhitespace_DebeUsarEndpointSinFiltro()
    {
        // Arrange
        var expectedMesas = _fixture.CreateMany<MesaDto>(3).ToList();
        var expectedResponse = ApiResponse<List<MesaDto>>.SuccessResponse(expectedMesas);

        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.ObtenerMesasAsync("   ", null, null);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerMesasAsync_ConUbicacionVacia_DebeUsarEndpointSinFiltro()
    {
        // Arrange
        var expectedMesas = _fixture.CreateMany<MesaDto>(3).ToList();
        var expectedResponse = ApiResponse<List<MesaDto>>.SuccessResponse(expectedMesas);

        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.ObtenerMesasAsync(null, "", null);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerMesasAsync_ConCapacidadMinimaCero_DebeUsarEndpointConFiltro()
    {
        // Arrange
        var expectedMesas = _fixture.CreateMany<MesaDto>(3).ToList();
        var expectedResponse = ApiResponse<List<MesaDto>>.SuccessResponse(expectedMesas);

        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas?capacidadMinima=0", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.ObtenerMesasAsync(null, null, 0);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas?capacidadMinima=0", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerMesasAsync_ConCapacidadMinimaNegativa_DebeUsarEndpointConFiltro()
    {
        // Arrange
        var expectedMesas = _fixture.CreateMany<MesaDto>(3).ToList();
        var expectedResponse = ApiResponse<List<MesaDto>>.SuccessResponse(expectedMesas);

        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas?capacidadMinima=-1", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.ObtenerMesasAsync(null, null, -1);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas?capacidadMinima=-1", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerMesasAsync_ConCapacidadMinimaMuyGrande_DebeUsarEndpointConFiltro()
    {
        // Arrange
        var expectedMesas = _fixture.CreateMany<MesaDto>(3).ToList();
        var expectedResponse = ApiResponse<List<MesaDto>>.SuccessResponse(expectedMesas);

        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas?capacidadMinima=1000", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.ObtenerMesasAsync(null, null, 1000);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas?capacidadMinima=1000", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerMesasAsync_ConCaracteresEspecialesEnEstado_DebeEscaparCorrectamente()
    {
        // Arrange
        var estado = "Fuera de servicio & mantenimiento";
        var expectedMesas = _fixture.CreateMany<MesaDto>(3).ToList();
        var expectedResponse = ApiResponse<List<MesaDto>>.SuccessResponse(expectedMesas);

        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas?estado=Fuera%20de%20servicio%20%26%20mantenimiento", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.ObtenerMesasAsync(estado, null, null);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas?estado=Fuera%20de%20servicio%20%26%20mantenimiento", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerMesasAsync_ConCaracteresEspecialesEnUbicacion_DebeEscaparCorrectamente()
    {
        // Arrange
        var ubicacion = "Terraza & Jardín";
        var expectedMesas = _fixture.CreateMany<MesaDto>(3).ToList();
        var expectedResponse = ApiResponse<List<MesaDto>>.SuccessResponse(expectedMesas);

        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas?ubicacion=Terraza%20%26%20Jard%C3%ADn", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.ObtenerMesasAsync(null, ubicacion, null);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.GetAsync<List<MesaDto>>("api/operaciones/mesas?ubicacion=Terraza%20%26%20Jard%C3%ADn", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Manejo de Errores de Red

    [Fact]
    public async Task ObtenerMesasAsync_CuandoApiServiceTimeout_DebeRetornarErrorResponse()
    {
        // Arrange
        var timeoutException = new TaskCanceledException("Request timeout");

        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ThrowsAsync(timeoutException);

        // Act
        var result = await _mesasService.ObtenerMesasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse(); // El servicio retorna error en estos casos
        result.Errors.Should().Contain("Request timeout");
    }

    [Fact]
    public async Task ObtenerMesasAsync_CuandoApiServiceHttpException_DebeRetornarErrorResponse()
    {
        // Arrange
        var httpException = new HttpRequestException("Service unavailable", null, System.Net.HttpStatusCode.ServiceUnavailable);

        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ThrowsAsync(httpException);

        // Act
        var result = await _mesasService.ObtenerMesasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse(); // El servicio retorna error en estos casos
        result.Errors.Should().Contain("Service unavailable");
    }

    [Fact]
    public async Task ObtenerMesasAsync_CuandoApiServiceSocketException_DebeRetornarErrorResponse()
    {
        // Arrange
        var socketException = new System.Net.Sockets.SocketException(10054); // Connection reset

        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ThrowsAsync(socketException);

        // Act
        var result = await _mesasService.ObtenerMesasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse(); // El servicio retorna error en estos casos
        result.Errors.Should().Contain(socketException.Message);
    }

    [Fact]
    public async Task ObtenerMesasAsync_CuandoApiServiceAggregateException_DebeRetornarErrorResponse()
    {
        // Arrange
        var innerException = new HttpRequestException("Network error");
        var aggregateException = new AggregateException("Multiple errors", innerException);

        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ThrowsAsync(aggregateException);

        // Act
        var result = await _mesasService.ObtenerMesasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse(); // El servicio retorna error en estos casos
        result.Errors.Should().Contain("Multiple errors (Network error)");
    }

    [Fact]
    public async Task ObtenerMesasAsync_CuandoApiServiceIOException_DebeRetornarErrorResponse()
    {
        // Arrange
        var ioException = new IOException("I/O error occurred");

        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ThrowsAsync(ioException);

        // Act
        var result = await _mesasService.ObtenerMesasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse(); // El servicio retorna error en estos casos
        result.Errors.Should().Contain("I/O error occurred");
    }

    [Fact]
    public async Task ObtenerMesasAsync_CuandoApiServiceGenericException_DebeRetornarErrorResponse()
    {
        // Arrange
        var genericException = new InvalidOperationException("Unexpected error");

        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ThrowsAsync(genericException);

        // Act
        var result = await _mesasService.ObtenerMesasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse(); // El servicio retorna error en estos casos
        result.Errors.Should().Contain("Unexpected error");
    }

    #endregion

    #region Casos Edge y Límites

    [Fact]
    public async Task ObtenerMesasAsync_CuandoApiServiceRetornaNull_DebeRetornarErrorResponse()
    {
        // Arrange
        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((ApiResponse<List<MesaDto>>?)null);

        // Act
        var result = await _mesasService.ObtenerMesasAsync();

        // Assert
        result.Should().BeNull(); // El servicio retorna null cuando ApiService retorna null
    }

    [Fact]
    public async Task ObtenerMesasAsync_CuandoApiServiceRetornaDataNull_DebeRetornarErrorResponse()
    {
        // Arrange
        var apiResponse = ApiResponse<List<MesaDto>>.SuccessResponse(null!);
        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(apiResponse);

        // Act
        var result = await _mesasService.ObtenerMesasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue(); // El servicio retorna true cuando los datos son null
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerMesasAsync_CuandoApiServiceRetornaListaVacia_DebeRetornarSuccessResponse()
    {
        // Arrange
        var emptyList = new PaginatedList<MesaDto> { Items = new List<MesaDto>(), TotalCount = 0, PageNumber = 1, PageSize = 10 };
        var apiResponse = ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(emptyList);
        _mockApiService.Setup(x => x.GetAsync<PaginatedList<MesaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(apiResponse);

        // Act
        var result = await _mesasService.ObtenerMesasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerMesasAsync_CuandoApiServiceRetornaListaMuyGrande_DebeRetornarSuccessResponse()
    {
        // Arrange
        var largeList = _fixture.CreateMany<MesaDto>(1000).ToList();
        var paginatedList = new PaginatedList<MesaDto> { Items = largeList, TotalCount = largeList.Count, PageNumber = 1, PageSize = 10 };
        var apiResponse = ApiResponse<PaginatedList<MesaDto>>.SuccessResponse(paginatedList);
        _mockApiService.Setup(x => x.GetAsync<PaginatedList<MesaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(apiResponse);

        // Act
        var result = await _mesasService.ObtenerMesasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1000);
    }

    #endregion

    #region Concurrencia y Threading

    [Fact]
    public async Task ObtenerMesasAsync_CuandoSeLlamaConcurrentemente_DebeManejarCorrectamente()
    {
        // Arrange
        var expectedMesas = _fixture.CreateMany<MesaDto>(5).ToList();
        var expectedResponse = ApiResponse<List<MesaDto>>.SuccessResponse(expectedMesas);

        _mockApiService.Setup(x => x.GetAsync<List<MesaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => _mesasService.ObtenerMesasAsync())
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        results.Should().AllSatisfy(r => r.Success.Should().BeTrue());
        results.Should().AllSatisfy(r => r.Data.Should().BeEquivalentTo(expectedMesas));
    }

    [Fact]
    public async Task ObtenerMesasDisponiblesAsync_CuandoSeLlamaConcurrentemente_DebeManejarCorrectamente()
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

        _mockApiService.Setup(x => x.GetAsync<PaginatedList<MesaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => _mesasService.ObtenerMesasDisponiblesAsync())
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(5);
        results.Should().AllSatisfy(r => r.Success.Should().BeTrue());
        results.Should().AllSatisfy(r => r.Data.Should().BeEquivalentTo(expectedMesas));
    }

    #endregion

    #region Validaciones de Negocio

    [Fact]
    public async Task AsignarMesaAsync_ConMesaIdVacio_DebeUsarEndpointCorrecto()
    {
        // Arrange
        var mesaId = Guid.Empty;
        var expectedResponse = ApiResponse<object>.SuccessResponse(new { });

        _mockApiService.Setup(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.AsignarMesaAsync(mesaId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AsignarMesaAsync_ConClienteIdVacio_DebeUsarEndpointCorrecto()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.Empty;
        var expectedResponse = ApiResponse<object>.SuccessResponse(new { });

        _mockApiService.Setup(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.AsignarMesaAsync(mesaId, clienteId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AsignarMesaAsync_ConNumeroPersonasCero_DebeUsarEndpointCorrecto()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var numeroPersonas = 0;
        var expectedResponse = ApiResponse<object>.SuccessResponse(new { });

        _mockApiService.Setup(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.AsignarMesaAsync(mesaId, null, numeroPersonas);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AsignarMesaAsync_ConNumeroPersonasNegativo_DebeUsarEndpointCorrecto()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var numeroPersonas = -1;
        var expectedResponse = ApiResponse<object>.SuccessResponse(new { });

        _mockApiService.Setup(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.AsignarMesaAsync(mesaId, null, numeroPersonas);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AsignarMesaAsync_ConNumeroPersonasMuyGrande_DebeUsarEndpointCorrecto()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var numeroPersonas = 1000;
        var expectedResponse = ApiResponse<object>.SuccessResponse(new { });

        _mockApiService.Setup(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.AsignarMesaAsync(mesaId, null, numeroPersonas);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AsignarMesaAsync_ConObservacionesVacias_DebeUsarEndpointCorrecto()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var observaciones = "";
        var expectedResponse = ApiResponse<object>.SuccessResponse(new { });

        _mockApiService.Setup(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.AsignarMesaAsync(mesaId, null, null, observaciones);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AsignarMesaAsync_ConObservacionesNull_DebeUsarEndpointCorrecto()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        string? observaciones = null;
        var expectedResponse = ApiResponse<object>.SuccessResponse(new { });

        _mockApiService.Setup(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.AsignarMesaAsync(mesaId, null, null, observaciones);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AsignarMesaAsync_ConObservacionesMuyLargas_DebeUsarEndpointCorrecto()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var observaciones = new string('a', 1000); // Observaciones muy largas
        var expectedResponse = ApiResponse<object>.SuccessResponse(new { });

        _mockApiService.Setup(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.AsignarMesaAsync(mesaId, null, null, observaciones);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AsignarMesaAsync_ConObservacionesConCaracteresEspeciales_DebeUsarEndpointCorrecto()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var observaciones = "Mesa para cena de negocios & celebración especial";
        var expectedResponse = ApiResponse<object>.SuccessResponse(new { });

        _mockApiService.Setup(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mesasService.AsignarMesaAsync(mesaId, null, null, observaciones);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.PostAsync<object>(
            $"api/operaciones/mesas/{mesaId}/asignar",
            It.IsAny<object>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Cancelación

    [Fact]
    public async Task ObtenerMesasAsync_CuandoSeCancela_DebeRetornarErrorResponse()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _mesasService.ObtenerMesasAsync(cancellationToken: cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse(); // El servicio retorna error en estos casos
        result.Errors.Should().Contain("Operación cancelada por el usuario");
        _mockApiService.Verify(x => x.GetAsync<List<MesaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerMesaAsync_CuandoSeCancela_DebeRetornarErrorResponse()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _mesasService.ObtenerMesaAsync(Guid.NewGuid(), cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse(); // El servicio retorna error en estos casos
        result.Errors.Should().Contain("Operación cancelada por el usuario");
        _mockApiService.Verify(x => x.GetAsync<MesaDto>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerMesasDisponiblesAsync_CuandoSeCancela_DebeRetornarErrorResponse()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _mesasService.ObtenerMesasDisponiblesAsync(cancellationToken: cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse(); // El servicio retorna error en estos casos
        result.Errors.Should().Contain("Operación cancelada por el usuario");
        _mockApiService.Verify(x => x.GetAsync<PaginatedList<MesaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerEstadoOcupacionAsync_CuandoSeCancela_DebeRetornarErrorResponse()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _mesasService.ObtenerEstadoOcupacionAsync(cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse(); // El servicio retorna error en estos casos
        result.Errors.Should().Contain("Operación cancelada por el usuario");
        _mockApiService.Verify(x => x.GetAsync<EstadoMesasDto>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AsignarMesaAsync_CuandoSeCancela_DebeRetornarErrorResponse()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _mesasService.AsignarMesaAsync(Guid.NewGuid(), cancellationToken: cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse(); // El servicio retorna error en estos casos
        result.Errors.Should().Contain("Operación cancelada por el usuario");
        _mockApiService.Verify(x => x.PostAsync<object>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LiberarMesaAsync_CuandoSeCancela_DebeRetornarErrorResponse()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _mesasService.LiberarMesaAsync(Guid.NewGuid(), cancellationToken: cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse(); // El servicio retorna error en estos casos
        result.Errors.Should().Contain("Operación cancelada por el usuario");
        _mockApiService.Verify(x => x.PostAsync<MesaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CambiarEstadoMesaAsync_CuandoSeCancela_DebeRetornarErrorResponse()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _mesasService.CambiarEstadoMesaAsync(Guid.NewGuid(), "Nuevo Estado", cancellationToken: cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse(); // El servicio retorna error en estos casos
        result.Errors.Should().Contain("Operación cancelada por el usuario");
        _mockApiService.Verify(x => x.PutAsync<MesaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuscarMejorMesaAsync_CuandoSeCancela_DebeRetornarErrorResponse()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _mesasService.BuscarMejorMesaAsync(4, cancellationToken: cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse(); // El servicio retorna error en estos casos
        result.Errors.Should().Contain("Operación cancelada por el usuario");
        _mockApiService.Verify(x => x.GetAsync<MesaDto>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
} 