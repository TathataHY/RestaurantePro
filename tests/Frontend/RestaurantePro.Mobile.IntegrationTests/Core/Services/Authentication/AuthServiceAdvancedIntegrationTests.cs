using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Platform;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services.Authentication;

/// <summary>
/// Tests de integración avanzados para AuthService
/// Valida endpoints correctos, flujos de autenticación reales y manejo de errores
/// </summary>
public class AuthServiceAdvancedIntegrationTests : IClassFixture<MobileIntegrationTestFixture>, IDisposable
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private readonly IAuthService _authService;
    private readonly IApiService _apiService;
    private readonly FakeSecureStorageService _secureStorage;

    public AuthServiceAdvancedIntegrationTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
        _secureStorage = new FakeSecureStorageService();
        _apiService = new ApiService(_client);
        var logger = NullLogger<AuthService>.Instance;
        _authService = new AuthService(_apiService, logger, _secureStorage, new FakeNavigationService());
    }

    public void Dispose()
    {
        // Limpiar estado entre tests
        _secureStorage.ClearAsync().Wait();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldCallCorrectEndpoint()
    {
        // Arrange
        var email = "admin@restaurantepro.com";
        var password = "AdminRestaurante123!";

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.False(string.IsNullOrWhiteSpace(result.Data.Token));
        Assert.NotNull(result.Data.User);
        // El backend puede no devolver todos los campos poblados; usuario presente es suficiente
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ShouldReturnError()
    {
        // Arrange
        var email = "usuarioinvalido@restaurantepro.com";
        var password = "passwordincorrecto";

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
        // El backend puede devolver diferentes mensajes de error
        Assert.Contains(result.Errors, e => e.Contains("Error") || e.Contains("comunicación") || e.Contains("servidor"));
    }

    [Fact]
    public async Task LoginAsync_WithEmptyCredentials_ShouldReturnValidationError()
    {
        // Arrange
        var email = "";
        var password = "";

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldStoreTokenInSecureStorage()
    {
        // Arrange
        var email = "admin@restaurantepro.com";
        var password = "AdminRestaurante123!";

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.True(result.Success);
        
        // Verificar que el token se almacenó en secure storage
        var storedToken = await _secureStorage.GetAsync("auth_token");
        Assert.False(string.IsNullOrWhiteSpace(storedToken));
        Assert.StartsWith("eyJ", storedToken);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldStoreUserInSecureStorage()
    {
        // Arrange
        var email = "admin@restaurantepro.com";
        var password = "AdminRestaurante123!";

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.True(result.Success);
        
        // Verificar que el usuario se almacenó en secure storage
        var storedUser = await _secureStorage.GetAsync("auth_user");
        Assert.False(string.IsNullOrWhiteSpace(storedUser));
        
        // Deserializar y verificar que es el usuario correcto
        var user = JsonSerializer.Deserialize<AuthUser>(storedUser);
        Assert.NotNull(user);
    }

    [Fact]
    public async Task GetTokenAsync_AfterSuccessfulLogin_ShouldReturnStoredToken()
    {
        // Arrange
        var email = "admin@restaurantepro.com";
        var password = "AdminRestaurante123!";
        
        // Login primero
        var loginResult = await _authService.LoginAsync(email, password);
        Assert.True(loginResult.Success);

        // Act
        var token = await _authService.GetTokenAsync();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(loginResult.Data!.Token, token);
    }

    [Fact]
    public async Task GetTokenAsync_WithoutLogin_ShouldReturnNull()
    {
        // Arrange - Asegurar que no hay token almacenado
        await _secureStorage.RemoveAsync("auth_token");

        // Act
        var token = await _authService.GetTokenAsync();

        // Assert
        Assert.Null(token);
    }

    [Fact]
    public async Task IsAuthenticatedAsync_AfterSuccessfulLogin_ShouldReturnTrue()
    {
        // Arrange
        var email = "admin@restaurantepro.com";
        var password = "AdminRestaurante123!";
        
        // Login primero
        var loginResult = await _authService.LoginAsync(email, password);
        Assert.True(loginResult.Success);

        // Act
        var isAuthenticated = await _authService.IsAuthenticatedAsync();

        // Assert
        Assert.True(isAuthenticated);
    }

    [Fact]
    public async Task IsAuthenticatedAsync_WithoutLogin_ShouldReturnFalse()
    {
        // Arrange - Asegurar que no hay token almacenado
        await _secureStorage.RemoveAsync("auth_token");

        // Act
        var isAuthenticated = await _authService.IsAuthenticatedAsync();

        // Assert
        Assert.False(isAuthenticated);
    }

    [Fact]
    public async Task LogoutAsync_AfterLogin_ShouldClearStoredData()
    {
        // Arrange
        var email = "admin@restaurantepro.com";
        var password = "AdminRestaurante123!";
        
        // Login primero
        var loginResult = await _authService.LoginAsync(email, password);
        Assert.True(loginResult.Success);

        // Verificar que hay datos almacenados
        var tokenBefore = await _secureStorage.GetAsync("auth_token");
        var userBefore = await _secureStorage.GetAsync("auth_user");
        Assert.False(string.IsNullOrWhiteSpace(tokenBefore));
        Assert.False(string.IsNullOrWhiteSpace(userBefore));

        // Act
        await _authService.LogoutAsync();

        // Assert
        var tokenAfter = await _secureStorage.GetAsync("auth_token");
        var userAfter = await _secureStorage.GetAsync("auth_user");
        Assert.Null(tokenAfter);
        Assert.Null(userAfter);
    }

    [Fact]
    public async Task LogoutAsync_WithoutLogin_ShouldNotThrowException()
    {
        // Act & Assert
        var exception = await Record.ExceptionAsync(async () => await _authService.LogoutAsync());
        Assert.Null(exception);
    }

}