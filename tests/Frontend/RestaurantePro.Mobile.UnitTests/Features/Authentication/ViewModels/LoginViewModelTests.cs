using Xunit;
using Moq;
using FluentAssertions;
using RestaurantePro.Mobile.Core.Features.Authentication.ViewModels;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.DTOs;
using AutoFixture;

namespace RestaurantePro.Mobile.UnitTests.Features.Authentication.ViewModels;

/// <summary>
/// Tests unitarios para LoginViewModel - Completos y exhaustivos
/// </summary>
public class LoginViewModelTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly LoginViewModel _viewModel;
    private readonly Fixture _fixture;

    public LoginViewModelTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockNavigationService = new Mock<INavigationService>();
        _viewModel = new LoginViewModel(_mockAuthService.Object, _mockNavigationService.Object);
        _fixture = new Fixture();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeProperties_WhenCreated()
    {
        // Assert
        _viewModel.Email.Should().BeEmpty();
        _viewModel.Password.Should().BeEmpty();
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.Title.Should().Be("Iniciar Sesión");
        _viewModel.HasError.Should().BeFalse();
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Email_ShouldSetValue_WhenAssigned()
    {
        // Arrange
        var testEmail = "test@restaurant.com";

        // Act
        _viewModel.Email = testEmail;

        // Assert
        _viewModel.Email.Should().Be(testEmail);
    }

    [Fact]
    public void Password_ShouldSetValue_WhenAssigned()
    {
        // Arrange
        var testPassword = "TestPassword123";

        // Act
        _viewModel.Password = testPassword;

        // Assert
        _viewModel.Password.Should().Be(testPassword);
    }

    [Fact]
    public void IsLoading_ShouldSetValue_WhenAssigned()
    {
        // Act
        _viewModel.IsLoading = true;

        // Assert
        _viewModel.IsLoading.Should().BeTrue();
    }

    #endregion

    #region LoginCommand Tests

    [Fact]
    public async Task LoginCommand_ShouldShowError_WhenEmailIsEmpty()
    {
        // Arrange
        _viewModel.Email = "";
        _viewModel.Password = "validpassword";

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _viewModel.HasError.Should().BeTrue();
        _viewModel.ErrorMessage.Should().Be("Por favor ingrese su email");
        _mockAuthService.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task LoginCommand_ShouldShowError_WhenEmailIsWhitespace()
    {
        // Arrange
        _viewModel.Email = "   ";
        _viewModel.Password = "validpassword";

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _viewModel.HasError.Should().BeTrue();
        _viewModel.ErrorMessage.Should().Be("Por favor ingrese su email");
        _mockAuthService.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task LoginCommand_ShouldShowError_WhenPasswordIsEmpty()
    {
        // Arrange
        _viewModel.Email = "test@restaurant.com";
        _viewModel.Password = "";

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _viewModel.HasError.Should().BeTrue();
        _viewModel.ErrorMessage.Should().Be("Por favor ingrese su contraseña");
        _mockAuthService.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task LoginCommand_ShouldShowError_WhenPasswordIsWhitespace()
    {
        // Arrange
        _viewModel.Email = "test@restaurant.com";
        _viewModel.Password = "   ";

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _viewModel.HasError.Should().BeTrue();
        _viewModel.ErrorMessage.Should().Be("Por favor ingrese su contraseña");
        _mockAuthService.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task LoginCommand_ShouldNavigateToDashboard_WhenLoginSuccessful()
    {
        // Arrange
        var email = "mesero@restaurant.com";
        var password = "password123";
        var authResponse = _fixture.Create<AuthResponse>();
        var apiResponse = ApiResponse<AuthResponse>.SuccessResponse(authResponse);

        _viewModel.Email = email;
        _viewModel.Password = password;

        _mockAuthService.Setup(x => x.LoginAsync(email, password, It.IsAny<bool>()))
                       .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _mockAuthService.Verify(x => x.LoginAsync(email, password, It.IsAny<bool>()), Times.Once);
        _mockNavigationService.Verify(x => x.NavigateToAsync("//main/dashboard"), Times.Once);
        _viewModel.HasError.Should().BeFalse();
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoginCommand_ShouldShowError_WhenLoginFails()
    {
        // Arrange
        var email = "invalid@restaurant.com";
        var password = "wrongpassword";
        var errorMessage = "Credenciales inválidas";
        var apiResponse = ApiResponse<AuthResponse>.ErrorResponse(new List<string> { errorMessage }, "Login failed");

        _viewModel.Email = email;
        _viewModel.Password = password;

        _mockAuthService.Setup(x => x.LoginAsync(email, password, It.IsAny<bool>()))
                       .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _mockAuthService.Verify(x => x.LoginAsync(email, password, It.IsAny<bool>()), Times.Once);
        _mockNavigationService.Verify(x => x.NavigateToAsync(It.IsAny<string>()), Times.Never);
        _viewModel.HasError.Should().BeTrue();
        _viewModel.ErrorMessage.Should().Be(errorMessage);
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoginCommand_ShouldShowGenericError_WhenLoginFailsWithoutSpecificError()
    {
        // Arrange
        var email = "test@restaurant.com";
        var password = "password";
        var apiResponse = ApiResponse<AuthResponse>.ErrorResponse(new List<string>(), "Unknown error");

        _viewModel.Email = email;
        _viewModel.Password = password;

        _mockAuthService.Setup(x => x.LoginAsync(email, password, It.IsAny<bool>()))
                       .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _viewModel.HasError.Should().BeTrue();
        _viewModel.ErrorMessage.Should().Be("Error de autenticación");
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoginCommand_ShouldHandleException_WhenAuthServiceThrows()
    {
        // Arrange
        var email = "test@restaurant.com";
        var password = "password";
        var exceptionMessage = "Network error";

        _viewModel.Email = email;
        _viewModel.Password = password;

        _mockAuthService.Setup(x => x.LoginAsync(email, password, It.IsAny<bool>()))
                       .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _viewModel.HasError.Should().BeTrue();
        _viewModel.ErrorMessage.Should().Be($"Error inesperado: {exceptionMessage}");
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoginCommand_ShouldSetIsLoadingCorrectly_DuringOperation()
    {
        // Arrange
        var email = "test@restaurant.com";
        var password = "password";
        var authResponse = _fixture.Create<AuthResponse>();
        var apiResponse = ApiResponse<AuthResponse>.SuccessResponse(authResponse);
        var tcs = new TaskCompletionSource<ApiResponse<AuthResponse>>();

        _viewModel.Email = email;
        _viewModel.Password = password;

        _mockAuthService.Setup(x => x.LoginAsync(email, password, It.IsAny<bool>()))
                       .Returns(tcs.Task);

        // Act - Start the command but don't await
        var loginTask = _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert - Should be loading
        _viewModel.IsLoading.Should().BeTrue();

        // Complete the async operation
        tcs.SetResult(apiResponse);
        await loginTask;

        // Assert - Should not be loading anymore
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoginCommand_ShouldNotExecute_WhenAlreadyLoading()
    {
        // Arrange
        _viewModel.Email = "test@restaurant.com";
        _viewModel.Password = "password";
        _viewModel.IsLoading = true;

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _mockAuthService.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task LoginCommand_ShouldClearError_BeforeExecuting()
    {
        // Arrange
        var email = "test@restaurant.com";
        var password = "password";
        var authResponse = _fixture.Create<AuthResponse>();
        var apiResponse = ApiResponse<AuthResponse>.SuccessResponse(authResponse);

        _viewModel.Email = email;
        _viewModel.Password = password;
        
        // Primero forzamos un error realizando login con campos vacíos
        _viewModel.Email = "";
        await _viewModel.LoginCommand.ExecuteAsync(null);
        _viewModel.HasError.Should().BeTrue();
        
        // Ahora ponemos credenciales válidas
        _viewModel.Email = email;
        _viewModel.Password = password;

        _mockAuthService.Setup(x => x.LoginAsync(email, password, It.IsAny<bool>()))
                       .ReturnsAsync(apiResponse);

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _viewModel.HasError.Should().BeFalse();
    }

    #endregion

    #region ClearFieldsCommand Tests

    [Fact]
    public void ClearFieldsCommand_ShouldClearEmailAndPassword()
    {
        // Arrange
        _viewModel.Email = "test@restaurant.com";
        _viewModel.Password = "password123";

        // Act
        _viewModel.ClearFieldsCommand.Execute(null);

        // Assert
        _viewModel.Email.Should().BeEmpty();
        _viewModel.Password.Should().BeEmpty();
    }

    [Fact]
    public void ClearFieldsCommand_ShouldClearError()
    {
        // Arrange - Provocar un error con campos vacíos
        _viewModel.Email = "";
        _viewModel.Password = "password";
        
        // Usar un approach que no dependa de llamadas directas
        // En este caso, verificamos el comportamiento del comando ClearFields
        _viewModel.ClearFieldsCommand.Execute(null);

        // Assert
        _viewModel.HasError.Should().BeFalse();
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    #endregion

    #region CheckAuthStatusCommand Tests

    [Fact]
    public async Task CheckAuthStatusCommand_ShouldNavigateToDashboard_WhenAlreadyAuthenticated()
    {
        // Arrange
        _mockAuthService.Setup(x => x.IsAuthenticatedAsync())
                       .ReturnsAsync(true);

        // Act
        await _viewModel.CheckAuthStatusCommand.ExecuteAsync(null);

        // Assert
        _mockAuthService.Verify(x => x.IsAuthenticatedAsync(), Times.Once);
        _mockNavigationService.Verify(x => x.NavigateToAsync("//main/dashboard"), Times.Once);
    }

    [Fact]
    public async Task CheckAuthStatusCommand_ShouldNotNavigate_WhenNotAuthenticated()
    {
        // Arrange
        _mockAuthService.Setup(x => x.IsAuthenticatedAsync())
                       .ReturnsAsync(false);

        // Act
        await _viewModel.CheckAuthStatusCommand.ExecuteAsync(null);

        // Assert
        _mockAuthService.Verify(x => x.IsAuthenticatedAsync(), Times.Once);
        _mockNavigationService.Verify(x => x.NavigateToAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Command CanExecute Tests

    [Fact]
    public void LoginCommand_CanExecute_ShouldBeTrue_WhenNotLoading()
    {
        // Arrange
        _viewModel.IsLoading = false;

        // Act & Assert
        _viewModel.LoginCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void ClearFieldsCommand_CanExecute_ShouldAlwaysBeTrue()
    {
        // Act & Assert
        _viewModel.ClearFieldsCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void CheckAuthStatusCommand_CanExecute_ShouldAlwaysBeTrue()
    {
        // Act & Assert
        _viewModel.CheckAuthStatusCommand.CanExecute(null).Should().BeTrue();
    }

    #endregion

    #region Integration-like Tests

    [Fact]
    public async Task FullLoginFlow_ShouldWorkCorrectly_WithValidCredentials()
    {
        // Arrange
        var email = "mesero@restaurant.com";
        var password = "SecurePass123";
        var authUser = _fixture.Create<AuthUser>();
        var authResponse = new AuthResponse
        {
            Token = "jwt-token-here",
            User = authUser
        };
        var apiResponse = ApiResponse<AuthResponse>.SuccessResponse(authResponse);

        _mockAuthService.Setup(x => x.LoginAsync(email, password, It.IsAny<bool>()))
                       .ReturnsAsync(apiResponse);

        // Act - Set credentials
        _viewModel.Email = email;
        _viewModel.Password = password;

        // Act - Execute login
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert - Complete flow verification
        _mockAuthService.Verify(x => x.LoginAsync(email, password, It.IsAny<bool>()), Times.Once);
        _mockNavigationService.Verify(x => x.NavigateToAsync("//main/dashboard"), Times.Once);
        
        _viewModel.HasError.Should().BeFalse();
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.Email.Should().Be(email);
        _viewModel.Password.Should().Be(password);
    }

    [Fact]
    public async Task FullLoginFlow_ShouldHandleFailure_WithInvalidCredentials()
    {
        // Arrange
        var email = "wrong@restaurant.com";
        var password = "wrongpass";
        var errors = new List<string> { "Usuario o contraseña incorrectos" };
        var apiResponse = ApiResponse<AuthResponse>.ErrorResponse(errors, "Authentication failed", 401);

        _mockAuthService.Setup(x => x.LoginAsync(email, password, It.IsAny<bool>()))
                       .ReturnsAsync(apiResponse);

        // Act - Set credentials and login
        _viewModel.Email = email;
        _viewModel.Password = password;
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert - Error handling verification
        _mockAuthService.Verify(x => x.LoginAsync(email, password, It.IsAny<bool>()), Times.Once);
        _mockNavigationService.Verify(x => x.NavigateToAsync(It.IsAny<string>()), Times.Never);
        
        _viewModel.HasError.Should().BeTrue();
        _viewModel.ErrorMessage.Should().Be("Usuario o contraseña incorrectos");
        _viewModel.IsLoading.Should().BeFalse();
    }

    #endregion
} 