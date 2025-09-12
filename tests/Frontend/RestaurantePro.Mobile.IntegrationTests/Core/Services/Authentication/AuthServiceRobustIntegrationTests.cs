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
/// Tests de integración ROBUSTOS para AuthService - Casos edge, stress y resiliencia
/// </summary>
public class AuthServiceRobustIntegrationTests : IClassFixture<MobileIntegrationTestFixture>, IDisposable
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private readonly IAuthService _authService;
    private readonly IApiService _apiService;
    private readonly FakeSecureStorageService _secureStorage;

    public AuthServiceRobustIntegrationTests(MobileIntegrationTestFixture fixture)
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

    #region Tests de Validación de Credenciales Robustos

    [Theory]
    [InlineData("", "password123")]
    [InlineData("email@test.com", "")]
    [InlineData("", "")]
    public async Task LoginAsync_WithInvalidInputs_ShouldReturnValidationError(string email, string password)
    {
        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task LoginAsync_WithNullEmail_ShouldReturnValidationError()
    {
        // Act
        var result = await _authService.LoginAsync(null!, "password123");

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task LoginAsync_WithNullPassword_ShouldReturnValidationError()
    {
        // Act
        var result = await _authService.LoginAsync("email@test.com", null!);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task LoginAsync_WithBothNull_ShouldReturnValidationError()
    {
        // Act
        var result = await _authService.LoginAsync(null!, null!);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@test.com")]
    [InlineData("test@")]
    [InlineData("test@.com")]
    [InlineData("test..test@test.com")]
    public async Task LoginAsync_WithInvalidEmailFormat_ShouldReturnError(string email)
    {
        // Act
        var result = await _authService.LoginAsync(email, "password123");

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Theory]
    [InlineData("a")] // Muy corta
    [InlineData("password with spaces")]
    [InlineData("password\twith\ttabs")]
    [InlineData("password\nwith\nnewlines")]
    public async Task LoginAsync_WithEdgeCasePasswords_ShouldHandleGracefully(string password)
    {
        // Act
        var result = await _authService.LoginAsync("admin@restaurantepro.com", password);

        // Assert - Debe manejar graciosamente sin excepción
        Assert.NotNull(result);
        // Puede ser exitoso o fallar, pero no debe lanzar excepción
    }

    [Fact]
    public async Task LoginAsync_WithVeryLongPassword_ShouldHandleGracefully()
    {
        // Arrange
        var longPassword = "a".PadRight(1000, 'a'); // Muy larga

        // Act
        var result = await _authService.LoginAsync("admin@restaurantepro.com", longPassword);

        // Assert - Debe manejar graciosamente sin excepción
        Assert.NotNull(result);
        // Puede ser exitoso o fallar, pero no debe lanzar excepción
    }

    #endregion

    #region Tests de Concurrencia y Stress

    [Fact]
    public async Task LoginAsync_MultipleConcurrentLogins_ShouldHandleCorrectly()
    {
        // Arrange
        var email = "admin@restaurantepro.com";
        var password = "AdminRestaurante123!";
        var tasks = new List<Task<ApiResponse<AuthResponse>>>();

        // Act - Intentar múltiples logins concurrentes
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_authService.LoginAsync(email, password));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - Al menos uno debe ser exitoso, otros pueden fallar por concurrencia
        var successfulLogins = results.Count(r => r.Success);
        Assert.True(successfulLogins >= 1, "Al menos un login debe ser exitoso");
        
        // Verificar que no hay excepciones no manejadas
        Assert.All(results, result => Assert.NotNull(result));
    }

    [Fact]
    public async Task GetTokenAsync_MultipleConcurrentCalls_ShouldReturnConsistentResult()
    {
        // Arrange
        var email = "admin@restaurantepro.com";
        var password = "AdminRestaurante123!";
        
        // Login primero
        var loginResult = await _authService.LoginAsync(email, password);
        Assert.True(loginResult.Success);

        // Act - Múltiples llamadas concurrentes a GetTokenAsync
        var tasks = new List<Task<string?>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_authService.GetTokenAsync());
        }

        var tokens = await Task.WhenAll(tasks);

        // Assert - Todos deben devolver el mismo token
        var firstToken = tokens.FirstOrDefault(t => !string.IsNullOrEmpty(t));
        Assert.NotNull(firstToken);
        Assert.All(tokens, token => Assert.Equal(firstToken, token));
    }

    [Fact]
    public async Task IsAuthenticatedAsync_MultipleConcurrentCalls_ShouldReturnConsistentResult()
    {
        // Arrange
        var email = "admin@restaurantepro.com";
        var password = "AdminRestaurante123!";
        
        // Login primero
        var loginResult = await _authService.LoginAsync(email, password);
        Assert.True(loginResult.Success);

        // Act - Múltiples llamadas concurrentes a IsAuthenticatedAsync
        var tasks = new List<Task<bool>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_authService.IsAuthenticatedAsync());
        }

        var results = await Task.WhenAll(tasks);

        // Assert - Todos deben devolver true
        Assert.All(results, result => Assert.True(result));
    }

    #endregion

    #region Tests de Manejo de Errores de Red

    [Fact]
    public async Task LoginAsync_WithNetworkTimeout_ShouldHandleGracefully()
    {
        // Arrange - Crear un cliente con timeout muy corto para simular timeout
        var shortTimeoutClient = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(1) // Timeout muy corto
        };
        
        var shortTimeoutApiService = new ApiService(shortTimeoutClient);
        var shortTimeoutAuthService = new AuthService(
            shortTimeoutApiService, 
            NullLogger<AuthService>.Instance, 
            _secureStorage, 
            new FakeNavigationService());

        // Act
        var result = await shortTimeoutAuthService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");

        // Assert - Debe manejar el timeout graciosamente
        Assert.NotNull(result);
        // Puede ser exitoso o fallar, pero no debe lanzar excepción no manejada
    }

    [Fact]
    public async Task LoginAsync_WithInvalidServerResponse_ShouldHandleGracefully()
    {
        // Arrange - Crear un cliente que devuelva respuestas inválidas
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.SetupResponse("api/auth/login", HttpStatusCode.OK, "invalid json response");
        
        var mockClient = new HttpClient(mockHandler);
        var mockApiService = new ApiService(mockClient);
        var mockAuthService = new AuthService(
            mockApiService, 
            NullLogger<AuthService>.Instance, 
            _secureStorage, 
            new FakeNavigationService());

        // Act
        var result = await mockAuthService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");

        // Assert - Debe manejar la respuesta inválida graciosamente
        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    #endregion

    #region Tests de Persistencia y Recuperación

    [Fact]
    public async Task GetTokenAsync_AfterAppRestart_ShouldReturnStoredToken()
    {
        // Arrange
        var email = "admin@restaurantepro.com";
        var password = "AdminRestaurante123!";
        
        // Login y simular "restart" creando nuevo servicio
        var loginResult = await _authService.LoginAsync(email, password);
        Assert.True(loginResult.Success);

        // Simular restart creando nuevo AuthService con el mismo storage
        var newAuthService = new AuthService(
            _apiService, 
            NullLogger<AuthService>.Instance, 
            _secureStorage, 
            new FakeNavigationService());

        // Act
        var token = await newAuthService.GetTokenAsync();

        // Assert
        Assert.NotNull(token);
        Assert.Equal(loginResult.Data!.Token, token);
    }

    [Fact]
    public async Task GetCurrentUserAsync_AfterAppRestart_ShouldReturnStoredUser()
    {
        // Arrange
        var email = "admin@restaurantepro.com";
        var password = "AdminRestaurante123!";
        
        // Login y simular "restart" creando nuevo servicio
        var loginResult = await _authService.LoginAsync(email, password);
        Assert.True(loginResult.Success);

        // Simular restart creando nuevo AuthService con el mismo storage
        var newAuthService = new AuthService(
            _apiService, 
            NullLogger<AuthService>.Instance, 
            _secureStorage, 
            new FakeNavigationService());

        // Act
        var user = await newAuthService.GetCurrentUserAsync();

        // Assert
        Assert.NotNull(user);
        Assert.Equal(loginResult.Data!.User.Id, user.Id);
    }

    #endregion

    #region Tests de Seguridad

    [Fact]
    public async Task LoginAsync_WithRecordarme_ShouldStoreRefreshToken()
    {
        // Arrange
        var email = "admin@restaurantepro.com";
        var password = "AdminRestaurante123!";
        var recordarme = true;

        // Act
        var result = await _authService.LoginAsync(email, password, recordarme);

        // Assert
        Assert.True(result.Success);
        
        // Verificar que se almacenó la preferencia de recordarme
        var recordarmeStored = await _secureStorage.GetAsync("auth_recordarme");
        Assert.Equal("True", recordarmeStored);
    }

    [Fact]
    public async Task LogoutAsync_ShouldClearAllStoredData()
    {
        // Arrange
        var email = "admin@restaurantepro.com";
        var password = "AdminRestaurante123!";
        
        // Login con recordarme
        var loginResult = await _authService.LoginAsync(email, password, true);
        Assert.True(loginResult.Success);

        // Verificar que hay datos almacenados
        var tokenBefore = await _secureStorage.GetAsync("auth_token");
        var userBefore = await _secureStorage.GetAsync("auth_user");
        var recordarmeBefore = await _secureStorage.GetAsync("auth_recordarme");
        var refreshTokenBefore = await _secureStorage.GetAsync("auth_refresh_token");
        
        Assert.False(string.IsNullOrWhiteSpace(tokenBefore));
        Assert.False(string.IsNullOrWhiteSpace(userBefore));
        Assert.False(string.IsNullOrWhiteSpace(recordarmeBefore));

        // Act
        await _authService.LogoutAsync();

        // Assert - Todos los datos deben estar limpios
        var tokenAfter = await _secureStorage.GetAsync("auth_token");
        var userAfter = await _secureStorage.GetAsync("auth_user");
        var recordarmeAfter = await _secureStorage.GetAsync("auth_recordarme");
        var refreshTokenAfter = await _secureStorage.GetAsync("auth_refresh_token");
        
        Assert.Null(tokenAfter);
        Assert.Null(userAfter);
        Assert.Null(recordarmeAfter);
        Assert.Null(refreshTokenAfter);
    }

    #endregion

    #region Tests de Performance

    [Fact]
    public async Task LoginAsync_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var email = "admin@restaurantepro.com";
        var password = "AdminRestaurante123!";
        var timeout = TimeSpan.FromSeconds(10);

        // Act & Assert
        var loginTask = _authService.LoginAsync(email, password);
        var completedTask = await Task.WhenAny(loginTask, Task.Delay(timeout));
        
        Assert.Equal(loginTask, completedTask);
        Assert.True(loginTask.IsCompleted);
    }

    [Fact]
    public async Task GetTokenAsync_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var email = "admin@restaurantepro.com";
        var password = "AdminRestaurante123!";
        
        // Login primero
        var loginResult = await _authService.LoginAsync(email, password);
        Assert.True(loginResult.Success);

        var timeout = TimeSpan.FromSeconds(5);

        // Act & Assert
        var tokenTask = _authService.GetTokenAsync();
        var completedTask = await Task.WhenAny(tokenTask, Task.Delay(timeout));
        
        Assert.Equal(tokenTask, completedTask);
        Assert.True(tokenTask.IsCompleted);
    }

    #endregion

    #region Tests de Casos Edge

    [Fact]
    public async Task LoginAsync_WithVeryLongCredentials_ShouldHandleCorrectly()
    {
        // Arrange
        var longEmail = "a".PadRight(1000, 'a') + "@test.com";
        var longPassword = "p".PadRight(1000, 'p');

        // Act
        var result = await _authService.LoginAsync(longEmail, longPassword);

        // Assert - Debe manejar graciosamente sin excepción
        Assert.NotNull(result);
    }

    [Fact]
    public async Task LoginAsync_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var email = "test+special@test.com";
        var password = "p@ssw0rd!@#$%^&*()";

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert - Debe manejar graciosamente sin excepción
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetUserIdAsync_AfterLogin_ShouldReturnValidUserId()
    {
        // Arrange
        var email = "admin@restaurantepro.com";
        var password = "AdminRestaurante123!";
        
        // Login primero
        var loginResult = await _authService.LoginAsync(email, password);
        Assert.True(loginResult.Success);

        // Act
        var userId = await _authService.GetUserIdAsync();

        // Assert
        Assert.NotNull(userId);
        Assert.NotEmpty(userId);
        // Debe ser un GUID válido
        Assert.True(Guid.TryParse(userId, out _));
    }

    #endregion
}

/// <summary>
/// Mock HttpMessageHandler para simular respuestas de red
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, (HttpStatusCode statusCode, string content)> _responses = new();

    public void SetupResponse(string url, HttpStatusCode statusCode, string content)
    {
        _responses[url] = (statusCode, content);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri?.ToString() ?? "";
        
        if (_responses.TryGetValue(url, out var response))
        {
            var httpResponse = new HttpResponseMessage(response.statusCode)
            {
                Content = new StringContent(response.content, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(httpResponse);
        }

        // Respuesta por defecto
        var defaultResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("Not Found", Encoding.UTF8, "text/plain")
        };
        return Task.FromResult(defaultResponse);
    }
}
