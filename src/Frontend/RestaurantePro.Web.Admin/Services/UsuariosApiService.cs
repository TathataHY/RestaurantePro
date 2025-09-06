using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public class UsuariosApiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenStore _tokenStore;

    public UsuariosApiService(IHttpClientFactory httpFactory, TokenStore tokenStore)
    {
        _httpFactory = httpFactory;
        _tokenStore = tokenStore;
    }

    public async Task<List<UsuarioDto>> ObtenerUsuariosAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? filtro = null,
        bool soloActivos = true,
        string orderBy = "NombreCompleto",
        string orderDirection = "asc")
    {
        var http = _httpFactory.CreateClient("Api");
        // Refuerzo: adjuntar explícitamente el token
        if (!string.IsNullOrWhiteSpace(_tokenStore.Token))
        {
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStore.Token);
            if (!http.DefaultRequestHeaders.Contains("X-Bearer-Token"))
            {
                http.DefaultRequestHeaders.Add("X-Bearer-Token", _tokenStore.Token);
            }
        }
        var url = $"api/core/usuarios?pageNumber={pageNumber}&pageSize={pageSize}&soloActivos={soloActivos}&orderBy={orderBy}&orderDirection={orderDirection}";
        if (!string.IsNullOrWhiteSpace(filtro))
        {
            url += $"&filtro={Uri.EscapeDataString(filtro)}";
        }
        var res = await http.GetAsync(url);
        if (!res.IsSuccessStatusCode)
        {
            return new List<UsuarioDto>();
        }
        var resp = await res.Content.ReadFromJsonAsync<ApiResponse<List<UsuarioDto>>>();
        return resp?.Data ?? new List<UsuarioDto>();
    }

    public async Task<UsuarioDto?> ObtenerPorIdAsync(Guid id)
    {
        var http = _httpFactory.CreateClient("Api");
        if (!string.IsNullOrWhiteSpace(_tokenStore.Token))
        {
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStore.Token);
            if (!http.DefaultRequestHeaders.Contains("X-Bearer-Token"))
            {
                http.DefaultRequestHeaders.Add("X-Bearer-Token", _tokenStore.Token);
            }
        }
        var resp = await http.GetFromJsonAsync<ApiResponse<UsuarioDto>>($"api/core/usuarios/{id}");
        return resp?.Data;
    }

    public async Task<UsuarioDto?> CrearAsync(CreateUsuarioRequest request)
    {
        var http = _httpFactory.CreateClient("Api");
        if (!string.IsNullOrWhiteSpace(_tokenStore.Token))
        {
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStore.Token);
            if (!http.DefaultRequestHeaders.Contains("X-Bearer-Token"))
            {
                http.DefaultRequestHeaders.Add("X-Bearer-Token", _tokenStore.Token);
            }
        }
        var res = await http.PostAsJsonAsync("api/core/usuarios", request);
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }
        var resp = await res.Content.ReadFromJsonAsync<ApiResponse<UsuarioDto>>();
        return resp?.Data;
    }

    public async Task<UsuarioDto?> ActualizarAsync(Guid id, UpdateUsuarioRequest request)
    {
        var http = _httpFactory.CreateClient("Api");
        if (!string.IsNullOrWhiteSpace(_tokenStore.Token))
        {
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStore.Token);
            if (!http.DefaultRequestHeaders.Contains("X-Bearer-Token"))
            {
                http.DefaultRequestHeaders.Add("X-Bearer-Token", _tokenStore.Token);
            }
        }
        var res = await http.PutAsJsonAsync($"api/core/usuarios/{id}", request);
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }
        var resp = await res.Content.ReadFromJsonAsync<ApiResponse<UsuarioDto>>();
        return resp?.Data;
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        var http = _httpFactory.CreateClient("Api");
        if (!string.IsNullOrWhiteSpace(_tokenStore.Token))
        {
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStore.Token);
            if (!http.DefaultRequestHeaders.Contains("X-Bearer-Token"))
            {
                http.DefaultRequestHeaders.Add("X-Bearer-Token", _tokenStore.Token);
            }
        }
        var res = await http.DeleteAsync($"api/core/usuarios/{id}");
        return res.IsSuccessStatusCode;
    }
}


