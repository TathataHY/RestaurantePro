using System.Net;
using System.Text;
using System.Text.Json;

namespace RestaurantePro.Web.Admin.UnitTests.Services;

public class FacturasApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<TokenStore> _tokenStoreMock;
    private readonly FacturasApiService _service;

    public FacturasApiServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _tokenStoreMock = new Mock<TokenStore>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };
        _httpClientFactoryMock.Setup(x => x.CreateClient("Api")).Returns(httpClient);

        _service = new FacturasApiService(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public async Task ObtenerFacturasAsync_ConFiltrosValidos_DeberiaRetornarFacturas()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto
        {
            PageNumber = 1,
            PageSize = 10,
            Estado = "Pagada",
            TipoPago = "Efectivo"
        };

        var facturasEsperadas = new PaginatedList<FacturaDto>
        {
            Items = new List<FacturaDto>
            {
                new() { Id = Guid.NewGuid(), NumeroFactura = "FAC-001", Estado = "Pagada", Total = 150.50m },
                new() { Id = Guid.NewGuid(), NumeroFactura = "FAC-002", Estado = "Pagada", Total = 200.75m }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<FacturaDto>>
        {
            Success = true,
            Data = facturasEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerFacturasAsync(filtros);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Items.Should().HaveCount(2);
        resultado.TotalCount.Should().Be(2);
        resultado.Items.First().NumeroFactura.Should().Be("FAC-001");
    }

    [Fact]
    public async Task ObtenerFacturaAsync_ConIdValido_DeberiaRetornarFactura()
    {
        // Arrange
        var id = Guid.NewGuid();
        var facturaEsperada = new FacturaDto
        {
            Id = id,
            NumeroFactura = "FAC-001",
            Estado = "Pagada",
            Total = 150.50m
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<FacturaDto>
        {
            Success = true,
            Data = facturaEsperada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerFacturaAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(id);
        resultado.NumeroFactura.Should().Be("FAC-001");
    }

    [Fact]
    public async Task CrearFacturaAsync_ConDatosValidos_DeberiaRetornarFacturaCreada()
    {
        // Arrange
        var request = new CrearFacturaRequest
        {
            ClienteId = Guid.NewGuid(),
            MesaId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            Descuento = 0,
            TipoPago = "Efectivo",
            MetodoPago = "Efectivo"
        };

        var facturaCreada = new FacturaDto
        {
            Id = Guid.NewGuid(),
            NumeroFactura = "FAC-001",
            Estado = "Pendiente",
            Total = 150.50m
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<FacturaDto>
        {
            Success = true,
            Data = facturaCreada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearFacturaAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.NumeroFactura.Should().Be("FAC-001");
    }

    [Fact]
    public async Task ActualizarFacturaAsync_ConDatosValidos_DeberiaRetornarFacturaActualizada()
    {
        // Arrange
        var request = new ActualizarFacturaRequest
        {
            Id = Guid.NewGuid(),
            Descuento = 0,
            Estado = "Pagada"
        };

        var facturaActualizada = new FacturaDto
        {
            Id = request.Id,
            NumeroFactura = "FAC-001",
            Estado = request.Estado,
            Total = 200.75m
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<FacturaDto>
        {
            Success = true,
            Data = facturaActualizada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ActualizarFacturaAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Estado.Should().Be("Pagada");
    }

    [Fact]
    public async Task EliminarFacturaAsync_ConIdValido_DeberiaRetornarTrue()
    {
        // Arrange
        var id = Guid.NewGuid();

        var responseContent = JsonSerializer.Serialize(new ApiResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Factura eliminada correctamente"
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.EliminarFacturaAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().BeTrue();
    }

    [Fact]
    public async Task CancelarFacturaAsync_ConDatosValidos_DeberiaRetornarTrue()
    {
        // Arrange
        var request = new CancelarFacturaRequest
        {
            FacturaId = Guid.NewGuid(),
            Motivo = "Error en la factura"
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Factura cancelada correctamente"
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CancelarFacturaAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().BeTrue();
    }

    [Fact]
    public async Task RegistrarPagoAsync_ConDatosValidos_DeberiaRetornarPagoRegistrado()
    {
        // Arrange
        var request = new RegistrarPagoRequest
        {
            FacturaId = Guid.NewGuid(),
            Monto = 150.50m,
            MetodoPago = "Efectivo",
            Referencia = "REF-001"
        };

        var pagoRegistrado = new FacturaPagoDto
        {
            Id = Guid.NewGuid(),
            FacturaId = request.FacturaId,
            Monto = request.Monto,
            MetodoPago = request.MetodoPago
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<FacturaPagoDto>
        {
            Success = true,
            Data = pagoRegistrado
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.RegistrarPagoAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Monto.Should().Be(150.50m);
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConRespuestaExitosa_DeberiaRetornarEstadisticas()
    {
        // Arrange
        var estadisticas = new FacturaEstadisticasDto
        {
            TotalFacturas = 100,
            FacturasPagadas = 80,
            FacturasPendientes = 15,
            FacturasCanceladas = 5,
            TotalVentas = 15000.50m,
            PromedioFactura = 150.01m
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<FacturaEstadisticasDto>
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
        resultado!.TotalFacturas.Should().Be(100);
        resultado.TotalVentas.Should().Be(15000.50m);
    }

    [Fact]
    public async Task ObtenerMetodosPagoAsync_ConRespuestaExitosa_DeberiaRetornarMetodos()
    {
        // Arrange
        var metodos = new List<string> { "Efectivo", "Tarjeta", "Transferencia" };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<string>>
        {
            Success = true,
            Data = metodos
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerMetodosPagoAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(3);
        resultado.Should().Contain("Efectivo");
    }

    // ===== PRUEBAS DE ERROR =====

    [Fact]
    public async Task ObtenerFacturasAsync_ConErrorEnApi_DeberiaRetornarNull()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto { PageNumber = 1, PageSize = 10 };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Error interno", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerFacturasAsync(filtros);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerFacturaAsync_ConIdInexistente_DeberiaRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Factura no encontrada", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerFacturaAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CrearFacturaAsync_ConDatosInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var request = new CrearFacturaRequest
        {
            Descuento = -100 // Descuento inválido
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<FacturaDto>
        {
            Success = false,
            Message = "Monto inválido"
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearFacturaAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeFalse();
        resultado.Message.Should().Contain("Monto inválido");
    }

    [Fact]
    public async Task EliminarFacturaAsync_ConErrorDeServidor_DeberiaRetornarError()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.EliminarFacturaAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeFalse();
        resultado.Message.Should().Contain("Error al eliminar factura");
    }

    // ===== PRUEBAS DE SEGURIDAD =====

    [Fact]
    public async Task ObtenerFacturasAsync_ConInyeccionSQL_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto
        {
            PageNumber = 1,
            PageSize = 10,
            Busqueda = "'; DROP TABLE facturas; --",
            Estado = "'; DROP TABLE facturas; --"
        };

        var facturasEsperadas = new PaginatedList<FacturaDto>
        {
            Items = new List<FacturaDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<FacturaDto>>
        {
            Success = true,
            Data = facturasEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerFacturasAsync(filtros);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CrearFacturaAsync_ConXSS_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new CrearFacturaRequest
        {
            ClienteId = Guid.NewGuid(),
            MesaId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            Descuento = 0,
            Observaciones = "<script>alert('xss')</script>",
            NotasInternas = "Notas <img src=x onerror=alert('xss')>"
        };

        var facturaCreada = new FacturaDto
        {
            Id = Guid.NewGuid(),
            NumeroFactura = "FAC-XSS",
            Estado = "Pendiente",
            Total = 150.50m
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<FacturaDto>
        {
            Success = true,
            Data = facturaCreada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearFacturaAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
    }

    // ===== PRUEBAS DE CONCURRENCIA =====

    [Fact]
    public async Task ObtenerFacturasAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto { PageNumber = 1, PageSize = 10 };

        var facturasEsperadas = new PaginatedList<FacturaDto>
        {
            Items = new List<FacturaDto>
            {
                new() { Id = Guid.NewGuid(), NumeroFactura = "FAC-001", Estado = "Pagada" }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<FacturaDto>>
        {
            Success = true,
            Data = facturasEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Simular concurrencia con múltiples tareas
        var tasks = new List<Task<PaginatedList<FacturaDto>?>>();
        for (int i = 0; i < 12; i++)
        {
            tasks.Add(_service.ObtenerFacturasAsync(filtros));
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(12);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().AllSatisfy(r => r!.Items.Should().HaveCount(1));
    }

    [Fact]
    public async Task RegistrarPagoAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new RegistrarPagoRequest
        {
            FacturaId = Guid.NewGuid(),
            Monto = 150.50m,
            MetodoPago = "Efectivo"
        };

        var pagoRegistrado = new FacturaPagoDto
        {
            Id = Guid.NewGuid(),
            FacturaId = request.FacturaId,
            Monto = request.Monto,
            MetodoPago = request.MetodoPago
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<FacturaPagoDto>
        {
            Success = true,
            Data = pagoRegistrado
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Simular concurrencia con múltiples tareas
        var tasks = new List<Task<ApiResponse<FacturaPagoDto>?>>();
        for (int i = 0; i < 8; i++)
        {
            tasks.Add(_service.RegistrarPagoAsync(request));
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(8);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().AllSatisfy(r => r!.Success.Should().BeTrue());
    }

    // ===== PRUEBAS DE LÍMITES =====

    [Fact]
    public async Task ObtenerFacturasAsync_ConPaginacionMasiva_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto
        {
            PageNumber = 999999,
            PageSize = 10000,
            OrdenarPor = "FechaCreacion",
            DireccionOrden = "desc"
        };

        var facturasMasivas = new PaginatedList<FacturaDto>
        {
            Items = new List<FacturaDto>(),
            TotalCount = 0,
            PageNumber = 999999,
            PageSize = 10000
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<FacturaDto>>
        {
            Success = true,
            Data = facturasMasivas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerFacturasAsync(filtros);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.PageNumber.Should().Be(999999);
        resultado.PageSize.Should().Be(10000);
        resultado.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CrearFacturaAsync_ConDatosMasivos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new CrearFacturaRequest
        {
            ClienteId = Guid.NewGuid(),
            MesaId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            Descuento = 0,
            Observaciones = new string('A', 10000), // Observaciones muy largas
            NotasInternas = new string('B', 5000) // Notas muy largas
        };

        var facturaCreada = new FacturaDto
        {
            Id = Guid.NewGuid(),
            NumeroFactura = "FAC-MASIVO",
            Estado = "Pendiente",
            Total = 150.50m
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<FacturaDto>
        {
            Success = true,
            Data = facturaCreada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearFacturaAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.NumeroFactura.Should().Be("FAC-MASIVO");
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConDatosMasivos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var estadisticas = new FacturaEstadisticasDto
        {
            TotalFacturas = 1000000,
            FacturasPagadas = 800000,
            FacturasPendientes = 150000,
            FacturasCanceladas = 50000,
            TotalVentas = decimal.MaxValue,
            PromedioFactura = 999999.99m
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<FacturaEstadisticasDto>
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
        resultado!.TotalFacturas.Should().Be(1000000);
        resultado.TotalVentas.Should().Be(decimal.MaxValue);
    }
}
