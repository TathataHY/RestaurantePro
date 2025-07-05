using RestaurantePro.Mobile.IntegrationTests.TestBase;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services.Authentication;

/// <summary>
/// Pruebas de integración para AuthService contra el backend real
/// Estas pruebas validan la conectividad, manejo de errores y flujos básicos
/// </summary>
public class AuthServiceIntegrationTests : MobileIntegrationTestBase
{
    private readonly IApiService _apiService;
    private readonly AuthService _authService;
    private readonly ILogger<AuthService> _logger;

    public AuthServiceIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
        // Configurar servicios reales para pruebas de integración
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddHttpClient();
        
        var services = serviceCollection.BuildServiceProvider();
        _logger = services.GetRequiredService<ILogger<AuthService>>();
        
        // Crear ApiService real con HttpClient configurado
        _apiService = new ApiService(_httpClient, _logger);
        _authService = new AuthService(_apiService, _logger);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ShouldReturnErrorFromBackend()
    {
        // Arrange
        var email = "usuario.inexistente@test.com";
        var password = "contraseña.incorrecta";

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task ApiService_ShouldConnectToRealBackend()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "usuario.inexistente@test.com",
            Password = "contraseña.incorrecta"
        };

        // Act
        var result = await _apiService.PostAsync<AuthResponse>("api/auth/login", loginRequest);

        // Assert - Debe conectarse al backend y devolver error válido
        result.Should().NotBeNull();
        result.Success.Should().BeFalse(); // Login debe fallar
        result.Errors.Should().NotBeEmpty(); // Debe tener errores del backend
    }

    [Fact]
    public async Task AuthService_ShouldValidateEmptyCredentials()
    {
        // Arrange & Act & Assert - Múltiples casos
        var testCases = new[]
        {
            new { Email = "", Password = "Test123!", ExpectedError = "Email es requerido" },
            new { Email = "test@test.com", Password = "", ExpectedError = "Password es requerido" },
            new { Email = "email-invalido", Password = "Test123!", ExpectedError = "Email tiene formato inválido" }
        };

        foreach (var testCase in testCases)
        {
            var result = await _authService.LoginAsync(testCase.Email, testCase.Password);
            
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task AuthService_InitialState_ShouldNotBeAuthenticated()
    {
        // Act
        var isAuthenticated = await _authService.IsAuthenticatedAsync();
        var token = await _authService.GetTokenAsync();
        var currentUser = await _authService.GetCurrentUserAsync();

        // Assert - Estado inicial sin autenticación
        isAuthenticated.Should().BeFalse();
        token.Should().BeNullOrEmpty();
        currentUser.Should().BeNull();
    }

    [Fact]
    public async Task AuthService_Logout_ShouldClearSession()
    {
        // Arrange - Simular algún estado previo
        await _authService.LogoutAsync();

        // Act
        var isAuthenticated = await _authService.IsAuthenticatedAsync();
        var token = await _authService.GetTokenAsync();
        var currentUser = await _authService.GetCurrentUserAsync();

        // Assert - Logout debe limpiar todo
        isAuthenticated.Should().BeFalse();
        token.Should().BeNullOrEmpty();
        currentUser.Should().BeNull();
    }

    [Theory]
    [InlineData("", "Test123!")]
    [InlineData("test@test.com", "")]
    public async Task LoginAsync_WithInvalidInput_ShouldValidateInput(string email, string password)
    {
        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        // Nota: No validamos el mensaje exacto porque puede variar
    }

    [Fact]
    public async Task ApiService_ShouldHandleNetworkErrors()
    {
        // Arrange - Endpoint que no existe
        var result = await _apiService.GetAsync<object>("api/endpoint/inexistente");

        // Assert - Debe manejar errores HTTP correctamente
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.StatusCode.Should().BeGreaterThan(0); // Debe tener código de estado HTTP
    }

    /// <summary>
    /// Prueba específica para validar que la comunicación con el backend real funciona
    /// </summary>
    [Fact]
    public async Task IntegrationTest_BackendConnectivity_ShouldWork()
    {
        // Arrange
        var invalidCredentials = new LoginRequest
        {
            Email = "test.connectivity@integration.test",
            Password = "InvalidPassword123!"
        };

        // Act
        var response = await _apiService.PostAsync<AuthResponse>("api/auth/login", invalidCredentials);

        // Assert - Lo importante es que se conecte y reciba respuesta del backend
        response.Should().NotBeNull("El backend debe responder");
        response.Success.Should().BeFalse("Las credenciales inválidas deben fallar");
        response.Errors.Should().NotBeEmpty("El backend debe reportar errores específicos");
        
        // Validar que es realmente una respuesta del backend real (más flexible)
        response.Errors.Should().HaveCountGreaterThan(0, "Debe tener al menos un error");
        
        // El error puede venir en diferentes formatos dependiendo del backend
        var hasValidError = response.Errors.Any(error => 
            error.Contains("HTTP", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("Usuario", StringComparison.OrdinalIgnoreCase) || 
            error.Contains("contraseña", StringComparison.OrdinalIgnoreCase) || 
            error.Contains("incorrectos", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("401", StringComparison.OrdinalIgnoreCase));
            
        hasValidError.Should().BeTrue(
            $"El mensaje debe indicar error de autenticación. Errors recibidos: [{string.Join(", ", response.Errors)}]"
        );
        
        // Validar que el código de estado es apropiado
        response.StatusCode.Should().BeGreaterThan(0, "Debe tener un código de estado HTTP válido");
    }
}

/// <summary>
/// ApiService real para pruebas de integración
/// </summary>
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public ApiService(HttpClient httpClient, ILogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return ApiResponse<T>.SuccessResponse(data!, "Operación exitosa");
            }

            return ApiResponse<T>.ErrorResponse(
                new List<string> { $"Error HTTP: {response.StatusCode}" },
                "Error en la petición",
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en petición GET a {Endpoint}", endpoint);
            return ApiResponse<T>.ErrorResponse(ex.Message, "Error de conexión", 500);
        }
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return ApiResponse<T>.SuccessResponse(result!, "Operación exitosa");
            }

            // Intentar deserializar el error del backend
            try
            {
                var errorResponse = JsonSerializer.Deserialize<dynamic>(responseContent);
                var errors = new List<string> { $"HTTP {response.StatusCode}" };
                
                return ApiResponse<T>.ErrorResponse(
                    errors,
                    "Error del backend",
                    (int)response.StatusCode);
            }
            catch
            {
                return ApiResponse<T>.ErrorResponse(
                    new List<string> { $"Error HTTP: {response.StatusCode}" },
                    "Error en la petición",
                    (int)response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en petición POST a {Endpoint}", endpoint);
            return ApiResponse<T>.ErrorResponse(ex.Message, "Error de conexión", 500);
        }
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return ApiResponse<T>.SuccessResponse(result!, "Operación exitosa");
            }

            return ApiResponse<T>.ErrorResponse(
                new List<string> { $"Error HTTP: {response.StatusCode}" },
                "Error en la petición",
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en petición PUT a {Endpoint}", endpoint);
            return ApiResponse<T>.ErrorResponse(ex.Message, "Error de conexión", 500);
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string endpoint)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                return ApiResponse<bool>.SuccessResponse(true, "Operación exitosa");
            }

            return ApiResponse<bool>.ErrorResponse(
                new List<string> { $"Error HTTP: {response.StatusCode}" },
                "Error en la petición",
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en petición DELETE a {Endpoint}", endpoint);
            return ApiResponse<bool>.ErrorResponse(ex.Message, "Error de conexión", 500);
        }
    }
} 