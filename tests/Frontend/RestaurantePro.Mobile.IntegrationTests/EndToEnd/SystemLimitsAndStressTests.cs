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
/// Pruebas de límites de sistema y estrés para el frontend móvil
/// </summary>
public class SystemLimitsAndStressTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;
    private readonly FakeSecureStorageService _secureStorage = new();
    private readonly FakeNavigationService _navigationService = new();

    public SystemLimitsAndStressTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
        _apiService = new ApiService(_client);
        _authService = new AuthService(_apiService, NullLogger<AuthService>.Instance, _secureStorage, _navigationService);
    }

    #region Pruebas de Límites de Memoria

    [Fact]
    public async Task MemoryLimit_WithLargeDataSets_ShouldHandleGracefully()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        // Act - Crear muchos ViewModels para simular uso intensivo de memoria
        var viewModels = new List<ProductosViewModel>();
        var initialMemory = GC.GetTotalMemory(false);

        for (int i = 0; i < 200; i++) // 200 ViewModels
        {
            var viewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);
            viewModels.Add(viewModel);
            
            // Cargar datos en cada ViewModel
            await viewModel.LoadProductosCommand.ExecuteAsync(null);
        }

        var finalMemory = GC.GetTotalMemory(true); // Forzar garbage collection
        var memoryIncrease = finalMemory - initialMemory;

        // Assert - El aumento de memoria debe ser razonable (menos de 200MB)
        Assert.True(memoryIncrease < 200 * 1024 * 1024, 
            $"El aumento de memoria fue {memoryIncrease / 1024 / 1024}MB, que excede el límite de 200MB");
    }

    [Fact]
    public async Task MemoryLimit_WithConcurrentOperations_ShouldNotLeak()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var comandasService = new ComandasService(_apiService, _authService);
        var mesasService = new MesasService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        // Act - Ejecutar muchas operaciones concurrentes
        var initialMemory = GC.GetTotalMemory(false);
        var tasks = new List<Task>();

        for (int i = 0; i < 100; i++)
        {
            var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);
            var notificationService = new Mock<INotificationService>();
            var comandaRealtimeService = new Mock<IComandaRealtimeService>();
            var preferencesService = new Mock<IPreferencesService>();
            var comandasViewModel = new ComandasViewModel(comandasService, dialogService.Object, navigationService, mesasService, notificationService.Object, comandaRealtimeService.Object, preferencesService.Object);
            var mesasViewModel = new MesasViewModel(mesasService, dialogService.Object, navigationService);

            tasks.Add(productosViewModel.LoadProductosCommand.ExecuteAsync(null));
            tasks.Add(comandasViewModel.LoadComandasCommand.ExecuteAsync(null));
            tasks.Add(mesasViewModel.LoadMesasCommand.ExecuteAsync(null));
        }

        await Task.WhenAll(tasks);

        var finalMemory = GC.GetTotalMemory(true); // Forzar garbage collection
        var memoryIncrease = finalMemory - initialMemory;

        // Assert - No debe haber fuga de memoria significativa
        Assert.True(memoryIncrease < 100 * 1024 * 1024, 
            $"El aumento de memoria fue {memoryIncrease / 1024 / 1024}MB, que excede el límite de 100MB");
    }

    #endregion

    #region Pruebas de Límites de CPU

    [Fact]
    public async Task CPULimit_WithIntensiveOperations_ShouldHandleGracefully()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Ejecutar operaciones intensivas de CPU
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(productosViewModel.LoadProductosCommand.ExecuteAsync(null));
            tasks.Add(productosViewModel.RefreshProductosCommand.ExecuteAsync(null));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Debe completarse en tiempo razonable (menos de 30 segundos)
        Assert.True(stopwatch.ElapsedMilliseconds < 30000, 
            $"Las operaciones intensivas tomaron {stopwatch.ElapsedMilliseconds}ms, que excede el límite de 30000ms");
    }

    [Fact]
    public async Task CPULimit_WithSearchOperations_ShouldHandleGracefully()
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

        // Act - Ejecutar muchas búsquedas intensivas
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var searchTerms = new[] { "pizza", "bebida", "postre", "entrada", "plato", "ensalada", "sopa", "carne", "pescado", "pollo" };
        var tasks = new List<Task>();

        for (int i = 0; i < 50; i++)
        {
            foreach (var term in searchTerms)
            {
                productosViewModel.TextoBusqueda = term;
                tasks.Add(productosViewModel.BuscarProductosCommand.ExecuteAsync(null));
            }
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Debe completarse en tiempo razonable
        Assert.True(stopwatch.ElapsedMilliseconds < 60000, 
            $"Las búsquedas intensivas tomaron {stopwatch.ElapsedMilliseconds}ms, que excede el límite de 60000ms");
    }

    #endregion

    #region Pruebas de Límites de Red

    [Fact]
    public async Task NetworkLimit_WithHighConcurrency_ShouldHandleGracefully()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var comandasService = new ComandasService(_apiService, _authService);
        var mesasService = new MesasService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        // Act - Ejecutar muchas operaciones de red concurrentes
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tasks = new List<Task>();

        for (int i = 0; i < 50; i++)
        {
            var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);
            var notificationService = new Mock<INotificationService>();
            var comandaRealtimeService = new Mock<IComandaRealtimeService>();
            var preferencesService = new Mock<IPreferencesService>();
            var comandasViewModel = new ComandasViewModel(comandasService, dialogService.Object, navigationService, mesasService, notificationService.Object, comandaRealtimeService.Object, preferencesService.Object);
            var mesasViewModel = new MesasViewModel(mesasService, dialogService.Object, navigationService);

            tasks.Add(productosViewModel.LoadProductosCommand.ExecuteAsync(null));
            tasks.Add(comandasViewModel.LoadComandasCommand.ExecuteAsync(null));
            tasks.Add(mesasViewModel.LoadMesasCommand.ExecuteAsync(null));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Debe manejar la alta concurrencia sin fallar
        Assert.True(stopwatch.ElapsedMilliseconds < 120000, 
            $"Las operaciones de alta concurrencia tomaron {stopwatch.ElapsedMilliseconds}ms, que excede el límite de 120000ms");
    }

    [Fact]
    public async Task NetworkLimit_WithRateLimiting_ShouldHandleGracefully()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Ejecutar operaciones muy rápidas para simular rate limiting
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tasks = new List<Task>();

        for (int i = 0; i < 100; i++)
        {
            tasks.Add(productosViewModel.LoadProductosCommand.ExecuteAsync(null));
            await Task.Delay(10); // Pequeña pausa para simular rate limiting
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Debe manejar el rate limiting sin fallar
        Assert.True(stopwatch.ElapsedMilliseconds < 300000, 
            $"Las operaciones con rate limiting tomaron {stopwatch.ElapsedMilliseconds}ms, que excede el límite de 300000ms");
    }

    #endregion

    #region Pruebas de Límites de UI

    [Fact]
    public async Task UILimit_WithRapidUserInteractions_ShouldHandleGracefully()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Simular interacciones rápidas del usuario
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tasks = new List<Task>();

        for (int i = 0; i < 50; i++)
        {
            // Simular carga
            tasks.Add(productosViewModel.LoadProductosCommand.ExecuteAsync(null));
            
            // Simular refresh
            tasks.Add(productosViewModel.RefreshProductosCommand.ExecuteAsync(null));
            
            // Simular búsqueda
            productosViewModel.TextoBusqueda = $"búsqueda {i}";
            tasks.Add(productosViewModel.BuscarProductosCommand.ExecuteAsync(null));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Debe manejar las interacciones rápidas sin fallar
        Assert.True(stopwatch.ElapsedMilliseconds < 60000, 
            $"Las interacciones rápidas tomaron {stopwatch.ElapsedMilliseconds}ms, que excede el límite de 60000ms");
    }

    [Fact]
    public async Task UILimit_WithComplexNavigation_ShouldHandleGracefully()
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

        // Act - Simular navegación compleja
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tasks = new List<Task>();

        for (int i = 0; i < 20; i++)
        {
            // Cargar datos
            tasks.Add(productosViewModel.LoadProductosCommand.ExecuteAsync(null));
            tasks.Add(comandasViewModel.LoadComandasCommand.ExecuteAsync(null));
            tasks.Add(mesasViewModel.LoadMesasCommand.ExecuteAsync(null));
            
            // Simular navegación
            tasks.Add(navigationService.NavigateToAsync("productos", new Dictionary<string, object>()));
            tasks.Add(navigationService.NavigateToAsync("comandas", new Dictionary<string, object>()));
            tasks.Add(navigationService.NavigateToAsync("mesas", new Dictionary<string, object>()));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Debe manejar la navegación compleja sin fallar
        Assert.True(stopwatch.ElapsedMilliseconds < 120000, 
            $"La navegación compleja tomó {stopwatch.ElapsedMilliseconds}ms, que excede el límite de 120000ms");
    }

    #endregion

    #region Pruebas de Límites de Datos

    [Fact]
    public async Task DataLimit_WithLargePayloads_ShouldHandleGracefully()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Cargar datos grandes múltiples veces
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < 10; i++)
        {
            await productosViewModel.LoadProductosCommand.ExecuteAsync(null);
            await productosViewModel.RefreshProductosCommand.ExecuteAsync(null);
        }

        stopwatch.Stop();

        // Assert - Debe manejar los datos grandes sin fallar
        Assert.True(stopwatch.ElapsedMilliseconds < 60000, 
            $"El procesamiento de datos grandes tomó {stopwatch.ElapsedMilliseconds}ms, que excede el límite de 60000ms");
    }

    [Fact]
    public async Task DataLimit_WithComplexQueries_ShouldHandleGracefully()
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

        // Act - Ejecutar consultas complejas
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tasks = new List<Task>();

        var complexSearchTerms = new[]
        {
            "pizza con queso y pepperoni",
            "bebida refrescante y fría",
            "postre delicioso y dulce",
            "entrada caliente y sabrosa",
            "plato principal y nutritivo"
        };

        for (int i = 0; i < 20; i++)
        {
            foreach (var term in complexSearchTerms)
            {
                productosViewModel.TextoBusqueda = term;
                tasks.Add(productosViewModel.BuscarProductosCommand.ExecuteAsync(null));
            }
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Debe manejar las consultas complejas sin fallar
        Assert.True(stopwatch.ElapsedMilliseconds < 120000, 
            $"Las consultas complejas tomaron {stopwatch.ElapsedMilliseconds}ms, que excede el límite de 120000ms");
    }

    #endregion

    #region Pruebas de Límites de Tiempo

    [Fact]
    public async Task TimeLimit_WithLongRunningOperations_ShouldHandleGracefully()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Ejecutar operaciones de larga duración
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tasks = new List<Task>();

        for (int i = 0; i < 30; i++)
        {
            tasks.Add(productosViewModel.LoadProductosCommand.ExecuteAsync(null));
            tasks.Add(productosViewModel.RefreshProductosCommand.ExecuteAsync(null));
            
            // Simular pausa entre operaciones
            await Task.Delay(100);
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Debe completarse en tiempo razonable
        Assert.True(stopwatch.ElapsedMilliseconds < 180000, 
            $"Las operaciones de larga duración tomaron {stopwatch.ElapsedMilliseconds}ms, que excede el límite de 180000ms");
    }

    [Fact]
    public async Task TimeLimit_WithTimeoutScenarios_ShouldHandleGracefully()
    {
        // Arrange - Crear cliente con timeout muy corto
        var httpClient = new HttpClient
        {
            BaseAddress = _client.BaseAddress,
            Timeout = TimeSpan.FromMilliseconds(100) // Timeout muy corto
        };

        var apiService = new ApiService(httpClient);
        var authService = new AuthService(apiService, NullLogger<AuthService>.Instance, _secureStorage, _navigationService);
        var productosService = new ProductosService(apiService, authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Intentar operaciones con timeout
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < 10; i++)
        {
            await productosViewModel.LoadProductosCommand.ExecuteAsync(null);
        }

        stopwatch.Stop();

        // Assert - Debe manejar los timeouts sin fallar
        Assert.True(stopwatch.ElapsedMilliseconds < 30000, 
            $"Las operaciones con timeout tomaron {stopwatch.ElapsedMilliseconds}ms, que excede el límite de 30000ms");
    }

    #endregion
}
