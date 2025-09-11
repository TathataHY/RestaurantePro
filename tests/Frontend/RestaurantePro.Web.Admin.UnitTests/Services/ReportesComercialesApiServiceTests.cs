using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Web.Admin.Services;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Services;

public class ReportesComercialesApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly TokenStore _tokenStore;
    private readonly ReportesComercialesApiService _service;

    public ReportesComercialesApiServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _tokenStore = new TokenStore();
        _tokenStore.Token = "test-token";

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.restaurantepro.com/")
        };

        _httpClientFactoryMock
            .Setup(x => x.CreateClient("Api"))
            .Returns(httpClient);

        _service = new ReportesComercialesApiService(_httpClientFactoryMock.Object, _tokenStore);
    }

    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public async Task ObtenerAnalisisClientesAsync_ConFechasValidas_DeberiaRetornarDatos()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;
        var datosAnalisis = new { TotalClientes = 150, ClientesNuevos = 25, ClientesActivos = 120 };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosAnalisis
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
        var resultado = await _service.ObtenerAnalisisClientesAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisClientesAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerAnalisisClientesAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerSegmentacionClientesAsync_ConDatosValidos_DeberiaRetornarDatos()
    {
        // Arrange
        var datosSegmentacion = new { Segmentos = new[] { "VIP", "Regular", "Nuevo" }, TotalSegmentos = 3 };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosSegmentacion
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
        var resultado = await _service.ObtenerSegmentacionClientesAsync();

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerSegmentacionClientesAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerSegmentacionClientesAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisProductosAsync_ConFechasValidas_DeberiaRetornarDatos()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;
        var datosAnalisis = new { ProductosMasVendidos = new[] { "Pizza Margherita", "Pasta Carbonara" }, TotalProductos = 25 };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosAnalisis
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
        var resultado = await _service.ObtenerAnalisisProductosAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisProductosAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerAnalisisProductosAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerRentabilidadProductosAsync_ConFechasValidas_DeberiaRetornarDatos()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;
        var datosRentabilidad = new { ProductosRentables = new[] { "Pizza Margherita", "Pasta Carbonara" }, MargenPromedio = 0.35m };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosRentabilidad
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
        var resultado = await _service.ObtenerRentabilidadProductosAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerRentabilidadProductosAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerRentabilidadProductosAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisPromocionesAsync_ConFechasValidas_DeberiaRetornarDatos()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;
        var datosPromociones = new { PromocionesActivas = 5, DescuentoPromedio = 0.15m, VentasConPromocion = 120 };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosPromociones
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
        var resultado = await _service.ObtenerAnalisisPromocionesAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisPromocionesAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerAnalisisPromocionesAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerTendenciasVentasAsync_ConFechasValidas_DeberiaRetornarDatos()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;
        var datosTendencias = new { Tendencia = "Creciente", CrecimientoPorcentual = 12.5m, Periodo = "Último mes" };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosTendencias
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
        var resultado = await _service.ObtenerTendenciasVentasAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerTendenciasVentasAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerTendenciasVentasAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisCanalesAsync_ConFechasValidas_DeberiaRetornarDatos()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;
        var datosCanales = new { Canales = new[] { "Presencial", "Delivery", "Takeaway" }, VentasPorCanal = new[] { 1000, 800, 600 } };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosCanales
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
        var resultado = await _service.ObtenerAnalisisCanalesAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisCanalesAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerAnalisisCanalesAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisEstacionalidadAsync_ConAñoValido_DeberiaRetornarDatos()
    {
        // Arrange
        var año = 2024;
        var datosEstacionalidad = new { MesesPico = new[] { "Diciembre", "Enero" }, MesesBajos = new[] { "Febrero", "Marzo" } };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosEstacionalidad
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
        var resultado = await _service.ObtenerAnalisisEstacionalidadAsync(año);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisEstacionalidadAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var año = 2024;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerAnalisisEstacionalidadAsync(año);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisCompetenciaAsync_ConDatosValidos_DeberiaRetornarDatos()
    {
        // Arrange
        var datosCompetencia = new { Competidores = new[] { "Restaurante A", "Restaurante B" }, Posicion = 2 };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosCompetencia
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
        var resultado = await _service.ObtenerAnalisisCompetenciaAsync();

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisCompetenciaAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerAnalisisCompetenciaAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerProyeccionesComercialesAsync_ConMesesValidos_DeberiaRetornarDatos()
    {
        // Arrange
        var meses = 6;
        var datosProyecciones = new { ProyeccionVentas = 50000m, CrecimientoEsperado = 0.15m, Meses = meses };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosProyecciones
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
        var resultado = await _service.ObtenerProyeccionesComercialesAsync(meses);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerProyeccionesComercialesAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var meses = 6;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerProyeccionesComercialesAsync(meses);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ExportarReporteComercialAsync_ConParametrosValidos_DeberiaRetornarBytes()
    {
        // Arrange
        var tipoReporte = "analisis-clientes";
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;
        var excelBytes = Encoding.UTF8.GetBytes("Excel content");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(excelBytes)
            });

        // Act
        var resultado = await _service.ExportarReporteComercialAsync(tipoReporte, fechaInicio, fechaFin);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(13); // "Excel content" length
    }

    [Fact]
    public async Task ExportarReporteComercialAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var tipoReporte = "analisis-clientes";
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ExportarReporteComercialAsync(tipoReporte, fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ExportarReporteComercialAsync_ConErrorDeServidor_DeberiaRetornarNull()
    {
        // Arrange
        var tipoReporte = "analisis-clientes";
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.ExportarReporteComercialAsync(tipoReporte, fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }
}
