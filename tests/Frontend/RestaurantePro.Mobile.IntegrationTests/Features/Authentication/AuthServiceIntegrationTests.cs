using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Platform;
using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Features.Authentication;

public class AuthServiceIntegrationTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private IAuthService _authService;

    public AuthServiceIntegrationTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
        Setup();
    }

    private void Setup()
    {
        // Crear servicios móviles localmente para evitar conflictos con el backend
        var httpClient = _client;
        var apiService = new ApiService(httpClient);
        var authService = new AuthService(apiService, NullLogger<AuthService>.Instance, new FakeSecureStorageService(), new FakeNavigationService());
        _authService = authService;
    }

    [Fact(DisplayName = "Login exitoso con credenciales válidas")]
    public async Task LoginAsync_ConCredencialesValidas_DebeSerExitoso()
    {
        // Arrange
        var authService = _authService;
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
        var authService = _authService;
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