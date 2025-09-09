using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;

namespace RestaurantePro.Mobile.IntegrationTests.TestBase;

public static class TestDataBuilders
{
    public static async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var api = new ApiService(client);
        var secure = new FakeSecureStorageService();
        var auth = new AuthService(api, NullLogger<AuthService>.Instance, secure, new FakeNavigationService());
        var login = await auth.LoginAsync(email, password);
        if (!login.Success) throw new InvalidOperationException($"No se pudo hacer login: {login.Error}");
        var token = await auth.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token)) throw new InvalidOperationException("Token vacío tras login");
        return token;
    }

    public static void SetBearer(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task<Guid> GetAnyProductoIdAsync(HttpClient client)
    {
        var resp = await client.GetAsync("/api/core/productos");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var first = doc.RootElement.GetProperty("Data").GetProperty("Items").EnumerateArray().First();
        return first.GetProperty("Id").GetGuid();
    }

    public static async Task<Guid> GetAnyMesaDisponibleIdAsync(HttpClient client)
    {
        var resp = await client.GetAsync("/api/operaciones/mesas/disponibles");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var first = doc.RootElement.GetProperty("Data").GetProperty("Items").EnumerateArray().First();
        return first.GetProperty("Id").GetGuid();
    }

    public static async Task<Guid> CreateComandaAsync(HttpClient client, Guid meseroId, Guid mesaId, Guid productoId, int cantidad = 1, string? observaciones = null)
    {
        var payload = new
        {
            MeseroId = meseroId,
            MesaId = mesaId,
            Observaciones = observaciones ?? "Comanda de prueba",
            ProductosIniciales = new[] { new { ProductoId = productoId, Cantidad = cantidad } }
        };
        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await client.PostAsync("/api/operaciones/comandas", content);
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("Data").GetProperty("Id").GetGuid();
    }

    public static async Task<Guid> GetCurrentUserIdAsync(HttpClient client, string email, string password)
    {
        var loginPayload = JsonSerializer.Serialize(new { Email = email, Password = password });
        using var content = new StringContent(loginPayload, Encoding.UTF8, "application/json");
        var resp = await client.PostAsync("/api/auth/login", content);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var userIdStr = doc.RootElement.GetProperty("Data").GetProperty("UserId").GetString();
        if (string.IsNullOrWhiteSpace(userIdStr)) throw new InvalidOperationException("UserId vacío en login");
        return Guid.Parse(userIdStr);
    }
}


