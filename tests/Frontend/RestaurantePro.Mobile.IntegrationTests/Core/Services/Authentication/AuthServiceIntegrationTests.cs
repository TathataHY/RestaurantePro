using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Platform;
using RestaurantePro.Mobile.IntegrationTests.TestBase;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services.Authentication
{
    public class AuthServiceIntegrationTests : MobileIntegrationTestBase
    {
            [Fact]
    public void AuthService_CanBeCreated()
    {
        // Arrange & Act
        var httpClient = CreateClient();
        var secureStorage = new FakeSecureStorageService();
        var apiService = new ApiService(httpClient);
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<RestaurantePro.Mobile.Core.Services.Authentication.AuthService>.Instance;
        var authService = new AuthService(apiService, logger, secureStorage);

        // Assert
        Assert.NotNull(authService);
        Assert.IsType<AuthService>(authService);
    }

    [Fact]
    public void ApiService_CanBeCreated()
    {
        // Arrange & Act
        var httpClient = CreateClient();
        var apiService = new ApiService(httpClient);

        // Assert
        Assert.NotNull(apiService);
        Assert.IsType<ApiService>(apiService);
    }

    [Fact]
    public void SecureStorageService_CanBeCreated()
    {
        // Arrange & Act
        var secureStorage = new FakeSecureStorageService();

        // Assert
        Assert.NotNull(secureStorage);
        Assert.IsType<FakeSecureStorageService>(secureStorage);
    }
    }
} 