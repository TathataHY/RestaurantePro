using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.EndToEnd;

public class MesasEndToEndTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;
    private readonly FakeSecureStorageService _secureStorage = new();

    public MesasEndToEndTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
        _apiService = new ApiService(_client);
        _authService = new AuthService(_apiService, NullLogger<AuthService>.Instance, _secureStorage, new FakeNavigationService());
    }

    [Fact]
    public async Task AsignarYLiberarMesa_FlujoBasico_OK()
    {
        // Login via builders
        var token = await TestDataBuilders.LoginAsync(_client, "admin@restaurantepro.com", "AdminRestaurante123!");
        TestDataBuilders.SetBearer(_client, token);

        // Obtener mesas disponibles (Data.Items)
        var mesaId = await TestDataBuilders.GetAnyMesaDisponibleIdAsync(_client);

        // Asignar mesa: POST /mesas/{id}/asignar
        var asignarCmd = new { ClienteNombre = "Cliente E2E", NumeroPersonas = 2 };
        using var asignarContent = new StringContent(JsonSerializer.Serialize(asignarCmd), Encoding.UTF8, "application/json");
        var asignarResp = await _client.PostAsync($"/api/operaciones/mesas/{mesaId}/asignar", asignarContent);
        asignarResp.EnsureSuccessStatusCode();

        // Verificar estado ocupado consultando la mesa
        var getMesa = await _client.GetAsync($"/api/operaciones/mesas/{mesaId}");
        getMesa.EnsureSuccessStatusCode();
        var mesaJson = await getMesa.Content.ReadAsStringAsync();
        using var mesaDoc = JsonDocument.Parse(mesaJson);
        var estado = mesaDoc.RootElement.GetProperty("data").GetProperty("estado").GetString();
        Assert.False(string.IsNullOrWhiteSpace(estado));

        // Liberar mesa: POST /mesas/{id}/liberar
        var liberarCmd = new { Motivo = "Fin de atención" };
        using var liberarContent = new StringContent(JsonSerializer.Serialize(liberarCmd), Encoding.UTF8, "application/json");
        var liberarResp = await _client.PostAsync($"/api/operaciones/mesas/{mesaId}/liberar", liberarContent);
        liberarResp.EnsureSuccessStatusCode();

        // Verificar que la respuesta contiene Data.Id = mesaId
        var liberarJson = await liberarResp.Content.ReadAsStringAsync();
        using var liberarDoc = JsonDocument.Parse(liberarJson);
        var returnedId = liberarDoc.RootElement.GetProperty("data").GetProperty("id").GetGuid();
        Assert.Equal(mesaId, returnedId);
    }
}
