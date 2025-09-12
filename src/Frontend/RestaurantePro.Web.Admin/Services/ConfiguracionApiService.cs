using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public class ConfiguracionApiService : IConfiguracionApiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenStore _tokenStore;

    public ConfiguracionApiService(IHttpClientFactory httpFactory, TokenStore tokenStore)
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
    /// Obtiene la configuración principal
    /// </summary>
    public async Task<ConfiguracionDto?> ObtenerConfiguracionAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<ConfiguracionDto>>("api/admin/configuracion");
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener configuración: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene configuración por categoría
    /// </summary>
    public async Task<List<ConfiguracionDto>> ObtenerPorCategoriaAsync(string categoria)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<List<ConfiguracionDto>>>($"api/admin/configuracion/categoria/{categoria}");
            return resp?.Data ?? new List<ConfiguracionDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener configuración de categoría {categoria}: {ex.Message}");
            return new List<ConfiguracionDto>();
        }
    }

    /// <summary>
    /// Actualiza un parámetro de configuración
    /// </summary>
    public async Task<bool> ActualizarParametroAsync(ActualizarConfiguracionRequest request)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.PutAsJsonAsync("api/admin/configuracion", request);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar parámetro {request.Clave}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Obtiene configuración de notificaciones
    /// </summary>
    public async Task<ConfiguracionNotificacionesDto?> ObtenerConfiguracionNotificacionesAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<ConfiguracionNotificacionesDto>>("api/admin/configuracion/notificaciones");
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener configuración de notificaciones: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Actualiza configuración de notificaciones
    /// </summary>
    public async Task<bool> ActualizarConfiguracionNotificacionesAsync(ConfiguracionNotificacionesDto configuracion)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.PostAsJsonAsync("api/admin/configuracion/notificaciones", configuracion);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar configuración de notificaciones: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Obtiene configuración de fidelización
    /// </summary>
    public async Task<ConfiguracionFidelizacionDto?> ObtenerConfiguracionFidelizacionAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<ConfiguracionFidelizacionDto>>("api/admin/configuracion/fidelizacion");
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener configuración de fidelización: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Actualiza configuración de fidelización
    /// </summary>
    public async Task<bool> ActualizarConfiguracionFidelizacionAsync(ConfiguracionFidelizacionDto configuracion)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.PostAsJsonAsync("api/admin/configuracion/fidelizacion", configuracion);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar configuración de fidelización: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Obtiene estadísticas de configuración
    /// </summary>
    public async Task<object?> ObtenerEstadisticasAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>("api/admin/configuracion/estadisticas");
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener estadísticas de configuración: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Resetea configuración a valores por defecto
    /// </summary>
    public async Task<bool> ResetearConfiguracionAsync(string categoria)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.PostAsync($"api/admin/configuracion/reset/{categoria}", null);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al resetear configuración de {categoria}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Actualiza la configuración principal
    /// </summary>
    public async Task<bool> ActualizarConfiguracionAsync(ConfiguracionDto configuracion)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.PutAsJsonAsync("api/admin/configuracion", configuracion);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar configuración: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Restablece la configuración a valores por defecto
    /// </summary>
    public async Task<bool> RestablecerConfiguracionAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.PostAsync("api/admin/configuracion/restablecer", null);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al restablecer configuración: {ex.Message}");
            return false;
        }
    }
}
