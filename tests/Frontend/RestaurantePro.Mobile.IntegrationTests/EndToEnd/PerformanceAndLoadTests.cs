using Microsoft.Extensions.Logging.Abstractions;
using RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;
using RestaurantePro.Mobile.Core.Features.Operations.Mesas.ViewModels;
using RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Notifications;
using RestaurantePro.Mobile.Core.Services.Realtime;
using RestaurantePro.Mobile.Core.Services.Preferences;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.EndToEnd;

/// <summary>
/// Pruebas de rendimiento y carga para el frontend móvil
/// </summary>
public class PerformanceAndLoadTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;
    private readonly FakeSecureStorageService _secureStorage = new();
    private readonly FakeNavigationService _navigationService = new();

    public PerformanceAndLoadTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
        _apiService = new ApiService(_client);
        _authService = new AuthService(_apiService, NullLogger<AuthService>.Instance, _secureStorage, _navigationService);
    }

    #region Pruebas de Carga de Datos

    [Fact]
    public async Task LoadLargeDataSet_Productos_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Medir tiempo de carga
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await productosViewModel.LoadProductosCommand.ExecuteAsync(null);
        stopwatch.Stop();

        // Assert - Debe completarse en menos de 5 segundos
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"La carga tomó {stopwatch.ElapsedMilliseconds}ms, que excede el límite de 5000ms");
        Assert.True(productosViewModel.Productos.Count > 0, "Debe cargar al menos un producto");
    }

    [Fact]
    public async Task LoadLargeDataSet_Comandas_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var comandasService = new ComandasService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var mesasService = new MesasService(_apiService, _authService);
        var notificationService = new Mock<INotificationService>();
        var comandaRealtimeService = new Mock<IComandaRealtimeService>();
        var preferencesService = new Mock<IPreferencesService>();
        var comandasViewModel = new ComandasViewModel(comandasService, dialogService.Object, navigationService, mesasService, notificationService.Object, comandaRealtimeService.Object, preferencesService.Object);

        // Act - Medir tiempo de carga
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await comandasViewModel.LoadComandasCommand.ExecuteAsync(null);
        stopwatch.Stop();

        // Assert - Debe completarse en menos de 3 segundos
        Assert.True(stopwatch.ElapsedMilliseconds < 3000, $"La carga tomó {stopwatch.ElapsedMilliseconds}ms, que excede el límite de 3000ms");
    }

    [Fact]
    public async Task LoadLargeDataSet_Mesas_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var mesasService = new MesasService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var mesasViewModel = new MesasViewModel(mesasService, dialogService.Object, navigationService);

        // Act - Medir tiempo de carga
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await mesasViewModel.LoadMesasCommand.ExecuteAsync(null);
        stopwatch.Stop();

        // Assert - Debe completarse en menos de 2 segundos
        Assert.True(stopwatch.ElapsedMilliseconds < 2000, $"La carga tomó {stopwatch.ElapsedMilliseconds}ms, que excede el límite de 2000ms");
    }

    #endregion

    #region Pruebas de Concurrencia

    [Fact]
    public async Task ConcurrentLoadOperations_ShouldHandleGracefully()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var comandasService = new ComandasService(_apiService, _authService);
        var mesasService = new MesasService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);
        var notificationService = new Mock<INotificationService>();
        var comandaRealtimeService = new Mock<IComandaRealtimeService>();
        var preferencesService = new Mock<IPreferencesService>();
        var comandasViewModel = new ComandasViewModel(comandasService, dialogService.Object, navigationService, mesasService, notificationService.Object, comandaRealtimeService.Object, preferencesService.Object);
        var mesasViewModel = new MesasViewModel(mesasService, dialogService.Object, navigationService);

        // Act - Ejecutar operaciones concurrentes
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var task1 = productosViewModel.LoadProductosCommand.ExecuteAsync(null);
        var task2 = comandasViewModel.LoadComandasCommand.ExecuteAsync(null);
        var task3 = mesasViewModel.LoadMesasCommand.ExecuteAsync(null);

        await Task.WhenAll(task1, task2, task3);
        stopwatch.Stop();

        // Assert - Todas las operaciones deben completarse
        Assert.True(stopwatch.ElapsedMilliseconds < 10000, $"Las operaciones concurrentes tomaron {stopwatch.ElapsedMilliseconds}ms, que excede el límite de 10000ms");
    }

    [Fact]
    public async Task ConcurrentSearchOperations_ShouldHandleGracefully()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Cargar productos primero
        await productosViewModel.LoadProductosCommand.ExecuteAsync(null);

        // Act - Ejecutar búsquedas concurrentes
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var searchTasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            productosViewModel.TextoBusqueda = $"búsqueda {i}";
            searchTasks.Add(productosViewModel.BuscarProductosCommand.ExecuteAsync(null));
        }

        await Task.WhenAll(searchTasks);
        stopwatch.Stop();

        // Assert - Todas las búsquedas deben completarse
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Las búsquedas concurrentes tomaron {stopwatch.ElapsedMilliseconds}ms, que excede el límite de 5000ms");
    }

    #endregion

    #region Pruebas de Memoria

    [Fact]
    public async Task MemoryUsage_LargeDataLoad_ShouldNotExceedLimits()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Cargar datos múltiples veces para simular uso intensivo
        var initialMemory = GC.GetTotalMemory(false);
        
        for (int i = 0; i < 5; i++)
        {
            await productosViewModel.LoadProductosCommand.ExecuteAsync(null);
            await productosViewModel.RefreshProductosCommand.ExecuteAsync(null);
        }

        var finalMemory = GC.GetTotalMemory(true); // Forzar garbage collection
        var memoryIncrease = finalMemory - initialMemory;

        // Assert - El aumento de memoria debe ser razonable (menos de 50MB)
        Assert.True(memoryIncrease < 50 * 1024 * 1024, $"El aumento de memoria fue {memoryIncrease / 1024 / 1024}MB, que excede el límite de 50MB");
    }

    [Fact]
    public async Task MemoryUsage_ConcurrentOperations_ShouldNotLeak()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var comandasService = new ComandasService(_apiService, _authService);
        var mesasService = new MesasService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        // Act - Crear múltiples ViewModels y ejecutar operaciones
        var initialMemory = GC.GetTotalMemory(false);
        
        for (int i = 0; i < 10; i++)
        {
            var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);
            var notificationService = new Mock<INotificationService>();
            var comandaRealtimeService = new Mock<IComandaRealtimeService>();
            var preferencesService = new Mock<IPreferencesService>();
            var comandasViewModel = new ComandasViewModel(comandasService, dialogService.Object, navigationService, mesasService, notificationService.Object, comandaRealtimeService.Object, preferencesService.Object);
            var mesasViewModel = new MesasViewModel(mesasService, dialogService.Object, navigationService);

            await productosViewModel.LoadProductosCommand.ExecuteAsync(null);
            await comandasViewModel.LoadComandasCommand.ExecuteAsync(null);
            await mesasViewModel.LoadMesasCommand.ExecuteAsync(null);
        }

        var finalMemory = GC.GetTotalMemory(true); // Forzar garbage collection
        var memoryIncrease = finalMemory - initialMemory;

        // Assert - El aumento de memoria debe ser razonable
        Assert.True(memoryIncrease < 100 * 1024 * 1024, $"El aumento de memoria fue {memoryIncrease / 1024 / 1024}MB, que excede el límite de 100MB");
    }

    #endregion

    #region Pruebas de Rendimiento de UI

    [Fact]
    public async Task UIResponsiveness_HeavyOperations_ShouldNotBlockUI()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Ejecutar operaciones pesadas y verificar que no bloquean la UI
        var tasks = new List<Task>();
        
        // Simular operaciones de UI que deberían ejecutarse en paralelo
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await productosViewModel.LoadProductosCommand.ExecuteAsync(null);
                await productosViewModel.RefreshProductosCommand.ExecuteAsync(null);
            }));
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Las operaciones deben completarse en tiempo razonable
        Assert.True(stopwatch.ElapsedMilliseconds < 15000, $"Las operaciones de UI tomaron {stopwatch.ElapsedMilliseconds}ms, que excede el límite de 15000ms");
    }

    [Fact]
    public async Task UIResponsiveness_SearchOperations_ShouldBeFast()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Cargar productos primero
        await productosViewModel.LoadProductosCommand.ExecuteAsync(null);

        // Act - Ejecutar búsquedas rápidas
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var searchTerms = new[] { "pizza", "bebida", "postre", "entrada", "plato" };
        foreach (var term in searchTerms)
        {
            productosViewModel.TextoBusqueda = term;
            await productosViewModel.BuscarProductosCommand.ExecuteAsync(null);
        }

        stopwatch.Stop();

        // Assert - Las búsquedas deben ser rápidas (menos de 1 segundo por búsqueda)
        Assert.True(stopwatch.ElapsedMilliseconds < searchTerms.Length * 1000, 
            $"Las búsquedas tomaron {stopwatch.ElapsedMilliseconds}ms, que excede el límite de {searchTerms.Length * 1000}ms");
    }

    #endregion

    #region Pruebas de Límites de Sistema

    [Fact]
    public async Task SystemLimits_MaximumConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        // Act - Crear muchas operaciones concurrentes
        var tasks = new List<Task>();
        for (int i = 0; i < 50; i++) // 50 operaciones concurrentes
        {
            var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);
            tasks.Add(productosViewModel.LoadProductosCommand.ExecuteAsync(null));
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Debe manejar la carga sin fallar
        Assert.True(stopwatch.ElapsedMilliseconds < 30000, $"Las operaciones concurrentes tomaron {stopwatch.ElapsedMilliseconds}ms, que excede el límite de 30000ms");
    }

    [Fact]
    public async Task SystemLimits_DataVolume_ShouldHandleLargeDatasets()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Cargar datos múltiples veces para simular volumen alto
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < 20; i++)
        {
            await productosViewModel.LoadProductosCommand.ExecuteAsync(null);
            await productosViewModel.RefreshProductosCommand.ExecuteAsync(null);
        }

        stopwatch.Stop();

        // Assert - Debe manejar el volumen sin degradación significativa
        Assert.True(stopwatch.ElapsedMilliseconds < 60000, $"El procesamiento de volumen alto tomó {stopwatch.ElapsedMilliseconds}ms, que excede el límite de 60000ms");
    }

    #endregion
}
