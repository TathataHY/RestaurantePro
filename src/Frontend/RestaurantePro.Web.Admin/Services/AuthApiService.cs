using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public class AuthApiService : IAuthApiService
{
    private readonly IHttpClientFactory _httpFactory;

    public AuthApiService(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var http = _httpFactory.CreateClient("Api");
        var res = await http.PostAsJsonAsync("api/auth/login", request);
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }
        var api = await res.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        return api?.Data;
    }

    public async Task<AuthUserDto?> GetProfileAsync()
    {
        var http = _httpFactory.CreateClient("Api");
        var res = await http.GetAsync("api/auth/profile");
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }
        var api = await res.Content.ReadFromJsonAsync<ApiResponse<AuthUserDto>>();
        return api?.Data;
    }
}


