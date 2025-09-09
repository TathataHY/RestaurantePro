using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.EndToEnd;

public class ProductosSmokeEndToEndTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;

    public ProductosSmokeEndToEndTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
        _apiService = new ApiService(_client);
        _authService = new AuthService(_apiService, NullLogger<AuthService>.Instance, new FakeSecureStorageService(), new FakeNavigationService());
    }

    [Fact]
    public async Task Productos_Smoke_ListarYDetalle_OK()
    {
        // Login
        var login = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(login.Success);
        var token = await _authService.GetTokenAsync();
        Assert.False(string.IsNullOrWhiteSpace(token));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Listar productos (paginado)
        var listResp = await _client.GetAsync("/api/core/productos");
        listResp.EnsureSuccessStatusCode();
        var listJson = await listResp.Content.ReadAsStringAsync();
        using var listDoc = JsonDocument.Parse(listJson);
        var items = listDoc.RootElement.GetProperty("Data").GetProperty("Items").EnumerateArray().ToList();
        Assert.True(items.Count > 0);
        var firstId = items.First().GetProperty("Id").GetGuid();

        // Detalle por Id
        var detailResp = await _client.GetAsync($"/api/core/productos/{firstId}");
        detailResp.EnsureSuccessStatusCode();
        var detailJson = await detailResp.Content.ReadAsStringAsync();
        using var detailDoc = JsonDocument.Parse(detailJson);
        var data = detailDoc.RootElement.GetProperty("Data");
        Assert.Equal(firstId, data.GetProperty("Id").GetGuid());
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("Nombre").GetString()));
    }
}


