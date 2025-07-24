using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Features.Authentication;

/// <summary>
/// Tests de integración para AuthService usando API en memoria y fake de SecureStorage
/// </summary>
public class AuthServiceIntegrationTests : MobileIntegrationTestBase
{
    private IAuthService GetAuthService()
    {
        // Crear servicios móviles localmente para evitar conflictos con el backend
        var httpClient = CreateClient(); // Usar el cliente HTTP del backend
        var secureStorage = new FakeSecureStorageService();
        var apiService = new ApiService(httpClient);
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<RestaurantePro.Mobile.Core.Services.Authentication.AuthService>.Instance;
        return new AuthService(apiService, logger, secureStorage);
    }

    [Fact(DisplayName = "Login exitoso con credenciales válidas")]
    public async Task LoginAsync_ConCredencialesValidas_DebeSerExitoso()
    {
        // Arrange
        var authService = GetAuthService();
        var email = "admin@restaurantepro.com"; // Usa un usuario seed válido
        var password = "AdminRestaurante123!";

        // Act
        var result = await authService.LoginAsync(email, password);

        // Assert
        Assert.True(result.Success, $"El login debería ser exitoso. Error: {string.Join(", ", result.Errors)}");
        Assert.False(string.IsNullOrEmpty(result.Data?.Token), "El token JWT no debe ser nulo o vacío");
    }

    [Fact(DisplayName = "Login fallido con credenciales inválidas")]
    public async Task LoginAsync_ConCredencialesInvalidas_DebeFallar()
    {
        // Arrange
        var authService = GetAuthService();
        var email = "usuarioinvalido@restaurantepro.com";
        var password = "ClaveIncorrecta";

        // Act
        var result = await authService.LoginAsync(email, password);

        // Assert
        Assert.False(result.Success, "El login debería fallar con credenciales inválidas");
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }
} 