using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging.Abstractions;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.EndToEnd;

public class AuthorizationRolesEndToEndTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;

    public AuthorizationRolesEndToEndTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Unauthorized_NoToken_OnComandas_Should401()
    {
        var client = _fixture.CreateClient();
        var resp = await client.GetAsync("/api/operaciones/comandas");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Allowed_Mesero_OnFacturas_Should200()
    {
        var client = _fixture.CreateClient();
        var api = new ApiService(client);
        var auth = new AuthService(api, NullLogger<AuthService>.Instance, new FakeSecureStorageService(), new FakeNavigationService());

        var login = await auth.LoginAsync("mesero@restaurantepro.com", "Mesero123!");
        Assert.True(login.Success);
        var token = await auth.GetTokenAsync();
        Assert.False(string.IsNullOrWhiteSpace(token));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await client.GetAsync("/api/comercial/facturas");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task Forbidden_Mesero_OnUsuarios_Should403()
    {
        var client = _fixture.CreateClient();
        var api = new ApiService(client);
        var auth = new AuthService(api, NullLogger<AuthService>.Instance, new FakeSecureStorageService(), new FakeNavigationService());

        var login = await auth.LoginAsync("mesero@restaurantepro.com", "Mesero123!");
        Assert.True(login.Success);
        var token = await auth.GetTokenAsync();
        Assert.False(string.IsNullOrWhiteSpace(token));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await client.GetAsync("/api/core/usuarios");
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }
}


