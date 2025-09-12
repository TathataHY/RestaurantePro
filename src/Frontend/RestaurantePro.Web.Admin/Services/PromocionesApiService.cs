using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

/// <summary>
/// Servicio para gestionar promociones del restaurante
/// </summary>
public class PromocionesApiService : IPromocionesApiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenStore _tokenStore;

    public PromocionesApiService(IHttpClientFactory httpFactory, TokenStore tokenStore)
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
    /// Obtiene una lista paginada de promociones
    /// </summary>
    public async Task<PaginatedList<PromocionDto>?> ObtenerPromocionesAsync(
        int pageNumber = 1, 
        int pageSize = 20, 
        PromocionFiltrosDto? filtros = null)
    {
        try
        {
            var http = CreateClient();
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (filtros != null)
            {
                if (!string.IsNullOrEmpty(filtros.Busqueda))
                    queryParams.Add($"busqueda={Uri.EscapeDataString(filtros.Busqueda)}");
                
                if (filtros.Tipo.HasValue)
                    queryParams.Add($"tipo={filtros.Tipo.Value}");
                
                if (filtros.EstaActiva.HasValue)
                    queryParams.Add($"estaActiva={filtros.EstaActiva.Value}");
                
                if (filtros.FechaInicioDesde.HasValue)
                    queryParams.Add($"fechaInicioDesde={filtros.FechaInicioDesde.Value:yyyy-MM-dd}");
                
                if (filtros.FechaInicioHasta.HasValue)
                    queryParams.Add($"fechaInicioHasta={filtros.FechaInicioHasta.Value:yyyy-MM-dd}");
                
                if (filtros.FechaFinDesde.HasValue)
                    queryParams.Add($"fechaFinDesde={filtros.FechaFinDesde.Value:yyyy-MM-dd}");
                
                if (filtros.FechaFinHasta.HasValue)
                    queryParams.Add($"fechaFinHasta={filtros.FechaFinHasta.Value:yyyy-MM-dd}");
                
                if (!string.IsNullOrEmpty(filtros.OrdenarPor))
                    queryParams.Add($"ordenarPor={filtros.OrdenarPor}");
                
                if (!string.IsNullOrEmpty(filtros.DireccionOrden))
                    queryParams.Add($"direccionOrden={filtros.DireccionOrden}");
            }

            var queryString = string.Join("&", queryParams);
            var response = await http.GetFromJsonAsync<ApiResponse<PaginatedList<PromocionDto>>>($"api/comercial/promociones?{queryString}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener promociones: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene una promoción por su ID
    /// </summary>
    public async Task<PromocionDto?> ObtenerPromocionAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<PromocionDto>>($"api/comercial/promociones/{id}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener promoción: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Crea una nueva promoción (interno)
    /// </summary>
    public async Task<ApiResponse<PromocionDto>?> CrearPromocionInternalAsync(CrearPromocionRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync("api/comercial/promociones", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<PromocionDto>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear promoción: {ex.Message}");
            return new ApiResponse<PromocionDto>
            {
                Success = false,
                Message = $"Error al crear promoción: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Actualiza una promoción existente
    /// </summary>
    public async Task<ApiResponse<PromocionDto>?> ActualizarPromocionAsync(ActualizarPromocionRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PutAsJsonAsync($"api/comercial/promociones/{request.Id}", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<PromocionDto>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar promoción: {ex.Message}");
            return new ApiResponse<PromocionDto>
            {
                Success = false,
                Message = $"Error al actualizar promoción: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Elimina una promoción (interno)
    /// </summary>
    public async Task<ApiResponse<bool>?> EliminarPromocionInternalAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var response = await http.DeleteAsync($"api/comercial/promociones/{id}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar promoción: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al eliminar promoción: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Activa o desactiva una promoción
    /// </summary>
    public async Task<ApiResponse<bool>?> ToggleActivarPromocionAsync(Guid id, bool activar)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PatchAsync($"api/comercial/promociones/{id}/toggle-activar?activar={activar}", null);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cambiar estado de promoción: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al cambiar estado de promoción: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Obtiene estadísticas de promociones
    /// </summary>
    public async Task<PromocionEstadisticasDto?> ObtenerEstadisticasAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<PromocionEstadisticasDto>>("api/comercial/promociones/estadisticas");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener estadísticas de promociones: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene productos asociados a una promoción
    /// </summary>
    public async Task<List<PromocionProductoDto>?> ObtenerProductosPromocionAsync(Guid promocionId)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<PromocionProductoDto>>>($"api/comercial/promociones/{promocionId}/productos");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener productos de promoción: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Asocia productos a una promoción
    /// </summary>
    public async Task<ApiResponse<bool>?> AsociarProductosAsync(Guid promocionId, List<Guid> productosIds)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/comercial/promociones/{promocionId}/productos", productosIds);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al asociar productos a promoción: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al asociar productos: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Valida un código de promoción
    /// </summary>
    public async Task<ApiResponse<PromocionDto>?> ValidarCodigoAsync(string codigo)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<PromocionDto>>($"api/comercial/promociones/validar-codigo/{codigo}");
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al validar código de promoción: {ex.Message}");
            return new ApiResponse<PromocionDto>
            {
                Success = false,
                Message = $"Error al validar código: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Obtiene todas las promociones
    /// </summary>
    public async Task<List<PromocionDto>> ObtenerPromocionesAsync()
    {
        try
        {
            var result = await ObtenerPromocionesAsync(1, 1000);
            return result?.Items ?? new List<PromocionDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener promociones: {ex.Message}");
            return new List<PromocionDto>();
        }
    }

    /// <summary>
    /// Obtiene una promoción por ID
    /// </summary>
    public async Task<PromocionDto?> ObtenerPromocionPorIdAsync(Guid id)
    {
        return await ObtenerPromocionAsync(id);
    }

    /// <summary>
    /// Crea una nueva promoción
    /// </summary>
    public async Task<PromocionDto?> CrearPromocionAsync(CrearPromocionRequest request)
    {
        try
        {
            var response = await CrearPromocionInternalAsync(request);
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear promoción: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Actualiza una promoción existente
    /// </summary>
    public async Task<PromocionDto?> ActualizarPromocionAsync(Guid id, ActualizarPromocionRequest request)
    {
        try
        {
            var http = CreateClient();
            var updateRequest = new ActualizarPromocionRequest
            {
                Id = id,
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                Tipo = request.Tipo,
                ValorDescuento = request.ValorDescuento,
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin,
                Activa = request.Activa,
                Codigo = request.Codigo,
                UsoMaximo = request.UsoMaximo,
                UsoActual = request.UsoActual
            };
            var response = await http.PutAsJsonAsync($"api/comercial/promociones/{id}", updateRequest);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<ApiResponse<PromocionDto>>();
                return content?.Data;
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar promoción: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Elimina una promoción
    /// </summary>
    public async Task<bool> EliminarPromocionAsync(Guid id)
    {
        try
        {
            var response = await EliminarPromocionInternalAsync(id);
            return response?.Success ?? false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar promoción: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Cambia el estado de una promoción
    /// </summary>
    public async Task<bool> CambiarEstadoPromocionAsync(Guid id, bool activa)
    {
        try
        {
            var response = await ToggleActivarPromocionAsync(id, activa);
            return response?.Success ?? false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cambiar estado de promoción: {ex.Message}");
            return false;
        }
    }
}
