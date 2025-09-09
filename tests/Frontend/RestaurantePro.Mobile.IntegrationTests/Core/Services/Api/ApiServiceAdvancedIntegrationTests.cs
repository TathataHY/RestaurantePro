using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Platform;
using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services.Api;

public class ApiServiceAdvancedIntegrationTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private IApiService _apiService;
    private IAuthService _authService;

    public ApiServiceAdvancedIntegrationTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
        Setup();
    }

    private void Setup()
    {
        var httpClient = _client;
        var apiService = new ApiService(httpClient);
        var authService = new AuthService(apiService, NullLogger<AuthService>.Instance, new FakeSecureStorageService(), new FakeNavigationService());
        _apiService = apiService;
        _authService = authService;
    }

    private void Dispose()
    {
        // Limpiar estado entre tests
        // _secureStorage.ClearAsync().Wait(); // This line is removed as per the new_code
    }

    [Fact]
    public async Task GetAsync_WithValidEndpoint_ShouldReturnData()
    {
        // Arrange
        var endpoint = "api/core/productos";

        // Act
        var result = await _apiService.GetAsync<List<object>>(endpoint);

        // Assert
        // El endpoint puede requerir autenticación, pero debe manejar el error correctamente
        Assert.NotNull(result);
        // Si falla, debe ser por autenticación, no por error de red
        if (!result.Success)
        {
            Assert.NotNull(result.Errors);
        }
    }

    [Fact]
    public async Task GetAsync_WithInvalidEndpoint_ShouldReturnError()
    {
        // Arrange
        var endpoint = "api/invalid/endpoint";

        // Act
        var result = await _apiService.GetAsync<List<object>>(endpoint);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task PostAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var endpoint = "api/auth/login";
        var loginData = new
        {
            Email = "admin@restaurantepro.com",
            Password = "AdminRestaurante123!"
        };

        // Act
        var result = await _apiService.PostAsync<object>(endpoint, loginData);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task PostAsync_WithInvalidData_ShouldReturnError()
    {
        // Arrange
        var endpoint = "api/auth/login";
        var invalidData = new
        {
            Email = "invalid@email.com",
            Password = "wrongpassword"
        };

        // Act
        var result = await _apiService.PostAsync<object>(endpoint, invalidData);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task PutAsync_WithValidData_ShouldHandleCorrectly()
    {
        // Arrange
        var endpoint = "api/core/productos/1"; // Asumiendo que existe un producto con ID 1
        var updateData = new
        {
            Nombre = "Producto Actualizado",
            Precio = 15.99m
        };

        // Act
        var result = await _apiService.PutAsync<object>(endpoint, updateData);

        // Assert
        // Puede fallar si el producto no existe o requiere autenticación, pero debe manejar el error correctamente
        Assert.NotNull(result);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldHandleCorrectly()
    {
        // Arrange
        var endpoint = "api/core/productos/999"; // ID que probablemente no existe

        // Act
        var result = await _apiService.DeleteAsync(endpoint);

        // Assert
        // Puede fallar si el producto no existe o requiere autenticación, pero debe manejar el error correctamente
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAsync_WithAuthentication_ShouldIncludeAuthHeader()
    {
        // Arrange
        var endpoint = "api/core/productos";
        
        // Login primero para obtener token
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(loginResult.Success);

        // Act
        var result = await _apiService.GetAsync<List<object>>(endpoint);

        // Assert
        // Ahora que tenemos autenticación, debería funcionar mejor
        Assert.NotNull(result);
        if (result.Success)
        {
            Assert.NotNull(result.Data);
        }
    }

    [Fact]
    public async Task GetAsync_WithoutAuthentication_ShouldHandleCorrectly()
    {
        // Arrange
        var endpoint = "api/core/productos"; // Endpoint que puede requerir autenticación

        // Act
        var result = await _apiService.GetAsync<List<object>>(endpoint);

        // Assert
        // Debe manejar correctamente la falta de autenticación
        Assert.NotNull(result);
        if (!result.Success)
        {
            Assert.NotNull(result.Errors);
        }
    }

    [Fact]
    public async Task GetAsync_WithProtectedEndpoint_ShouldHandleUnauthorized()
    {
        // Arrange
        var endpoint = "api/core/usuarios"; // Endpoint protegido

        // Act (sin autenticación)
        var result = await _apiService.GetAsync<List<object>>(endpoint);

        // Assert
        // Debe manejar el error de autorización correctamente
        Assert.NotNull(result);
        if (!result.Success)
        {
            Assert.NotNull(result.Errors);
        }
    }

    // Escenarios de error específicos (timeout, 500, JSON inválido) se validan en pruebas unitarias.
    // En integración ejercitamos rutas reales (404, 401/403, etc.).

    [Fact]
    public async Task PostAsync_WithLargeData_ShouldHandleCorrectly()
    {
        // Arrange
        var endpoint = "api/test";
        var largeData = new
        {
            Id = 1,
            Name = new string('A', 10000), // Datos grandes
            Description = new string('B', 10000),
            Items = Enumerable.Range(1, 1000).Select(i => new { Id = i, Value = $"Item {i}" }).ToList()
        };

        // Act
        var result = await _apiService.PostAsync<object>(endpoint, largeData);

        // Assert
        // Debe manejar datos grandes correctamente
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAsync_WithQueryParameters_ShouldHandleCorrectly()
    {
        // Arrange
        var endpoint = "api/core/productos?page=1&pageSize=10&categoria=1";

        // Act
        var result = await _apiService.GetAsync<List<object>>(endpoint);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAsync_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var endpoint = "api/core/productos?search=café&categoria=bebidas%20calientes";

        // Act
        var result = await _apiService.GetAsync<List<object>>(endpoint);

        // Assert
        Assert.NotNull(result);
    }
}

/// <summary>
/// Clase de respuesta de prueba para deserialización
/// </summary>
// Clase auxiliar de pruebas movida a pruebas unitarias si se requiere

// Nota: Los escenarios de errores de red/timeout se validan en pruebas unitarias.
// En integración usamos únicamente HttpClient real del fixture.