using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public class CategoriasApiService : ICategoriasApiService
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

    public async Task<List<CategoriaProductoDto>> ObtenerAsync(bool soloActivas = false, bool soloInactivas = false, bool ocultarVacias = false)
    {
        var http = CreateClient();
        var resp = await http.GetFromJsonAsync<ApiResponse<List<CategoriaProductoDto>>>(
            $"api/core/categorias?soloActivas={soloActivas}&soloInactivas={soloInactivas}&ocultarVacias={ocultarVacias}");
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

    // Implementación de la interfaz ICategoriasApiService
    public async Task<List<CategoriaProductoDto>> ObtenerCategoriasAsync()
    {
        return await ObtenerAsync();
    }

    public async Task<CategoriaProductoDto?> ObtenerCategoriaPorIdAsync(Guid id)
    {
        return await ObtenerPorIdAsync(id);
    }

    public async Task<CategoriaProductoDto?> CrearCategoriaAsync(CreateCategoriaRequest request)
    {
        var response = await CrearAsync(request);
        return response?.Data;
    }

    public async Task<CategoriaProductoDto?> ActualizarCategoriaAsync(Guid id, UpdateCategoriaRequest request)
    {
        var response = await ActualizarAsync(id, request);
        return response?.Data;
    }

    public async Task<bool> EliminarCategoriaAsync(Guid id)
    {
        return await EliminarAsync(id);
    }

    public async Task<bool> CambiarEstadoCategoriaAsync(Guid id, bool activa)
    {
        var http = CreateClient();
        var resp = await http.PatchAsync($"api/core/categorias/{id}/estado?activa={activa}", null);
        return resp.IsSuccessStatusCode;
    }
}


