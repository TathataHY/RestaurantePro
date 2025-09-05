using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Api;

/// <summary>
/// Implementación del servicio de API - V1 Fundamental
/// </summary>
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        // DEBUG: Mostrar información completa del HttpClient
        var baseAddress = _httpClient.BaseAddress?.ToString() ?? "NULL";
        var timeout = _httpClient.Timeout.ToString();
        var defaultHeaders = string.Join(", ", _httpClient.DefaultRequestHeaders.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"));
        
        // DEBUG: Comentado para flujo normal - descomentar solo si hay problemas
        // Usar el DebugService del proyecto Mobile
        // try
        // {
        //     // Llamar al DebugService usando reflection ya que está en otro proyecto
        //     var debugServiceType = Type.GetType("RestaurantePro.Mobile.Services.DebugService, RestaurantePro.Mobile");
        //     if (debugServiceType != null)
        //     {
        //         var method = debugServiceType.GetMethod("ShowHttpClientInfo", 
        //             System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        //     method?.Invoke(null, new object[] { baseAddress, timeout, defaultHeaders });
        //     }
        // }
        // catch { /* Ignorar errores si no está disponible */ }
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, string? token = null)
    {
        try
        {
            AddAuthHeader(token);
            var response = await _httpClient.GetAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(json, GetJsonOptions());
                return result ?? ApiResponse<T>.ErrorResponse("Respuesta vacía del servidor");
            }
            
            return ApiResponse<T>.ErrorResponse(
                new List<string> { "Error en la comunicación con el servidor" },
                "Error de conexión", 
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.ErrorResponse(
                new List<string> { ex.Message },
                "Error inesperado", 
                500);
        }
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data, string? token = null)
    {
        try
        {
            // DEBUG: Mostrar información detallada
            var fullUrl = _httpClient.BaseAddress + endpoint;
            var requestJson = JsonSerializer.Serialize(data, GetJsonOptions());
            
            // DEBUG: Comentado para flujo normal - descomentar solo si hay problemas
            // ShowDebugPopup("🚀 POST Request", $"URL: {fullUrl}\nData: {requestJson}");
            
            AddAuthHeader(token);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            
            // DEBUG: Comentado para flujo normal - descomentar solo si hay problemas
            // ShowDebugPopup("📤 Enviando petición", $"URL: {fullUrl}");
            
            var response = await _httpClient.PostAsync(endpoint, content);
            
            // DEBUG: Comentado para flujo normal - descomentar solo si hay problemas
            // ShowDebugPopup("📥 Respuesta recibida", $"Status: {response.StatusCode}\nContent: {responseJson}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(responseJson, GetJsonOptions());
                return result ?? ApiResponse<T>.ErrorResponse("Respuesta vacía del servidor");
            }
            
            var errorResponseJson = await response.Content.ReadAsStringAsync();
            return ApiResponse<T>.ErrorResponse(
                new List<string> { $"Error HTTP: {response.StatusCode} - {errorResponseJson}" },
                "Error de conexión", 
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            // DEBUG: Comentado para flujo normal - descomentar solo si hay problemas
            // ShowDebugPopup("❌ EXCEPCIÓN", $"Tipo: {ex.GetType().Name}\nMensaje: {ex.Message}\nStackTrace: {ex.StackTrace}");
            
            return ApiResponse<T>.ErrorResponse(
                new List<string> { ex.Message },
                "Error inesperado", 
                500);
        }
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data, string? token = null)
    {
        try
        {
            AddAuthHeader(token);
            var json = JsonSerializer.Serialize(data, GetJsonOptions());
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync(endpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(responseJson, GetJsonOptions());
                return result ?? ApiResponse<T>.ErrorResponse("Respuesta vacía del servidor");
            }
            
            return ApiResponse<T>.ErrorResponse(
                new List<string> { "Error en la comunicación con el servidor" },
                "Error de conexión", 
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.ErrorResponse(
                new List<string> { ex.Message },
                "Error inesperado", 
                500);
        }
    }

    public async Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object data, string? token = null)
    {
        try
        {
            AddAuthHeader(token);
            var json = JsonSerializer.Serialize(data, GetJsonOptions());
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PatchAsync(endpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(responseJson, GetJsonOptions());
                return result ?? ApiResponse<T>.ErrorResponse("Respuesta vacía del servidor");
            }
            
            return ApiResponse<T>.ErrorResponse(
                new List<string> { "Error en la comunicación con el servidor" },
                "Error de conexión", 
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.ErrorResponse(
                new List<string> { ex.Message },
                "Error inesperado", 
                500);
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string endpoint, string? token = null)
    {
        try
        {
            AddAuthHeader(token);
            var response = await _httpClient.DeleteAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                return ApiResponse<bool>.SuccessResponse(true, "Eliminado exitosamente");
            }
            
            return ApiResponse<bool>.ErrorResponse(
                new List<string> { "Error en la comunicación con el servidor" },
                "Error de conexión", 
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse(
                new List<string> { ex.Message },
                "Error inesperado", 
                500);
        }
    }

    private void AddAuthHeader(string? token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Muestra un popup de debug con información detallada
    /// </summary>
    private void ShowDebugPopup(string title, string message)
    {
#if DEBUG
        try
        {
            // Llamar al DebugService usando reflection ya que está en otro proyecto
            var debugServiceType = Type.GetType("RestaurantePro.Mobile.Services.DebugService, RestaurantePro.Mobile");
            if (debugServiceType != null)
            {
                var method = debugServiceType.GetMethod("ShowDebugPopup", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                method?.Invoke(null, new object[] { title, message });
            }
        }
        catch { /* Ignorar errores si no está disponible */ }
#endif
    }
} 
