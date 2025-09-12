using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Services.Dashboard;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Analytics;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Platform;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services;

/// <summary>
/// Tests de integración para validar cálculos del dashboard con datos existentes
/// Valida que las métricas se calculen correctamente con los datos de la BD
/// </summary>
public class DashboardServiceValidationIntegrationTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private IDashboardService _dashboardService;
    private IAuthService _authService;

    public DashboardServiceValidationIntegrationTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
        Setup();
    }

    private void Setup()
    {
        // Crear servicios móviles localmente
        var httpClient = _client;
        var apiService = new ApiService(httpClient);
        var authService = new AuthService(apiService, NullLogger<AuthService>.Instance, new FakeSecureStorageService(), new FakeNavigationService());
        var analyticsService = new AnalyticsService(apiService, authService);
        var mesasService = new MesasService(apiService, authService);
        var logger = NullLogger<DashboardService>.Instance;

        _dashboardService = new DashboardService(apiService, analyticsService, mesasService, authService, logger);
        _authService = authService;
    }

    private async Task SetupAsync()
    {
        // Login automático para todos los tests
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        if (!loginResult.Success)
        {
            throw new InvalidOperationException($"Login falló: {loginResult.Message}");
        }
    }

    #region Tests de Validación de Cálculos

    [Fact]
    public async Task GetTodaySalesAsync_ShouldReturnValidSalesAmount()
    {
        // Arrange
        await SetupAsync();

        // Act
        var sales = await _dashboardService.GetTodaySalesAsync();

        // Assert
        Assert.True(sales >= 0, "Las ventas deben ser mayores o iguales a 0");
        Assert.True(sales <= 1000000, "Las ventas no pueden ser mayores a 1,000,000 (límite razonable)");
    }

    [Fact]
    public async Task GetSalesChangePercentageAsync_ShouldReturnValidPercentage()
    {
        // Arrange
        await SetupAsync();

        // Act
        var changePercentage = await _dashboardService.GetSalesChangePercentageAsync();

        // Assert
        Assert.True(changePercentage >= -100, "El porcentaje no puede ser menor a -100%");
        Assert.True(changePercentage <= 1000, "El porcentaje no puede ser mayor a 1000%");
    }

    [Fact]
    public async Task GetActiveOrdersCountAsync_ShouldReturnValidCount()
    {
        // Arrange
        await SetupAsync();

        // Act
        var activeCount = await _dashboardService.GetActiveOrdersCountAsync();

        // Assert
        Assert.True(activeCount >= 0, "El conteo de comandas activas debe ser mayor o igual a 0");
        Assert.True(activeCount <= 1000, "El conteo de comandas activas no puede ser mayor a 1000 (límite razonable)");
    }

    [Fact]
    public async Task GetPendingOrdersCountAsync_ShouldReturnValidCount()
    {
        // Arrange
        await SetupAsync();

        // Act
        var pendingCount = await _dashboardService.GetPendingOrdersCountAsync();

        // Assert
        Assert.True(pendingCount >= 0, "El conteo de comandas pendientes debe ser mayor o igual a 0");
        Assert.True(pendingCount <= 1000, "El conteo de comandas pendientes no puede ser mayor a 1000 (límite razonable)");
    }

    [Fact]
    public async Task GetTableStatusAsync_ShouldReturnValidStatistics()
    {
        // Arrange
        await SetupAsync();

        // Act
        var tableStatus = await _dashboardService.GetTableStatusAsync();

        // Assert
        Assert.NotNull(tableStatus);
        Assert.NotNull(tableStatus.Mesas);
        Assert.NotNull(tableStatus.Estadisticas);
        
        // Verificar que hay mesas
        Assert.True(tableStatus.Mesas.Count > 0, "Debe haber al menos una mesa");
        
        // Verificar estadísticas
        var stats = tableStatus.Estadisticas;
        Assert.True(stats.MesasDisponibles >= 0, "Las mesas disponibles deben ser >= 0");
        Assert.True(stats.MesasOcupadas >= 0, "Las mesas ocupadas deben ser >= 0");
        Assert.True(stats.MesasReservadas >= 0, "Las mesas reservadas deben ser >= 0");
        
        // Verificar que la suma de estados no exceda el total
        var totalMesas = stats.MesasDisponibles + stats.MesasOcupadas + stats.MesasReservadas;
        Assert.True(totalMesas <= tableStatus.Mesas.Count, "La suma de estados no puede exceder el total de mesas");
        
        // Verificar porcentajes
        Assert.True(stats.PorcentajeOcupacion >= 0 && stats.PorcentajeOcupacion <= 100, 
            "El porcentaje de ocupación debe estar entre 0 y 100");
        Assert.True(stats.PorcentajeDisponibilidad >= 0 && stats.PorcentajeDisponibilidad <= 100, 
            "El porcentaje de disponibilidad debe estar entre 0 y 100");
    }

    [Fact]
    public async Task GetRecentOrdersAsync_ShouldReturnValidOrders()
    {
        // Arrange
        await SetupAsync();

        // Act
        var recentOrders = await _dashboardService.GetRecentOrdersAsync();

        // Assert
        Assert.NotNull(recentOrders);
        Assert.True(recentOrders.Count >= 0, "El conteo de comandas recientes debe ser >= 0");
        Assert.True(recentOrders.Count <= 100, "El conteo de comandas recientes no puede ser mayor a 100 (límite razonable)");

        // Verificar estructura de comandas si hay alguna
        foreach (var order in recentOrders)
        {
            Assert.NotNull(order.OrderNumber);
            Assert.NotNull(order.Status);
            Assert.True(order.Total >= 0, "El total de la comanda debe ser >= 0");
            Assert.True(order.OrderTime <= DateTime.Now, "La fecha debe ser válida");
            Assert.NotNull(order.Items);
        }
    }

    [Fact]
    public async Task GetOrdersByStatusAsync_ShouldReturnValidOrders()
    {
        // Arrange
        await SetupAsync();

        // Act & Assert - Probar diferentes estados
        var pendientes = await _dashboardService.GetOrdersByStatusAsync("Pendiente");
        var enProgreso = await _dashboardService.GetOrdersByStatusAsync("En Progreso");
        var completadas = await _dashboardService.GetOrdersByStatusAsync("Completada");

        Assert.NotNull(pendientes);
        Assert.NotNull(enProgreso);
        Assert.NotNull(completadas);

        // Verificar conteos razonables
        Assert.True(pendientes.Count >= 0 && pendientes.Count <= 1000);
        Assert.True(enProgreso.Count >= 0 && enProgreso.Count <= 1000);
        Assert.True(completadas.Count >= 0 && completadas.Count <= 1000);

        // Verificar que las comandas filtradas tienen el estado correcto
        foreach (var order in pendientes)
        {
            Assert.Equal("Pendiente", order.Status);
        }

        foreach (var order in enProgreso)
        {
            Assert.Equal("En Progreso", order.Status);
        }

        foreach (var order in completadas)
        {
            Assert.Equal("Completada", order.Status);
        }
    }

    #endregion

    #region Tests de Consistencia de Datos

    [Fact]
    public async Task DashboardData_ShouldBeConsistentAcrossMultipleCalls()
    {
        // Arrange
        await SetupAsync();

        // Act - Múltiples llamadas
        var sales1 = await _dashboardService.GetTodaySalesAsync();
        var sales2 = await _dashboardService.GetTodaySalesAsync();
        var active1 = await _dashboardService.GetActiveOrdersCountAsync();
        var active2 = await _dashboardService.GetActiveOrdersCountAsync();
        var tableStatus1 = await _dashboardService.GetTableStatusAsync();
        var tableStatus2 = await _dashboardService.GetTableStatusAsync();

        // Assert - Los datos deben ser consistentes
        Assert.Equal(sales1, sales2);
        Assert.Equal(active1, active2);
        Assert.Equal(tableStatus1.Mesas.Count, tableStatus2.Mesas.Count);
        Assert.Equal(tableStatus1.Estadisticas.MesasDisponibles, tableStatus2.Estadisticas.MesasDisponibles);
        Assert.Equal(tableStatus1.Estadisticas.MesasOcupadas, tableStatus2.Estadisticas.MesasOcupadas);
    }

    [Fact]
    public async Task DashboardData_ShouldHandleConcurrentCalls()
    {
        // Arrange
        await SetupAsync();

        // Act - Llamadas concurrentes
        var tasks = new List<Task<object>>
        {
            _dashboardService.GetTodaySalesAsync().ContinueWith(t => (object)t.Result),
            _dashboardService.GetSalesChangePercentageAsync().ContinueWith(t => (object)t.Result),
            _dashboardService.GetActiveOrdersCountAsync().ContinueWith(t => (object)t.Result),
            _dashboardService.GetPendingOrdersCountAsync().ContinueWith(t => (object)t.Result),
            _dashboardService.GetRecentOrdersAsync().ContinueWith(t => (object)t.Result),
            _dashboardService.GetTableStatusAsync().ContinueWith(t => (object)t.Result)
        };

        var results = await Task.WhenAll(tasks);

        // Assert - Todas las llamadas deben completarse exitosamente
        Assert.NotNull(results);
        Assert.Equal(6, results.Length);
        Assert.All(results, result => Assert.NotNull(result));
    }

    [Fact]
    public async Task DashboardData_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        await SetupAsync();
        var timeout = TimeSpan.FromSeconds(10);

        // Act & Assert - Cada método debe completarse en tiempo razonable
        var startTime = DateTime.UtcNow;
        
        var salesTask = _dashboardService.GetTodaySalesAsync();
        var changeTask = _dashboardService.GetSalesChangePercentageAsync();
        var activeTask = _dashboardService.GetActiveOrdersCountAsync();
        var pendingTask = _dashboardService.GetPendingOrdersCountAsync();
        var recentTask = _dashboardService.GetRecentOrdersAsync();
        var tableTask = _dashboardService.GetTableStatusAsync();

        await Task.WhenAll(salesTask, changeTask, activeTask, pendingTask, recentTask, tableTask);
        
        var endTime = DateTime.UtcNow;
        var totalTime = endTime - startTime;

        Assert.True(totalTime < timeout, $"Operaciones completadas en {totalTime.TotalSeconds} segundos, excediendo el timeout de {timeout.TotalSeconds} segundos");
    }

    #endregion

    #region Tests de Validación de Lógica de Negocio

    [Fact]
    public async Task TableStatistics_ShouldCalculatePercentagesCorrectly()
    {
        // Arrange
        await SetupAsync();

        // Act
        var tableStatus = await _dashboardService.GetTableStatusAsync();

        // Assert
        var stats = tableStatus.Estadisticas;
        var totalMesas = tableStatus.Mesas.Count;
        
        if (totalMesas > 0)
        {
            // Verificar que los porcentajes se calculen correctamente
            var expectedOcupacion = (double)stats.MesasOcupadas / totalMesas * 100;
            var expectedDisponibilidad = (double)stats.MesasDisponibles / totalMesas * 100;
            
            Assert.Equal(expectedOcupacion, (double)stats.PorcentajeOcupacion, 1);
            Assert.Equal(expectedDisponibilidad, (double)stats.PorcentajeDisponibilidad, 1);
        }
    }

    [Fact]
    public async Task OrderCounts_ShouldBeLogicallyConsistent()
    {
        // Arrange
        await SetupAsync();

        // Act
        var activeCount = await _dashboardService.GetActiveOrdersCountAsync();
        var pendingCount = await _dashboardService.GetPendingOrdersCountAsync();
        var recentOrders = await _dashboardService.GetRecentOrdersAsync();

        // Assert
        // Las comandas activas + pendientes no pueden exceder las comandas recientes
        var totalActivePending = activeCount + pendingCount;
        Assert.True(totalActivePending <= recentOrders.Count + 100, 
            "La suma de comandas activas y pendientes no puede exceder significativamente las comandas recientes");
    }

    [Fact]
    public async Task SalesData_ShouldBeLogicallyConsistent()
    {
        // Arrange
        await SetupAsync();

        // Act
        var sales = await _dashboardService.GetTodaySalesAsync();
        var changePercentage = await _dashboardService.GetSalesChangePercentageAsync();

        // Assert
        // Si hay ventas, el porcentaje de cambio debe ser razonable
        if (sales > 0)
        {
            Assert.True(changePercentage >= -100, "El porcentaje de cambio no puede ser menor a -100%");
            Assert.True(changePercentage <= 1000, "El porcentaje de cambio no puede ser mayor a 1000%");
        }
    }

    #endregion
}
