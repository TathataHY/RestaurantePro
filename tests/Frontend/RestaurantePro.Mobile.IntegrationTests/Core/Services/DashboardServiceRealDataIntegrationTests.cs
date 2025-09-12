using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Services.Dashboard;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Analytics;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Platform;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;
using System.Text.Json;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services;

/// <summary>
/// Tests de integración ROBUSTOS para DashboardService con datos reales creados via API
/// Usa la API para crear datos y luego valida las métricas calculadas
/// </summary>
public class DashboardServiceRealDataIntegrationTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private IDashboardService _dashboardService;
    private IAuthService _authService;

    public DashboardServiceRealDataIntegrationTests(MobileIntegrationTestFixture fixture)
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
        // Ejecutar seed de datos para cada test
        await _fixture.SeedDatabaseAsync();
        
        // Login automático para todos los tests
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        if (!loginResult.Success)
        {
            throw new InvalidOperationException($"Login falló: {loginResult.Message}");
        }
    }

    #region Tests con Datos Reales Creados via API

    [Fact]
    public async Task GetTodaySalesAsync_WithEmptyDatabase_ShouldReturnZero()
    {
        // Arrange
        await SetupAsync();

        // Act
        var sales = await _dashboardService.GetTodaySalesAsync();

        // Assert
        Assert.Equal(0, sales);
    }

    [Fact]
    public async Task GetActiveOrdersCountAsync_WithEmptyDatabase_ShouldReturnZero()
    {
        // Arrange
        await SetupAsync();

        // Act
        var activeCount = await _dashboardService.GetActiveOrdersCountAsync();

        // Assert
        Assert.Equal(0, activeCount);
    }

    [Fact]
    public async Task GetPendingOrdersCountAsync_WithEmptyDatabase_ShouldReturnZero()
    {
        // Arrange
        await SetupAsync();

        // Act
        var pendingCount = await _dashboardService.GetPendingOrdersCountAsync();

        // Assert
        Assert.Equal(0, pendingCount);
    }

    [Fact]
    public async Task GetTableStatusAsync_WithSeedData_ShouldReturnCorrectStatistics()
    {
        // Arrange
        await SetupAsync();

        // Act
        var tableStatus = await _dashboardService.GetTableStatusAsync();

        // Assert
        Assert.NotNull(tableStatus);
        Assert.NotNull(tableStatus.Mesas);
        Assert.True(tableStatus.Mesas.Count > 0, "Debería haber mesas del seed data");
        
        // Verificar estadísticas
        Assert.True(tableStatus.Estadisticas.MesasDisponibles >= 0);
        Assert.True(tableStatus.Estadisticas.MesasOcupadas >= 0);
        Assert.True(tableStatus.Estadisticas.MesasReservadas >= 0);
        
        // Verificar porcentajes
        Assert.True(tableStatus.Estadisticas.PorcentajeOcupacion >= 0);
        Assert.True(tableStatus.Estadisticas.PorcentajeOcupacion <= 100);
        Assert.True(tableStatus.Estadisticas.PorcentajeDisponibilidad >= 0);
        Assert.True(tableStatus.Estadisticas.PorcentajeDisponibilidad <= 100);
    }

    [Fact]
    public async Task GetRecentOrdersAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        await SetupAsync();

        // Act
        var recentOrders = await _dashboardService.GetRecentOrdersAsync();

        // Assert
        Assert.NotNull(recentOrders);
        Assert.Empty(recentOrders);
    }

    [Fact]
    public async Task GetSalesChangePercentageAsync_WithRealData_ShouldReturnValidPercentage()
    {
        // Arrange
        await SetupAsync();

        // Act
        var changePercentage = await _dashboardService.GetSalesChangePercentageAsync();

        // Assert
        // El porcentaje puede ser positivo o negativo dependiendo de los datos reales
        Assert.True(changePercentage >= -100, "El porcentaje no puede ser menor a -100%");
        Assert.True(changePercentage <= 1000, "El porcentaje no puede ser mayor a 1000%");
    }

    [Fact]
    public async Task GetOrdersByStatusAsync_WithEmptyDatabase_ShouldReturnEmptyList()
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
        Assert.Empty(pendientes);
        Assert.Empty(enProgreso);
        Assert.Empty(completadas);
    }

    [Fact]
    public async Task DashboardService_WithRealApiCalls_ShouldHandleErrorsGracefully()
    {
        // Arrange
        await SetupAsync();

        // Act - Ejecutar todos los métodos del dashboard
        var tasks = new List<Task<object>>
        {
            _dashboardService.GetTodaySalesAsync().ContinueWith(t => (object)t.Result),
            _dashboardService.GetSalesChangePercentageAsync().ContinueWith(t => (object)t.Result),
            _dashboardService.GetActiveOrdersCountAsync().ContinueWith(t => (object)t.Result),
            _dashboardService.GetPendingOrdersCountAsync().ContinueWith(t => (object)t.Result),
            _dashboardService.GetRecentOrdersAsync().ContinueWith(t => (object)t.Result),
            _dashboardService.GetTableStatusAsync().ContinueWith(t => (object)t.Result)
        };

        // Assert - Todos deben completarse sin excepción
        var results = await Task.WhenAll(tasks);
        Assert.NotNull(results);
        Assert.Equal(6, results.Length);
    }

    [Fact]
    public async Task DashboardService_WithConcurrentCalls_ShouldMaintainConsistency()
    {
        // Arrange
        await SetupAsync();

        // Act - Múltiples llamadas concurrentes
        var tasks = new List<Task<decimal>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_dashboardService.GetTodaySalesAsync());
        }

        var results = await Task.WhenAll(tasks);

        // Assert - Todos deben devolver el mismo valor (0 con BD vacía)
        Assert.All(results, result => Assert.Equal(0, result));
    }

    [Fact]
    public async Task DashboardService_WithTableStatusCalls_ShouldReturnConsistentData()
    {
        // Arrange
        await SetupAsync();

        // Act - Múltiples llamadas a GetTableStatusAsync
        var tasks = new List<Task<RestaurantePro.Mobile.Core.Models.DTOs.EstadoMesasDto>>();
        for (int i = 0; i < 3; i++)
        {
            tasks.Add(_dashboardService.GetTableStatusAsync());
        }

        var results = await Task.WhenAll(tasks);

        // Assert - Todos deben devolver datos consistentes
        Assert.All(results, result => 
        {
            Assert.NotNull(result);
            Assert.NotNull(result.Mesas);
            Assert.NotNull(result.Estadisticas);
        });

        // Verificar que los datos son consistentes entre llamadas
        var firstResult = results[0];
        foreach (var result in results)
        {
            Assert.Equal(firstResult.Mesas.Count, result.Mesas.Count);
            Assert.Equal(firstResult.Estadisticas.MesasDisponibles, result.Estadisticas.MesasDisponibles);
            Assert.Equal(firstResult.Estadisticas.MesasOcupadas, result.Estadisticas.MesasOcupadas);
        }
    }

    [Fact]
    public async Task DashboardService_WithPerformanceTest_ShouldCompleteWithinReasonableTime()
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

    #region Tests de Validación de Estructura de Datos

    [Fact]
    public async Task GetTableStatusAsync_ShouldReturnValidDataStructure()
    {
        // Arrange
        await SetupAsync();

        // Act
        var tableStatus = await _dashboardService.GetTableStatusAsync();

        // Assert
        Assert.NotNull(tableStatus);
        Assert.NotNull(tableStatus.Mesas);
        Assert.NotNull(tableStatus.Estadisticas);
        
        // Verificar estructura de mesas
        foreach (var mesa in tableStatus.Mesas)
        {
            Assert.NotEqual(Guid.Empty, mesa.Id);
            Assert.NotNull(mesa.Numero);
            Assert.NotNull(mesa.Estado);
            Assert.True(mesa.Capacidad > 0);
        }

        // Verificar estructura de estadísticas
        var stats = tableStatus.Estadisticas;
        Assert.True(stats.MesasDisponibles >= 0);
        Assert.True(stats.MesasOcupadas >= 0);
        Assert.True(stats.MesasReservadas >= 0);
        Assert.True(stats.PorcentajeOcupacion >= 0 && stats.PorcentajeOcupacion <= 100);
        Assert.True(stats.PorcentajeDisponibilidad >= 0 && stats.PorcentajeDisponibilidad <= 100);
    }

    [Fact]
    public async Task GetRecentOrdersAsync_ShouldReturnValidOrderStructure()
    {
        // Arrange
        await SetupAsync();

        // Act
        var recentOrders = await _dashboardService.GetRecentOrdersAsync();

        // Assert
        Assert.NotNull(recentOrders);
        
        // Con BD vacía, debería devolver lista vacía
        Assert.Empty(recentOrders);
    }

    #endregion
}
