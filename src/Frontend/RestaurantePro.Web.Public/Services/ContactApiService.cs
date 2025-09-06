using System.Net.Http.Json;
using RestaurantePro.Web.Public.Models;

namespace RestaurantePro.Web.Public.Services;

/// <summary>
/// Servicio para enviar y listar mensajes de contacto.
/// </summary>
public class ContactApiService
{
    private readonly HttpClient _http;

    public ContactApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> EnviarAsync(CreateContactMessageRequest request)
    {
        var resp = await _http.PostAsJsonAsync("api/public/contact/messages", request);
        if (!resp.IsSuccessStatusCode) return false;
        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<ContactMessageDto>>();
        return body?.Success == true;
    }

    public async Task<List<ContactMessageDto>> ObtenerAsync()
    {
        var body = await _http.GetFromJsonAsync<ApiResponse<List<ContactMessageDto>>>("api/public/contact/messages");
        return body?.Data ?? new List<ContactMessageDto>();
    }
}


