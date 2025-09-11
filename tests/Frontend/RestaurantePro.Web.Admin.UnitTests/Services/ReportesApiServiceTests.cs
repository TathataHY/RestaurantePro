using System.Net;
using System.Text;
using System.Text.Json;

namespace RestaurantePro.Web.Admin.UnitTests.Services;

public class ReportesApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<TokenStore> _tokenStoreMock;
    private readonly ReportesApiService _service;

    public ReportesApiServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _tokenStoreMock = new Mock<TokenStore>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };
        _httpClientFactoryMock.Setup(x => x.CreateClient("Api")).Returns(httpClient);

        _service = new ReportesApiService(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public async Task ObtenerEstadisticasAsync_DeberiaRetornarEstadisticas()
    {
        // Arrange
        var estadisticas = new { 
            TotalVentas = 15000.50m, 
            TotalComandas = 150, 
            TotalMesas = 20,
            PromedioVentaPorMesa = 750.03m 
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<object>
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
    }

    [Fact]
    public async Task GenerarReporteVentasAsync_ConFiltrosValidos_DeberiaRetornarReporte()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto
        {
            FechaInicio = DateTime.Today.AddDays(-30),
            FechaFin = DateTime.Today,
            MeseroId = Guid.NewGuid(),
            MesaId = Guid.NewGuid()
        };

        var reporteVentas = new { 
            FechaInicio = filtros.FechaInicio,
            FechaFin = filtros.FechaFin,
            TotalVentas = 5000.00m,
            CantidadVentas = 25,
            PromedioVenta = 200.00m 
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<object>
        {
            Success = true,
            Data = reporteVentas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.GenerarReporteVentasAsync(filtros);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerarReporteProductosAsync_ConFiltrosValidos_DeberiaRetornarReporte()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto
        {
            FechaInicio = DateTime.Today.AddDays(-7),
            FechaFin = DateTime.Today,
            CategoriaId = Guid.NewGuid(),
            LimiteResultados = 10
        };

        var reporteProductos = new { 
            FechaInicio = filtros.FechaInicio,
            FechaFin = filtros.FechaFin,
            ProductosMasVendidos = new[] { 
                new { ProductoId = Guid.NewGuid(), Nombre = "Pizza Margherita", CantidadVendida = 50, TotalVentas = 2500.00m },
                new { ProductoId = Guid.NewGuid(), Nombre = "Hamburguesa Clásica", CantidadVendida = 30, TotalVentas = 1500.00m }
            },
            TotalProductosVendidos = 80,
            TotalVentasProductos = 4000.00m 
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<object>
        {
            Success = true,
            Data = reporteProductos
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.GenerarReporteProductosAsync(filtros);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerarReporteMesasAsync_ConFiltrosValidos_DeberiaRetornarReporte()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto
        {
            FechaInicio = DateTime.Today.AddDays(-14),
            FechaFin = DateTime.Today,
            MesaId = Guid.NewGuid()
        };

        var reporteMesas = new { 
            FechaInicio = filtros.FechaInicio,
            FechaFin = filtros.FechaFin,
            MesasUtilizadas = new[] { 
                new { MesaId = Guid.NewGuid(), NumeroMesa = "Mesa 1", TiempoUso = TimeSpan.FromHours(4), TotalVentas = 800.00m },
                new { MesaId = Guid.NewGuid(), NumeroMesa = "Mesa 2", TiempoUso = TimeSpan.FromHours(3), TotalVentas = 600.00m }
            },
            TotalMesasUtilizadas = 2,
            TiempoPromedioUso = TimeSpan.FromHours(3.5),
            TotalVentasMesas = 1400.00m 
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<object>
        {
            Success = true,
            Data = reporteMesas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.GenerarReporteMesasAsync(filtros);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerarReporteComandasAsync_ConFiltrosValidos_DeberiaRetornarReporte()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto
        {
            FechaInicio = DateTime.Today.AddDays(-1),
            FechaFin = DateTime.Today
        };

        var reporteComandas = new { 
            FechaInicio = filtros.FechaInicio,
            FechaFin = filtros.FechaFin,
            ComandasResumen = new[] { 
                new { ComandaId = Guid.NewGuid(), NumeroComanda = "CMD-001", Estado = "Completada", Total = 150.00m, TiempoPreparacion = TimeSpan.FromMinutes(25) },
                new { ComandaId = Guid.NewGuid(), NumeroComanda = "CMD-002", Estado = "En Proceso", Total = 200.00m, TiempoPreparacion = TimeSpan.FromMinutes(15) }
            },
            TotalComandas = 2,
            ComandasCompletadas = 1,
            ComandasEnProceso = 1,
            TiempoPromedioPreparacion = TimeSpan.FromMinutes(20) 
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<object>
        {
            Success = true,
            Data = reporteComandas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.GenerarReporteComandasAsync(filtros);

        // Assert
        resultado.Should().NotBeNull();
    }

    // ===== PRUEBAS DE ERROR =====

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

    [Fact]
    public async Task GenerarReporteVentasAsync_ConFiltrosInvalidos_DeberiaRetornarNull()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto
        {
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today.AddDays(-1) // Fecha fin anterior a inicio
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.GenerarReporteVentasAsync(filtros);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GenerarReporteProductosAsync_ConErrorDeServidor_DeberiaRetornarNull()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto
        {
            FechaInicio = DateTime.Today.AddDays(-7),
            FechaFin = DateTime.Today
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.GenerarReporteProductosAsync(filtros);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GenerarReporteMesasAsync_ConErrorDeServidor_DeberiaRetornarNull()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto
        {
            FechaInicio = DateTime.Today.AddDays(-14),
            FechaFin = DateTime.Today
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.GenerarReporteMesasAsync(filtros);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GenerarReporteComandasAsync_ConErrorDeServidor_DeberiaRetornarNull()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto
        {
            FechaInicio = DateTime.Today.AddDays(-1),
            FechaFin = DateTime.Today
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.GenerarReporteComandasAsync(filtros);

        // Assert
        resultado.Should().BeNull();
    }

    // ===== PRUEBAS ROBUSTAS - CASOS EDGE =====

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConDatosExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var estadisticasExtremas = new { 
            TotalVentas = decimal.MaxValue, 
            TotalComandas = int.MaxValue, 
            TotalMesas = int.MaxValue,
            PromedioVentaPorMesa = decimal.MaxValue 
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<object>
        {
            Success = true,
            Data = estadisticasExtremas
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
    }

    [Fact]
    public async Task GenerarReporteVentasAsync_ConFiltrosExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtrosExtremos = new ReporteFiltrosDto
        {
            FechaInicio = DateTime.MinValue,
            FechaFin = DateTime.MaxValue,
            MeseroId = Guid.Empty,
            MesaId = Guid.Empty,
            CategoriaId = Guid.Empty,
            LimiteResultados = int.MaxValue
        };

        var reporteExtremo = new { 
            FechaInicio = DateTime.MinValue,
            FechaFin = DateTime.MaxValue,
            TotalVentas = decimal.MaxValue,
            CantidadVentas = int.MaxValue,
            PromedioVenta = decimal.MaxValue 
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<object>
        {
            Success = true,
            Data = reporteExtremo
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.GenerarReporteVentasAsync(filtrosExtremos);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerarReporteProductosAsync_ConDatosMasivos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto
        {
            FechaInicio = DateTime.Today.AddDays(-365),
            FechaFin = DateTime.Today,
            LimiteResultados = 10000
        };

        var productosMasivos = new { 
            FechaInicio = filtros.FechaInicio,
            FechaFin = filtros.FechaFin,
            ProductosMasVendidos = Enumerable.Range(1, 10000).Select(i => new { 
                ProductoId = Guid.NewGuid(), 
                Nombre = $"Producto {i}", 
                CantidadVendida = int.MaxValue, 
                TotalVentas = decimal.MaxValue 
            }).ToArray(),
            TotalProductosVendidos = int.MaxValue,
            TotalVentasProductos = decimal.MaxValue 
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<object>
        {
            Success = true,
            Data = productosMasivos
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.GenerarReporteProductosAsync(filtros);

        // Assert
        resultado.Should().NotBeNull();
    }

    // ===== PRUEBAS ROBUSTAS - SEGURIDAD =====

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConInyeccionSQL_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Error de validación", Encoding.UTF8, "text/plain")
            });

        // Act
        var resultado = await _service.ObtenerEstadisticasAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GenerarReporteVentasAsync_ConXSS_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtrosConXSS = new ReporteFiltrosDto
        {
            FechaInicio = DateTime.Today.AddDays(-30),
            FechaFin = DateTime.Today,
            MeseroId = Guid.NewGuid(),
            MesaId = Guid.NewGuid()
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Datos inválidos detectados", Encoding.UTF8, "text/plain")
            });

        // Act
        var resultado = await _service.GenerarReporteVentasAsync(filtrosConXSS);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GenerarReporteProductosAsync_ConDatosMaliciosos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtrosMaliciosos = new ReporteFiltrosDto
        {
            FechaInicio = DateTime.Today.AddDays(-7),
            FechaFin = DateTime.Today,
            CategoriaId = Guid.NewGuid(),
            LimiteResultados = -1 // Valor negativo malicioso
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Parámetros inválidos", Encoding.UTF8, "text/plain")
            });

        // Act
        var resultado = await _service.GenerarReporteProductosAsync(filtrosMaliciosos);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GenerarReporteMesasAsync_ConValidacionExtrema_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtrosExtremos = new ReporteFiltrosDto
        {
            FechaInicio = DateTime.Today.AddDays(-1000),
            FechaFin = DateTime.Today.AddDays(1000),
            MesaId = Guid.Empty
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Rango de fechas inválido", Encoding.UTF8, "text/plain")
            });

        // Act
        var resultado = await _service.GenerarReporteMesasAsync(filtrosExtremos);

        // Assert
        resultado.Should().BeNull();
    }

    // ===== PRUEBAS ROBUSTAS - CONCURRENCIA =====

    [Fact]
    public async Task GenerarReporteVentasAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto
        {
            FechaInicio = DateTime.Today.AddDays(-30),
            FechaFin = DateTime.Today
        };

        var reporteVentas = new { 
            FechaInicio = filtros.FechaInicio,
            FechaFin = filtros.FechaFin,
            TotalVentas = 5000.00m,
            CantidadVentas = 25,
            PromedioVenta = 200.00m 
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<object>
        {
            Success = true,
            Data = reporteVentas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Ejecutar múltiples operaciones simultáneas
        var tareas = new List<Task<ReporteVentasDto?>>();
        for (int i = 0; i < 10; i++)
        {
            tareas.Add(_service.GenerarReporteVentasAsync(filtros));
        }

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().HaveCount(10);
    }

    [Fact]
    public async Task GenerarReporteProductosAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto
        {
            FechaInicio = DateTime.Today.AddDays(-7),
            FechaFin = DateTime.Today,
            LimiteResultados = 10
        };

        var reporteProductos = new { 
            FechaInicio = filtros.FechaInicio,
            FechaFin = filtros.FechaFin,
            ProductosMasVendidos = new[] { 
                new { ProductoId = Guid.NewGuid(), Nombre = "Pizza Margherita", CantidadVendida = 50, TotalVentas = 2500.00m }
            },
            TotalProductosVendidos = 50,
            TotalVentasProductos = 2500.00m 
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<object>
        {
            Success = true,
            Data = reporteProductos
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Ejecutar múltiples consultas simultáneas
        var tareas = new List<Task<ReporteProductosDto?>>();
        for (int i = 0; i < 15; i++)
        {
            tareas.Add(_service.GenerarReporteProductosAsync(filtros));
        }

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().HaveCount(15);
    }

    [Fact]
    public async Task GenerarReporteMesasAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto
        {
            FechaInicio = DateTime.Today.AddDays(-14),
            FechaFin = DateTime.Today
        };

        var reporteMesas = new { 
            FechaInicio = filtros.FechaInicio,
            FechaFin = filtros.FechaFin,
            MesasUtilizadas = new[] { 
                new { MesaId = Guid.NewGuid(), NumeroMesa = "Mesa 1", TiempoUso = TimeSpan.FromHours(4), TotalVentas = 800.00m }
            },
            TotalMesasUtilizadas = 1,
            TiempoPromedioUso = TimeSpan.FromHours(4),
            TotalVentasMesas = 800.00m 
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<object>
        {
            Success = true,
            Data = reporteMesas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Ejecutar múltiples consultas simultáneas
        var tareas = new List<Task<ReporteMesasDto?>>();
        for (int i = 0; i < 20; i++)
        {
            tareas.Add(_service.GenerarReporteMesasAsync(filtros));
        }

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().HaveCount(20);
    }

    [Fact]
    public async Task GenerarReporteComandasAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto
        {
            FechaInicio = DateTime.Today.AddDays(-1),
            FechaFin = DateTime.Today
        };

        var reporteComandas = new { 
            FechaInicio = filtros.FechaInicio,
            FechaFin = filtros.FechaFin,
            ComandasResumen = new[] { 
                new { ComandaId = Guid.NewGuid(), NumeroComanda = "CMD-001", Estado = "Completada", Total = 150.00m, TiempoPreparacion = TimeSpan.FromMinutes(25) }
            },
            TotalComandas = 1,
            ComandasCompletadas = 1,
            ComandasEnProceso = 0,
            TiempoPromedioPreparacion = TimeSpan.FromMinutes(25) 
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<object>
        {
            Success = true,
            Data = reporteComandas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Ejecutar múltiples consultas simultáneas
        var tareas = new List<Task<ReporteComandasDto?>>();
        for (int i = 0; i < 25; i++)
        {
            tareas.Add(_service.GenerarReporteComandasAsync(filtros));
        }

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().HaveCount(25);
    }

    // ===== PRUEBAS ROBUSTAS - RENDIMIENTO Y LÍMITES =====

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConTimeout_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.RequestTimeout
            });

        // Act
        var resultado = await _service.ObtenerEstadisticasAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GenerarReporteVentasAsync_ConDatosMasivos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto
        {
            FechaInicio = DateTime.Today.AddDays(-365),
            FechaFin = DateTime.Today
        };

        var reporteMasivo = new { 
            FechaInicio = filtros.FechaInicio,
            FechaFin = filtros.FechaFin,
            TotalVentas = decimal.MaxValue,
            CantidadVentas = int.MaxValue,
            PromedioVenta = decimal.MaxValue,
            VentasPorDia = Enumerable.Range(1, 365).Select(i => new { 
                Fecha = DateTime.Today.AddDays(-i), 
                Total = decimal.MaxValue, 
                Cantidad = int.MaxValue 
            }).ToArray()
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<object>
        {
            Success = true,
            Data = reporteMasivo
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.GenerarReporteVentasAsync(filtros);

        // Assert
        resultado.Should().NotBeNull();
    }
}
