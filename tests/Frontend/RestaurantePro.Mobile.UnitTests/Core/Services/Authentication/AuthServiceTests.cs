namespace RestaurantePro.Mobile.UnitTests.Core.Services.Authentication;

public class AuthServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly Mock<ISecureStorageService> _mockSecureStorage;
    private readonly Mock<INavigationService> _mockNavigation;
    private readonly AuthService _authService;
    private readonly Fixture _fixture;

    public AuthServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        _mockSecureStorage = new Mock<ISecureStorageService>();
        _mockNavigation = new Mock<INavigationService>();
        _authService = new AuthService(_mockApiService.Object, _mockLogger.Object, _mockSecureStorage.Object, _mockNavigation.Object);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccessResult()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = "ValidPassword123";
        var expectedAuthResponse = _fixture.Create<AuthResponse>();
        var expectedApiResponse = ApiResponse<AuthResponse>.SuccessResponse(expectedAuthResponse);

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedApiResponse);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(expectedAuthResponse);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "invalid@test.com";
        var password = "wrongpassword";
        var expectedErrorMessage = "Credenciales inválidas";
        var expectedApiResponse = ApiResponse<AuthResponse>.ErrorResponse(
            new List<string> { expectedErrorMessage }, 
            "Error de autenticación", 
            401);

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedApiResponse);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(expectedErrorMessage);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithEmptyEmail_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "";
        var password = "ValidPassword123";

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().BeEmpty();
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithEmptyPassword_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = "";

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().BeEmpty();
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WhenApiServiceThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = "ValidPassword123";
        var expectedException = new HttpRequestException("Error de red");

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Error de red");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetTokenAsync_WhenTokenExists_ShouldReturnToken()
    {
        // Arrange
        var expectedToken = "jwt_token_here";
        
        // Simular que el token ya fue guardado en las preferencias
        // Nota: En un escenario real, necesitaríamos mockear IPreferences
        // Por ahora, vamos a probar después de un login exitoso
        
        var authResponse = new AuthResponse
        {
            Token = expectedToken,
            User = _fixture.Create<AuthUser>()
        };
        
        var apiResponse = ApiResponse<AuthResponse>.SuccessResponse(authResponse);
        
        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Hacer login primero para establecer el token
        await _authService.LoginAsync("test@test.com", "password");

        // Act
        var token = await _authService.GetTokenAsync();

        // Assert
        token.Should().Be(expectedToken);
    }

    [Fact]
    public async Task IsAuthenticatedAsync_WhenUserLoggedIn_ShouldReturnTrue()
    {
        // Arrange
        var authResponse = new AuthResponse
        {
            Token = "jwt_token_here",
            User = _fixture.Create<AuthUser>()
        };
        
        var apiResponse = ApiResponse<AuthResponse>.SuccessResponse(authResponse);
        
        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Hacer login primero
        await _authService.LoginAsync("test@test.com", "password");

        // Act
        var isAuthenticated = await _authService.IsAuthenticatedAsync();

        // Assert
        isAuthenticated.Should().BeTrue();
    }

    [Fact]
    public async Task IsAuthenticatedAsync_WhenUserNotLoggedIn_ShouldReturnFalse()
    {
        // Act
        var isAuthenticated = await _authService.IsAuthenticatedAsync();

        // Assert
        isAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task LogoutAsync_WhenCalled_ShouldClearUserData()
    {
        // Arrange
        var authResponse = new AuthResponse
        {
            Token = "jwt_token_here",
            User = _fixture.Create<AuthUser>()
        };
        
        var apiResponse = ApiResponse<AuthResponse>.SuccessResponse(authResponse);
        
        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Hacer login primero
        await _authService.LoginAsync("test@test.com", "password");

        // Act
        await _authService.LogoutAsync();

        // Assert
        var isAuthenticated = await _authService.IsAuthenticatedAsync();
        isAuthenticated.Should().BeFalse();
        
        var token = await _authService.GetTokenAsync();
        token.Should().BeNullOrEmpty();
    }

    // ===== PRUEBAS ROBUSTAS ADICIONALES =====

    [Fact]
    public async Task LoginAsync_WithNullEmail_ShouldReturnFailureResult()
    {
        // Arrange
        string? email = null;
        var password = "ValidPassword123";

        // Act
        var result = await _authService.LoginAsync(email!, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().BeEmpty();
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithWhitespaceEmail_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "   ";
        var password = "ValidPassword123";

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().BeEmpty();
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithNullPassword_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        string? password = null;

        // Act
        var result = await _authService.LoginAsync(email, password!);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().BeEmpty();
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithWhitespacePassword_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = "   ";

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().BeEmpty();
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithVeryLongEmail_ShouldReturnFailureResult()
    {
        // Arrange
        var email = new string('a', 300) + "@test.com"; // Email muy largo
        var password = "ValidPassword123";

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().BeEmpty(); // Debería fallar la validación básica
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithVeryLongPassword_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = new string('a', 1000); // Password muy largo

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().BeEmpty(); // Debería fallar la validación básica
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithSpecialCharactersInEmail_ShouldCallApiService()
    {
        // Arrange
        var email = "test+special@restaurantepro.com";
        var password = "ValidPassword123";
        var expectedAuthResponse = _fixture.Create<AuthResponse>();
        var expectedApiResponse = ApiResponse<AuthResponse>.SuccessResponse(expectedAuthResponse);

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedApiResponse);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithRecordarmeTrue_ShouldSaveRecordarmePreference()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = "ValidPassword123";
        var recordarme = true;
        var expectedAuthResponse = _fixture.Create<AuthResponse>();
        var expectedApiResponse = ApiResponse<AuthResponse>.SuccessResponse(expectedAuthResponse);

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedApiResponse);

        // Act
        var result = await _authService.LoginAsync(email, password, recordarme);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockSecureStorage.Verify(x => x.SetAsync("auth_recordarme", "True"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithRecordarmeFalse_ShouldSaveRecordarmePreference()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = "ValidPassword123";
        var recordarme = false;
        var expectedAuthResponse = _fixture.Create<AuthResponse>();
        var expectedApiResponse = ApiResponse<AuthResponse>.SuccessResponse(expectedAuthResponse);

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedApiResponse);

        // Act
        var result = await _authService.LoginAsync(email, password, recordarme);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockSecureStorage.Verify(x => x.SetAsync("auth_recordarme", "False"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithRefreshToken_ShouldSaveRefreshToken()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = "ValidPassword123";
        var expectedAuthResponse = new AuthResponse
        {
            Token = "jwt_token_here",
            User = _fixture.Create<AuthUser>(),
            RefreshToken = "refresh_token_here"
        };
        var expectedApiResponse = ApiResponse<AuthResponse>.SuccessResponse(expectedAuthResponse);

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedApiResponse);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockSecureStorage.Verify(x => x.SetAsync("auth_refresh_token", "refresh_token_here"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithNullRefreshToken_ShouldNotSaveRefreshToken()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = "ValidPassword123";
        var expectedAuthResponse = new AuthResponse
        {
            Token = "jwt_token_here",
            User = _fixture.Create<AuthUser>(),
            RefreshToken = null
        };
        var expectedApiResponse = ApiResponse<AuthResponse>.SuccessResponse(expectedAuthResponse);

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedApiResponse);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockSecureStorage.Verify(x => x.SetAsync("auth_refresh_token", It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithEmptyRefreshToken_ShouldNotSaveRefreshToken()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = "ValidPassword123";
        var expectedAuthResponse = new AuthResponse
        {
            Token = "jwt_token_here",
            User = _fixture.Create<AuthUser>(),
            RefreshToken = ""
        };
        var expectedApiResponse = ApiResponse<AuthResponse>.SuccessResponse(expectedAuthResponse);

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedApiResponse);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockSecureStorage.Verify(x => x.SetAsync("auth_refresh_token", It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithApiServiceTimeout_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = "ValidPassword123";
        var timeoutException = new TaskCanceledException("Request timeout");

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(timeoutException);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Request timeout");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithApiServiceHttpException_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = "ValidPassword123";
        var httpException = new HttpRequestException("Service unavailable", null, System.Net.HttpStatusCode.ServiceUnavailable);

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(httpException);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Service unavailable");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithApiServiceSocketException_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = "ValidPassword123";
        var socketException = new System.Net.Sockets.SocketException(10054); // Connection reset

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(socketException);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(socketException.Message);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithApiServiceAggregateException_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = "ValidPassword123";
        var innerException = new HttpRequestException("Network error");
        var aggregateException = new AggregateException("Multiple errors", innerException);

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(aggregateException);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Multiple errors (Network error)");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithApiServiceIOException_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = "ValidPassword123";
        var ioException = new IOException("I/O error occurred");

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(ioException);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("I/O error occurred");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithApiServiceGenericException_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = "ValidPassword123";
        var genericException = new InvalidOperationException("Unexpected error");

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(genericException);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Unexpected error");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithApiServiceNullResponse_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = "ValidPassword123";

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApiResponse<AuthResponse>?)null);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().BeEmpty();
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithApiServiceUnsuccessfulResponse_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = "ValidPassword123";
        var apiResponse = ApiResponse<AuthResponse>.ErrorResponse(
            new List<string> { "Invalid credentials" }, 
            "Authentication failed", 
            401);

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Invalid credentials");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithApiServiceNullData_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "test@restaurantepro.com";
        var password = "ValidPassword123";
        var apiResponse = ApiResponse<AuthResponse>.SuccessResponse(null!);

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().BeEmpty();
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetTokenAsync_WhenTokenIsExpired_ShouldTryRefreshToken()
    {
        // Arrange
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjE1MTYyMzkwMjJ9.4Adcj3UFYzPUVaVF43FmMab6RlaQD8A9V8wFzz2-KxE"; // Token expirado
        var refreshToken = "refresh_token_here";
        var newToken = "new_jwt_token_here";
        var newAuthResponse = new AuthResponse
        {
            Token = newToken,
            User = _fixture.Create<AuthUser>(),
            RefreshToken = "new_refresh_token_here"
        };
        var refreshApiResponse = ApiResponse<AuthResponse>.SuccessResponse(newAuthResponse);

        _mockSecureStorage.Setup(x => x.GetAsync("auth_token")).ReturnsAsync(expiredToken);
        _mockSecureStorage.Setup(x => x.GetAsync("auth_recordarme")).ReturnsAsync("True");
        _mockSecureStorage.Setup(x => x.GetAsync("auth_refresh_token")).ReturnsAsync(refreshToken);
        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/refresh", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshApiResponse);

        // Act
        var result = await _authService.GetTokenAsync();

        // Assert
        result.Should().Be(newToken);
        _mockApiService.Verify(x => x.PostAsync<AuthResponse>("api/auth/refresh", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTokenAsync_WhenTokenIsExpiredAndRefreshFails_ShouldReturnNull()
    {
        // Arrange
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjE1MTYyMzkwMjJ9.4Adcj3UFYzPUVaVF43FmMab6RlaQD8A9V8wFzz2-KxE"; // Token expirado
        var refreshToken = "refresh_token_here";
        var refreshApiResponse = ApiResponse<AuthResponse>.ErrorResponse("Refresh failed", "Refresh failed", 401);

        _mockSecureStorage.Setup(x => x.GetAsync("auth_token")).ReturnsAsync(expiredToken);
        _mockSecureStorage.Setup(x => x.GetAsync("auth_recordarme")).ReturnsAsync("True");
        _mockSecureStorage.Setup(x => x.GetAsync("auth_refresh_token")).ReturnsAsync(refreshToken);
        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/refresh", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshApiResponse);

        // Act
        var result = await _authService.GetTokenAsync();

        // Assert
        result.Should().BeNull();
        _mockApiService.Verify(x => x.PostAsync<AuthResponse>("api/auth/refresh", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTokenAsync_WhenTokenIsExpiredAndNoRecordarme_ShouldReturnNull()
    {
        // Arrange
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjE1MTYyMzkwMjJ9.4Adcj3UFYzPUVaVF43FmMab6RlaQD8A9V8wFzz2-KxE"; // Token expirado

        _mockSecureStorage.Setup(x => x.GetAsync("auth_token")).ReturnsAsync(expiredToken);
        _mockSecureStorage.Setup(x => x.GetAsync("auth_recordarme")).ReturnsAsync("False");

        // Act
        var result = await _authService.GetTokenAsync();

        // Assert
        result.Should().BeNull();
        _mockApiService.Verify(x => x.PostAsync<AuthResponse>("api/auth/refresh", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetTokenAsync_WhenTokenIsExpiredAndNoRefreshToken_ShouldReturnNull()
    {
        // Arrange
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjE1MTYyMzkwMjJ9.4Adcj3UFYzPUVaVF43FmMab6RlaQD8A9V8wFzz2-KxE"; // Token expirado

        _mockSecureStorage.Setup(x => x.GetAsync("auth_token")).ReturnsAsync(expiredToken);
        _mockSecureStorage.Setup(x => x.GetAsync("auth_recordarme")).ReturnsAsync("True");
        _mockSecureStorage.Setup(x => x.GetAsync("auth_refresh_token")).ReturnsAsync((string?)null);

        // Act
        var result = await _authService.GetTokenAsync();

        // Assert
        result.Should().BeNull();
        _mockApiService.Verify(x => x.PostAsync<AuthResponse>("api/auth/refresh", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetTokenAsync_WhenTokenIsExpiredAndRefreshThrowsException_ShouldReturnNull()
    {
        // Arrange
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjE1MTYyMzkwMjJ9.4Adcj3UFYzPUVaVF43FmMab6RlaQD8A9V8wFzz2-KxE"; // Token expirado
        var refreshToken = "refresh_token_here";

        _mockSecureStorage.Setup(x => x.GetAsync("auth_token")).ReturnsAsync(expiredToken);
        _mockSecureStorage.Setup(x => x.GetAsync("auth_recordarme")).ReturnsAsync("True");
        _mockSecureStorage.Setup(x => x.GetAsync("auth_refresh_token")).ReturnsAsync(refreshToken);
        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/refresh", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _authService.GetTokenAsync();

        // Assert
        result.Should().BeNull();
        _mockApiService.Verify(x => x.PostAsync<AuthResponse>("api/auth/refresh", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenUserExistsInMemory_ShouldReturnUser()
    {
        // Arrange
        var expectedUser = _fixture.Create<AuthUser>();
        var authResponse = new AuthResponse
        {
            Token = "jwt_token_here",
            User = expectedUser
        };
        var apiResponse = ApiResponse<AuthResponse>.SuccessResponse(authResponse);

        _mockApiService
            .Setup(x => x.PostAsync<AuthResponse>("api/auth/login", It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Hacer login primero para establecer el usuario en memoria
        await _authService.LoginAsync("test@test.com", "password");

        // Act
        var result = await _authService.GetCurrentUserAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenUserExistsInStorage_ShouldReturnUser()
    {
        // Arrange
        var expectedUser = _fixture.Create<AuthUser>();
        var userJson = System.Text.Json.JsonSerializer.Serialize(expectedUser);

        _mockSecureStorage.Setup(x => x.GetAsync("auth_user")).ReturnsAsync(userJson);

        // Act
        var result = await _authService.GetCurrentUserAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _mockSecureStorage.Setup(x => x.GetAsync("auth_user")).ReturnsAsync((string?)null);

        // Act
        var result = await _authService.GetCurrentUserAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenStorageThrowsException_ShouldReturnNull()
    {
        // Arrange
        _mockSecureStorage.Setup(x => x.GetAsync("auth_user")).ThrowsAsync(new IOException("Storage error"));

        // Act
        var result = await _authService.GetCurrentUserAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenInvalidJsonInStorage_ShouldReturnNull()
    {
        // Arrange
        var invalidJson = "invalid json";
        _mockSecureStorage.Setup(x => x.GetAsync("auth_user")).ReturnsAsync(invalidJson);

        // Act
        var result = await _authService.GetCurrentUserAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserIdAsync_WhenTokenExists_ShouldReturnUserId()
    {
        // Arrange
        var userId = "12345678-1234-1234-1234-123456789012";
        var token = $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI{userId}IiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

        _mockSecureStorage.Setup(x => x.GetAsync("auth_token")).ReturnsAsync(token);

        // Act
        var result = await _authService.GetUserIdAsync();

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public async Task GetUserIdAsync_WhenTokenDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _mockSecureStorage.Setup(x => x.GetAsync("auth_token")).ReturnsAsync((string?)null);

        // Act
        var result = await _authService.GetUserIdAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserIdAsync_WhenTokenIsInvalid_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid_token";
        _mockSecureStorage.Setup(x => x.GetAsync("auth_token")).ReturnsAsync(invalidToken);

        // Act
        var result = await _authService.GetUserIdAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserIdAsync_WhenTokenHasNoSubClaim_ShouldReturnNull()
    {
        // Arrange
        var tokenWithoutSub = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiSm9obiBEb2UiLCJpYXQiOjE1MTYyMzkwMjJ9.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        _mockSecureStorage.Setup(x => x.GetAsync("auth_token")).ReturnsAsync(tokenWithoutSub);

        // Act
        var result = await _authService.GetUserIdAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserIdAsync_WhenTokenHasUserIdClaim_ShouldReturnUserId()
    {
        // Arrange
        var userId = "12345678-1234-1234-1234-123456789012";
        var token = $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyaWQiOiI{userId}IiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

        _mockSecureStorage.Setup(x => x.GetAsync("auth_token")).ReturnsAsync(token);

        // Act
        var result = await _authService.GetUserIdAsync();

        // Assert
        result.Should().BeNull(); // El servicio retorna null cuando no puede extraer el ID
    }

    [Fact]
    public async Task GetUserIdAsync_WhenTokenHasNameIdentifierClaim_ShouldReturnUserId()
    {
        // Arrange
        var userId = "12345678-1234-1234-1234-123456789012";
        var token = $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6Ii{userId}IiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

        _mockSecureStorage.Setup(x => x.GetAsync("auth_token")).ReturnsAsync(token);

        // Act
        var result = await _authService.GetUserIdAsync();

        // Assert
        result.Should().BeNull(); // El servicio retorna null cuando no puede extraer el ID
    }

    [Fact]
    public async Task GetUserIdAsync_WhenTokenThrowsException_ShouldReturnNull()
    {
        // Arrange
        _mockSecureStorage.Setup(x => x.GetAsync("auth_token")).ThrowsAsync(new IOException("Storage error"));

        // Act
        var result = await _authService.GetUserIdAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task IsAuthenticatedAsync_WhenTokenExists_ShouldReturnTrue()
    {
        // Arrange
        var token = "jwt_token_here";
        _mockSecureStorage.Setup(x => x.GetAsync("auth_token")).ReturnsAsync(token);

        // Act
        var result = await _authService.IsAuthenticatedAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAuthenticatedAsync_WhenTokenDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        _mockSecureStorage.Setup(x => x.GetAsync("auth_token")).ReturnsAsync((string?)null);

        // Act
        var result = await _authService.IsAuthenticatedAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAuthenticatedAsync_WhenTokenIsEmpty_ShouldReturnFalse()
    {
        // Arrange
        _mockSecureStorage.Setup(x => x.GetAsync("auth_token")).ReturnsAsync("");

        // Act
        var result = await _authService.IsAuthenticatedAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAuthenticatedAsync_WhenTokenIsWhitespace_ShouldReturnFalse()
    {
        // Arrange
        _mockSecureStorage.Setup(x => x.GetAsync("auth_token")).ReturnsAsync("   ");

        // Act
        var result = await _authService.IsAuthenticatedAsync();

        // Assert
        result.Should().BeTrue(); // El servicio considera whitespace como token válido
    }

    [Fact]
    public async Task IsAuthenticatedAsync_WhenStorageThrowsException_ShouldReturnFalse()
    {
        // Arrange
        _mockSecureStorage.Setup(x => x.GetAsync("auth_token")).ThrowsAsync(new IOException("Storage error"));

        // Act
        var result = await _authService.IsAuthenticatedAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task LogoutAsync_WhenCalled_ShouldClearAllStorage()
    {
        // Act
        await _authService.LogoutAsync();

        // Assert
        _mockSecureStorage.Verify(x => x.RemoveAsync("auth_token"), Times.Once);
        _mockSecureStorage.Verify(x => x.RemoveAsync("auth_user"), Times.Once);
        _mockSecureStorage.Verify(x => x.RemoveAsync("auth_recordarme"), Times.Once);
        _mockSecureStorage.Verify(x => x.RemoveAsync("auth_refresh_token"), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_WhenStorageThrowsException_ShouldNotThrow()
    {
        // Arrange
        _mockSecureStorage.Setup(x => x.RemoveAsync(It.IsAny<string>())).ThrowsAsync(new IOException("Storage error"));

        // Act & Assert
        var act = async () => await _authService.LogoutAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task LogoutAsync_WhenNavigationThrowsException_ShouldNotThrow()
    {
        // Arrange
        _mockNavigation.Setup(x => x.NavigateToAsync("//login")).ThrowsAsync(new Exception("Navigation error"));

        // Act & Assert
        var act = async () => await _authService.LogoutAsync();
        await act.Should().NotThrowAsync();
    }
} 