using System.Net.Http.Json;
using RestaurantePro.Web.Public.Models;

namespace RestaurantePro.Web.Public.Services;

/// <summary>
/// Servicio para consultar categorías y productos públicos del menú.
/// </summary>
public class MenuApiService
{
    private readonly HttpClient _http;

    public MenuApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<CategoriaProductoDto>> ObtenerCategoriasAsync(bool soloActivas = true, bool ocultarVacias = true)
    {
        var resp = await _http.GetFromJsonAsync<ApiResponse<List<CategoriaProductoDto>>>($"api/core/categorias?soloActivas={soloActivas}&ocultarVacias={ocultarVacias}");
        return resp?.Data ?? new List<CategoriaProductoDto>();
    }

    public async Task<List<ProductoDto>> ObtenerProductosPorCategoriaAsync(Guid categoriaId, bool soloActivos = true, bool ordenarPorPopularidad = false)
    {
        var resp = await _http.GetFromJsonAsync<ApiResponse<List<ProductoDto>>>($"api/core/productos/categoria/{categoriaId}?soloActivos={soloActivos}&ordenarPorPopularidad={ordenarPorPopularidad}");
        return resp?.Data ?? new List<ProductoDto>();
    }

    public async Task<PaginatedList<ProductoDto>> ObtenerProductosPaginadosAsync(Guid categoriaId, int pageNumber, int pageSize, bool soloActivos, string orderBy, string orderDirection)
    {
        var url = $"api/core/productos?CategoriaId={categoriaId}&PageNumber={pageNumber}&PageSize={pageSize}&SoloActivos={soloActivos}&OrderBy={Uri.EscapeDataString(orderBy)}&OrderDirection={Uri.EscapeDataString(orderDirection)}";
        var resp = await _http.GetFromJsonAsync<ApiResponse<PaginatedList<ProductoDto>>>(url);
        return resp?.Data ?? new PaginatedList<ProductoDto>();
    }

    public async Task<List<CategoriaProductoDto>> BuscarCategoriasAsync(string nombre)
    {
        var resp = await _http.GetFromJsonAsync<ApiResponse<List<CategoriaProductoDto>>>("api/core/categorias/buscar?nombre=" + Uri.EscapeDataString(nombre));
        return resp?.Data ?? new List<CategoriaProductoDto>();
    }
}


