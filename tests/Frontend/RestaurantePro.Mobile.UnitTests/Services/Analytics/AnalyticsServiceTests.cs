using Moq;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Analytics;
using RestaurantePro.Mobile.Core.Services.Api;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Services.Analytics;

public class AnalyticsServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AnalyticsService _analyticsService;

    public AnalyticsServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _mockAuthService = new Mock<IAuthService>();
        _analyticsService = new AnalyticsService(_mockApiService.Object, _mockAuthService.Object);
    }

    [Fact]
    public async Task ObtenerMetricasDiaAsync_WithValidResponse_ShouldReturnSuccess()
    {
        // Arrange
        var metricas = new MetricasDiaDto
        {
            Fecha = DateTime.Today,
            TotalVentas = 2500.00m,
            TotalComandas = 45,
            TotalProductosVendidos = 120,
            TiempoPromedioPreparacion = 15,
            PorcentajeOcupacionMesas = 75.0m,
            ClientesAtendidos = 38,
            TopProductos = new List<TopProductoDto>()
        };

        _mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");
        var apiResponse = ApiResponse<MetricasDiaDto>.SuccessResponse(metricas);
        _mockApiService.Setup(x => x.GetAsync<MetricasDiaDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _analyticsService.ObtenerMetricasDiaAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2500.00m, result.Data.TotalVentas);
        Assert.Equal(45, result.Data.TotalComandas);
        Assert.Equal(38, result.Data.ClientesAtendidos);
        _mockApiService.Verify(x => x.GetAsync<MetricasDiaDto>("api/analytics/metricas-dia", "test-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerMetricasRangoAsync_WithValidDates_ShouldReturnSuccess()
    {
        // Arrange
        var fechaDesde = new DateTime(2024, 12, 1);
        var fechaHasta = new DateTime(2024, 12, 15);
        var metricas = new MetricasRangoDto
        {
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            TotalVentas = 15000.00m,
            TotalComandas = 280,
            TotalProductosVendidos = 850,
            TiempoPromedioPreparacion = 18,
            PorcentajeOcupacionMesas = 75.0m,
            ClientesAtendidos = 220,
            TopProductos = new List<TopProductoDto>(),
            VentasPorHora = new List<VentasHoraDto>()
        };

        _mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");
        var apiResponse = ApiResponse<MetricasRangoDto>.SuccessResponse(metricas);
        _mockApiService.Setup(x => x.GetAsync<MetricasRangoDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _analyticsService.ObtenerMetricasRangoAsync(fechaDesde, fechaHasta);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(15000.00m, result.Data.TotalVentas);
        Assert.Equal(280, result.Data.TotalComandas);
        _mockApiService.Verify(x => x.GetAsync<MetricasRangoDto>($"api/analytics/metricas-rango?fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}", "test-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerTopProductosAsync_WithValidLimit_ShouldReturnSuccess()
    {
        // Arrange
        var limite = 5;
        var productos = new List<TopProductoDto>
        {
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Hamburguesa Clásica", CantidadVendida = 45, TotalVentas = 675.00m },
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Pizza Margherita", CantidadVendida = 38, TotalVentas = 570.00m },
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Ensalada César", CantidadVendida = 32, TotalVentas = 400.00m },
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Pasta Carbonara", CantidadVendida = 28, TotalVentas = 420.00m },
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Tiramisú", CantidadVendida = 25, TotalVentas = 187.50m }
        };

        _mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");
        var apiResponse = ApiResponse<List<TopProductoDto>>.SuccessResponse(productos);
        _mockApiService.Setup(x => x.GetAsync<List<TopProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _analyticsService.ObtenerTopProductosAsync(limite);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(5, result.Data.Count);
        Assert.Equal("Hamburguesa Clásica", result.Data[0].NombreProducto);
        Assert.Equal(45, result.Data[0].CantidadVendida);
        _mockApiService.Verify(x => x.GetAsync<List<TopProductoDto>>($"api/analytics/top-productos?limite={limite}", "test-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerTopProductosAsync_WithDateRange_ShouldReturnSuccess()
    {
        // Arrange
        var limite = 3;
        var fechaDesde = new DateTime(2024, 12, 1);
        var fechaHasta = new DateTime(2024, 12, 7);
        var productos = new List<TopProductoDto>
        {
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Hamburguesa Clásica", CantidadVendida = 25, TotalVentas = 375.00m },
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Pizza Margherita", CantidadVendida = 20, TotalVentas = 300.00m },
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Ensalada César", CantidadVendida = 18, TotalVentas = 225.00m }
        };

        _mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");
        var apiResponse = ApiResponse<List<TopProductoDto>>.SuccessResponse(productos);
        _mockApiService.Setup(x => x.GetAsync<List<TopProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _analyticsService.ObtenerTopProductosAsync(limite, fechaDesde, fechaHasta);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<List<TopProductoDto>>($"api/analytics/top-productos?limite={limite}&fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}", "test-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerOcupacionMesasAsync_WithValidDate_ShouldReturnSuccess()
    {
        // Arrange
        var fecha = new DateTime(2024, 12, 15);
        var ocupacion = new OcupacionMesasDto
        {
            Fecha = fecha,
            TotalMesas = 20,
            MesasOcupadas = 15,
            MesasLibres = 5,
            PorcentajeOcupacion = 75.0m,
            TiempoPromedioOcupacion = 120.5m
        };

        _mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");
        var apiResponse = ApiResponse<OcupacionMesasDto>.SuccessResponse(ocupacion);
        _mockApiService.Setup(x => x.GetAsync<OcupacionMesasDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _analyticsService.ObtenerOcupacionMesasAsync(fecha);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(20, result.Data.TotalMesas);
        Assert.Equal(15, result.Data.MesasOcupadas);
        Assert.Equal(75.0m, result.Data.PorcentajeOcupacion);
        _mockApiService.Verify(x => x.GetAsync<OcupacionMesasDto>($"api/analytics/ocupacion-mesas?fecha={fecha:yyyy-MM-dd}", "test-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerTiempoPreparacionAsync_WithoutDateRange_ShouldReturnSuccess()
    {
        // Arrange
        var tiempoPreparacion = new TiempoPreparacionDto
        {
            TiempoPromedioMinutos = 18,
            TiempoMinimoMinutos = 8,
            TiempoMaximoMinutos = 45,
            TotalPreparaciones = 150
        };

        _mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");
        var apiResponse = ApiResponse<TiempoPreparacionDto>.SuccessResponse(tiempoPreparacion);
        _mockApiService.Setup(x => x.GetAsync<TiempoPreparacionDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _analyticsService.ObtenerTiempoPreparacionAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(18, result.Data.TiempoPromedioMinutos);
        _mockApiService.Verify(x => x.GetAsync<TiempoPreparacionDto>("api/analytics/tiempo-preparacion", "test-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerTiempoPreparacionAsync_WithDateRange_ShouldReturnSuccess()
    {
        // Arrange
        var fechaDesde = new DateTime(2024, 12, 1);
        var fechaHasta = new DateTime(2024, 12, 7);
        var tiempoPreparacion = new TiempoPreparacionDto
        {
            TiempoPromedioMinutos = 16,
            TiempoMinimoMinutos = 7,
            TiempoMaximoMinutos = 38,
            TotalPreparaciones = 85
        };

        _mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");
        var apiResponse = ApiResponse<TiempoPreparacionDto>.SuccessResponse(tiempoPreparacion);
        _mockApiService.Setup(x => x.GetAsync<TiempoPreparacionDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _analyticsService.ObtenerTiempoPreparacionAsync(fechaDesde, fechaHasta);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(16, result.Data.TiempoPromedioMinutos);
        _mockApiService.Verify(x => x.GetAsync<TiempoPreparacionDto>($"api/analytics/tiempo-preparacion?fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}", "test-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerVentasPorHoraAsync_WithValidDate_ShouldReturnSuccess()
    {
        // Arrange
        var fecha = new DateTime(2024, 12, 15);
        var ventasPorHora = new List<VentasHoraDto>
        {
            new() { Hora = 12, TotalVentas = 450.00m, TotalComandas = 8 },
            new() { Hora = 13, TotalVentas = 850.00m, TotalComandas = 15 },
            new() { Hora = 14, TotalVentas = 650.00m, TotalComandas = 12 },
            new() { Hora = 19, TotalVentas = 750.00m, TotalComandas = 14 },
            new() { Hora = 20, TotalVentas = 950.00m, TotalComandas = 18 },
            new() { Hora = 21, TotalVentas = 600.00m, TotalComandas = 11 }
        };

        _mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");
        var apiResponse = ApiResponse<List<VentasHoraDto>>.SuccessResponse(ventasPorHora);
        _mockApiService.Setup(x => x.GetAsync<List<VentasHoraDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _analyticsService.ObtenerVentasPorHoraAsync(fecha);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(6, result.Data.Count);
        Assert.Equal(20, result.Data[4].Hora); // Hora pico
        Assert.Equal(950.00m, result.Data[4].TotalVentas);
        _mockApiService.Verify(x => x.GetAsync<List<VentasHoraDto>>($"api/analytics/ventas-hora?fecha={fecha:yyyy-MM-dd}", "test-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerMetricasDiaAsync_WithApiError_ShouldReturnFailure()
    {
        // Arrange
        _mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");
        var errorResponse = ApiResponse<MetricasDiaDto>.ErrorResponse(new List<string> { "Error de API" }, "Error de API", 500);
        _mockApiService.Setup(x => x.GetAsync<MetricasDiaDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(errorResponse);

        // Act
        var result = await _analyticsService.ObtenerMetricasDiaAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de API", result.Error);
    }

    [Fact]
    public async Task ObtenerMetricasRangoAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var fechaDesde = new DateTime(2024, 12, 1);
        var fechaHasta = new DateTime(2024, 12, 15);
        
        _mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");
        _mockApiService.Setup(x => x.GetAsync<MetricasRangoDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _analyticsService.ObtenerMetricasRangoAsync(fechaDesde, fechaHasta);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error al obtener métricas del rango", result.Error);
        Assert.Contains("Error al obtener métricas del rango", result.Error);
    }

    [Fact]
    public async Task ObtenerTopProductosAsync_WithEmptyResult_ShouldReturnSuccess()
    {
        // Arrange
        var limite = 5;
        var productos = new List<TopProductoDto>();

        _mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");
        var apiResponse = ApiResponse<List<TopProductoDto>>.SuccessResponse(productos);
        _mockApiService.Setup(x => x.GetAsync<List<TopProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _analyticsService.ObtenerTopProductosAsync(limite);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<TopProductoDto>>($"api/analytics/top-productos?limite={limite}", "test-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerOcupacionMesasAsync_WithNoOcupacion_ShouldReturnSuccess()
    {
        // Arrange
        var fecha = new DateTime(2024, 12, 15);
        var ocupacion = new OcupacionMesasDto
        {
            Fecha = fecha,
            TotalMesas = 20,
            MesasOcupadas = 0,
            MesasLibres = 20,
            PorcentajeOcupacion = 0.0m,
            TiempoPromedioOcupacion = 0.0m
        };

        _mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");
        var apiResponse = ApiResponse<OcupacionMesasDto>.SuccessResponse(ocupacion);
        _mockApiService.Setup(x => x.GetAsync<OcupacionMesasDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _analyticsService.ObtenerOcupacionMesasAsync(fecha);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(0, result.Data.MesasOcupadas);
        Assert.Equal(20, result.Data.MesasLibres);
        Assert.Equal(0.0m, result.Data.PorcentajeOcupacion);
    }

    [Fact]
    public async Task ObtenerVentasPorHoraAsync_WithNoVentas_ShouldReturnSuccess()
    {
        // Arrange
        var fecha = new DateTime(2024, 12, 15);
        var ventasPorHora = new List<VentasHoraDto>();

        _mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");
        var apiResponse = ApiResponse<List<VentasHoraDto>>.SuccessResponse(ventasPorHora);
        _mockApiService.Setup(x => x.GetAsync<List<VentasHoraDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _analyticsService.ObtenerVentasPorHoraAsync(fecha);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<VentasHoraDto>>($"api/analytics/ventas-hora?fecha={fecha:yyyy-MM-dd}", "test-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    // Tests para 401/403/429
    [Fact]
    public async Task ObtenerMetricasDiaAsync_WithUnauthorized_ShouldPropagate401()
    {
        // Arrange
        var apiResponse = ApiResponse<MetricasDiaDto>.ErrorResponse(new List<string> { "Unauthorized" }, "Unauthorized", 401);
        _mockApiService.Setup(x => x.GetAsync<MetricasDiaDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _analyticsService.ObtenerMetricasDiaAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(401, result.StatusCode);
        Assert.Contains("Unauthorized", result.Error);
    }

    [Fact]
    public async Task ObtenerMetricasDiaAsync_WithForbidden_ShouldPropagate403()
    {
        // Arrange
        var apiResponse = ApiResponse<MetricasDiaDto>.ErrorResponse(new List<string> { "Forbidden" }, "Forbidden", 403);
        _mockApiService.Setup(x => x.GetAsync<MetricasDiaDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _analyticsService.ObtenerMetricasDiaAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Forbidden", result.Error);
    }

    [Fact]
    public async Task ObtenerMetricasDiaAsync_WithTooManyRequests_ShouldPropagate429()
    {
        // Arrange
        var apiResponse = ApiResponse<MetricasDiaDto>.ErrorResponse(new List<string> { "Too Many Requests" }, "Too Many Requests", 429);
        _mockApiService.Setup(x => x.GetAsync<MetricasDiaDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _analyticsService.ObtenerMetricasDiaAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(429, result.StatusCode);
        Assert.Contains("Too Many Requests", result.Error);
    }

    // Tests para 204/empty body
    [Fact]
    public async Task ObtenerTopProductosAsync_WithEmptyBody_ShouldReturnEmptyList()
    {
        // Arrange
        var apiResponse = ApiResponse<List<TopProductoDto>>.SuccessResponse(new List<TopProductoDto>());
        _mockApiService.Setup(x => x.GetAsync<List<TopProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _analyticsService.ObtenerTopProductosAsync(10);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    // Tests para cancelación
    [Fact]
    public async Task ObtenerMetricasDiaAsync_WhenCancelled_ShouldReturnCancelled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel the token immediately

        // Act
        var result = await _analyticsService.ObtenerMetricasDiaAsync(cts.Token);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Operación cancelada por el usuario", result.Error);
        _mockApiService.Verify(x => x.GetAsync<MetricasDiaDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerTopProductosAsync_WhenCancelled_ShouldReturnCancelled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel the token immediately

        // Act
        var result = await _analyticsService.ObtenerTopProductosAsync(10, cancellationToken: cts.Token);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Operación cancelada por el usuario", result.Error);
        _mockApiService.Verify(x => x.GetAsync<List<TopProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
} 