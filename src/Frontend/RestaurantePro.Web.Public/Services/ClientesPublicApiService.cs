using System.Net.Http.Json;
using RestaurantePro.Web.Public.Models;

namespace RestaurantePro.Web.Public.Services;

public class ClientesPublicApiService
{
    private readonly HttpClient _http;

    public ClientesPublicApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> RegistrarAsync(PublicClienteRegisterRequest request)
    {
        var resp = await _http.PostAsJsonAsync("api/public/clientes", request);
        if (!resp.IsSuccessStatusCode) return false;
        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return body?.Success == true;
    }
}

public class PublicClienteRegisterRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
}
