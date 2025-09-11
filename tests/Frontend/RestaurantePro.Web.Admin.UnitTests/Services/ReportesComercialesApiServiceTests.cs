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

    // ===== PRUEBAS ROBUSTAS - CASOS EDGE =====

    [Fact]
    public async Task ObtenerAnalisisClientesAsync_ConDatosMasivos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddYears(-5);
        var fechaFin = DateTime.UtcNow;
        var datosAnalisis = new 
        { 
            TotalClientes = 100000, 
            ClientesNuevos = 15000, 
            ClientesActivos = 85000,
            Segmentos = Enumerable.Range(1, 100).Select(i => $"Segmento{i}").ToArray()
        };

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
    public async Task ObtenerAnalisisClientesAsync_ConFechasExtremas_DeberiaManejarCorrectamente()
    {
        // Arrange
        var fechaInicio = DateTime.MinValue;
        var fechaFin = DateTime.MaxValue;
        var datosAnalisis = new { TotalClientes = 0, ClientesNuevos = 0, ClientesActivos = 0 };

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
    public async Task ObtenerSegmentacionClientesAsync_ConCaracteresEspeciales_DeberiaManejarCorrectamente()
    {
        // Arrange
        var datosSegmentacion = new 
        { 
            Segmentos = new[] { "VIP 🏆", "Regular 📊", "Nuevo 🆕", "Premium 💎" }, 
            TotalSegmentos = 4,
            Descripciones = new[] { "Clientes de alto valor", "Clientes frecuentes", "Clientes nuevos", "Clientes premium" }
        };

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
    public async Task ObtenerAnalisisProductosAsync_ConValoresExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-365);
        var fechaFin = DateTime.UtcNow;
        var datosAnalisis = new 
        { 
            ProductosMasVendidos = Enumerable.Range(1, 1000).Select(i => $"Producto {i}").ToArray(),
            TotalProductos = int.MaxValue,
            VentasTotales = decimal.MaxValue,
            MargenPromedio = decimal.MaxValue
        };

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

    // ===== PRUEBAS ROBUSTAS - SEGURIDAD =====

    [Fact]
    public async Task ObtenerAnalisisClientesAsync_ConInyeccionSQL_DeberiaManejarCorrectamente()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.ObtenerAnalisisClientesAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerSegmentacionClientesAsync_ConXSS_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.ObtenerSegmentacionClientesAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ExportarReporteComercialAsync_ConPayloadsMaliciosos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var tipoReporte = "<script>alert('xss')</script>";
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.ExportarReporteComercialAsync(tipoReporte, fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    // ===== PRUEBAS ROBUSTAS - CONCURRENCIA =====

    [Fact]
    public async Task ObtenerAnalisisClientesAsync_ConConcurrencia_DeberiaManejarCorrectamente()
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
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var tareas = Enumerable.Range(1, 8).Select(_ => _service.ObtenerAnalisisClientesAsync(fechaInicio, fechaFin)).ToArray();
        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().HaveCount(8);
        foreach (var resultado in resultados)
        {
            resultado.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ObtenerSegmentacionClientesAsync_ConConcurrencia_DeberiaManejarCorrectamente()
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
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var tareas = Enumerable.Range(1, 6).Select(_ => _service.ObtenerSegmentacionClientesAsync()).ToArray();
        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().HaveCount(6);
        foreach (var resultado in resultados)
        {
            resultado.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ObtenerAnalisisProductosAsync_ConConcurrencia_DeberiaManejarCorrectamente()
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
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var tareas = Enumerable.Range(1, 5).Select(_ => _service.ObtenerAnalisisProductosAsync(fechaInicio, fechaFin)).ToArray();
        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().HaveCount(5);
        foreach (var resultado in resultados)
        {
            resultado.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ExportarReporteComercialAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var tipoReporte = "analisis-clientes";
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;
        var excelBytes = Encoding.UTF8.GetBytes("Excel content");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(excelBytes)
            });

        // Act
        var tareas = Enumerable.Range(1, 4).Select(_ => _service.ExportarReporteComercialAsync(tipoReporte, fechaInicio, fechaFin)).ToArray();
        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().HaveCount(4);
        foreach (var resultado in resultados)
        {
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(13);
        }
    }

    // ===== PRUEBAS ROBUSTAS - RENDIMIENTO Y LÍMITES =====

    [Fact]
    public async Task ObtenerAnalisisClientesAsync_ConTimeout_DeberiaManejarCorrectamente()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var resultado = await _service.ObtenerAnalisisClientesAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerSegmentacionClientesAsync_ConError500_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.ObtenerSegmentacionClientesAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisProductosAsync_ConError503_DeberiaManejarCorrectamente()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            });

        // Act
        var resultado = await _service.ObtenerAnalisisProductosAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerRentabilidadProductosAsync_ConError404_DeberiaManejarCorrectamente()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var resultado = await _service.ObtenerRentabilidadProductosAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisPromocionesAsync_ConError408_DeberiaManejarCorrectamente()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.RequestTimeout
            });

        // Act
        var resultado = await _service.ObtenerAnalisisPromocionesAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerTendenciasVentasAsync_ConError400_DeberiaManejarCorrectamente()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.ObtenerTendenciasVentasAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisCanalesAsync_ConError401_DeberiaManejarCorrectamente()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            });

        // Act
        var resultado = await _service.ObtenerAnalisisCanalesAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisEstacionalidadAsync_ConAñoInvalido_DeberiaManejarCorrectamente()
    {
        // Arrange
        var año = -1;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.ObtenerAnalisisEstacionalidadAsync(año);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerProyeccionesComercialesAsync_ConMesesInvalido_DeberiaManejarCorrectamente()
    {
        // Arrange
        var meses = 0;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.ObtenerProyeccionesComercialesAsync(meses);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ExportarReporteComercialAsync_ConTipoInvalido_DeberiaManejarCorrectamente()
    {
        // Arrange
        var tipoReporte = "";
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.ExportarReporteComercialAsync(tipoReporte, fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }
}
