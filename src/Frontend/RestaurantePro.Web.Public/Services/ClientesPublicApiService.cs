using System.Net.Http.Json;
using System.ComponentModel.DataAnnotations;
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
        try
        {
            var resp = await _http.PostAsJsonAsync("api/public/clientes", request);
            if (!resp.IsSuccessStatusCode) return false;
            var body = await resp.Content.ReadFromJsonAsync<ApiResponse<object>>();
            return body?.Success == true;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}

public class PublicClienteRegisterRequest
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Telefono { get; set; } = string.Empty;

    [Required]
    public DateTime FechaNacimiento { get; set; }
}
