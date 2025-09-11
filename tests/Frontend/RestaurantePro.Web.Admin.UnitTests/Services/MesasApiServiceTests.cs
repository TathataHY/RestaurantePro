using System.Net;
using System.Text;
using System.Text.Json;
using Moq.Protected;

namespace RestaurantePro.Web.Admin.UnitTests.Services;

public class MesasApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<TokenStore> _tokenStoreMock;
    private readonly MesasApiService _service;

    public MesasApiServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _tokenStoreMock = new Mock<TokenStore>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _httpClientFactoryMock.Setup(x => x.CreateClient("Api")).Returns(httpClient);

        _service = new MesasApiService(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public async Task ObtenerAsync_ConRespuestaExitosa_DeberiaRetornarMesas()
    {
        // Arrange
        var mesasEsperadas = new List<MesaDto>
        {
            new() { Id = Guid.NewGuid(), Numero = "1", Capacidad = 4, Estado = "Disponible", Zona = "Salón Principal" },
            new() { Id = Guid.NewGuid(), Numero = "2", Capacidad = 6, Estado = "Ocupada", Zona = "Terraza" }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<MesaDto>>
        {
            Success = true,
            Data = mesasEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(2);
        resultado.First().Numero.Should().Be("1");
        resultado.Last().Numero.Should().Be("2");
    }

    [Fact]
    public async Task ObtenerAsync_ConFiltros_DeberiaRetornarMesasFiltradas()
    {
        // Arrange
        var mesasEsperadas = new List<MesaDto>
        {
            new() { Id = Guid.NewGuid(), Numero = "1", Estado = "Disponible", Zona = "Salón Principal", Capacidad = 4 }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<MesaDto>>
        {
            Success = true,
            Data = mesasEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerAsync(estado: "Disponible", ubicacion: "Salón Principal", capacidadMinima: 4);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(1);
        resultado.First().Estado.Should().Be("Disponible");
    }

    [Fact]
    public async Task ObtenerAsync_ConErrorEnApi_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConIdValido_DeberiaRetornarMesa()
    {
        // Arrange
        var id = Guid.NewGuid();
        var mesaEsperada = new MesaDto
        {
            Id = id,
            Numero = "1",
            Capacidad = 4,
            Estado = "Disponible",
            Zona = "Salón Principal"
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<MesaDto>
        {
            Success = true,
            Data = mesaEsperada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerPorIdAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(id);
        resultado.Numero.Should().Be("1");
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConIdInexistente_DeberiaRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Mesa no encontrada", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerPorIdAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CrearAsync_ConDatosValidos_DeberiaRetornarMesaCreada()
    {
        // Arrange
        var request = new CrearMesaRequest
        {
            Numero = 10,
            Capacidad = 8,
            Zona = "Terraza",
            Descripcion = "Mesa para 8 personas en terraza"
        };

        var mesaCreada = new MesaDto
        {
            Id = Guid.NewGuid(),
            Numero = request.Numero.ToString(),
            Capacidad = request.Capacidad,
            Zona = request.Zona,
            Estado = "Disponible"
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<MesaDto>
        {
            Success = true,
            Data = mesaCreada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Numero.Should().Be("10");
    }

    [Fact]
    public async Task CrearAsync_ConDatosInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var request = new CrearMesaRequest
        {
            Numero = 0, // Número inválido
            Capacidad = 0, // Capacidad inválida
            Zona = "Terraza"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Datos inválidos", Encoding.UTF8, "application/json")
            });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CrearAsync(request));
    }

    [Fact]
    public async Task ActualizarAsync_ConDatosValidos_DeberiaRetornarMesaActualizada()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new ActualizarMesaRequest
        {
            Numero = 1,
            Capacidad = 6,
            Zona = "Salón Principal",
            Descripcion = "Mesa actualizada"
        };

        var mesaActualizada = new MesaDto
        {
            Id = id,
            Numero = request.Numero.ToString(),
            Capacidad = request.Capacidad,
            Zona = request.Zona,
            Estado = "Disponible"
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<MesaDto>
        {
            Success = true,
            Data = mesaActualizada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ActualizarAsync(id, request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Numero.Should().Be("1");
    }

    [Fact]
    public async Task EliminarAsync_ConIdValido_DeberiaRetornarTrue()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        var resultado = await _service.EliminarAsync(id);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task EliminarAsync_ConIdInexistente_DeberiaRetornarFalse()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var resultado = await _service.EliminarAsync(id);

        // Assert
        resultado.Should().BeFalse();
    }


    // ===== PRUEBAS ROBUSTAS ADICIONALES =====

    [Fact]
    public async Task ObtenerAsync_ConFiltrosExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var mesasEsperadas = new List<MesaDto>();

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<MesaDto>>
        {
            Success = true,
            Data = mesasEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Filtros extremos
        var resultado = await _service.ObtenerAsync(
            estado: "EstadoInexistente@#$%^&*()",
            ubicacion: "UbicacionInexistente@#$%^&*()",
            capacidadMinima: 999999
        );

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task CrearAsync_ConTimeout_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new CrearMesaRequest
        {
            Numero = 10,
            Capacidad = 4,
            Zona = "Terraza"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => _service.CrearAsync(request));
    }

    [Fact]
    public async Task ObtenerAsync_ConJsonMalformado_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{ json malformado }", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerAsync();

        // Assert
        resultado.Should().BeNull();
    }

}
