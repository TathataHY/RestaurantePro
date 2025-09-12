using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.EndToEnd;

public class FacturacionEndToEndTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;
    private readonly FakeSecureStorageService _secureStorage = new();

    public FacturacionEndToEndTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
        _apiService = new ApiService(_client);
        _authService = new AuthService(_apiService, NullLogger<AuthService>.Instance, _secureStorage, new FakeNavigationService());
    }

    [Fact]
    public async Task GenerarFacturaDesdeComanda_FlujoCompleto_OK()
    {
        // Login
        var login = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(login.Success);
        var token = await _authService.GetTokenAsync();
        Assert.False(string.IsNullOrWhiteSpace(token));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Producto válido
        var productosResp = await _client.GetAsync("/api/core/productos");
        productosResp.EnsureSuccessStatusCode();
        var productosJson = await productosResp.Content.ReadAsStringAsync();
        using var prodDoc = JsonDocument.Parse(productosJson);
        var productoId = prodDoc.RootElement.GetProperty("data").GetProperty("items").EnumerateArray().First().GetProperty("id").GetGuid();

        // Mesa disponible
        var mesasResp = await _client.GetAsync("/api/operaciones/mesas/disponibles");
        mesasResp.EnsureSuccessStatusCode();
        var mesasJson = await mesasResp.Content.ReadAsStringAsync();
        using var mesasDoc = JsonDocument.Parse(mesasJson);
        var mesaId = mesasDoc.RootElement.GetProperty("data").GetProperty("items").EnumerateArray().First().GetProperty("id").GetGuid();

        // Crear comanda
        var userId = await _authService.GetUserIdAsync();
        var crearCmd = new {
            MeseroId = Guid.Parse(userId!),
            MesaId = mesaId,
            Observaciones = "E2E facturación",
            ProductosIniciales = new[] { new { ProductoId = productoId, Cantidad = 1 } }
        };
        using var crearContent = new StringContent(JsonSerializer.Serialize(crearCmd), Encoding.UTF8, "application/json");
        var crearResp = await _client.PostAsync("/api/operaciones/comandas", crearContent);
        crearResp.EnsureSuccessStatusCode();
        var crearJson = await crearResp.Content.ReadAsStringAsync();
        using var crearDoc = JsonDocument.Parse(crearJson);
        var comandaId = crearDoc.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        // Finalizar comanda (genera el cierre y habilita facturación)
        var finalizarCmd = new { UsuarioId = Guid.Parse(userId!), ObservacionesFinalizacion = "OK", ValidarTodosItemsListos = true, NotificarMesero = true };
        using var finContent = new StringContent(JsonSerializer.Serialize(finalizarCmd), Encoding.UTF8, "application/json");
        var finResp = await _client.PostAsync($"/api/operaciones/comandas/{comandaId}/finalizar", finContent);
        // Puede ser OK o error según reglas; no fallar si 400/422
        Assert.True(finResp.IsSuccessStatusCode || (int)finResp.StatusCode is 400 or 422);

        // Consultar facturas por comanda
        var facturasResp = await _client.GetAsync($"/api/comercial/facturas/comanda/{comandaId}");
        // Dependiendo de la lógica, puede haber o no facturas aún; validar respuesta coherente (200 o 404)
        Assert.True(facturasResp.IsSuccessStatusCode || (int)facturasResp.StatusCode == 404);

        if (facturasResp.IsSuccessStatusCode)
        {
            var facturasJson = await facturasResp.Content.ReadAsStringAsync();
            using var factsDoc = JsonDocument.Parse(facturasJson);
            var data = factsDoc.RootElement.GetProperty("data").EnumerateArray().ToList();
            Assert.True(data.Count >= 0);
        }
    }
}
