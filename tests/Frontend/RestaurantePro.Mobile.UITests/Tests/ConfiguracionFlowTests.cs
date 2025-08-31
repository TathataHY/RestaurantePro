using FluentAssertions;
using RestaurantePro.Mobile.UITests.PageObjects;
using RestaurantePro.Mobile.UITests.TestBase;

namespace RestaurantePro.Mobile.UITests.Tests;

/// <summary>
/// Pruebas completas para el flujo de configuración y perfil de usuario
/// </summary>
public class ConfiguracionFlowTests : AppiumTestBase
{
    private LoginPageObject _loginPage = null!;
    private DashboardPageObject _dashboardPage = null!;
    private ConfiguracionPageObject _configuracionPage = null!;
    private PerfilPageObject _perfilPage = null!;

    public ConfiguracionFlowTests(ITestOutputHelper testOutput) : base(testOutput) { }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Configuracion_CompleteFlow_ShouldSucceed()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _configuracionPage = new ConfiguracionPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Configuración
        _dashboardPage.NavigateToConfiguracion();
        _configuracionPage.WaitForConfiguracionToLoad();

        // Verify configuración is loaded
        var numberOfOptions = _configuracionPage.GetNumberOfConfigOptions();
        numberOfOptions.Should().BeGreaterThan(0);

        TestOutput.WriteLine($"✅ Opciones de configuración cargadas: {numberOfOptions}");
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Configuracion_ViewProfile_ShouldDisplayCorrectly()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _configuracionPage = new ConfiguracionPageObject(Driver, TestOutput);
        _perfilPage = new PerfilPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Configuración
        _dashboardPage.NavigateToConfiguracion();
        _configuracionPage.WaitForConfiguracionToLoad();

        // Click on Profile
        _configuracionPage.ClickProfile();
        _perfilPage.WaitForPerfilToLoad();

        // Verify profile is displayed
        _perfilPage.IsPerfilDisplayed().Should().BeTrue();
        var profileData = _perfilPage.GetProfileData();
        profileData.Should().NotBeEmpty();

        TestOutput.WriteLine($"✅ Perfil mostrado correctamente: {profileData}");
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Configuracion_UpdateProfile_ShouldSucceed()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _configuracionPage = new ConfiguracionPageObject(Driver, TestOutput);
        _perfilPage = new PerfilPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Configuración
        _dashboardPage.NavigateToConfiguracion();
        _configuracionPage.WaitForConfiguracionToLoad();

        // Click on Profile
        _configuracionPage.ClickProfile();
        _perfilPage.WaitForPerfilToLoad();

        // Update profile information
        var newName = "Admin Actualizado";
        var newPhone = "+1234567890";
        
        _perfilPage.UpdateName(newName);
        _perfilPage.UpdatePhone(newPhone);
        _perfilPage.ClickSaveProfile();
        _perfilPage.WaitForProfileUpdate();

        // Verify profile was updated
        var updatedName = _perfilPage.GetName();
        var updatedPhone = _perfilPage.GetPhone();
        
        updatedName.Should().Be(newName);
        updatedPhone.Should().Be(newPhone);

        TestOutput.WriteLine($"✅ Perfil actualizado: Nombre: {updatedName}, Teléfono: {updatedPhone}");
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Configuracion_ChangePassword_ShouldSucceed()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _configuracionPage = new ConfiguracionPageObject(Driver, TestOutput);
        _perfilPage = new PerfilPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Configuración
        _dashboardPage.NavigateToConfiguracion();
        _configuracionPage.WaitForConfiguracionToLoad();

        // Click on Profile
        _configuracionPage.ClickProfile();
        _perfilPage.WaitForPerfilToLoad();

        // Change password
        var currentPassword = validPassword;
        var newPassword = "NewPassword123!";
        var confirmPassword = "NewPassword123!";
        
        _perfilPage.ClickChangePassword();
        _perfilPage.EnterCurrentPassword(currentPassword);
        _perfilPage.EnterNewPassword(newPassword);
        _perfilPage.EnterConfirmPassword(confirmPassword);
        _perfilPage.ClickSavePassword();
        _perfilPage.WaitForPasswordChange();

        // Verify password change was successful
        var successMessage = _perfilPage.GetPasswordChangeMessage();
        successMessage.Should().NotBeEmpty();
        successMessage.Should().ContainAny("cambiada", "actualizada", "exitosamente");

        TestOutput.WriteLine($"✅ Contraseña cambiada exitosamente: {successMessage}");
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Configuracion_ViewAppSettings_ShouldDisplayCorrectly()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _configuracionPage = new ConfiguracionPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Configuración
        _dashboardPage.NavigateToConfiguracion();
        _configuracionPage.WaitForConfiguracionToLoad();

        // Click on App Settings
        _configuracionPage.ClickAppSettings();
        _configuracionPage.WaitForAppSettingsToLoad();

        // Verify app settings are displayed
        var numberOfSettings = _configuracionPage.GetNumberOfAppSettings();
        numberOfSettings.Should().BeGreaterThan(0);

        TestOutput.WriteLine($"✅ Configuraciones de la app mostradas: {numberOfSettings} opciones");
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Configuracion_UpdateAppSettings_ShouldSucceed()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _configuracionPage = new ConfiguracionPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Configuración
        _dashboardPage.NavigateToConfiguracion();
        _configuracionPage.WaitForConfiguracionToLoad();

        // Click on App Settings
        _configuracionPage.ClickAppSettings();
        _configuracionPage.WaitForAppSettingsToLoad();

        // Update app settings
        _configuracionPage.ToggleNotifications(true);
        _configuracionPage.ToggleDarkMode(false);
        _configuracionPage.SetLanguage("Español");
        _configuracionPage.ClickSaveAppSettings();
        _configuracionPage.WaitForAppSettingsUpdate();

        // Verify app settings were updated
        var notificationsEnabled = _configuracionPage.AreNotificationsEnabled();
        var darkModeEnabled = _configuracionPage.IsDarkModeEnabled();
        var currentLanguage = _configuracionPage.GetCurrentLanguage();

        notificationsEnabled.Should().BeTrue();
        darkModeEnabled.Should().BeFalse();
        currentLanguage.Should().Be("Español");

        TestOutput.WriteLine($"✅ Configuraciones de la app actualizadas: Notificaciones: {notificationsEnabled}, Modo Oscuro: {darkModeEnabled}, Idioma: {currentLanguage}");
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Configuracion_ViewAbout_ShouldDisplayCorrectly()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _configuracionPage = new ConfiguracionPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Configuración
        _dashboardPage.NavigateToConfiguracion();
        _configuracionPage.WaitForConfiguracionToLoad();

        // Click on About
        _configuracionPage.ClickAbout();
        _configuracionPage.WaitForAboutToLoad();

        // Verify about information is displayed
        var appVersion = _configuracionPage.GetAppVersion();
        var appName = _configuracionPage.GetAppName();
        var companyInfo = _configuracionPage.GetCompanyInfo();

        appVersion.Should().NotBeEmpty();
        appName.Should().NotBeEmpty();
        companyInfo.Should().NotBeEmpty();

        TestOutput.WriteLine($"✅ Información de la app mostrada: Versión: {appVersion}, Nombre: {appName}, Empresa: {companyInfo}");
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Configuracion_Logout_ShouldSucceed()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _configuracionPage = new ConfiguracionPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Configuración
        _dashboardPage.NavigateToConfiguracion();
        _configuracionPage.WaitForConfiguracionToLoad();

        // Click on Logout
        _configuracionPage.ClickLogout();
        _configuracionPage.WaitForLogoutConfirmation();

        // Confirm logout
        _configuracionPage.ConfirmLogout();
        _configuracionPage.WaitForLogoutToComplete();

        // Verify logout was successful
        _loginPage.IsLoginPageDisplayed().Should().BeTrue();

        TestOutput.WriteLine("✅ Logout exitoso - Usuario regresó a la página de login");
    }
}
