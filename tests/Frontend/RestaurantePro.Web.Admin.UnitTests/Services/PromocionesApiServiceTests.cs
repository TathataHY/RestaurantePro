using System.Net;
using System.Text;
using System.Text.Json;

namespace RestaurantePro.Web.Admin.UnitTests.Services;

public class PromocionesApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<TokenStore> _tokenStoreMock;
    private readonly PromocionesApiService _service;

    public PromocionesApiServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _tokenStoreMock = new Mock<TokenStore>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };
        _httpClientFactoryMock.Setup(x => x.CreateClient("Api")).Returns(httpClient);

        _service = new PromocionesApiService(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public async Task ObtenerPromocionesAsync_ConParametrosValidos_DeberiaRetornarPromociones()
    {
        // Arrange
        var promocionesEsperadas = new PaginatedList<PromocionDto>
        {
            Items = new List<PromocionDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Promoción 1", EstaActiva = true },
                new() { Id = Guid.NewGuid(), Nombre = "Promoción 2", EstaActiva = true }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 20
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<PromocionDto>>
        {
            Success = true,
            Data = promocionesEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerPromocionesAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task CrearPromocionAsync_ConDatosValidos_DeberiaCrearPromocion()
    {
        // Arrange
        var nuevaPromocion = new CrearPromocionRequest
        {
            Nombre = "Descuento 20%",
            Codigo = "DESC20",
            Tipo = TipoPromocion.Porcentaje,
            ValorDescuento = 20,
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today.AddDays(30),
            EstaActiva = true
        };

        var promocionCreada = new PromocionDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Descuento 20%",
            Codigo = "DESC20",
            Tipo = TipoPromocion.Porcentaje,
            ValorDescuento = 20,
            EstaActiva = true
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PromocionDto>
        {
            Success = true,
            Data = promocionCreada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearPromocionAsync(nuevaPromocion);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Nombre.Should().Be("Descuento 20%");
        resultado.ValorDescuento.Should().Be(20);
        resultado.EstaActiva.Should().Be(true);
    }

    [Fact]
    public async Task ActualizarPromocionAsync_ConDatosValidos_DeberiaActualizarPromocion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var promocionActualizada = new ActualizarPromocionRequest
        {
            Id = id,
            Nombre = "Descuento 30%",
            Codigo = "DESC30",
            Tipo = TipoPromocion.Porcentaje,
            ValorDescuento = 30,
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today.AddDays(30),
            EstaActiva = true
        };

        var promocionResultado = new PromocionDto
        {
            Id = id,
            Nombre = "Descuento 30%",
            Codigo = "DESC30",
            Tipo = TipoPromocion.Porcentaje,
            ValorDescuento = 30,
            EstaActiva = true
        };

        var apiResponse = new ApiResponse<PromocionDto>
        {
            Success = true,
            Data = promocionResultado,
            Message = "Promoción actualizada correctamente"
        };
        var responseContent = JsonSerializer.Serialize(apiResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ActualizarPromocionAsync(id, promocionActualizada);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Nombre.Should().Be("Descuento 30%");
        resultado.ValorDescuento.Should().Be(30);
        resultado.EstaActiva.Should().Be(true);
    }

    [Fact]
    public async Task EliminarPromocionAsync_ConIdValido_DeberiaEliminarPromocion()
    {
        // Arrange
        var id = Guid.NewGuid();

        var responseContent = JsonSerializer.Serialize(new ApiResponse<bool>
        {
            Success = true,
            Data = true
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.EliminarPromocionAsync(id);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_DeberiaRetornarEstadisticas()
    {
        // Arrange
        var estadisticas = new PromocionEstadisticasDto
        {
            TotalPromociones = 10,
            PromocionesActivas = 5,
            PromocionesExpiradas = 3,
            PromocionesPendientes = 2,
            TotalUsos = 150,
            DescuentoTotalAplicado = 5000
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PromocionEstadisticasDto>
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
        resultado!.TotalPromociones.Should().Be(10);
        resultado.PromocionesActivas.Should().Be(5);
        resultado.TotalUsos.Should().Be(150);
    }

    // ===== PRUEBAS DE ERROR =====

    [Fact]
    public async Task ObtenerPromocionesAsync_ConErrorDeServidor_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.ObtenerPromocionesAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CrearPromocionAsync_ConDatosInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var promocionInvalida = new CrearPromocionRequest
        {
            Nombre = "", // Nombre vacío
            Codigo = "", // Código vacío
            Tipo = TipoPromocion.Porcentaje,
            ValorDescuento = -10 // Valor negativo
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PromocionDto>
        {
            Success = false,
            Message = "Datos de promoción inválidos"
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearPromocionAsync(promocionInvalida);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ActualizarPromocionAsync_ConIdInexistente_DeberiaRetornarError()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var promocionActualizada = new ActualizarPromocionRequest
        {
            Id = idInexistente,
            Nombre = "Promoción Actualizada",
            Codigo = "UPDATED",
            Tipo = TipoPromocion.Porcentaje,
            ValorDescuento = 25,
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today.AddDays(30),
            EstaActiva = true
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var resultado = await _service.ActualizarPromocionAsync(promocionActualizada);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task EliminarPromocionAsync_ConIdInexistente_DeberiaRetornarError()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var resultado = await _service.EliminarPromocionAsync(idInexistente);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConErrorDeServidor_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.ObtenerEstadisticasAsync();

        // Assert
        resultado.Should().BeNull();
    }

    // ===== PRUEBAS ROBUSTAS - CASOS EDGE =====

    [Fact]
    public async Task ObtenerPromocionesAsync_ConFiltrosExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtrosExtremos = new PromocionFiltrosDto
        {
            Busqueda = "A", // Búsqueda mínima
            FechaInicioDesde = DateTime.MinValue,
            FechaInicioHasta = DateTime.MaxValue,
            FechaFinDesde = DateTime.MinValue,
            FechaFinHasta = DateTime.MaxValue,
            EstaActiva = true,
            OrdenarPor = "FechaCreacion",
            DireccionOrden = "asc"
        };

        var promocionesEsperadas = new PaginatedList<PromocionDto>
        {
            Items = new List<PromocionDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 20
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<PromocionDto>>
        {
            Success = true,
            Data = promocionesEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerPromocionesAsync(1, 20, filtrosExtremos);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Items.Should().BeEmpty();
        resultado.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task CrearPromocionAsync_ConValoresExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var requestExtremo = new CrearPromocionRequest
        {
            Nombre = "A", // Nombre mínimo
            Descripcion = new string('A', 500), // Descripción máxima
            Codigo = "A", // Código mínimo
            Tipo = TipoPromocion.Porcentaje,
            ValorDescuento = 0.01m, // Valor mínimo
            ValorMinimoCompra = 0.01m,
            CantidadMaximaUsos = 1,
            FechaInicio = DateTime.MinValue,
            FechaFin = DateTime.MaxValue,
            EstaActiva = true,
            ProductosIds = new List<Guid> { Guid.Empty }
        };

        var promocionCreada = new PromocionDto
        {
            Id = Guid.NewGuid(),
            Nombre = "A",
            Codigo = "A",
            Tipo = TipoPromocion.Porcentaje,
            ValorDescuento = 0.01m,
            EstaActiva = true
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PromocionDto>
        {
            Success = true,
            Data = promocionCreada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearPromocionAsync(requestExtremo);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Nombre.Should().Be("A");
        resultado.ValorDescuento.Should().Be(0.01m);
    }

    // ===== PRUEBAS ROBUSTAS - SEGURIDAD =====

    [Fact]
    public async Task ObtenerPromocionesAsync_ConInyeccionSQL_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtrosMaliciosos = new PromocionFiltrosDto
        {
            Busqueda = "'; DROP TABLE Promociones; --",
            OrdenarPor = "'; DROP TABLE Promociones; --",
            DireccionOrden = "'; DROP TABLE Promociones; --"
        };

        var promocionesEsperadas = new PaginatedList<PromocionDto>
        {
            Items = new List<PromocionDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 20
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<PromocionDto>>
        {
            Success = true,
            Data = promocionesEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerPromocionesAsync(1, 20, filtrosMaliciosos);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CrearPromocionAsync_ConXSS_DeberiaManejarCorrectamente()
    {
        // Arrange
        var requestXSS = new CrearPromocionRequest
        {
            Nombre = "<script>alert('XSS')</script>",
            Descripcion = "<img src=x onerror=alert('XSS')>",
            Codigo = "XSS",
            Tipo = TipoPromocion.Porcentaje,
            ValorDescuento = 10,
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today.AddDays(30),
            EstaActiva = true,
            ProductosIds = new List<Guid>()
        };

        var promocionCreada = new PromocionDto
        {
            Id = Guid.NewGuid(),
            Nombre = "<script>alert('XSS')</script>",
            Codigo = "XSS",
            Tipo = TipoPromocion.Porcentaje,
            ValorDescuento = 10,
            EstaActiva = true
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PromocionDto>
        {
            Success = true,
            Data = promocionCreada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearPromocionAsync(requestXSS);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Nombre.Should().Be("<script>alert('XSS')</script>");
    }

    // ===== PRUEBAS ROBUSTAS - CONCURRENCIA =====

    [Fact]
    public async Task CrearPromocionAsync_Concurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new CrearPromocionRequest
        {
            Nombre = "Promoción Concurrente",
            Codigo = "CONCURRENT",
            Tipo = TipoPromocion.Porcentaje,
            ValorDescuento = 20,
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today.AddDays(30),
            EstaActiva = true,
            ProductosIds = new List<Guid>()
        };

        var promocionCreada = new PromocionDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Promoción Concurrente",
            Codigo = "CONCURRENT",
            Tipo = TipoPromocion.Porcentaje,
            ValorDescuento = 20,
            EstaActiva = true
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PromocionDto>
        {
            Success = true,
            Data = promocionCreada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Ejecutar múltiples operaciones simultáneas
        var tareas = new List<Task<PromocionDto?>>();
        for (int i = 0; i < 10; i++)
        {
            tareas.Add(_service.CrearPromocionAsync(request));
        }

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().HaveCount(10);
        resultados.Should().OnlyContain(r => r != null);
    }

    [Fact]
    public async Task ObtenerPromocionesAsync_Concurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var promocionesEsperadas = new PaginatedList<PromocionDto>
        {
            Items = new List<PromocionDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Promoción 1", EstaActiva = true }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 20
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<PromocionDto>>
        {
            Success = true,
            Data = promocionesEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Ejecutar múltiples consultas simultáneas
        var tareas = new List<Task<List<PromocionDto>>>();
        for (int i = 0; i < 20; i++)
        {
            tareas.Add(_service.ObtenerPromocionesAsync());
        }

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().HaveCount(20);
        resultados.Should().OnlyContain(r => r != null);
    }

    // ===== PRUEBAS ROBUSTAS - RENDIMIENTO =====

    [Fact]
    public async Task ObtenerPromocionesAsync_ConDatosMasivos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var promocionesMasivas = new List<PromocionDto>();
        for (int i = 0; i < 10000; i++)
        {
            promocionesMasivas.Add(new PromocionDto
            {
                Id = Guid.NewGuid(),
                Nombre = $"Promoción {i:D6}",
                Codigo = $"PROM{i:D6}",
                Tipo = TipoPromocion.Porcentaje,
                ValorDescuento = 10 + (i % 50),
                EstaActiva = i % 2 == 0
            });
        }

        var promocionesEsperadas = new PaginatedList<PromocionDto>
        {
            Items = promocionesMasivas.Take(1000).ToList(),
            TotalCount = 10000,
            PageNumber = 1,
            PageSize = 1000
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<PromocionDto>>
        {
            Success = true,
            Data = promocionesEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerPromocionesAsync(1, 1000);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Items.Should().HaveCount(1000);
        resultado.TotalCount.Should().Be(10000);
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConDatosMasivos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var estadisticasMasivas = new PromocionEstadisticasDto
        {
            TotalPromociones = 1000000,
            PromocionesActivas = 500000,
            PromocionesExpiradas = 300000,
            PromocionesPendientes = 200000,
            TotalUsos = 50000000,
            DescuentoTotalAplicado = decimal.MaxValue
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PromocionEstadisticasDto>
        {
            Success = true,
            Data = estadisticasMasivas
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
        resultado!.TotalPromociones.Should().Be(1000000);
        resultado.TotalUsos.Should().Be(50000000);
    }
}
