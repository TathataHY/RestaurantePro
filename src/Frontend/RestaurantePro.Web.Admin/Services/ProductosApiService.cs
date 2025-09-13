using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public class ProductosApiService : IProductosApiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenStore _tokenStore;

    public ProductosApiService(IHttpClientFactory httpFactory, TokenStore tokenStore)
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

    public async Task<List<CategoriaProductoDto>> ObtenerCategoriasAsync()
    {
        var http = CreateClient();
        var resp = await http.GetFromJsonAsync<ApiResponse<List<CategoriaProductoDto>>>("api/core/categorias?soloActivas=false&ocultarVacias=false");
        return resp?.Data ?? new List<CategoriaProductoDto>();
    }

    public async Task<PaginatedList<ProductoDto>> ObtenerProductosPaginadosAsync(int pageNumber, int pageSize, string? filtro, Guid? categoriaId, bool soloActivos, string orderBy, string orderDirection)
    {
        var http = CreateClient();
        var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var url = $"api/core/productos?PageNumber={pageNumber}&PageSize={pageSize}&soloActivos={soloActivos}&orderBy={Uri.EscapeDataString(orderBy)}&orderDirection={Uri.EscapeDataString(orderDirection)}&_ts={ts}";
        if (!string.IsNullOrWhiteSpace(filtro)) url += "&filtro=" + Uri.EscapeDataString(filtro);
        if (categoriaId.HasValue) url += "&categoriaId=" + categoriaId.Value;
        var resp = await http.GetFromJsonAsync<ApiResponse<PaginatedList<ProductoDto>>>(url);
        return resp?.Data ?? new PaginatedList<ProductoDto>();
    }

    public async Task<ProductoDto?> ObtenerPorIdAsync(Guid id)
    {
        var http = CreateClient();
        var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var resp = await http.GetFromJsonAsync<ApiResponse<ProductoDto>>($"api/core/productos/{id}?_ts={ts}");
        return resp?.Data;
    }

    public async Task<ProductoDto?> CrearAsync(CreateProductoRequest dto)
    {
        var http = CreateClient();
        var res = await http.PostAsJsonAsync("api/core/productos", dto);
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }
        var resp = await res.Content.ReadFromJsonAsync<ApiResponse<ProductoDto>>();
        return resp?.Data;
    }

    public async Task<ProductoDto?> ActualizarAsync(UpdateProductoRequest dto)
    {
        var http = CreateClient();
        var res = await http.PutAsJsonAsync($"api/core/productos/{dto.Id}", dto);
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }
        var resp = await res.Content.ReadFromJsonAsync<ApiResponse<ProductoDto>>();
        return resp?.Data;
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        var http = CreateClient();
        var res = await http.DeleteAsync($"api/core/productos/{id}");
        return res.IsSuccessStatusCode;
    }

    /// <summary>
    /// Obtiene productos con paginación
    /// </summary>
    public async Task<PaginatedList<ProductoDto>?> ObtenerProductosAsync(int pageNumber = 1, int pageSize = 20, string? filtro = null)
    {
        return await ObtenerProductosPaginadosAsync(pageNumber, pageSize, filtro ?? string.Empty, null, true, "Nombre", "asc");
    }

    /// <summary>
    /// Obtiene un producto por ID
    /// </summary>
    public async Task<ProductoDto?> ObtenerProductoPorIdAsync(Guid id)
    {
        return await ObtenerPorIdAsync(id);
    }

    /// <summary>
    /// Crea un nuevo producto
    /// </summary>
    public async Task<ProductoDto?> CrearProductoAsync(CrearProductoRequest request)
    {
        // Convertir CrearProductoRequest a CreateProductoRequest
        var createRequest = new CreateProductoRequest
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            CategoriaId = request.CategoriaId,
            Activo = request.Activo
        };
        return await CrearAsync(createRequest);
    }

    /// <summary>
    /// Actualiza un producto existente
    /// </summary>
    public async Task<ProductoDto?> ActualizarProductoAsync(Guid id, ActualizarProductoRequest request)
    {
        // Convertir ActualizarProductoRequest a UpdateProductoRequest
        var updateRequest = new UpdateProductoRequest
        {
            Id = id,
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Precio = request.Precio ?? 0,
            CategoriaId = request.CategoriaId ?? Guid.Empty,
            Activo = request.Activo
        };
        return await ActualizarAsync(updateRequest);
    }

    /// <summary>
    /// Elimina un producto
    /// </summary>
    public async Task<bool> EliminarProductoAsync(Guid id)
    {
        return await EliminarAsync(id);
    }

    /// <summary>
    /// Cambia el estado de un producto
    /// </summary>
    public async Task<bool> CambiarEstadoProductoAsync(Guid id, bool activo)
    {
        var http = CreateClient();
        var res = await http.PostAsync($"api/core/productos/{id}/cambiar-estado?activo={activo}", null);
        return res.IsSuccessStatusCode;
    }
}


