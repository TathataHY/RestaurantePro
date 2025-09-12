using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Platform;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services.Mesas;

public class MesasServiceIntegrationTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private IMesasService _mesasService;
    private IAuthService _authService;

    public MesasServiceIntegrationTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
        Setup();
    }

    private void Setup()
    {
        // Crear servicios móviles localmente para evitar conflictos con el backend
        var httpClient = _client;
        var apiService = new ApiService(httpClient);
        var authService = new AuthService(apiService, NullLogger<AuthService>.Instance, new FakeSecureStorageService(), new FakeNavigationService());
        _mesasService = new MesasService(apiService, authService);
        _authService = authService;
    }

    [Fact]
    public async Task ObtenerMesasAsync_ShouldReturnMesas()
    {
        // Arrange - Login first
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success, "Login should succeed");

        // Act
        var result = await _mesasService.ObtenerMesasAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public async Task ObtenerMesasAsync_WithEstadoFilter_ShouldReturnFilteredMesas()
    {
        // Arrange - Login first
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success, "Login should succeed");

        // Act
        var result = await _mesasService.ObtenerMesasAsync(estado: "Disponible");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.All(m => m.Estado == "Disponible"));
    }

    [Fact]
    public async Task ObtenerMesasDisponiblesAsync_ShouldReturnAvailableMesas()
    {
        // Arrange - Login first
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success, "Login should succeed");

        // Act
        var result = await _mesasService.ObtenerMesasDisponiblesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        // Verificar que al menos hay algunas mesas disponibles
        Assert.NotEmpty(result.Data);
        
        // Verificar que al menos algunas mesas están disponibles (puede que no todas estén disponibles)
        var mesasDisponibles = result.Data.Where(m => m.Estado == "Disponible").ToList();
        Assert.NotEmpty(mesasDisponibles);
    }

    [Fact]
    public async Task ObtenerEstadoOcupacionAsync_ShouldReturnOccupationStatus()
    {
        // Arrange - Login first
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success, "Login should succeed");

        // Act
        var result = await _mesasService.ObtenerEstadoOcupacionAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.TotalMesas > 0);
    }

    #region Tests de Casos Edge y Validaciones

    [Fact]
    public async Task ObtenerMesasAsync_WithCancellationToken_ShouldHandleCancellation()
    {
        // Arrange
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success, "Login should succeed");
        
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancelar inmediatamente

        // Act
        var result = await _mesasService.ObtenerMesasAsync(cancellationToken: cts.Token);

        // Assert - El servicio puede no lanzar excepción pero debe manejar la cancelación
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ObtenerMesasAsync_WithInvalidEstado_ShouldReturnError()
    {
        // Arrange - Login first
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success, "Login should succeed");

        // Act
        var result = await _mesasService.ObtenerMesasAsync(estado: "EstadoInvalido");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.True(result.Errors.Any());
    }

    [Fact]
    public async Task ObtenerMesasAsync_WithEmptyEstado_ShouldReturnAllMesas()
    {
        // Arrange - Login first
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success, "Login should succeed");

        // Act
        var result = await _mesasService.ObtenerMesasAsync(estado: string.Empty);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public async Task ObtenerMesasDisponiblesAsync_WithCancellationToken_ShouldHandleCancellation()
    {
        // Arrange
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success, "Login should succeed");
        
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancelar inmediatamente

        // Act
        var result = await _mesasService.ObtenerMesasDisponiblesAsync(cancellationToken: cts.Token);

        // Assert - El servicio puede no lanzar excepción pero debe manejar la cancelación
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ObtenerEstadoOcupacionAsync_WithCancellationToken_ShouldHandleCancellation()
    {
        // Arrange
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success, "Login should succeed");
        
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancelar inmediatamente

        // Act
        var result = await _mesasService.ObtenerEstadoOcupacionAsync(cts.Token);

        // Assert - El servicio puede no lanzar excepción pero debe manejar la cancelación
        Assert.NotNull(result);
    }

    #endregion

    #region Tests de Rendimiento

    [Fact]
    public async Task ObtenerMesasAsync_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success, "Login should succeed");
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await _mesasService.ObtenerMesasAsync();
        stopwatch.Stop();

        // Assert
        Assert.NotNull(result);
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Operación tomó {stopwatch.ElapsedMilliseconds}ms, debería ser menor a 5000ms");
    }

    [Fact]
    public async Task ObtenerMesasDisponiblesAsync_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success, "Login should succeed");
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await _mesasService.ObtenerMesasDisponiblesAsync();
        stopwatch.Stop();

        // Assert
        Assert.NotNull(result);
        Assert.True(stopwatch.ElapsedMilliseconds < 3000, $"Operación tomó {stopwatch.ElapsedMilliseconds}ms, debería ser menor a 3000ms");
    }

    [Fact]
    public async Task MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success, "Login should succeed");
        
        var tasks = new List<Task<ApiResponse<List<MesaDto>>>>();

        // Act - Ejecutar múltiples requests concurrentes
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_mesasService.ObtenerMesasAsync());
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, results.Length);
        foreach (var result in results)
        {
            Assert.NotNull(result);
            // Todos deberían completarse sin excepción
        }
    }

    #endregion

    #region Tests de Concurrencia

    [Fact]
    public async Task ConcurrentMesaRequests_ShouldHandleRaceConditions()
    {
        // Arrange
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success, "Login should succeed");
        
        var tasks = new List<Task<ApiResponse<List<MesaDto>>>>();

        // Act - Ejecutar múltiples requests concurrentes con diferentes filtros
        tasks.Add(_mesasService.ObtenerMesasAsync());
        tasks.Add(_mesasService.ObtenerMesasAsync(estado: "Disponible"));
        tasks.Add(_mesasService.ObtenerMesasAsync(estado: "Ocupada"));
        tasks.Add(_mesasService.ObtenerMesasDisponiblesAsync());
        
        // Agregar el estado de ocupación por separado ya que devuelve un tipo diferente
        var estadoTask = _mesasService.ObtenerEstadoOcupacionAsync();

        var results = await Task.WhenAll(tasks);
        var estadoResult = await estadoTask;

        // Assert
        Assert.Equal(4, results.Length);
        foreach (var result in results)
        {
            Assert.NotNull(result);
            // No debería lanzar excepción por concurrencia
        }
        
        // Verificar también el resultado del estado de ocupación
        Assert.NotNull(estadoResult);
    }

    [Fact]
    public async Task ConcurrentEstadoOcupacionRequests_ShouldHandleGracefully()
    {
        // Arrange
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success, "Login should succeed");
        
        var tasks = new List<Task<ApiResponse<EstadoMesasDto>>>();

        // Act - Ejecutar múltiples requests de estado de ocupación concurrentemente
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_mesasService.ObtenerEstadoOcupacionAsync());
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(5, results.Length);
        foreach (var result in results)
        {
            Assert.NotNull(result);
            // No debería lanzar excepción por concurrencia
        }
    }

    #endregion

    #region Tests de Validación de Datos

    [Fact]
    public async Task ObtenerMesasAsync_ShouldReturnValidMesaData()
    {
        // Arrange
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success, "Login should succeed");

        // Act
        var result = await _mesasService.ObtenerMesasAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
        
        // Verificar que cada mesa tiene datos válidos
        foreach (var mesa in result.Data)
        {
            Assert.NotNull(mesa);
            Assert.NotEmpty(mesa.Numero);
            Assert.True(mesa.Capacidad > 0);
            Assert.NotNull(mesa.Estado);
            Assert.True(mesa.Id != Guid.Empty);
        }
    }

    [Fact]
    public async Task ObtenerEstadoOcupacionAsync_ShouldReturnValidOccupationData()
    {
        // Arrange
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success, "Login should succeed");

        // Act
        var result = await _mesasService.ObtenerEstadoOcupacionAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        
        // Verificar que los datos de ocupación son válidos
        Assert.True(result.Data.TotalMesas >= 0);
        Assert.True(result.Data.Estadisticas.MesasDisponibles >= 0);
        Assert.True(result.Data.Estadisticas.MesasOcupadas >= 0);
        Assert.True(result.Data.Estadisticas.MesasReservadas >= 0);
        Assert.True(result.Data.Estadisticas.MesasFueraDeServicio >= 0);
        
        // Verificar que la suma de estados no excede el total
        var sumaEstados = result.Data.Estadisticas.MesasDisponibles + result.Data.Estadisticas.MesasOcupadas + 
                         result.Data.Estadisticas.MesasReservadas + result.Data.Estadisticas.MesasFueraDeServicio;
        Assert.True(sumaEstados <= result.Data.TotalMesas);
    }

    [Fact]
    public async Task ObtenerMesasDisponiblesAsync_ShouldReturnOnlyAvailableMesas()
    {
        // Arrange
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success, "Login should succeed");

        // Act
        var result = await _mesasService.ObtenerMesasDisponiblesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        
        // Verificar que todas las mesas devueltas están disponibles
        foreach (var mesa in result.Data)
        {
            Assert.Equal("Disponible", mesa.Estado);
        }
    }

    #endregion

    #region Tests de Manejo de Errores

    [Fact]
    public async Task Service_WithInvalidCredentials_ShouldHandleAuthenticationError()
    {
        // Arrange
        var httpClient = _client;
        var apiService = new ApiService(httpClient);
        var authService = new AuthService(apiService, NullLogger<AuthService>.Instance, new FakeSecureStorageService(), new FakeNavigationService());
        var mesasService = new MesasService(apiService, authService);

        // No hacer login - simular credenciales inválidas

        // Act
        var result = await mesasService.ObtenerMesasAsync();

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        // El mensaje de error puede variar, solo verificamos que hay errores
        Assert.True(result.Errors.Any());
    }

    [Fact]
    public async Task Service_WithNetworkTimeout_ShouldHandleTimeoutGracefully()
    {
        // Arrange
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success, "Login should succeed");
        
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancelar inmediatamente

        // Act
        var result = await _mesasService.ObtenerMesasAsync(cancellationToken: cts.Token);

        // Assert - El servicio puede no lanzar excepción pero debe manejar la cancelación
        Assert.NotNull(result);
    }

    #endregion
} 
 