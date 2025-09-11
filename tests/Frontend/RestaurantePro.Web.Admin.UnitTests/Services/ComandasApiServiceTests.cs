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

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };
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
            NumeroComanda = "CMD-001",
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
            MesaId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid()
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
            ComandasEnProceso = 30,
            ComandasListas = 25,
            ComandasEntregadas = 25,
            TiempoPromedioPreparacion = 25.5m,
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
        resultado.TiempoPromedioPreparacion.Should().Be(25.5m);
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
            MesaId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid()
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

    // ===== PRUEBAS DE SEGURIDAD =====

    [Fact]
    public async Task ObtenerComandasAsync_ConInyeccionSQL_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto
        {
            PageNumber = 1,
            PageSize = 10,
            Busqueda = "'; DROP TABLE comandas; --",
            Estado = "'; DROP TABLE comandas; --",
            Prioridad = "'; DROP TABLE comandas; --"
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
    public async Task CrearComandaAsync_ConXSS_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new CrearComandaRequest
        {
            MesaId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            Observaciones = "<script>alert('xss')</script>",
            NotasCocina = "Notas <img src=x onerror=alert('xss')>"
        };

        var comandaCreada = new ComandaDto
        {
            Id = Guid.NewGuid(),
            NumeroComanda = "CMD-XSS",
            Estado = "Pendiente",
            Prioridad = "Normal"
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
    }

    // ===== PRUEBAS DE CONCURRENCIA =====

    [Fact]
    public async Task ObtenerComandasActivasAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var comandasActivas = new List<ComandaDto>
        {
            new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-001", Estado = "Pendiente" },
            new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-002", Estado = "EnProceso" }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<ComandaDto>>
        {
            Success = true,
            Data = comandasActivas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Simular concurrencia con múltiples tareas
        var tasks = new List<Task<List<ComandaDto>?>>();
        for (int i = 0; i < 15; i++)
        {
            tasks.Add(_service.ObtenerComandasActivasAsync());
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(15);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().AllSatisfy(r => r!.Should().HaveCount(2));
    }

    [Fact]
    public async Task CambiarEstadoAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new CambiarEstadoComandaRequest
        {
            ComandaId = Guid.NewGuid(),
            NuevoEstado = "EnProceso",
            Observaciones = "Cambio de estado concurrente"
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Estado cambiado correctamente"
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Simular concurrencia con múltiples tareas
        var tasks = new List<Task<ApiResponse<bool>?>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_service.CambiarEstadoAsync(request));
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(10);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().AllSatisfy(r => r!.Success.Should().BeTrue());
    }

    // ===== PRUEBAS DE LÍMITES =====

    [Fact]
    public async Task ObtenerComandasAsync_ConPaginacionMasiva_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto
        {
            PageNumber = 999999,
            PageSize = 10000,
            OrdenarPor = "FechaCreacion",
            DireccionOrden = "desc"
        };

        var comandasMasivas = new PaginatedList<ComandaDto>
        {
            Items = new List<ComandaDto>(),
            TotalCount = 0,
            PageNumber = 999999,
            PageSize = 10000
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<ComandaDto>>
        {
            Success = true,
            Data = comandasMasivas
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
        resultado.PageSize.Should().Be(10000);
        resultado.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CrearComandaAsync_ConDatosMasivos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new CrearComandaRequest
        {
            MesaId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            Observaciones = new string('A', 10000), // Observaciones muy largas
            NotasCocina = new string('B', 5000), // Notas muy largas
            DireccionDomicilio = new string('C', 2000), // Dirección muy larga
            ReferenciasDomicilio = new string('D', 3000) // Referencias muy largas
        };

        var comandaCreada = new ComandaDto
        {
            Id = Guid.NewGuid(),
            NumeroComanda = "CMD-MASIVO",
            Estado = "Pendiente",
            Prioridad = "Normal"
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
        resultado.Data!.NumeroComanda.Should().Be("CMD-MASIVO");
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConDatosMasivos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var estadisticas = new ComandaEstadisticasDto
        {
            TotalComandas = 1000000,
            ComandasPendientes = 100000,
            ComandasEnProceso = 200000,
            ComandasListas = 300000,
            ComandasEntregadas = 400000,
            TiempoPromedioPreparacion = 999.99m,
            ComandasUrgentes = 50000
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
        resultado!.TotalComandas.Should().Be(1000000);
        resultado.ComandasPendientes.Should().Be(100000);
        resultado.TiempoPromedioPreparacion.Should().Be(999.99m);
    }

    [Fact]
    public async Task ObtenerComandasAsync_ConRateLimiting_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto
        {
            PageNumber = 1,
            PageSize = 10
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
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Simular múltiples consultas rápidas (rate limiting)
        var tasks = new List<Task<PaginatedList<ComandaDto>?>>();
        for (int i = 0; i < 25; i++)
        {
            tasks.Add(_service.ObtenerComandasAsync(filtros));
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(25);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().AllSatisfy(r => r!.Items.Should().BeEmpty());
    }

    // ===== PRUEBAS ROBUSTAS - CASOS EDGE =====

    [Fact]
    public async Task ObtenerComandasAsync_ConDatosMasivos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto
        {
            PageNumber = 1,
            PageSize = 20
        };

        var comandas = new List<ComandaDto>();
        for (int i = 0; i < 10000; i++)
        {
            comandas.Add(new ComandaDto
            {
                Id = Guid.NewGuid(),
                NumeroComanda = $"CMD-{i:D6}",
                Estado = (i % 5) switch { 0 => "Pendiente", 1 => "EnProceso", 2 => "Lista", 3 => "Entregada", _ => "Cancelada" },
                Prioridad = (i % 4) switch { 0 => "Baja", 1 => "Normal", 2 => "Alta", _ => "Urgente" },
                TipoComanda = (i % 3) switch { 0 => "Mesa", 1 => "Domicilio", _ => "Mostrador" },
                FechaCreacion = DateTime.UtcNow.AddMinutes(-i),
                Subtotal = (decimal)(i * 10.50),
                Total = (decimal)(i * 12.50),
                NumeroPersonas = (i % 8) + 1
            });
        }

        var paginatedList = new PaginatedList<ComandaDto>
        {
            Items = comandas.Take(20).ToList(),
            TotalCount = 10000,
            PageNumber = 1,
            PageSize = 20,
            TotalPages = 500
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<ComandaDto>>
        {
            Success = true,
            Data = paginatedList
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
        resultado!.TotalCount.Should().Be(10000);
        resultado.TotalPages.Should().Be(500);
    }

    [Fact]
    public async Task ObtenerComandasAsync_ConCaracteresEspeciales_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto
        {
            PageNumber = 1,
            PageSize = 20,
            Busqueda = "Comanda 🍕 Pizza & Pasta 🍝"
        };

        var comandas = new List<ComandaDto>
        {
            new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-🍕-001", Observaciones = "Pizza Margherita 🍕 con extra queso 🧀" },
            new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-🍝-002", Observaciones = "Pasta Carbonara 🍝 al dente" },
            new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-🌮-003", Observaciones = "Tacos al Pastor 🌮 con piña 🍍" },
            new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-🍣-004", Observaciones = "Sushi Roll 🍣 con wasabi extra" },
            new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-🍜-005", Observaciones = "Ramen Tonkotsu 🍜 con huevo poché" }
        };

        var paginatedList = new PaginatedList<ComandaDto>
        {
            Items = comandas,
            TotalCount = 5,
            PageNumber = 1,
            PageSize = 20,
            TotalPages = 1
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<ComandaDto>>
        {
            Success = true,
            Data = paginatedList
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
        resultado!.Items.Should().HaveCount(5);
        resultado.Items.First().NumeroComanda.Should().Contain("🍕");
    }

    [Fact]
    public async Task CrearComandaAsync_ConValoresExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new CrearComandaRequest
        {
            MesaId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            NumeroPersonas = int.MaxValue,
            Observaciones = new string('A', 500), // Máximo permitido
            NotasCocina = new string('B', 500), // Máximo permitido
            DireccionDomicilio = new string('D', 200), // Dirección máxima
            TelefonoDomicilio = new string('5', 20), // Teléfono máximo
            ReferenciasDomicilio = new string('E', 200), // Referencias máximas
            EsUrgente = true,
            RequiereFactura = true,
            EsDomicilio = true
        };

        var comandaCreada = new ComandaDto
        {
            Id = Guid.NewGuid(),
            NumeroComanda = "CMD-EXTREMO-001",
            MesaId = request.MesaId,
            MeseroId = request.MeseroId,
            ClienteId = request.ClienteId,
            NumeroPersonas = request.NumeroPersonas,
            Observaciones = request.Observaciones,
            NotasCocina = request.NotasCocina,
            DireccionDomicilio = request.DireccionDomicilio,
            TelefonoDomicilio = request.TelefonoDomicilio,
            ReferenciasDomicilio = request.ReferenciasDomicilio,
            EsUrgente = request.EsUrgente,
            RequiereFactura = request.RequiereFactura,
            EsDomicilio = request.EsDomicilio,
            FechaCreacion = DateTime.UtcNow
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
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearComandaAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data!.NumeroPersonas.Should().Be(int.MaxValue);
        resultado.Data.Observaciones.Should().HaveLength(500);
    }

    // ===== PRUEBAS ROBUSTAS - SEGURIDAD =====


    [Fact]
    public async Task ActualizarComandaAsync_ConPayloadsMaliciosos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new ActualizarComandaRequest
        {
            Id = Guid.NewGuid(),
            Observaciones = "'; DROP TABLE Comandas; -- <script>alert('xss')</script>",
            NotasCocina = "javascript:alert('xss'); <img src=x onerror=alert('xss')>",
            NotasEntrega = "<svg onload=alert('xss')>"
        };

        var comandaActualizada = new ComandaDto
        {
            Id = request.Id,
            Observaciones = request.Observaciones,
            NotasCocina = request.NotasCocina,
            NotasEntrega = request.NotasEntrega,
            FechaCreacion = DateTime.UtcNow
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ComandaDto>
        {
            Success = true,
            Data = comandaActualizada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ActualizarComandaAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data!.Observaciones.Should().Contain("DROP TABLE");
    }

    // ===== PRUEBAS ROBUSTAS - CONCURRENCIA =====

    [Fact]
    public async Task ObtenerComandasAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto
        {
            PageNumber = 1,
            PageSize = 20
        };

        var comandas = new List<ComandaDto>
        {
            new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-CONCURRENCIA-001", Estado = "Pendiente" }
        };

        var paginatedList = new PaginatedList<ComandaDto>
        {
            Items = comandas,
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 20,
            TotalPages = 1
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<ComandaDto>>
        {
            Success = true,
            Data = paginatedList
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var tasks = new List<Task<PaginatedList<ComandaDto>?>>();
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(_service.ObtenerComandasAsync(filtros));
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(50);
        resultados.All(r => r != null).Should().BeTrue();
        resultados.All(r => r!.Items.Count == 1).Should().BeTrue();
    }

    [Fact]
    public async Task CrearComandaAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new ApiResponse<ComandaDto>
                {
                    Success = true,
                    Data = new ComandaDto { Id = Guid.NewGuid(), NumeroComanda = "CMD-TEST" }
                }), Encoding.UTF8, "application/json")
            });

        // Act
        var tasks = new List<Task<ApiResponse<ComandaDto>?>>();
        for (int i = 0; i < 30; i++)
        {
            var request = new CrearComandaRequest
            {
                MesaId = Guid.NewGuid(),
                MeseroId = Guid.NewGuid()
            };
            tasks.Add(_service.CrearComandaAsync(request));
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(30);
        resultados.All(r => r != null).Should().BeTrue();
        resultados.All(r => r!.Success).Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var estadisticas = new ComandaEstadisticasDto
        {
            TotalComandas = 1000,
            ComandasPendientes = 100,
            ComandasEnProceso = 200,
            ComandasListas = 300,
            ComandasEntregadas = 400,
            TiempoPromedioPreparacion = 25.5m,
            ComandasUrgentes = 50
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ComandaEstadisticasDto>
        {
            Success = true,
            Data = estadisticas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var tasks = new List<Task<ComandaEstadisticasDto?>>();
        for (int i = 0; i < 40; i++)
        {
            tasks.Add(_service.ObtenerEstadisticasAsync());
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(40);
        resultados.All(r => r != null).Should().BeTrue();
        resultados.All(r => r!.TotalComandas == 1000).Should().BeTrue();
    }


    // ===== PRUEBAS ROBUSTAS - RENDIMIENTO Y LÍMITES =====

    [Fact]
    public async Task ObtenerComandasAsync_ConTimeout_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto
        {
            PageNumber = 1,
            PageSize = 20
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var resultado = await _service.ObtenerComandasAsync(filtros);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerComandasAsync_ConError500_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto
        {
            PageNumber = 1,
            PageSize = 20
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.ObtenerComandasAsync(filtros);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerComandasAsync_ConError503_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto
        {
            PageNumber = 1,
            PageSize = 20
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            });

        // Act
        var resultado = await _service.ObtenerComandasAsync(filtros);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ExportarComandasAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto
        {
            PageNumber = 1,
            PageSize = 20
        };

        var excelBytes = Encoding.UTF8.GetBytes("Excel content");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(excelBytes)
            });

        // Act
        var tasks = new List<Task<ApiResponse<byte[]>?>>();
        for (int i = 0; i < 15; i++)
        {
            tasks.Add(_service.ExportarComandasAsync(filtros));
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(15);
        resultados.All(r => r != null).Should().BeTrue();
        resultados.All(r => r!.Success).Should().BeTrue();
        resultados.All(r => r.Data!.Length == 13).Should().BeTrue();
    }
}
