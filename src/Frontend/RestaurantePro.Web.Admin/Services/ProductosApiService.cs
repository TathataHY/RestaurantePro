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

    public async Task<PaginatedList<ProductoDto>> ObtenerProductosPaginadosAsync(
        int pageNumber, 
        int pageSize, 
        string? filtro, 
        Guid? categoriaId, 
        bool soloActivos, 
        decimal? precioMinimo = null,
        decimal? precioMaximo = null,
        DateTime? fechaCreacionDesde = null,
        DateTime? fechaCreacionHasta = null,
        int? popularidadMinima = null,
        int? popularidadMaxima = null,
        string orderBy = "Nombre", 
        string orderDirection = "asc")
    {
        var http = CreateClient();
        var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var url = $"api/core/productos?PageNumber={pageNumber}&PageSize={pageSize}&soloActivos={soloActivos}&orderBy={Uri.EscapeDataString(orderBy)}&orderDirection={Uri.EscapeDataString(orderDirection)}&_ts={ts}";
        
        if (!string.IsNullOrWhiteSpace(filtro)) url += "&filtro=" + Uri.EscapeDataString(filtro);
        if (categoriaId.HasValue) url += "&categoriaId=" + categoriaId.Value;
        if (precioMinimo.HasValue) url += "&precioMinimo=" + precioMinimo.Value;
        if (precioMaximo.HasValue) url += "&precioMaximo=" + precioMaximo.Value;
        if (fechaCreacionDesde.HasValue) url += "&fechaCreacionDesde=" + fechaCreacionDesde.Value.ToString("yyyy-MM-dd");
        if (fechaCreacionHasta.HasValue) url += "&fechaCreacionHasta=" + fechaCreacionHasta.Value.ToString("yyyy-MM-dd");
        if (popularidadMinima.HasValue) url += "&popularidadMinima=" + popularidadMinima.Value;
        if (popularidadMaxima.HasValue) url += "&popularidadMaxima=" + popularidadMaxima.Value;
        
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
        return await ObtenerProductosPaginadosAsync(pageNumber, pageSize, filtro ?? string.Empty, null, true, null, null, null, null, null, null, "Nombre", "asc");
    }

    /// <summary>
    /// Obtiene estadísticas de productos
    /// </summary>
    public async Task<EstadisticasProductosDto?> ObtenerEstadisticasAsync()
    {
        try
        {
            var http = CreateClient();
            var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var resp = await http.GetFromJsonAsync<ApiResponse<EstadisticasProductosDto>>($"api/core/productos/estadisticas?_ts={ts}");
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener estadísticas de productos: {ex.Message}");
            return null;
        }
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

    /// <summary>
    /// Sube una imagen para un producto
    /// </summary>
    public async Task<string?> SubirImagenAsync(Guid productoId, Stream archivo, string nombreArchivo, string contentType)
    {
        try
        {
            var http = CreateClient();
            var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(archivo);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(streamContent, "archivo", nombreArchivo);

            var response = await http.PostAsync($"api/core/productos/{productoId}/imagen", content);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
                return result?.Data;
            }
            
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}


