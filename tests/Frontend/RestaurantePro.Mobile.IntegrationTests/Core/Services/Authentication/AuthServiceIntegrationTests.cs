using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Platform;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services.Authentication
{
    public class AuthServiceIntegrationTests : IClassFixture<MobileIntegrationTestFixture>
    {
        private readonly MobileIntegrationTestFixture _fixture;
        private readonly HttpClient _client;

        public AuthServiceIntegrationTests(MobileIntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.CreateClient();
        }

        [Fact]
        public void AuthService_CanBeCreated()
        {
            // Arrange & Act
            var httpClient = _client;
            var secureStorage = new FakeSecureStorageService();
            var apiService = new ApiService(httpClient);
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<RestaurantePro.Mobile.Core.Services.Authentication.AuthService>.Instance;
            var authService = new AuthService(apiService, logger, secureStorage, new FakeNavigationService());

            // Assert
            Assert.NotNull(authService);
            Assert.IsType<AuthService>(authService);
        }

        [Fact]
        public void ApiService_CanBeCreated()
        {
            // Arrange & Act
            var httpClient = _client;
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