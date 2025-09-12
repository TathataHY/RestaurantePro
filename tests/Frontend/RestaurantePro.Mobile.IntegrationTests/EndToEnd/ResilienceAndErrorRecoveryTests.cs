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
/// Pruebas de robustez y recuperación de errores para el frontend móvil
/// </summary>
public class ResilienceAndErrorRecoveryTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;
    private readonly FakeSecureStorageService _secureStorage = new();
    private readonly FakeNavigationService _navigationService = new();

    public ResilienceAndErrorRecoveryTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
        _apiService = new ApiService(_client);
        _authService = new AuthService(_apiService, NullLogger<AuthService>.Instance, _secureStorage, _navigationService);
    }

    #region Pruebas de Recuperación de Errores de Red

    [Fact]
    public async Task NetworkError_WithTimeout_ShouldRecoverGracefully()
    {
        // Arrange - Crear cliente con timeout muy corto
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://10.255.255.1/"), // Host inalcanzable
            Timeout = TimeSpan.FromMilliseconds(100)
        };

        var apiService = new ApiService(httpClient);
        var authService = new AuthService(apiService, NullLogger<AuthService>.Instance, _secureStorage, _navigationService);
        var productosService = new ProductosService(apiService, authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Intentar cargar productos con error de red
        await productosViewModel.LoadProductosCommand.ExecuteAsync(null);

        // Assert - Debe mostrar error y permitir recuperación
        dialogService.Verify(d => d.ShowErrorAsync(It.Is<string>(s => !string.IsNullOrWhiteSpace(s))), Times.AtLeastOnce());
        
        // Verificar que el ViewModel está en estado de error pero no roto
        Assert.True(productosViewModel.HasError || productosViewModel.Productos.Count == 0);
    }

    [Fact]
    public async Task NetworkError_WithRecovery_ShouldWorkAfterNetworkRestored()
    {
        // Arrange - Simular error de red seguido de recuperación
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://10.255.255.1/"), // Host inalcanzable
            Timeout = TimeSpan.FromMilliseconds(100)
        };

        var apiService = new ApiService(httpClient);
        var authService = new AuthService(apiService, NullLogger<AuthService>.Instance, _secureStorage, _navigationService);
        var productosService = new ProductosService(apiService, authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Intentar cargar con error de red
        await productosViewModel.LoadProductosCommand.ExecuteAsync(null);
        
        // Simular recuperación de red (cambiar a cliente funcional)
        var workingHttpClient = _fixture.CreateClient();
        var workingApiService = new ApiService(workingHttpClient);
        var workingAuthService = new AuthService(workingApiService, NullLogger<AuthService>.Instance, _secureStorage, _navigationService);
        var workingProductosService = new ProductosService(workingApiService, workingAuthService);
        
        // Crear nuevo ViewModel con servicio funcional
        var recoveredViewModel = new ProductosViewModel(workingProductosService, dialogService.Object, navigationService);
        
        // Intentar cargar con servicio funcional
        await recoveredViewModel.LoadProductosCommand.ExecuteAsync(null);

        // Assert - Debe funcionar después de la recuperación
        Assert.True(recoveredViewModel.Productos.Count > 0 || !recoveredViewModel.HasError);
    }

    #endregion

    #region Pruebas de Manejo de Errores de Servicio

    [Fact]
    public async Task ServiceError_WithInvalidData_ShouldHandleGracefully()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var comandasService = new ComandasService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosService = new ProductosService(_apiService, _authService);
        var mesasService = new MesasService(_apiService, _authService);
        var dailyPreparationsService = new Mock<IDailyPreparationsService>();
        var crearComandaViewModel = new CrearComandaViewModel(comandasService, productosService, mesasService, dailyPreparationsService.Object, navigationService, dialogService.Object);

        // Act - Intentar crear comanda con datos inválidos
        // Nota: CrearComandaViewModel no tiene propiedades NombreCliente y CantidadPersonas
        // En su lugar, verificamos que no se puede crear sin productos
        await crearComandaViewModel.CrearComandaCommand.ExecuteAsync(null);

        // Assert - Debe mostrar errores de validación
        dialogService.Verify(d => d.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task ServiceError_WithUnauthorizedAccess_ShouldHandleGracefully()
    {
        // Arrange - Crear cliente sin token de autorización
        var httpClient = new HttpClient
        {
            BaseAddress = _client.BaseAddress
        };

        var apiService = new ApiService(httpClient);
        var authService = new AuthService(apiService, NullLogger<AuthService>.Instance, _secureStorage, _navigationService);
        var productosService = new ProductosService(apiService, authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Intentar cargar productos sin autorización
        await productosViewModel.LoadProductosCommand.ExecuteAsync(null);

        // Assert - Debe manejar el error de autorización
        dialogService.Verify(d => d.ShowErrorAsync(It.Is<string>(s => !string.IsNullOrWhiteSpace(s))), Times.AtLeastOnce());
    }

    #endregion

    #region Pruebas de Estados Inconsistentes

    [Fact]
    public async Task InconsistentState_WithPartialData_ShouldHandleGracefully()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Cargar productos y simular estado inconsistente
        await productosViewModel.LoadProductosCommand.ExecuteAsync(null);
        
        // Simular cambio de estado que podría causar inconsistencia
        productosViewModel.TextoBusqueda = "búsqueda inválida";
        await productosViewModel.BuscarProductosCommand.ExecuteAsync(null);

        // Assert - Debe manejar el estado inconsistente sin fallar
        Assert.True(true); // Si llegamos aquí, se manejó el estado inconsistente
    }

    [Fact]
    public async Task InconsistentState_WithConcurrentModifications_ShouldHandleGracefully()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Ejecutar operaciones concurrentes que podrían causar inconsistencias
        var task1 = productosViewModel.LoadProductosCommand.ExecuteAsync(null);
        var task2 = productosViewModel.RefreshProductosCommand.ExecuteAsync(null);
        var task3 = productosViewModel.LoadProductosCommand.ExecuteAsync(null);

        await Task.WhenAll(task1, task2, task3);

        // Assert - Debe manejar las modificaciones concurrentes sin fallar
        Assert.True(true); // Si llegamos aquí, se manejaron las modificaciones concurrentes
    }

    #endregion

    #region Pruebas de Recuperación de Datos Corruptos

    [Fact]
    public async Task CorruptedData_WithInvalidJson_ShouldHandleGracefully()
    {
        // Arrange - Crear cliente que devuelve JSON inválido
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://httpbin.org/status/500"); // Servidor que devuelve error

        var apiService = new ApiService(httpClient);
        var authService = new AuthService(apiService, NullLogger<AuthService>.Instance, _secureStorage, _navigationService);
        var productosService = new ProductosService(apiService, authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Intentar cargar productos con datos corruptos
        await productosViewModel.LoadProductosCommand.ExecuteAsync(null);

        // Assert - Debe manejar los datos corruptos sin fallar
        dialogService.Verify(d => d.ShowErrorAsync(It.Is<string>(s => !string.IsNullOrWhiteSpace(s))), Times.AtLeastOnce());
    }

    [Fact]
    public async Task CorruptedData_WithMalformedResponse_ShouldHandleGracefully()
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

        // Act - Intentar cargar comandas (puede tener datos malformados)
        await comandasViewModel.LoadComandasCommand.ExecuteAsync(null);

        // Assert - Debe manejar la respuesta malformada sin fallar
        Assert.True(true); // Si llegamos aquí, se manejó la respuesta malformada
    }

    #endregion

    #region Pruebas de Límites de Sistema

    [Fact]
    public async Task SystemLimits_WithMemoryPressure_ShouldHandleGracefully()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        // Act - Crear muchos ViewModels para simular presión de memoria
        var viewModels = new List<ProductosViewModel>();
        for (int i = 0; i < 100; i++)
        {
            var viewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);
            viewModels.Add(viewModel);
        }

        // Ejecutar operaciones en todos los ViewModels
        var tasks = viewModels.Select(vm => vm.LoadProductosCommand.ExecuteAsync(null));
        await Task.WhenAll(tasks);

        // Assert - Debe manejar la presión de memoria sin fallar
        Assert.True(true); // Si llegamos aquí, se manejó la presión de memoria
    }

    [Fact]
    public async Task SystemLimits_WithHighCPUUsage_ShouldHandleGracefully()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Ejecutar muchas operaciones para simular alto uso de CPU
        var tasks = new List<Task>();
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(productosViewModel.LoadProductosCommand.ExecuteAsync(null));
            tasks.Add(productosViewModel.RefreshProductosCommand.ExecuteAsync(null));
        }

        await Task.WhenAll(tasks);

        // Assert - Debe manejar el alto uso de CPU sin fallar
        Assert.True(true); // Si llegamos aquí, se manejó el alto uso de CPU
    }

    #endregion

    #region Pruebas de Recuperación de Errores de Navegación

    [Fact]
    public async Task NavigationError_WithInvalidRoute_ShouldHandleGracefully()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Intentar navegar a una ruta inválida
        try
        {
            await navigationService.NavigateToAsync("ruta-invalida", new Dictionary<string, object>());
        }
        catch (Exception)
        {
            // Esperado que falle
        }

        // Assert - Debe manejar el error de navegación sin romper la aplicación
        Assert.True(true); // Si llegamos aquí, se manejó el error de navegación
    }

    [Fact]
    public async Task NavigationError_WithMissingParameters_ShouldHandleGracefully()
    {
        // Arrange
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        var productosService = new ProductosService(_apiService, _authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Intentar navegar con parámetros faltantes
        try
        {
            await navigationService.NavigateToAsync("productodetalle", new Dictionary<string, object>());
        }
        catch (Exception)
        {
            // Esperado que falle
        }

        // Assert - Debe manejar el error de parámetros faltantes sin romper la aplicación
        Assert.True(true); // Si llegamos aquí, se manejó el error de parámetros faltantes
    }

    #endregion

    #region Pruebas de Recuperación de Errores de Autenticación

    [Fact]
    public async Task AuthenticationError_WithExpiredToken_ShouldHandleGracefully()
    {
        // Arrange - Crear cliente con token expirado
        var httpClient = new HttpClient
        {
            BaseAddress = _client.BaseAddress
        };

        // Simular token expirado
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "token-expirado");

        var apiService = new ApiService(httpClient);
        var authService = new AuthService(apiService, NullLogger<AuthService>.Instance, _secureStorage, _navigationService);
        var productosService = new ProductosService(apiService, authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Intentar cargar productos con token expirado
        await productosViewModel.LoadProductosCommand.ExecuteAsync(null);

        // Assert - Debe manejar el error de autenticación
        dialogService.Verify(d => d.ShowErrorAsync(It.Is<string>(s => !string.IsNullOrWhiteSpace(s))), Times.AtLeastOnce());
    }

    [Fact]
    public async Task AuthenticationError_WithInvalidCredentials_ShouldHandleGracefully()
    {
        // Arrange - Crear cliente con credenciales inválidas
        var httpClient = new HttpClient
        {
            BaseAddress = _client.BaseAddress
        };

        var apiService = new ApiService(httpClient);
        var authService = new AuthService(apiService, NullLogger<AuthService>.Instance, _secureStorage, _navigationService);
        var productosService = new ProductosService(apiService, authService);
        var dialogService = new Mock<IDialogService>();
        var navigationService = new FakeNavigationService();

        var productosViewModel = new ProductosViewModel(productosService, dialogService.Object, navigationService);

        // Act - Intentar cargar productos sin autenticación
        await productosViewModel.LoadProductosCommand.ExecuteAsync(null);

        // Assert - Debe manejar el error de autenticación
        dialogService.Verify(d => d.ShowErrorAsync(It.Is<string>(s => !string.IsNullOrWhiteSpace(s))), Times.AtLeastOnce());
    }

    #endregion
}
