using Microsoft.Extensions.Logging.Abstractions;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.EndToEnd;

public class AuthLogoutEndToEndTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;

    public AuthLogoutEndToEndTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Logout_Should_ClearToken_And_ProtectedEndpoint_ShouldReturn401()
    {
        var client = _fixture.CreateClient();
        var api = new ApiService(client);
        var secure = new FakeSecureStorageService();
        var auth = new AuthService(api, NullLogger<AuthService>.Instance, secure, new FakeNavigationService());

        // 1) Login
        var login = await auth.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(login.Success);
        var token = await auth.GetTokenAsync();
        Assert.False(string.IsNullOrWhiteSpace(token));

        // 2) Validar endpoint protegido OK con token
        var okResp = await api.GetAsync<object>("api/core/productos", token);
        Assert.True(okResp.Success);

        // 3) Logout limpia token y storage
        await auth.LogoutAsync();
        var tokenAfter = await auth.GetTokenAsync();
        Assert.True(string.IsNullOrWhiteSpace(tokenAfter));

        // 4) Endpoint protegido sin token => 401/403
        var unauthorized = await api.GetAsync<object>("api/core/productos", tokenAfter);
        Assert.False(unauthorized.Success);
        Assert.True(unauthorized.StatusCode == 401 || unauthorized.StatusCode == 403);
    }
}


