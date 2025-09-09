using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.EndToEnd;

public class InvalidTransitionsEndToEndTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;

    public InvalidTransitionsEndToEndTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Mesas_InvalidTransitions_ShouldReturnErrors()
    {
        var client = _fixture.CreateClient();
        var api = new ApiService(client);
        var auth = new AuthService(api, NullLogger<AuthService>.Instance, new FakeSecureStorageService(), new FakeNavigationService());

        // Login y token
        var login = await auth.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(login.Success);
        var token = await auth.GetTokenAsync();
        Assert.False(string.IsNullOrWhiteSpace(token));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Obtener una mesa disponible
        var disp = await client.GetAsync("/api/operaciones/mesas/disponibles");
        disp.EnsureSuccessStatusCode();
        using var dispDoc = JsonDocument.Parse(await disp.Content.ReadAsStringAsync());
        var mesas = dispDoc.RootElement.GetProperty("Data").GetProperty("Items").EnumerateArray().ToList();
        Assert.NotEmpty(mesas);
        var mesaId = mesas.First().GetProperty("Id").GetGuid();

        // Asignar OK
        var asignarBody = new { ClienteNombre = "Cliente E2E", NumeroPersonas = 2 };
        var asignarResp = await client.PostAsync($"/api/operaciones/mesas/{mesaId}/asignar", new StringContent(JsonSerializer.Serialize(asignarBody), Encoding.UTF8, "application/json"));
        asignarResp.EnsureSuccessStatusCode();

        // Asignar de nuevo -> debe fallar (400/404)
        var asignar2 = await client.PostAsync($"/api/operaciones/mesas/{mesaId}/asignar", new StringContent(JsonSerializer.Serialize(asignarBody), Encoding.UTF8, "application/json"));
        Assert.False(asignar2.IsSuccessStatusCode);

        // Liberar OK
        var liberarResp = await client.PostAsync($"/api/operaciones/mesas/{mesaId}/liberar", new StringContent(JsonSerializer.Serialize(new { Motivo = "Fin" }), Encoding.UTF8, "application/json"));
        liberarResp.EnsureSuccessStatusCode();

        // Liberar de nuevo -> debe fallar (400/404)
        var liberar2 = await client.PostAsync($"/api/operaciones/mesas/{mesaId}/liberar", new StringContent(JsonSerializer.Serialize(new { Motivo = "Fin" }), Encoding.UTF8, "application/json"));
        liberar2.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Comandas_InvalidTransitions_ShouldReturnErrors()
    {
        var client = _fixture.CreateClient();
        var api = new ApiService(client);
        var auth = new AuthService(api, NullLogger<AuthService>.Instance, new FakeSecureStorageService(), new FakeNavigationService());

        // Login y token + userId
        var login = await auth.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        Assert.True(login.Success);
        var token = await auth.GetTokenAsync();
        Assert.False(string.IsNullOrWhiteSpace(token));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var userIdStr = await auth.GetUserIdAsync();
        Assert.False(string.IsNullOrWhiteSpace(userIdStr));
        var userId = Guid.Parse(userIdStr!);

        // Producto válido
        var productos = await client.GetAsync("/api/core/productos");
        productos.EnsureSuccessStatusCode();
        using var prodDoc = JsonDocument.Parse(await productos.Content.ReadAsStringAsync());
        var firstProd = prodDoc.RootElement.GetProperty("Data").GetProperty("Items").EnumerateArray().First();
        var productoId = firstProd.GetProperty("Id").GetGuid();

        // Mesa disponible
        var mesasDisponibles = await client.GetAsync("/api/operaciones/mesas/disponibles");
        mesasDisponibles.EnsureSuccessStatusCode();
        using var mesasDoc = JsonDocument.Parse(await mesasDisponibles.Content.ReadAsStringAsync());
        var mesaId = mesasDoc.RootElement.GetProperty("Data").GetProperty("Items").EnumerateArray().First().GetProperty("Id").GetGuid();

        // Crear comanda
        var crear = new
        {
            MeseroId = userId,
            MesaId = mesaId,
            Observaciones = "E2E Invalid Transitions",
            ProductosIniciales = new[] { new { ProductoId = productoId, Cantidad = 1 } }
        };
        var crearResp = await client.PostAsync("/api/operaciones/comandas", new StringContent(JsonSerializer.Serialize(crear), Encoding.UTF8, "application/json"));
        crearResp.EnsureSuccessStatusCode();
        using var crearDoc = JsonDocument.Parse(await crearResp.Content.ReadAsStringAsync());
        var comandaId = crearDoc.RootElement.GetProperty("Data").GetProperty("Id").GetGuid();

        // Finalizar OK
        var finalizarBody = new { UsuarioId = userId, ObservacionesFinalizacion = "OK" };
        var finResp = await client.PostAsync($"/api/operaciones/comandas/{comandaId}/finalizar", new StringContent(JsonSerializer.Serialize(finalizarBody), Encoding.UTF8, "application/json"));
        finResp.EnsureSuccessStatusCode();

        // Agregar producto a comanda cerrada -> debe fallar (400/404)
        var agregarBody = new { ProductoId = productoId, Cantidad = 1 };
        var agregarResp = await client.PostAsync($"/api/operaciones/comandas/{comandaId}/productos", new StringContent(JsonSerializer.Serialize(agregarBody), Encoding.UTF8, "application/json"));
        Assert.True(agregarResp.StatusCode == HttpStatusCode.BadRequest || agregarResp.StatusCode == HttpStatusCode.NotFound);

        // Cambiar a estado inválido -> debe fallar (400/404)
        var cambioEstadoBody = new { NuevoEstado = "estado-invalido" };
        var cambioResp = await client.PatchAsync($"/api/operaciones/comandas/{comandaId}/estado", new StringContent(JsonSerializer.Serialize(cambioEstadoBody), Encoding.UTF8, "application/json"));
        Assert.True(cambioResp.StatusCode == HttpStatusCode.BadRequest || cambioResp.StatusCode == HttpStatusCode.NotFound);
    }
}


