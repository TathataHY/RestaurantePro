using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services.Authentication;

public class AuthTokenRefreshIntegrationTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;

    public AuthTokenRefreshIntegrationTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetTokenAsync_WithExpiredTokenAndRemember_RefreshesTokenSuccessfully()
    {
        var client = _fixture.CreateClient();
        var api = new ApiService(client);
        var storage = new FakeSecureStorageService();
        var nav = new FakeNavigationService();

        // 1) Login con Recordarme=true para obtener refresh token
        var auth = new AuthService(api, NullLogger<AuthService>.Instance, storage, nav);
        var login = await auth.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!", recordarme: true);
        Assert.True(login.Success);
        Assert.NotNull(login.Data);
        Assert.False(string.IsNullOrWhiteSpace(login.Data!.RefreshToken));

        // 2) Sobrescribir el token por uno expirado (manteniendo el refresh token)
        var expiredJwt = CreateExpiredJwt(sub: await auth.GetUserIdAsync() ?? Guid.NewGuid().ToString());
        await storage.SetAsync("auth_token", expiredJwt);

        // 3) Crear un nuevo AuthService que lea desde storage y fuerce GetToken -> refresh
        var auth2 = new AuthService(api, NullLogger<AuthService>.Instance, storage, nav);
        var refreshedToken = await auth2.GetTokenAsync();

        Assert.False(string.IsNullOrWhiteSpace(refreshedToken));
        Assert.NotEqual(expiredJwt, refreshedToken);

        // 4) Validar que AuthService puede leer UserId del token renovado
        var userId = await auth2.GetUserIdAsync();
        Assert.False(string.IsNullOrWhiteSpace(userId));
    }

    [Fact]
    public async Task GetTokenAsync_WithExpiredTokenAndNoRemember_FailsAndClearsSession()
    {
        var client = _fixture.CreateClient();
        var api = new ApiService(client);
        var storage = new FakeSecureStorageService();
        var nav = new FakeNavigationService();

        // 1) Login con Recordarme=false (no guarda refresh token)
        var auth = new AuthService(api, NullLogger<AuthService>.Instance, storage, nav);
        var login = await auth.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!", recordarme: false);
        Assert.True(login.Success);

        // 2) Forzar token expirado en storage
        var expiredJwt = CreateExpiredJwt(sub: await auth.GetUserIdAsync() ?? Guid.NewGuid().ToString());
        await storage.SetAsync("auth_token", expiredJwt);

        // 3) Nuevo AuthService intenta GetToken -> no puede renovar -> sesión limpiada
        var auth2 = new AuthService(api, NullLogger<AuthService>.Instance, storage, nav);
        var token = await auth2.GetTokenAsync();
        Assert.True(string.IsNullOrWhiteSpace(token));

        // Verificar que un endpoint protegido falla sin token
        var profile = await api.GetAsync<object>("api/auth/profile", token);
        Assert.False(profile.Success);
        Assert.True(profile.StatusCode == 401 || profile.StatusCode == 403);
    }

    private static string CreateExpiredJwt(string sub)
    {
        // Construir un JWT simple con exp en el pasado. No se valida firma en ReadJwtToken.
        var header = JsonSerializer.Serialize(new { alg = "HS256", typ = "JWT" });
        var payload = JsonSerializer.Serialize(new
        {
            sub,
            exp = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeSeconds()
        });

        string B64(string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        return $"{B64(header)}.{B64(payload)}.signature";
    }
}


