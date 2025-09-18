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
        try
        {
            var url = $"api/core/categorias?soloActivas={soloActivas}&ocultarVacias={ocultarVacias}";
            Console.WriteLine($"[MenuApiService] Obteniendo categorías desde: {url}");
            
            var resp = await _http.GetFromJsonAsync<ApiResponse<List<CategoriaProductoDto>>>(url);
            
            Console.WriteLine($"[MenuApiService] Respuesta recibida - Success: {resp?.Success}, Count: {resp?.Data?.Count}");
            
            return resp?.Data ?? new List<CategoriaProductoDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MenuApiService] Error obteniendo categorías: {ex.Message}");
            return new List<CategoriaProductoDto>();
        }
    }

    public async Task<List<ProductoDto>> ObtenerProductosPorCategoriaAsync(Guid categoriaId, bool soloActivos = true, bool ordenarPorPopularidad = false)
    {
        try
        {
            var url = $"api/core/productos/categoria/{categoriaId}?soloActivos={soloActivos}&ordenarPorPopularidad={ordenarPorPopularidad}";
            Console.WriteLine($"[MenuApiService] Obteniendo productos desde: {url}");
            
            var resp = await _http.GetFromJsonAsync<ApiResponse<List<ProductoDto>>>(url);
            
            Console.WriteLine($"[MenuApiService] Productos recibidos - Success: {resp?.Success}, Count: {resp?.Data?.Count}");
            
            return resp?.Data ?? new List<ProductoDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MenuApiService] Error obteniendo productos: {ex.Message}");
            return new List<ProductoDto>();
        }
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

    public async Task<List<ProductoDto>> BuscarProductosAsync(string texto, bool soloActivos = true, int limite = 100)
    {
        try
        {
            var url = $"api/core/productos/buscar?texto={Uri.EscapeDataString(texto)}&soloActivos={soloActivos}&limite={limite}";
            Console.WriteLine($"[MenuApiService] Buscando productos: {url}");
            
            var resp = await _http.GetFromJsonAsync<ApiResponse<List<ProductoDto>>>(url);
            
            Console.WriteLine($"[MenuApiService] Productos encontrados - Success: {resp?.Success}, Count: {resp?.Data?.Count}");
            
            return resp?.Data ?? new List<ProductoDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MenuApiService] Error buscando productos: {ex.Message}");
            return new List<ProductoDto>();
        }
    }
}


