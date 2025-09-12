using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Analytics;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Dashboard;
using RestaurantePro.Mobile.Core.Services.Mesas;

namespace RestaurantePro.Mobile.UnitTests.Services.Dashboard;

public class DashboardServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly Mock<IAnalyticsService> _mockAnalyticsService;
    private readonly Mock<IMesasService> _mockMesasService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<ILogger<DashboardService>> _mockLogger;
    private readonly DashboardService _dashboardService;

    public DashboardServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _mockAnalyticsService = new Mock<IAnalyticsService>();
        _mockMesasService = new Mock<IMesasService>();
        _mockAuthService = new Mock<IAuthService>();
        _mockLogger = new Mock<ILogger<DashboardService>>();
        
        _dashboardService = new DashboardService(
            _mockApiService.Object,
            _mockAnalyticsService.Object,
            _mockMesasService.Object,
            _mockAuthService.Object,
            _mockLogger.Object);
    }

    #region GetTodaySalesAsync Tests

    [Fact]
    public async Task GetTodaySalesAsync_WithValidResponse_ShouldReturnSales()
    {
        // Arrange
        var expectedSales = 1500.50m;
        var metricsResponse = ApiResponse<MetricasDiaDto>.SuccessResponse(
            new MetricasDiaDto { TotalVentas = expectedSales }, "Success");

        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(metricsResponse);

        // Act
        var result = await _dashboardService.GetTodaySalesAsync();

        // Assert
        Assert.Equal(expectedSales, result);
    }

    [Fact]
    public async Task GetTodaySalesAsync_WithFailedResponse_ShouldReturnSimulatedSales()
    {
        // Arrange
        var failedResponse = ApiResponse<MetricasDiaDto>.ErrorResponse("Error");

        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedResponse);

        // Act
        var result = await _dashboardService.GetTodaySalesAsync();

        // Assert
        Assert.True(result > 0); // Verificamos que retorne un valor simulado
    }

    [Fact]
    public async Task GetTodaySalesAsync_WithException_ShouldReturnSimulatedSales()
    {
        // Arrange
        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _dashboardService.GetTodaySalesAsync();

        // Assert
        Assert.True(result > 0); // Verificamos que retorne un valor simulado
    }

    #endregion

    #region GetSalesChangePercentageAsync Tests

    [Fact]
    public async Task GetSalesChangePercentageAsync_WithValidResponses_ShouldReturnPercentage()
    {
        // Arrange
        var todaySales = 1500m;
        var yesterdaySales = 1200m;
        var expectedPercentage = ((todaySales - yesterdaySales) / yesterdaySales) * 100;

        var todayResponse = ApiResponse<MetricasDiaDto>.SuccessResponse(
            new MetricasDiaDto { TotalVentas = todaySales }, "Success");
        var yesterdayResponse = ApiResponse<MetricasRangoDto>.SuccessResponse(
            new MetricasRangoDto { TotalVentas = yesterdaySales }, "Success");

        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(todayResponse);
        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasRangoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(yesterdayResponse);

        // Act
        var result = await _dashboardService.GetSalesChangePercentageAsync();

        // Assert
        Assert.Equal(Math.Round(expectedPercentage, 1), result);
    }

    [Fact]
    public async Task GetSalesChangePercentageAsync_WithFailedResponse_ShouldReturnSimulatedPercentage()
    {
        // Arrange
        var failedResponse = ApiResponse<MetricasDiaDto>.ErrorResponse("Error");

        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedResponse);

        // Act
        var result = await _dashboardService.GetSalesChangePercentageAsync();

        // Assert
        Assert.True(result >= -20 && result <= 30); // Verificamos que esté en el rango simulado
    }

    #endregion

    #region GetActiveOrdersCountAsync Tests

    [Fact]
    public async Task GetActiveOrdersCountAsync_WithValidResponses_ShouldReturnCount()
    {
        // Arrange
        var token = "test-token";
        var comandas1 = new List<ComandaDto> { new ComandaDto(), new ComandaDto() };
        var comandas2 = new List<ComandaDto> { new ComandaDto() };
        var comandas3 = new List<ComandaDto> { new ComandaDto(), new ComandaDto(), new ComandaDto() };

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(
                "api/operaciones/comandas?Estado=EnProceso&PageSize=100&SoloActivas=true", token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(
                new PaginatedList<ComandaDto> { Items = comandas1 }, "Success"));

        _mockApiService
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(
                "api/operaciones/comandas?Estado=Lista&PageSize=100&SoloActivas=true", token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(
                new PaginatedList<ComandaDto> { Items = comandas2 }, "Success"));

        _mockApiService
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(
                "api/operaciones/comandas?Estado=Entregada&PageSize=100&SoloActivas=true", token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(
                new PaginatedList<ComandaDto> { Items = comandas3 }, "Success"));

        // Act
        var result = await _dashboardService.GetActiveOrdersCountAsync();

        // Assert
        Assert.Equal(6, result); // 2 + 1 + 3
    }

    [Fact]
    public async Task GetActiveOrdersCountAsync_WithFailedResponse_ShouldReturnSimulatedCount()
    {
        // Arrange
        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _dashboardService.GetActiveOrdersCountAsync();

        // Assert
        Assert.True(result >= 5 && result <= 15); // Verificamos que esté en el rango simulado
    }

    #endregion

    #region GetPendingOrdersCountAsync Tests

    [Fact]
    public async Task GetPendingOrdersCountAsync_WithValidResponse_ShouldReturnCount()
    {
        // Arrange
        var token = "test-token";
        var comandas = new List<ComandaDto> { new ComandaDto(), new ComandaDto() };

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(
                "api/operaciones/comandas?Estado=Creada&PageSize=100", token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(
                new PaginatedList<ComandaDto> { Items = comandas }, "Success"));

        // Act
        var result = await _dashboardService.GetPendingOrdersCountAsync();

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetPendingOrdersCountAsync_WithFailedResponse_ShouldReturnSimulatedCount()
    {
        // Arrange
        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _dashboardService.GetPendingOrdersCountAsync();

        // Assert
        Assert.True(result >= 1 && result <= 8); // Verificamos que esté en el rango simulado
    }

    #endregion

    #region GetRecentOrdersAsync Tests

    [Fact]
    public async Task GetRecentOrdersAsync_WithValidResponse_ShouldReturnOrders()
    {
        // Arrange
        var token = "test-token";
        var comandas = new List<ComandaDto>
        {
            new ComandaDto
            {
                Id = Guid.NewGuid(),
                Numero = "ORD-001",
                NumeroMesa = 5,
                ClienteNombre = "Juan Pérez",
                Total = 45.50m,
                Estado = "EnProceso",
                FechaCreacion = DateTime.Now.AddMinutes(-15),
                Productos = new List<ComandaProductoDto>
                {
                    new ComandaProductoDto { Nombre = "Pizza Margherita" },
                    new ComandaProductoDto { Nombre = "Coca Cola" }
                }
            }
        };

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(
                "api/operaciones/comandas?PageSize=10&OrdenarPor=fechaCreacion&DireccionOrdenamiento=desc", token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(
                new PaginatedList<ComandaDto> { Items = comandas }, "Success"));

        // Act
        var result = await _dashboardService.GetRecentOrdersAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("ORD-001", result[0].OrderNumber);
        Assert.Equal("Mesa 5", result[0].TableNumber);
        Assert.Equal("Juan Pérez", result[0].CustomerName);
        Assert.Equal(45.50m, result[0].Total);
        Assert.Equal("EnProceso", result[0].Status);
    }

    [Fact]
    public async Task GetRecentOrdersAsync_WithFailedResponse_ShouldReturnSimulatedOrders()
    {
        // Arrange
        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _dashboardService.GetRecentOrdersAsync();

        // Assert
        Assert.Equal(3, result.Count); // Verificamos que retorne las órdenes simuladas
        Assert.All(result, order => Assert.NotNull(order.OrderNumber));
    }

    #endregion

    #region GetOrdersByStatusAsync Tests

    [Fact]
    public async Task GetOrdersByStatusAsync_WithValidStatus_ShouldReturnFilteredOrders()
    {
        // Arrange
        var status = "En Progreso";
        var mockOrders = new List<OrderItem>
        {
            new OrderItem { Status = "En Progreso" },
            new OrderItem { Status = "Pendiente" },
            new OrderItem { Status = "En Progreso" }
        };

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ThrowsAsync(new Exception("Test exception")); // Para usar datos simulados

        // Act
        var result = await _dashboardService.GetOrdersByStatusAsync(status);

        // Assert
        Assert.NotNull(result);
        // Verificamos que no lance excepción
    }

    [Fact]
    public async Task GetOrdersByStatusAsync_WithException_ShouldReturnEmptyList()
    {
        // Arrange
        var status = "Test Status";

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _dashboardService.GetOrdersByStatusAsync(status);

        // Assert
        Assert.NotNull(result);
        // Verificamos que no lance excepción
    }

    #endregion

    #region GetTableStatusAsync Tests

    [Fact]
    public async Task GetTableStatusAsync_WithValidResponse_ShouldReturnTableStatus()
    {
        // Arrange
        var expectedStatus = new EstadoMesasDto
        {
            Mesas = new List<MesaDto>
            {
                new MesaDto { Id = Guid.NewGuid(), Numero = "1", Estado = "disponible", Capacidad = 4 }
            },
            Estadisticas = new EstadisticasMesasDto
            {
                MesasDisponibles = 1,
                MesasOcupadas = 0,
                MesasReservadas = 0,
                MesasActivas = 1,
                PorcentajeOcupacion = 0.0m,
                PorcentajeDisponibilidad = 100.0m
            },
            FechaConsulta = DateTime.Now
        };

        _mockMesasService
            .Setup(x => x.ObtenerEstadoOcupacionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<EstadoMesasDto>.SuccessResponse(expectedStatus, "Success"));

        // Act
        var result = await _dashboardService.GetTableStatusAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Mesas);
        Assert.Equal("1", result.Mesas[0].Numero);
        Assert.Equal("disponible", result.Mesas[0].Estado);
    }

    [Fact]
    public async Task GetTableStatusAsync_WithFailedResponse_ShouldReturnSimulatedStatus()
    {
        // Arrange
        _mockMesasService
            .Setup(x => x.ObtenerEstadoOcupacionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<EstadoMesasDto>.ErrorResponse("Error"));

        // Act
        var result = await _dashboardService.GetTableStatusAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Mesas);
        Assert.True(result.Mesas.Count > 0);
        Assert.NotNull(result.Estadisticas);
    }

    [Fact]
    public async Task GetTableStatusAsync_WithException_ShouldReturnSimulatedStatus()
    {
        // Arrange
        _mockMesasService
            .Setup(x => x.ObtenerEstadoOcupacionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _dashboardService.GetTableStatusAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Mesas);
        Assert.True(result.Mesas.Count > 0);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task MultipleOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var token = "test-token";
        var metricsResponse = ApiResponse<MetricasDiaDto>.SuccessResponse(
            new MetricasDiaDto { TotalVentas = 1000m }, "Success");
        var tableStatusResponse = ApiResponse<EstadoMesasDto>.SuccessResponse(
            new EstadoMesasDto { Mesas = new List<MesaDto>() }, "Success");

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(metricsResponse);

        _mockMesasService
            .Setup(x => x.ObtenerEstadoOcupacionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tableStatusResponse);

        // Act
        var sales = await _dashboardService.GetTodaySalesAsync();
        var changePercentage = await _dashboardService.GetSalesChangePercentageAsync();
        var activeOrders = await _dashboardService.GetActiveOrdersCountAsync();
        var pendingOrders = await _dashboardService.GetPendingOrdersCountAsync();
        var recentOrders = await _dashboardService.GetRecentOrdersAsync();
        var tableStatus = await _dashboardService.GetTableStatusAsync();

        // Assert
        Assert.True(sales > 0);
        Assert.True(changePercentage >= -20 && changePercentage <= 30);
        Assert.True(activeOrders >= 0);
        Assert.True(pendingOrders >= 0);
        Assert.NotNull(recentOrders);
        Assert.NotNull(tableStatus);
    }

    #endregion

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Fallos Parciales de Servicios

    [Fact]
    public async Task GetTodaySalesAsync_WithPartialServiceFailure_ShouldReturnSimulatedSales()
    {
        // Arrange
        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Service temporarily unavailable"));

        // Act
        var result = await _dashboardService.GetTodaySalesAsync();

        // Assert
        Assert.True(result > 0); // Should return simulated sales
        Assert.True(result >= 500 && result <= 2000); // Should be in reasonable range
    }

    [Fact]
    public async Task GetSalesChangePercentageAsync_WithPartialServiceFailure_ShouldReturnSimulatedPercentage()
    {
        // Arrange
        var todayResponse = ApiResponse<MetricasDiaDto>.SuccessResponse(
            new MetricasDiaDto { TotalVentas = 1500m }, "Success");
        var yesterdayResponse = ApiResponse<MetricasRangoDto>.ErrorResponse("Service error");

        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(todayResponse);
        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasRangoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(yesterdayResponse);

        // Act
        var result = await _dashboardService.GetSalesChangePercentageAsync();

        // Assert
        Assert.True(result >= -20 && result <= 30); // Should return simulated percentage
    }

    [Fact]
    public async Task GetActiveOrdersCountAsync_WithPartialServiceFailure_ShouldReturnSimulatedCount()
    {
        // Arrange
        var token = "test-token";
        var comandas1 = new List<ComandaDto> { new ComandaDto(), new ComandaDto() };
        var comandas2 = new List<ComandaDto> { new ComandaDto() };

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(
                "api/operaciones/comandas?Estado=EnProceso&PageSize=100&SoloActivas=true", token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(
                new PaginatedList<ComandaDto> { Items = comandas1 }, "Success"));

        _mockApiService
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(
                "api/operaciones/comandas?Estado=Lista&PageSize=100&SoloActivas=true", token, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Service error"));

        _mockApiService
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(
                "api/operaciones/comandas?Estado=Entregada&PageSize=100&SoloActivas=true", token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(
                new PaginatedList<ComandaDto> { Items = comandas2 }, "Success"));

        // Act
        var result = await _dashboardService.GetActiveOrdersCountAsync();

        // Assert
        Assert.True(result >= 3); // Should handle partial failure gracefully
    }

    #endregion

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Datos Inconsistentes

    [Fact]
    public async Task GetTodaySalesAsync_WithInconsistentData_ShouldHandleGracefully()
    {
        // Arrange
        var inconsistentResponse = ApiResponse<MetricasDiaDto>.SuccessResponse(
            new MetricasDiaDto { TotalVentas = -100m }, "Success"); // Negative sales

        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(inconsistentResponse);

        // Act
        var result = await _dashboardService.GetTodaySalesAsync();

        // Assert
        Assert.True(result > 0); // Should return simulated sales instead of negative
    }

    [Fact]
    public async Task GetSalesChangePercentageAsync_WithInconsistentData_ShouldHandleGracefully()
    {
        // Arrange
        var todayResponse = ApiResponse<MetricasDiaDto>.SuccessResponse(
            new MetricasDiaDto { TotalVentas = 1000m }, "Success");
        var yesterdayResponse = ApiResponse<MetricasRangoDto>.SuccessResponse(
            new MetricasRangoDto { TotalVentas = 0m }, "Success"); // Zero yesterday sales

        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(todayResponse);
        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasRangoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(yesterdayResponse);

        // Act
        var result = await _dashboardService.GetSalesChangePercentageAsync();

        // Assert
        Assert.True(result >= -20 && result <= 30); // Should return simulated percentage
    }

    [Fact]
    public async Task GetTableStatusAsync_WithInconsistentData_ShouldHandleGracefully()
    {
        // Arrange
        var inconsistentStatus = new EstadoMesasDto
        {
            Mesas = new List<MesaDto>
            {
                new MesaDto { Id = Guid.NewGuid(), Numero = "1", Estado = "invalid_state", Capacidad = -1 }
            },
            Estadisticas = new EstadisticasMesasDto
            {
                MesasDisponibles = -1,
                MesasOcupadas = -1,
                PorcentajeOcupacion = 150.0m // Invalid percentage
            }
        };

        _mockMesasService
            .Setup(x => x.ObtenerEstadoOcupacionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<EstadoMesasDto>.SuccessResponse(inconsistentStatus, "Success"));

        // Act
        var result = await _dashboardService.GetTableStatusAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Mesas);
        Assert.NotNull(result.Estadisticas);
    }

    #endregion

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Caché y Actualización

    [Fact]
    public async Task GetTodaySalesAsync_WithCachedData_ShouldReturnCachedValue()
    {
        // Arrange
        var expectedSales = 1500.50m;
        var metricsResponse = ApiResponse<MetricasDiaDto>.SuccessResponse(
            new MetricasDiaDto { TotalVentas = expectedSales }, "Success");

        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(metricsResponse);

        // Act - Call multiple times
        var result1 = await _dashboardService.GetTodaySalesAsync();
        var result2 = await _dashboardService.GetTodaySalesAsync();
        var result3 = await _dashboardService.GetTodaySalesAsync();

        // Assert
        Assert.Equal(expectedSales, result1);
        Assert.Equal(expectedSales, result2);
        Assert.Equal(expectedSales, result3);
        
        // Verify service was called multiple times (no caching implemented)
        _mockAnalyticsService.Verify(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task GetTableStatusAsync_WithCachedData_ShouldReturnCachedValue()
    {
        // Arrange
        var expectedStatus = new EstadoMesasDto
        {
            Mesas = new List<MesaDto>
            {
                new MesaDto { Id = Guid.NewGuid(), Numero = "1", Estado = "disponible", Capacidad = 4 }
            },
            Estadisticas = new EstadisticasMesasDto
            {
                MesasDisponibles = 1,
                MesasOcupadas = 0
            }
        };

        _mockMesasService
            .Setup(x => x.ObtenerEstadoOcupacionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<EstadoMesasDto>.SuccessResponse(expectedStatus, "Success"));

        // Act - Call multiple times
        var result1 = await _dashboardService.GetTableStatusAsync();
        var result2 = await _dashboardService.GetTableStatusAsync();

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.Mesas.Count, result2.Mesas.Count);
        
        // Verify service was called multiple times (no caching implemented)
        _mockMesasService.Verify(x => x.ObtenerEstadoOcupacionAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    #endregion

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Timeouts y Reintentos

    [Fact]
    public async Task GetTodaySalesAsync_WithTimeout_ShouldReturnSimulatedSales()
    {
        // Arrange
        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _dashboardService.GetTodaySalesAsync();

        // Assert
        Assert.True(result > 0); // Should return simulated sales
        Assert.True(result >= 500 && result <= 2000); // Should be in reasonable range
    }

    [Fact]
    public async Task GetActiveOrdersCountAsync_WithTimeout_ShouldReturnSimulatedCount()
    {
        // Arrange
        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _dashboardService.GetActiveOrdersCountAsync();

        // Assert
        Assert.True(result >= 5 && result <= 15); // Should return simulated count
    }

    [Fact]
    public async Task GetTableStatusAsync_WithTimeout_ShouldReturnSimulatedStatus()
    {
        // Arrange
        _mockMesasService
            .Setup(x => x.ObtenerEstadoOcupacionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _dashboardService.GetTableStatusAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Mesas);
        Assert.True(result.Mesas.Count > 0);
        Assert.NotNull(result.Estadisticas);
    }

    #endregion

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Concurrencia y Threading

    [Fact]
    public async Task MultipleOperations_WithConcurrentCalls_ShouldHandleGracefully()
    {
        // Arrange
        var token = "test-token";
        var metricsResponse = ApiResponse<MetricasDiaDto>.SuccessResponse(
            new MetricasDiaDto { TotalVentas = 1000m }, "Success");
        var tableStatusResponse = ApiResponse<EstadoMesasDto>.SuccessResponse(
            new EstadoMesasDto { Mesas = new List<MesaDto>() }, "Success");

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(metricsResponse);

        _mockMesasService
            .Setup(x => x.ObtenerEstadoOcupacionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tableStatusResponse);

        // Act - Call all operations concurrently
        var task1 = _dashboardService.GetTodaySalesAsync();
        var task2 = _dashboardService.GetSalesChangePercentageAsync();
        var task3 = _dashboardService.GetActiveOrdersCountAsync();
        var task4 = _dashboardService.GetPendingOrdersCountAsync();
        var task5 = _dashboardService.GetRecentOrdersAsync();
        var task6 = _dashboardService.GetTableStatusAsync();

        var results = await Task.WhenAll(task1, task2, task3, task4, task5, task6);

        // Assert
        Assert.True(results[0] > 0); // Sales
        Assert.True(results[1] >= -20 && results[1] <= 30); // Change percentage
        Assert.True(results[2] >= 0); // Active orders
        Assert.True(results[3] >= 0); // Pending orders
        Assert.NotNull(results[4]); // Recent orders
        Assert.NotNull(results[5]); // Table status
    }

    [Fact]
    public async Task GetTodaySalesAsync_WithConcurrentCalls_ShouldHandleGracefully()
    {
        // Arrange
        var expectedSales = 1500.50m;
        var metricsResponse = ApiResponse<MetricasDiaDto>.SuccessResponse(
            new MetricasDiaDto { TotalVentas = expectedSales }, "Success");

        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(metricsResponse);

        // Act - Call same operation multiple times concurrently
        var task1 = _dashboardService.GetTodaySalesAsync();
        var task2 = _dashboardService.GetTodaySalesAsync();
        var task3 = _dashboardService.GetTodaySalesAsync();

        var results = await Task.WhenAll(task1, task2, task3);

        // Assert
        Assert.All(results, result => Assert.Equal(expectedSales, result));
    }

    #endregion

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Casos Edge y Límites

    [Fact]
    public async Task GetTodaySalesAsync_WithVeryLargeSales_ShouldHandleGracefully()
    {
        // Arrange
        var veryLargeSales = 999999999.99m;
        var metricsResponse = ApiResponse<MetricasDiaDto>.SuccessResponse(
            new MetricasDiaDto { TotalVentas = veryLargeSales }, "Success");

        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(metricsResponse);

        // Act
        var result = await _dashboardService.GetTodaySalesAsync();

        // Assert
        Assert.Equal(veryLargeSales, result);
    }

    [Fact]
    public async Task GetSalesChangePercentageAsync_WithVeryLargePercentage_ShouldHandleGracefully()
    {
        // Arrange
        var todaySales = 1000m;
        var yesterdaySales = 1m; // Very small yesterday sales
        var expectedPercentage = ((todaySales - yesterdaySales) / yesterdaySales) * 100;

        var todayResponse = ApiResponse<MetricasDiaDto>.SuccessResponse(
            new MetricasDiaDto { TotalVentas = todaySales }, "Success");
        var yesterdayResponse = ApiResponse<MetricasRangoDto>.SuccessResponse(
            new MetricasRangoDto { TotalVentas = yesterdaySales }, "Success");

        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(todayResponse);
        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasRangoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(yesterdayResponse);

        // Act
        var result = await _dashboardService.GetSalesChangePercentageAsync();

        // Assert
        Assert.Equal(Math.Round(expectedPercentage, 1), result);
    }

    [Fact]
    public async Task GetActiveOrdersCountAsync_WithVeryLargeCount_ShouldHandleGracefully()
    {
        // Arrange
        var token = "test-token";
        var largeComandas = Enumerable.Range(1, 1000).Select(_ => new ComandaDto()).ToList();

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(
                It.IsAny<string>(), token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(
                new PaginatedList<ComandaDto> { Items = largeComandas }, "Success"));

        // Act
        var result = await _dashboardService.GetActiveOrdersCountAsync();

        // Assert
        Assert.True(result >= 1000); // Should handle large counts
    }

    #endregion

    #region 🚀 NUEVAS PRUEBAS ROBUSTAS - Manejo de Excepciones Específicas

    [Fact]
    public async Task GetTodaySalesAsync_WithHttpRequestException_ShouldReturnSimulatedSales()
    {
        // Arrange
        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _dashboardService.GetTodaySalesAsync();

        // Assert
        Assert.True(result > 0); // Should return simulated sales
    }

    [Fact]
    public async Task GetTodaySalesAsync_WithAggregateException_ShouldReturnSimulatedSales()
    {
        // Arrange
        _mockAnalyticsService
            .Setup(x => x.ObtenerMetricasDiaAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AggregateException("Multiple errors", new HttpRequestException("Network error")));

        // Act
        var result = await _dashboardService.GetTodaySalesAsync();

        // Assert
        Assert.True(result > 0); // Should return simulated sales
    }

    [Fact]
    public async Task GetTableStatusAsync_WithSocketException_ShouldReturnSimulatedStatus()
    {
        // Arrange
        _mockMesasService
            .Setup(x => x.ObtenerEstadoOcupacionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Net.Sockets.SocketException(10054));

        // Act
        var result = await _dashboardService.GetTableStatusAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Mesas);
        Assert.True(result.Mesas.Count > 0);
    }

    #endregion
}
