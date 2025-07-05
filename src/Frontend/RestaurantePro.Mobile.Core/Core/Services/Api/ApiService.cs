using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Authentication;

namespace RestaurantePro.Mobile.Core.Services.Api;

/// <summary>
/// Implementación del servicio de API - V1 Fundamental
/// </summary>
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public ApiService(HttpClient httpClient, IAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            await AddAuthHeader();
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

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            await AddAuthHeader();
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

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            await AddAuthHeader();
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

    public async Task<ApiResponse<bool>> DeleteAsync(string endpoint)
    {
        try
        {
            await AddAuthHeader();
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

    private async Task AddAuthHeader()
    {
        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
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