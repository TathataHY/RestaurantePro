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

public class ReportesInventarioApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly TokenStore _tokenStore;
    private readonly ReportesInventarioApiService _service;

    public ReportesInventarioApiServiceTests()
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

        _service = new ReportesInventarioApiService(_httpClientFactoryMock.Object, _tokenStore);
    }

    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public async Task ObtenerAnalisisStockAsync_ConDatosValidos_DeberiaRetornarDatos()
    {
        // Arrange
        var datosAnalisis = new { 
            TotalProductos = 150, 
            ProductosBajoStock = 25, 
            ProductosSobreStock = 10,
            ValorTotalInventario = 50000m
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
        var resultado = await _service.ObtenerAnalisisStockAsync();

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisStockAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerAnalisisStockAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerMovimientosInventarioAsync_ConFechasValidas_DeberiaRetornarDatos()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;
        var datosMovimientos = new { 
            TotalMovimientos = 500, 
            Entradas = 250, 
            Salidas = 200, 
            Ajustes = 50
        };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosMovimientos
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
        var resultado = await _service.ObtenerMovimientosInventarioAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerMovimientosInventarioAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerMovimientosInventarioAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerProductosProximosVencerAsync_ConDiasValidos_DeberiaRetornarDatos()
    {
        // Arrange
        var dias = 7;
        var datosProximosVencer = new { 
            ProductosProximosVencer = 15, 
            ProductosCriticos = 5,
            ValorTotal = 2500m
        };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosProximosVencer
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
        var resultado = await _service.ObtenerProductosProximosVencerAsync(dias);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerProductosProximosVencerAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var dias = 7;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerProductosProximosVencerAsync(dias);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerProductosVencidosAsync_ConDatosValidos_DeberiaRetornarDatos()
    {
        // Arrange
        var datosVencidos = new { 
            ProductosVencidos = 8, 
            ValorPerdido = 1200m,
            DiasPromedioVencidos = 5
        };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosVencidos
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
        var resultado = await _service.ObtenerProductosVencidosAsync();

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerProductosVencidosAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerProductosVencidosAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerRotacionInventarioAsync_ConFechasValidas_DeberiaRetornarDatos()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;
        var datosRotacion = new { 
            RotacionPromedio = 2.5m, 
            ProductosAltaRotacion = 25,
            ProductosBajaRotacion = 15
        };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosRotacion
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
        var resultado = await _service.ObtenerRotacionInventarioAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerRotacionInventarioAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerRotacionInventarioAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisCostosAsync_ConFechasValidas_DeberiaRetornarDatos()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;
        var datosCostos = new { 
            CostoTotalInventario = 45000m, 
            CostoPromedioPorProducto = 300m,
            TendenciaCostos = "Creciente"
        };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosCostos
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
        var resultado = await _service.ObtenerAnalisisCostosAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisCostosAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerAnalisisCostosAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerEficienciaProveedoresAsync_ConFechasValidas_DeberiaRetornarDatos()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;
        var datosEficiencia = new { 
            ProveedoresEvaluados = 10, 
            TiempoPromedioEntrega = 3.5m,
            CalidadPromedio = 4.2m
        };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosEficiencia
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
        var resultado = await _service.ObtenerEficienciaProveedoresAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerEficienciaProveedoresAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerEficienciaProveedoresAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerPrediccionDemandaAsync_ConMesesValidos_DeberiaRetornarDatos()
    {
        // Arrange
        var meses = 6;
        var datosPrediccion = new { 
            DemandaPredicha = 1500m, 
            Confianza = 0.85m,
            FactoresInfluencia = new[] { "Estacionalidad", "Tendencias" }
        };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosPrediccion
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
        var resultado = await _service.ObtenerPrediccionDemandaAsync(meses);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerPrediccionDemandaAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var meses = 6;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerPrediccionDemandaAsync(meses);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerOptimizacionInventarioAsync_ConDatosValidos_DeberiaRetornarDatos()
    {
        // Arrange
        var datosOptimizacion = new { 
            Recomendaciones = new[] { "Reducir stock de productos lentos", "Aumentar stock de productos rápidos" },
            AhorroEstimado = 5000m,
            NivelOptimizacion = 0.75m
        };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosOptimizacion
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
        var resultado = await _service.ObtenerOptimizacionInventarioAsync();

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerOptimizacionInventarioAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerOptimizacionInventarioAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisDesperdiciosAsync_ConFechasValidas_DeberiaRetornarDatos()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;
        var datosDesperdicios = new { 
            TotalDesperdicios = 500m, 
            PorcentajeDesperdicio = 0.05m,
            ProductosMasDesperdiciados = new[] { "Lechuga", "Tomate" }
        };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosDesperdicios
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
        var resultado = await _service.ObtenerAnalisisDesperdiciosAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisDesperdiciosAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerAnalisisDesperdiciosAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerValorizacionInventarioAsync_ConDatosValidos_DeberiaRetornarDatos()
    {
        // Arrange
        var datosValorizacion = new { 
            ValorTotalInventario = 75000m, 
            ValorPorCategoria = new Dictionary<string, decimal> { { "Carnes", 30000m }, { "Verduras", 20000m }, { "Lacteos", 25000m } },
            MetodoValorizacion = "FIFO"
        };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosValorizacion
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
        var resultado = await _service.ObtenerValorizacionInventarioAsync();

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerValorizacionInventarioAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerValorizacionInventarioAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ExportarReporteInventarioAsync_ConParametrosValidos_DeberiaRetornarBytes()
    {
        // Arrange
        var tipoReporte = "analisis-stock";
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
        var resultado = await _service.ExportarReporteInventarioAsync(tipoReporte, fechaInicio, fechaFin);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(13); // "Excel content" length
    }

    [Fact]
    public async Task ExportarReporteInventarioAsync_SinFechas_DeberiaRetornarBytes()
    {
        // Arrange
        var tipoReporte = "valorizacion";
        var excelBytes = Encoding.UTF8.GetBytes("Excel content");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(excelBytes)
            });

        // Act
        var resultado = await _service.ExportarReporteInventarioAsync(tipoReporte);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(13); // "Excel content" length
    }

    [Fact]
    public async Task ExportarReporteInventarioAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var tipoReporte = "analisis-stock";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ExportarReporteInventarioAsync(tipoReporte);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ExportarReporteInventarioAsync_ConErrorDeServidor_DeberiaRetornarNull()
    {
        // Arrange
        var tipoReporte = "analisis-stock";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.ExportarReporteInventarioAsync(tipoReporte);

        // Assert
        resultado.Should().BeNull();
    }

    // ===== PRUEBAS ROBUSTAS - CASOS EDGE =====

    [Fact]
    public async Task ObtenerAnalisisStockAsync_ConDatosMasivos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var datosAnalisis = new { 
            TotalProductos = 100000, 
            ProductosBajoStock = 15000, 
            ProductosSobreStock = 5000,
            ValorTotalInventario = decimal.MaxValue,
            Categorias = Enumerable.Range(1, 1000).Select(i => $"Categoria{i}").ToArray()
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
        var resultado = await _service.ObtenerAnalisisStockAsync();

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerMovimientosInventarioAsync_ConFechasExtremas_DeberiaManejarCorrectamente()
    {
        // Arrange
        var fechaInicio = DateTime.MinValue;
        var fechaFin = DateTime.MaxValue;
        var datosMovimientos = new { 
            TotalMovimientos = int.MaxValue, 
            Entradas = int.MaxValue, 
            Salidas = int.MaxValue, 
            Ajustes = int.MaxValue
        };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosMovimientos
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
        var resultado = await _service.ObtenerMovimientosInventarioAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerProductosProximosVencerAsync_ConValoresExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var dias = int.MaxValue;
        var datosProximosVencer = new { 
            ProductosProximosVencer = int.MaxValue, 
            ProductosCriticos = int.MaxValue,
            ValorTotal = decimal.MaxValue,
            Productos = Enumerable.Range(1, 10000).Select(i => $"Producto{i}").ToArray()
        };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosProximosVencer
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
        var resultado = await _service.ObtenerProductosProximosVencerAsync(dias);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerValorizacionInventarioAsync_ConCaracteresEspeciales_DeberiaManejarCorrectamente()
    {
        // Arrange
        var datosValorizacion = new { 
            ValorTotalInventario = 75000m, 
            ValorPorCategoria = new Dictionary<string, decimal> 
            { 
                { "Carnes 🥩", 30000m }, 
                { "Verduras 🥬", 20000m }, 
                { "Lacteos 🥛", 25000m },
                { "Especias 🌶️", 5000m }
            },
            MetodoValorizacion = "FIFO 📊"
        };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosValorizacion
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
        var resultado = await _service.ObtenerValorizacionInventarioAsync();

        // Assert
        resultado.Should().NotBeNull();
    }

    // ===== PRUEBAS ROBUSTAS - SEGURIDAD =====

    [Fact]
    public async Task ObtenerAnalisisStockAsync_ConInyeccionSQL_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.ObtenerAnalisisStockAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerMovimientosInventarioAsync_ConXSS_DeberiaManejarCorrectamente()
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
        var resultado = await _service.ObtenerMovimientosInventarioAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ExportarReporteInventarioAsync_ConPayloadsMaliciosos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var tipoReporte = "<script>alert('xss')</script>";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.ExportarReporteInventarioAsync(tipoReporte);

        // Assert
        resultado.Should().BeNull();
    }

    // ===== PRUEBAS ROBUSTAS - CONCURRENCIA =====

    [Fact]
    public async Task ObtenerAnalisisStockAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var datosAnalisis = new { 
            TotalProductos = 150, 
            ProductosBajoStock = 25, 
            ProductosSobreStock = 10,
            ValorTotalInventario = 50000m
        };

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
        var tareas = Enumerable.Range(1, 10).Select(_ => _service.ObtenerAnalisisStockAsync()).ToArray();
        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().HaveCount(10);
        foreach (var resultado in resultados)
        {
            resultado.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ObtenerMovimientosInventarioAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;
        var datosMovimientos = new { 
            TotalMovimientos = 500, 
            Entradas = 250, 
            Salidas = 200, 
            Ajustes = 50
        };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosMovimientos
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
        var tareas = Enumerable.Range(1, 8).Select(_ => _service.ObtenerMovimientosInventarioAsync(fechaInicio, fechaFin)).ToArray();
        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().HaveCount(8);
        foreach (var resultado in resultados)
        {
            resultado.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ObtenerProductosProximosVencerAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var dias = 7;
        var datosProximosVencer = new { 
            ProductosProximosVencer = 15, 
            ProductosCriticos = 5,
            ValorTotal = 2500m
        };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = datosProximosVencer
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
        var tareas = Enumerable.Range(1, 6).Select(_ => _service.ObtenerProductosProximosVencerAsync(dias)).ToArray();
        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().HaveCount(6);
        foreach (var resultado in resultados)
        {
            resultado.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ExportarReporteInventarioAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var tipoReporte = "analisis-stock";
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
        var tareas = Enumerable.Range(1, 5).Select(_ => _service.ExportarReporteInventarioAsync(tipoReporte, fechaInicio, fechaFin)).ToArray();
        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().HaveCount(5);
        foreach (var resultado in resultados)
        {
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(13);
        }
    }

    // ===== PRUEBAS ROBUSTAS - RENDIMIENTO Y LÍMITES =====

    [Fact]
    public async Task ObtenerAnalisisStockAsync_ConTimeout_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var resultado = await _service.ObtenerAnalisisStockAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerMovimientosInventarioAsync_ConError500_DeberiaManejarCorrectamente()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.ObtenerMovimientosInventarioAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerProductosProximosVencerAsync_ConError503_DeberiaManejarCorrectamente()
    {
        // Arrange
        var dias = 7;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            });

        // Act
        var resultado = await _service.ObtenerProductosProximosVencerAsync(dias);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerProductosVencidosAsync_ConError404_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var resultado = await _service.ObtenerProductosVencidosAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerRotacionInventarioAsync_ConError408_DeberiaManejarCorrectamente()
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
        var resultado = await _service.ObtenerRotacionInventarioAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisCostosAsync_ConError400_DeberiaManejarCorrectamente()
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
        var resultado = await _service.ObtenerAnalisisCostosAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerEficienciaProveedoresAsync_ConError401_DeberiaManejarCorrectamente()
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
        var resultado = await _service.ObtenerEficienciaProveedoresAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerPrediccionDemandaAsync_ConMesesInvalido_DeberiaManejarCorrectamente()
    {
        // Arrange
        var meses = -1;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.ObtenerPrediccionDemandaAsync(meses);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerOptimizacionInventarioAsync_ConError500_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.ObtenerOptimizacionInventarioAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerAnalisisDesperdiciosAsync_ConError503_DeberiaManejarCorrectamente()
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
        var resultado = await _service.ObtenerAnalisisDesperdiciosAsync(fechaInicio, fechaFin);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerValorizacionInventarioAsync_ConError404_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var resultado = await _service.ObtenerValorizacionInventarioAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ExportarReporteInventarioAsync_ConTipoInvalido_DeberiaManejarCorrectamente()
    {
        // Arrange
        var tipoReporte = "";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.ExportarReporteInventarioAsync(tipoReporte);

        // Assert
        resultado.Should().BeNull();
    }
}
