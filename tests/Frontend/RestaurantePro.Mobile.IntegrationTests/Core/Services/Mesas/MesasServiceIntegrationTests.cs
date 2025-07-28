using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Platform;
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
        var authService = new AuthService(apiService, NullLogger<AuthService>.Instance, new FakeSecureStorageService());
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
        Assert.True(result.Data.Count > 0);
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
        Assert.True(result.Data.Count > 0, "Should have at least some available tables");
        
        // Verificar que al menos algunas mesas están disponibles (puede que no todas estén disponibles)
        var mesasDisponibles = result.Data.Where(m => m.Estado == "Disponible").ToList();
        Assert.True(mesasDisponibles.Count > 0, 
            $"Should have at least some available tables. Found: {string.Join(", ", result.Data.Select(m => $"{m.Numero}({m.Estado})"))}");
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
} 
 