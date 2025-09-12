using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.EndToEnd;

public class ReservacionesEndToEndTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;
    private readonly FakeSecureStorageService _secureStorage = new();

    public ReservacionesEndToEndTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
        _apiService = new ApiService(_client);
        _authService = new AuthService(_apiService, NullLogger<AuthService>.Instance, _secureStorage, new FakeNavigationService());
    }

    [Fact]
    public async Task ConfirmarReservacion_FlujoCompleto_OK()
    {
        // 1) Login
        var login = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(login.Success);
        var token = await _authService.GetTokenAsync();
        Assert.False(string.IsNullOrWhiteSpace(token));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 2) Obtener un cliente para la reservación
        var clientesResp = await _client.GetAsync("/api/comercial/clientes");
        clientesResp.EnsureSuccessStatusCode();
        var clientesJson = await clientesResp.Content.ReadAsStringAsync();
        using var cliDoc = JsonDocument.Parse(clientesJson);
        var clienteId = cliDoc.RootElement.GetProperty("data").GetProperty("items").EnumerateArray().First().GetProperty("id").GetGuid();

        // 3) Crear reservación (usando el contrato CrearReservacionCommand)
        var crear = new {
            ClienteId = clienteId,
            NombreCliente = "Cliente E2E",
            Telefono = "+51999999999",
            Email = "res.e2e@example.com",
            // Validar ventana: 12:00 a 22:00 local
            FechaHora = DateTime.Now.Date.AddDays(1).AddHours(19),
            NumeroPersonas = 2,
            Observaciones = "E2E Reservación",
            RequiereConfirmacion = false
        };
        using var crearContent = new StringContent(JsonSerializer.Serialize(crear), Encoding.UTF8, "application/json");
        var crearResp = await _client.PostAsync("/api/operaciones/reservaciones", crearContent);
        crearResp.EnsureSuccessStatusCode();
        var crearJson = await crearResp.Content.ReadAsStringAsync();
        using var crearDoc = JsonDocument.Parse(crearJson);
        var reservacionId = crearDoc.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        // 4) Confirmar reservación
        var usuarioId = await _authService.GetUserIdAsync();
        var confirmar = new {
            UsuarioId = Guid.Parse(usuarioId!),
            Observaciones = "Confirmada E2E",
            MetodoConfirmacion = "App"
        };
        using var confirmContent = new StringContent(JsonSerializer.Serialize(confirmar), Encoding.UTF8, "application/json");
        var confirmResp = await _client.PostAsync($"/api/operaciones/reservaciones/{reservacionId}/confirmar", confirmContent);
        confirmResp.EnsureSuccessStatusCode();

        var confirmJson = await confirmResp.Content.ReadAsStringAsync();
        using var confirmDoc = JsonDocument.Parse(confirmJson);
        var estado = confirmDoc.RootElement.GetProperty("data").GetProperty("estado").GetString();
        Assert.False(string.IsNullOrWhiteSpace(estado));
    }
}
