using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public class RecetasApiService : IRecetasApiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenStore _tokenStore;

    public RecetasApiService(IHttpClientFactory httpFactory, TokenStore tokenStore)
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

    /// <summary>
    /// Obtiene todas las recetas con paginación
    /// </summary>
    public async Task<PaginatedList<RecetaDto>> ObtenerRecetasPaginadasAsync(
        int pageNumber = 1, 
        int pageSize = 20, 
        bool? soloActivas = null, 
        Guid? productoId = null, 
        string? filtroTexto = null, 
        string ordenarPor = "FechaCreacion", 
        string direccionOrden = "Desc")
    {
        try
        {
            var http = CreateClient();
            var url = $"api/core/recetas?pageNumber={pageNumber}&pageSize={pageSize}&ordenarPor={Uri.EscapeDataString(ordenarPor)}&direccionOrden={Uri.EscapeDataString(direccionOrden)}";
            
            if (soloActivas.HasValue) url += $"&soloActivas={soloActivas.Value}";
            if (productoId.HasValue) url += $"&productoId={productoId.Value}";
            if (!string.IsNullOrWhiteSpace(filtroTexto)) url += $"&filtroTexto={Uri.EscapeDataString(filtroTexto)}";
            
            var resp = await http.GetFromJsonAsync<ApiResponse<PaginatedList<RecetaDto>>>(url);
            return resp?.Data ?? new PaginatedList<RecetaDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener recetas: {ex.Message}");
            return new PaginatedList<RecetaDto>();
        }
    }

    /// <summary>
    /// Obtiene una receta específica por ID
    /// </summary>
    public async Task<RecetaDto?> ObtenerPorIdAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<RecetaDto>>($"api/core/recetas/{id}");
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener receta {id}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Crea una nueva receta
    /// </summary>
    public async Task<RecetaDto?> CrearRecetaAsync(CrearRecetaRequest request)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.PostAsJsonAsync("api/core/recetas", request);
            
            if (resp.IsSuccessStatusCode)
            {
                var result = await resp.Content.ReadFromJsonAsync<ApiResponse<RecetaDto>>();
                return result?.Data;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear receta: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Actualiza una receta existente
    /// </summary>
    public async Task<RecetaDto?> ActualizarRecetaAsync(Guid id, ActualizarRecetaRequest request)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.PutAsJsonAsync($"api/core/recetas/{id}", request);
            
            if (resp.IsSuccessStatusCode)
            {
                var result = await resp.Content.ReadFromJsonAsync<ApiResponse<RecetaDto>>();
                return result?.Data;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar receta {id}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Elimina una receta
    /// </summary>
    public async Task<bool> EliminarRecetaAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.DeleteAsync($"api/core/recetas/{id}");
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar receta {id}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Obtiene las recetas de un producto específico
    /// </summary>
    public async Task<List<RecetaDto>> ObtenerRecetasPorProductoAsync(Guid productoId)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<List<RecetaDto>>>($"api/core/recetas/producto/{productoId}");
            return resp?.Data ?? new List<RecetaDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener recetas del producto {productoId}: {ex.Message}");
            return new List<RecetaDto>();
        }
    }

    /// <summary>
    /// Calcula el costo de una receta
    /// </summary>
    public async Task<decimal> CalcularCostoRecetaAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<decimal>>($"api/core/recetas/{id}/costo");
            return resp?.Data ?? 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al calcular costo de receta {id}: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Verifica la disponibilidad de ingredientes para una receta
    /// </summary>
    public async Task<DisponibilidadRecetaDto?> VerificarDisponibilidadAsync(Guid id, int cantidad = 1)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<DisponibilidadRecetaDto>>($"api/core/recetas/{id}/disponibilidad?cantidad={cantidad}");
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al verificar disponibilidad de receta {id}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene todas las recetas (sin paginación)
    /// </summary>
    public async Task<List<RecetaDto>> ObtenerRecetasAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<List<RecetaDto>>>("api/core/recetas/todas");
            return resp?.Data ?? new List<RecetaDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener recetas: {ex.Message}");
            return new List<RecetaDto>();
        }
    }

    /// <summary>
    /// Obtiene receta por ID (alias)
    /// </summary>
    public async Task<RecetaDto?> ObtenerRecetaPorIdAsync(Guid id)
    {
        return await ObtenerPorIdAsync(id);
    }

    /// <summary>
    /// Cambia el estado de una receta
    /// </summary>
    public async Task<bool> CambiarEstadoRecetaAsync(Guid id, bool activa)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.PostAsync($"api/core/recetas/{id}/cambiar-estado?activa={activa}", null);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cambiar estado de receta {id}: {ex.Message}");
            return false;
        }
    }
}

/// <summary>
/// Request para crear una receta
/// </summary>
public class CrearRecetaRequest
{
    public Guid ProductoId { get; set; }
    public string Preparacion { get; set; } = string.Empty;
    public int TiempoPreparacionMinutos { get; set; }
    public List<AgregarIngredienteDto> Ingredientes { get; set; } = new();
    public bool EstaActiva { get; set; } = true;
}

/// <summary>
/// Request para actualizar una receta
/// </summary>
public class ActualizarRecetaRequest
{
    public string Preparacion { get; set; } = string.Empty;
    public int TiempoPreparacionMinutos { get; set; }
    public List<AgregarIngredienteDto> Ingredientes { get; set; } = new();
    public bool EstaActiva { get; set; } = true;
}
