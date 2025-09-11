using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public class CategoriasApiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenStore _tokenStore;

    public CategoriasApiService(IHttpClientFactory httpFactory, TokenStore tokenStore)
    {
        _httpFactory = httpFactory;
        _tokenStore = tokenStore;
    }

    private HttpClient CreateClient()
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
        return http;
    }

    public async Task<List<CategoriaProductoDto>> ObtenerAsync(bool soloActivas = false, bool ocultarVacias = false)
    {
        var http = CreateClient();
        var resp = await http.GetFromJsonAsync<ApiResponse<List<CategoriaProductoDto>>>(
            $"api/core/categorias?soloActivas={soloActivas}&ocultarVacias={ocultarVacias}");
        return resp?.Data ?? new List<CategoriaProductoDto>();
    }

    public async Task<List<CategoriaProductoDto>> BuscarAsync(string nombre)
    {
        var http = CreateClient();
        var resp = await http.GetFromJsonAsync<ApiResponse<List<CategoriaProductoDto>>>(
            $"api/core/categorias/buscar?nombre={Uri.EscapeDataString(nombre)}");
        return resp?.Data ?? new List<CategoriaProductoDto>();
    }

    public async Task<CategoriaProductoDto?> ObtenerPorIdAsync(Guid id)
    {
        var http = CreateClient();
        var resp = await http.GetFromJsonAsync<ApiResponse<CategoriaProductoDto>>(
            $"api/core/categorias/{id}");
        return resp?.Data;
    }

    public async Task<ApiResponse<CategoriaProductoDto>?> CrearAsync(CreateCategoriaRequest request)
    {
        var http = CreateClient();
        var resp = await http.PostAsJsonAsync("api/core/categorias", request);
        if (resp.IsSuccessStatusCode)
        {
            return await resp.Content.ReadFromJsonAsync<ApiResponse<CategoriaProductoDto>>();
        }
        return null;
    }

    public async Task<ApiResponse<CategoriaProductoDto>?> ActualizarAsync(Guid id, UpdateCategoriaRequest request)
    {
        var http = CreateClient();
        var resp = await http.PutAsJsonAsync($"api/core/categorias/{id}", request);
        if (resp.IsSuccessStatusCode)
        {
            return await resp.Content.ReadFromJsonAsync<ApiResponse<CategoriaProductoDto>>();
        }
        return null;
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        var http = CreateClient();
        var resp = await http.DeleteAsync($"api/core/categorias/{id}");
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> ValidarNombreUnicoAsync(string nombre, Guid? idExcluir = null)
    {
        var http = CreateClient();
        var url = $"api/core/categorias/validar-nombre?nombre={Uri.EscapeDataString(nombre)}";
        if (idExcluir.HasValue)
        {
            url += $"&idExcluir={idExcluir.Value}";
        }
        var resp = await http.GetFromJsonAsync<ApiResponse<bool>>(url);
        return resp?.Data ?? false;
    }
}


