using System.Net.Http.Json;
using RestaurantePro.Web.Public.Models;

namespace RestaurantePro.Web.Public.Services;

/// <summary>
/// Servicio para listar y crear reseñas públicas.
/// </summary>
public class ReviewsApiService
{
    private readonly HttpClient _http;

    public ReviewsApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ReviewDto>> ObtenerAsync()
    {
        var resp = await _http.GetFromJsonAsync<ApiResponse<List<ReviewDto>>>("api/public/reviews");
        return resp?.Data ?? new List<ReviewDto>();
    }

    public async Task<bool> CrearAsync(CreateReviewRequest request)
    {
        var resp = await _http.PostAsJsonAsync("api/public/reviews", request);
        if (!resp.IsSuccessStatusCode)
            return false;

        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<ReviewDto>>();
        return body?.Success == true;
    }
}


