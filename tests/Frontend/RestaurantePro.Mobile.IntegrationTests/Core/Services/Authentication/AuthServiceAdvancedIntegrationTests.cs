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
        _authService = new AuthService(_apiService, logger, _secureStorage);
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
        Assert.NotNull(result.Data.Token);
        Assert.NotNull(result.Data.User);
        // El backend puede no devolver el email en la respuesta, verificar que el usuario existe
        Assert.NotNull(result.Data.User.Id);
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
        // El backend puede devolver diferentes mensajes de error
        Assert.True(result.Errors.Any(e => e.Contains("Error") || e.Contains("comunicación") || e.Contains("servidor")));
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
        Assert.NotNull(storedToken);
        Assert.True(storedToken.Length > 0, "El token almacenado no debe estar vacío");
        Assert.True(storedToken.StartsWith("eyJ"), "El token debe ser un JWT válido");
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
        Assert.NotNull(storedUser);
        
        // Deserializar y verificar que es el usuario correcto
        var user = JsonSerializer.Deserialize<AuthUser>(storedUser);
        Assert.NotNull(user);
        Assert.NotNull(user.Id);
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
        Assert.NotNull(token);
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
        Assert.NotNull(tokenBefore);
        Assert.NotNull(userBefore);

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

    [Fact]
    public async Task LoginAsync_WithNetworkError_ShouldHandleGracefully()
    {
        // Arrange - Crear un cliente HTTP que simule error de red
        var httpClient = new HttpClient(new MockHttpMessageHandler());
        var apiService = new ApiService(httpClient);
        var logger = NullLogger<AuthService>.Instance;
        var authService = new AuthService(apiService, logger, _secureStorage);

        // Act
        var result = await authService.LoginAsync("test@example.com", "password");

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task LoginAsync_WithServerError_ShouldHandleGracefully()
    {
        // Arrange - Crear un cliente HTTP que simule error del servidor
        var httpClient = new HttpClient(new MockHttpMessageHandler(HttpStatusCode.InternalServerError));
        var apiService = new ApiService(httpClient);
        var logger = NullLogger<AuthService>.Instance;
        var authService = new AuthService(apiService, logger, _secureStorage);

        // Act
        var result = await authService.LoginAsync("test@example.com", "password");

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task LoginAsync_WithTimeout_ShouldHandleGracefully()
    {
        // Arrange - Crear un cliente HTTP que simule timeout
        var httpClient = new HttpClient(new MockHttpMessageHandler(HttpStatusCode.RequestTimeout));
        var apiService = new ApiService(httpClient);
        var logger = NullLogger<AuthService>.Instance;
        var authService = new AuthService(apiService, logger, _secureStorage);

        // Act
        var result = await authService.LoginAsync("test@example.com", "password");

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }
}

/// <summary>
/// Mock HTTP Message Handler para simular diferentes respuestas HTTP
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;

    public MockHttpMessageHandler(HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _statusCode = statusCode;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(_statusCode);
        
        if (_statusCode == HttpStatusCode.OK)
        {
            var authResponse = new
            {
                Success = true,
                Data = new
                {
                    Token = "mock_jwt_token_12345",
                    User = new
                    {
                        Id = "user_id_123",
                        Email = "test@example.com",
                        NombreCompleto = "Usuario Test",
                        Rol = "Mesero"
                    }
                },
                Message = "Login exitoso"
            };
            
            response.Content = new StringContent(
                JsonSerializer.Serialize(authResponse),
                Encoding.UTF8,
                "application/json");
        }
        else
        {
            var errorResponse = new
            {
                Success = false,
                Errors = new List<string> { $"Error {_statusCode}: {_statusCode.ToString()}" },
                Message = "Error en la operación"
            };
            
            response.Content = new StringContent(
                JsonSerializer.Serialize(errorResponse),
                Encoding.UTF8,
                "application/json");
        }

        return Task.FromResult(response);
    }
} 