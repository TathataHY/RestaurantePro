using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public class NotificacionesApiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenStore _tokenStore;

    public NotificacionesApiService(IHttpClientFactory httpFactory, TokenStore tokenStore)
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
    /// Obtiene todas las notificaciones del usuario actual
    /// </summary>
    public async Task<List<NotificacionDto>> ObtenerNotificacionesAsync(bool soloNoLeidas = false)
    {
        try
        {
            var http = CreateClient();
            var url = $"api/core/notificaciones?soloNoLeidas={soloNoLeidas}";
            var resp = await http.GetFromJsonAsync<ApiResponse<List<NotificacionDto>>>(url);
            return resp?.Data ?? new List<NotificacionDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener notificaciones: {ex.Message}");
            return new List<NotificacionDto>();
        }
    }

    /// <summary>
    /// Obtiene una notificación específica por ID
    /// </summary>
    public async Task<NotificacionDto?> ObtenerPorIdAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<NotificacionDto>>($"api/core/notificaciones/{id}");
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener notificación {id}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Crea una nueva notificación
    /// </summary>
    public async Task<NotificacionDto?> CrearNotificacionAsync(CrearNotificacionRequest request)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.PostAsJsonAsync("api/core/notificaciones", request);
            
            if (resp.IsSuccessStatusCode)
            {
                var result = await resp.Content.ReadFromJsonAsync<ApiResponse<NotificacionDto>>();
                return result?.Data;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear notificación: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Marca una notificación como leída
    /// </summary>
    public async Task<bool> MarcarComoLeidaAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.PostAsync($"api/core/notificaciones/{id}/marcar-leida", null);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al marcar notificación {id} como leída: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Marca todas las notificaciones como leídas
    /// </summary>
    public async Task<int> MarcarTodasComoLeidasAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.PostAsync("api/core/notificaciones/marcar-leida", null);
            
            if (resp.IsSuccessStatusCode)
            {
                var result = await resp.Content.ReadFromJsonAsync<ApiResponse<int>>();
                return result?.Data ?? 0;
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al marcar todas las notificaciones como leídas: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Elimina una notificación
    /// </summary>
    public async Task<bool> EliminarNotificacionAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.DeleteAsync($"api/core/notificaciones/{id}");
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar notificación {id}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Obtiene el contador de notificaciones no leídas
    /// </summary>
    public async Task<int> ObtenerContadorNoLeidasAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<int>>("api/core/notificaciones/contador-no-leidas");
            return resp?.Data ?? 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener contador de notificaciones: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Obtiene la configuración de notificaciones
    /// </summary>
    public async Task<object?> ObtenerConfiguracionAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>("api/core/notificaciones/configuracion");
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener configuración de notificaciones: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Actualiza la configuración de notificaciones
    /// </summary>
    public async Task<bool> ActualizarConfiguracionAsync(object configuracion)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.PostAsJsonAsync("api/core/notificaciones/configuracion", configuracion);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar configuración de notificaciones: {ex.Message}");
            return false;
        }
    }
}

/// <summary>
/// Request para crear una notificación
/// </summary>
public class CrearNotificacionRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public Guid? EntidadRelacionadaId { get; set; }
}
