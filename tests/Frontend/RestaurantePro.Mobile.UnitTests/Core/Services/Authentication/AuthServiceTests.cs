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
        result.Errors.Should().Contain("Email es requerido");
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
        result.Errors.Should().Contain("Password es requerido");
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
} 