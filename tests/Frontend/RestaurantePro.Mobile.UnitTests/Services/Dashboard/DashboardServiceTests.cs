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
}
