using System.Net;
using System.Text;

namespace RestaurantePro.Web.Admin.UnitTests.Services;

public class DashboardApiServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<TokenStore> _tokenStoreMock;
    private readonly DashboardApiService _service;

    public DashboardApiServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _tokenStoreMock = new Mock<TokenStore>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri("http://localhost:8080");
        
        _httpClientFactoryMock.Setup(x => x.CreateClient("Api"))
            .Returns(httpClient);

        _service = new DashboardApiService(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    [Fact]
    public async Task ObtenerResumenAsync_ConRespuestaExitosa_DeberiaRetornarResumen()
    {
        // Arrange
        var resumenEsperado = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto
            {
                VentasHoy = 1500.00m,
                VentasAyer = 1200.00m,
                MesasOcupadas = 8,
                TotalMesas = 20,
                ComandasActivas = 12
            }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<DashboardResumenDto>
        {
            Success = true,
            Data = resumenEsperado
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerResumenAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Metricas.VentasHoy.Should().Be(1500.00m);
        resultado.Metricas.MesasOcupadas.Should().Be(8);
        resultado.Metricas.TotalMesas.Should().Be(20);
    }

    [Fact]
    public async Task ObtenerResumenAsync_ConErrorEnApi_DeberiaRetornarDatosEjemplo()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Error interno", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerResumenAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Metricas.Should().NotBeNull();
        resultado.Metricas.VentasHoy.Should().Be(1250.50m); // Valor de ejemplo
        resultado.Metricas.TotalMesas.Should().Be(20); // Valor de ejemplo
    }

    [Fact]
    public async Task ObtenerMetricasAsync_ConRespuestaExitosa_DeberiaRetornarMetricas()
    {
        // Arrange
        var metricasEsperadas = new DashboardMetricasDto
        {
            VentasHoy = 2000.00m,
            VentasAyer = 1800.00m,
            VentasSemana = 12000.00m,
            VentasMes = 45000.00m,
            MesasOcupadas = 10,
            MesasDisponibles = 10,
            TotalMesas = 20,
            ComandasActivas = 20,
            ComandasCompletadas = 50,
            ProductosVendidosHoy = 100,
            ClientesAtendidosHoy = 40,
            PromedioTicket = 50.00m,
            CrecimientoVentas = 10.0m,
            UltimaActualizacion = DateTime.Now
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<DashboardMetricasDto>
        {
            Success = true,
            Data = metricasEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerMetricasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.VentasHoy.Should().Be(2000.00m);
        resultado.VentasAyer.Should().Be(1800.00m);
        resultado.MesasOcupadas.Should().Be(10);
        resultado.TotalMesas.Should().Be(20);
        resultado.ComandasActivas.Should().Be(20);
        resultado.CrecimientoVentas.Should().Be(10.0m);
    }

    [Fact]
    public async Task ObtenerMetricasAsync_ConErrorEnApi_DeberiaRetornarDatosEjemplo()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Error interno", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerMetricasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.VentasHoy.Should().Be(1250.50m); // Valor de ejemplo
        resultado.TotalMesas.Should().Be(20); // Valor de ejemplo
        resultado.ComandasActivas.Should().Be(15); // Valor de ejemplo
    }

    [Fact]
    public async Task ObtenerProductosMasVendidosAsync_ConRespuestaExitosa_DeberiaRetornarProductos()
    {
        // Arrange
        var productosEsperados = new List<ProductoMasVendidoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Pizza Margherita", CategoriaNombre = "Pizzas", CantidadVendida = 30, Ingresos = 750.00m, PorcentajeTotal = 20.0m },
            new() { Id = Guid.NewGuid(), Nombre = "Hamburguesa Clásica", CategoriaNombre = "Hamburguesas", CantidadVendida = 25, Ingresos = 625.00m, PorcentajeTotal = 16.7m }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<ProductoMasVendidoDto>>
        {
            Success = true,
            Data = productosEsperados
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerProductosMasVendidosAsync(5);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(2);
        resultado.First().Nombre.Should().Be("Pizza Margherita");
        resultado.First().CantidadVendida.Should().Be(30);
        resultado.First().Ingresos.Should().Be(750.00m);
    }

    [Fact]
    public async Task ObtenerProductosMasVendidosAsync_ConErrorEnApi_DeberiaRetornarDatosEjemplo()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Error interno", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerProductosMasVendidosAsync(5);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(5); // Datos de ejemplo
        resultado.First().Nombre.Should().Be("Pizza Margherita"); // Primer producto de ejemplo
    }

    [Fact]
    public async Task ObtenerVentasUltimos7DiasAsync_ConRespuestaExitosa_DeberiaRetornarVentas()
    {
        // Arrange
        var ventasEsperadas = new List<VentaPorPeriodoDto>
        {
            new() { Fecha = DateTime.Today.AddDays(-1), Monto = 1200.00m, CantidadComandas = 20, CantidadProductos = 60 },
            new() { Fecha = DateTime.Today, Monto = 1500.00m, CantidadComandas = 25, CantidadProductos = 75 }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<VentaPorPeriodoDto>>
        {
            Success = true,
            Data = ventasEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerVentasUltimos7DiasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(2);
        resultado.First().Monto.Should().Be(1200.00m);
        resultado.Last().Monto.Should().Be(1500.00m);
    }

    [Fact]
    public async Task ObtenerEstadoMesasAsync_ConRespuestaExitosa_DeberiaRetornarEstado()
    {
        // Arrange
        var estadoEsperado = new EstadoMesasDto
        {
            Disponibles = 15,
            Ocupadas = 3,
            Reservadas = 2,
            EnLimpieza = 0,
            Total = 20
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<EstadoMesasDto>
        {
            Success = true,
            Data = estadoEsperado
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerEstadoMesasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Disponibles.Should().Be(15);
        resultado.Ocupadas.Should().Be(3);
        resultado.Reservadas.Should().Be(2);
        resultado.Total.Should().Be(20);
    }

    [Fact]
    public async Task ObtenerComandasPorEstadoAsync_ConRespuestaExitosa_DeberiaRetornarComandas()
    {
        // Arrange
        var comandasEsperadas = new ComandasPorEstadoDto
        {
            Pendientes = 3,
            EnPreparacion = 5,
            Listas = 2,
            Completadas = 30,
            Canceladas = 1,
            Total = 41
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ComandasPorEstadoDto>
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
        var resultado = await _service.ObtenerComandasPorEstadoAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Pendientes.Should().Be(3);
        resultado.EnPreparacion.Should().Be(5);
        resultado.Listas.Should().Be(2);
        resultado.Completadas.Should().Be(30);
        resultado.Total.Should().Be(41);
    }

    [Fact]
    public async Task ObtenerIngresosPorHoraAsync_ConRespuestaExitosa_DeberiaRetornarIngresos()
    {
        // Arrange
        var ingresosEsperados = new List<IngresosPorHoraDto>
        {
            new() { Hora = 12, Monto = 150.00m, CantidadComandas = 5 },
            new() { Hora = 13, Monto = 200.00m, CantidadComandas = 8 },
            new() { Hora = 14, Monto = 180.00m, CantidadComandas = 6 }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<IngresosPorHoraDto>>
        {
            Success = true,
            Data = ingresosEsperados
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerIngresosPorHoraAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(3);
        resultado.First().Hora.Should().Be(12);
        resultado.First().Monto.Should().Be(150.00m);
        resultado.First().CantidadComandas.Should().Be(5);
    }
}
