using System.Net;
using System.Text;
using System.Text.Json;
using Moq.Protected;

namespace RestaurantePro.Web.Admin.UnitTests.Services;

public class ComandasApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<TokenStore> _tokenStoreMock;
    private readonly ComandasApiService _service;

    public ComandasApiServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _tokenStoreMock = new Mock<TokenStore>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _httpClientFactoryMock.Setup(x => x.CreateClient("Api")).Returns(httpClient);

        _service = new ComandasApiService(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public async Task ObtenerComandasAsync_ConFiltrosValidos_DeberiaRetornarComandas()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto
        {
            PageNumber = 1,
            PageSize = 10,
            OrdenarPor = "FechaCreacion",
            DireccionOrden = "desc"
        };

        var comandasEsperadas = new PaginatedList<ComandaDto>
        {
            Items = new List<ComandaDto>
            {
                new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-001", Estado = "Pendiente", Prioridad = "Normal" },
                new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-002", Estado = "EnPreparacion", Prioridad = "Alta" }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<ComandaDto>>
        {
            Success = true,
            Data = comandasEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerComandasAsync(filtros);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Items.Should().HaveCount(2);
        resultado.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task ObtenerComandasAsync_ConFiltrosCompletos_DeberiaRetornarComandas()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto
        {
            PageNumber = 1,
            PageSize = 10,
            Busqueda = "pizza",
            Estado = "Pendiente",
            Prioridad = "Alta",
            TipoComanda = "Mesa",
            MesaId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today.AddDays(1),
            EsUrgente = true,
            EsDomicilio = false,
            EsLenta = false,
            TiempoMinimo = 10,
            TiempoMaximo = 60,
            OrdenarPor = "Prioridad",
            DireccionOrden = "asc"
        };

        var comandasEsperadas = new PaginatedList<ComandaDto>
        {
            Items = new List<ComandaDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<ComandaDto>>
        {
            Success = true,
            Data = comandasEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerComandasAsync(filtros);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerComandasAsync_ConErrorEnApi_DeberiaRetornarNull()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto
        {
            PageNumber = 1,
            PageSize = 10
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerComandasAsync(filtros);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerComandaAsync_ConIdValido_DeberiaRetornarComanda()
    {
        // Arrange
        var id = Guid.NewGuid();
        var comandaEsperada = new ComandaDto
        {
            Id = id,
            NumeroComanda = "CMD-001",
            Estado = "Pendiente",
            Prioridad = "Normal",
            FechaCreacion = DateTime.UtcNow
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ComandaDto>
        {
            Success = true,
            Data = comandaEsperada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerComandaAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(id);
        resultado.NumeroComanda.Should().Be("CMD-001");
    }

    [Fact]
    public async Task ObtenerComandaAsync_ConIdInexistente_DeberiaRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Comanda no encontrada", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerComandaAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CrearComandaAsync_ConDatosValidos_DeberiaRetornarComandaCreada()
    {
        // Arrange
        var request = new CrearComandaRequest
        {
            NumeroComanda = "CMD-001",
            MesaId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            TipoComanda = "Mesa",
            Prioridad = "Normal",
            Observaciones = "Sin cebolla"
        };

        var comandaCreada = new ComandaDto
        {
            Id = Guid.NewGuid(),
            NumeroComanda = request.NumeroComanda,
            Estado = "Pendiente",
            Prioridad = request.Prioridad
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ComandaDto>
        {
            Success = true,
            Data = comandaCreada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearComandaAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.NumeroComanda.Should().Be("CMD-001");
    }

    [Fact]
    public async Task CrearComandaAsync_ConErrorEnApi_DeberiaRetornarError()
    {
        // Arrange
        var request = new CrearComandaRequest
        {
            NumeroComanda = "CMD-001",
            MesaId = Guid.NewGuid()
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.CrearComandaAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeFalse();
        resultado.Message.Should().Contain("Error al crear comanda");
    }

    [Fact]
    public async Task CambiarEstadoAsync_ConDatosValidos_DeberiaRetornarExito()
    {
        // Arrange
        var request = new CambiarEstadoComandaRequest
        {
            ComandaId = Guid.NewGuid(),
            NuevoEstado = "EnPreparacion",
            Observaciones = "Iniciando preparación"
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Estado cambiado correctamente"
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CambiarEstadoAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerComandasActivasAsync_ConRespuestaExitosa_DeberiaRetornarComandas()
    {
        // Arrange
        var comandasActivas = new List<ComandaDto>
        {
            new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-001", Estado = "Pendiente" },
            new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-002", Estado = "EnPreparacion" }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<ComandaDto>>
        {
            Success = true,
            Data = comandasActivas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerComandasActivasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(2);
        resultado.First().Estado.Should().Be("Pendiente");
    }

    [Fact]
    public async Task ObtenerComandasUrgentesAsync_ConRespuestaExitosa_DeberiaRetornarComandas()
    {
        // Arrange
        var comandasUrgentes = new List<ComandaDto>
        {
            new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-URG-001", Prioridad = "Urgente" }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<ComandaDto>>
        {
            Success = true,
            Data = comandasUrgentes
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerComandasUrgentesAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(1);
        resultado.First().Prioridad.Should().Be("Urgente");
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConRespuestaExitosa_DeberiaRetornarEstadisticas()
    {
        // Arrange
        var estadisticas = new ComandaEstadisticasDto
        {
            TotalComandas = 100,
            ComandasPendientes = 20,
            ComandasEnPreparacion = 30,
            ComandasListas = 25,
            ComandasEntregadas = 25,
            TiempoPromedioPreparacion = 25.5,
            ComandasUrgentes = 5
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ComandaEstadisticasDto>
        {
            Success = true,
            Data = estadisticas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerEstadisticasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.TotalComandas.Should().Be(100);
        resultado.ComandasPendientes.Should().Be(20);
        resultado.TiempoPromedioPreparacion.Should().Be(25.5);
    }

    [Fact]
    public async Task ObtenerEstadosAsync_ConRespuestaExitosa_DeberiaRetornarEstados()
    {
        // Arrange
        var estados = new List<string> { "Pendiente", "EnPreparacion", "Lista", "Entregada", "Cancelada" };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<string>>
        {
            Success = true,
            Data = estados
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerEstadosAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(5);
        resultado.Should().Contain("Pendiente");
        resultado.Should().Contain("Entregada");
    }

    [Fact]
    public async Task ObtenerSiguienteNumeroComandaAsync_ConRespuestaExitosa_DeberiaRetornarNumero()
    {
        // Arrange
        var numeroComanda = "CMD-001";

        var responseContent = JsonSerializer.Serialize(new ApiResponse<string>
        {
            Success = true,
            Data = numeroComanda
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerSiguienteNumeroComandaAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().Be("CMD-001");
    }

    // ===== PRUEBAS ROBUSTAS ADICIONALES =====

    [Fact]
    public async Task ObtenerComandasAsync_ConFiltrosExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto
        {
            PageNumber = 999999,
            PageSize = 1000,
            Busqueda = "test@#$%^&*()",
            Estado = "EstadoInexistente",
            Prioridad = "PrioridadInexistente",
            TipoComanda = "TipoInexistente",
            MesaId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            FechaInicio = DateTime.MinValue,
            FechaFin = DateTime.MaxValue,
            EsUrgente = true,
            EsDomicilio = false,
            EsLenta = false,
            TiempoMinimo = 0,
            TiempoMaximo = 999999,
            OrdenarPor = "CampoInexistente",
            DireccionOrden = "direccion_invalida"
        };

        var comandasEsperadas = new PaginatedList<ComandaDto>
        {
            Items = new List<ComandaDto>(),
            TotalCount = 0,
            PageNumber = 999999,
            PageSize = 1000
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<ComandaDto>>
        {
            Success = true,
            Data = comandasEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerComandasAsync(filtros);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.PageNumber.Should().Be(999999);
        resultado.PageSize.Should().Be(1000);
        resultado.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CrearComandaAsync_ConTimeout_DeberiaRetornarError()
    {
        // Arrange
        var request = new CrearComandaRequest
        {
            NumeroComanda = "CMD-001",
            MesaId = Guid.NewGuid()
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var resultado = await _service.CrearComandaAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeFalse();
        resultado.Message.Should().Contain("Error al crear comanda");
    }

    [Fact]
    public async Task ObtenerComandasActivasAsync_ConJsonMalformado_DeberiaRetornarNull()
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
        var resultado = await _service.ObtenerComandasActivasAsync();

        // Assert
        resultado.Should().BeNull();
    }
}
