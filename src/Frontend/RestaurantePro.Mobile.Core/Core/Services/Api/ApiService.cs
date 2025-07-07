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
        // Mostrar popup con la BaseAddress al iniciar la app (solo para depuración)
        var baseAddress = _httpClient.BaseAddress?.ToString() ?? "NULL";
#if ANDROID || IOS || WINDOWS
        try
        {
            Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(() =>
            {
                Microsoft.Maui.Controls.Application.Current?.MainPage?.DisplayAlert("BaseAddress", baseAddress, "OK");
            });
        }
        catch { /* Ignorar errores si no hay MainPage aún */ }
#endif
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
            AddAuthHeader(token);
            var json = JsonSerializer.Serialize(data, GetJsonOptions());
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(endpoint, content);
            
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
} 