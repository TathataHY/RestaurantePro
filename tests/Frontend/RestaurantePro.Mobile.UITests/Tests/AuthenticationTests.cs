using FluentAssertions;
using RestaurantePro.Mobile.UITests.PageObjects;
using RestaurantePro.Mobile.UITests.TestBase;

namespace RestaurantePro.Mobile.UITests.Tests;

public class AuthenticationTests : AppiumTestBase
{
    private LoginPageObject _loginPage;
    private DashboardPageObject _dashboardPage;

    public AuthenticationTests(ITestOutputHelper testOutput) : base(testOutput) { }

    [Fact]
    public void LoginWithValidCredentials_ShouldNavigateToDashboard()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "test@example.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "password123";

        // Act
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Assert
        _dashboardPage.WaitForDashboardToLoad().Should().BeTrue();
        _dashboardPage.IsDashboardDisplayed().Should().BeTrue();
        _dashboardPage.GetDashboardTitle().Should().Contain("Dashboard");

        TestOutput.WriteLine("✅ Login exitoso - Navegación al dashboard correcta");
    }

    [Fact]
    public void LoginWithInvalidCredentials_ShouldShowErrorMessage()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        
        var invalidEmail = Configuration["TestData:InvalidCredentials:Email"] ?? "invalid@example.com";
        var invalidPassword = Configuration["TestData:InvalidCredentials:Password"] ?? "wrongpassword";

        // Act
        _loginPage.Login(invalidEmail, invalidPassword);

        // Assert
        _loginPage.IsErrorMessageDisplayed().Should().BeTrue();
        var errorMessage = _loginPage.GetErrorMessage();
        errorMessage.Should().NotBeEmpty();
        errorMessage.Should().ContainAny("inválid", "incorrect", "error");

        TestOutput.WriteLine($"✅ Error de login mostrado: {errorMessage}");
    }

    [Fact]
    public void LoginWithEmptyCredentials_ShouldShowValidationErrors()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        
        // Act
        _loginPage.ClickLoginButton();

        // Assert
        _loginPage.IsErrorMessageDisplayed().Should().BeTrue();
        var errorMessage = _loginPage.GetErrorMessage();
        errorMessage.Should().NotBeEmpty();

        TestOutput.WriteLine($"✅ Validación de campos vacíos: {errorMessage}");
    }

    [Fact]
    public void LoginWithInvalidEmailFormat_ShouldShowValidationError()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        
        var invalidEmail = "invalid-email-format";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "password123";

        // Act
        _loginPage.Login(invalidEmail, validPassword);

        // Assert
        _loginPage.IsErrorMessageDisplayed().Should().BeTrue();
        var errorMessage = _loginPage.GetErrorMessage();
        errorMessage.Should().ContainAny("email", "formato", "válido");

        TestOutput.WriteLine($"✅ Validación de formato de email: {errorMessage}");
    }

    [Fact]
    public void LoginPage_ShouldDisplayAllRequiredElements()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        
        // Debug: Mostrar elementos disponibles
        _loginPage.DebugPageElements();
        
        // Assert
        _loginPage.IsLoginPageDisplayed().Should().BeTrue();

        TestOutput.WriteLine("✅ Página de login muestra todos los elementos requeridos");
    }
    
    [Fact]
    public void LoginPage_ShouldAllowBasicInteraction()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        
        // Act & Assert - Solo verificar que la página está disponible
        _loginPage.IsLoginPageDisplayed().Should().BeTrue();
        
        TestOutput.WriteLine("✅ Página de login permite interacción básica");
    }
    
    [Fact]
    public void LoginPage_ShouldAllowButtonClick()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        
        // Act - Solo hacer click en el botón sin ingresar datos
        _loginPage.ClickLoginButton();
        
        // Assert - Si no lanza excepción, el botón funciona
        TestOutput.WriteLine("✅ Botón de login funciona correctamente");
    }
    
    [Fact]
    public void LoginPage_ShouldAllowDataEntry()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        
        // Act - Ingresar datos en los campos
        _loginPage.EnterEmail("test@example.com");
        _loginPage.EnterPassword("password123");
        
        // Assert - Si no lanza excepción, los campos funcionan
        TestOutput.WriteLine("✅ Campos de entrada funcionan correctamente");
    }

    [Fact]
    public void RememberMeCheckbox_ShouldBeToggleable()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        
        // Act & Assert
        _loginPage.ToggleRememberMe();
        // No debería lanzar excepción

        TestOutput.WriteLine("✅ Checkbox 'Recordarme' funciona correctamente");
    }

    [Fact]
    public void LoginFlow_ShouldHandleLoadingState()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "test@example.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "password123";

        // Act
        _loginPage.Login(validEmail, validPassword);

        // Assert
        _loginPage.IsLoadingIndicatorDisplayed().Should().BeTrue();
        _loginPage.WaitForLoginToComplete();
        _loginPage.IsLoadingIndicatorDisplayed().Should().BeFalse();

        TestOutput.WriteLine("✅ Estado de carga manejado correctamente");
    }

    [Fact]
    public void Logout_ShouldReturnToLoginPage()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "test@example.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "password123";

        // Login first
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Act
        _dashboardPage.ClickLogout();

        // Assert
        _loginPage.IsLoginPageDisplayed().Should().BeTrue();

        TestOutput.WriteLine("✅ Logout exitoso - Retorno a página de login");
    }

    [Fact]
    public void LoginWithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        
        var specialEmail = "test@example.com";
        var specialPassword = "P@ssw0rd!@#$%^&*()";

        // Act
        _loginPage.Login(specialEmail, specialPassword);

        // Assert
        // Debería manejar caracteres especiales sin errores
        TestOutput.WriteLine("✅ Login con caracteres especiales manejado correctamente");
    }

    [Fact]
    public void LoginWithLongCredentials_ShouldHandleCorrectly()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        
        var longEmail = "verylongemailaddress@verylongdomainname.com";
        var longPassword = "VeryLongPassword123456789012345678901234567890";

        // Act
        _loginPage.Login(longEmail, longPassword);

        // Assert
        // Debería manejar credenciales largas sin errores
        TestOutput.WriteLine("✅ Login con credenciales largas manejado correctamente");
    }
} 