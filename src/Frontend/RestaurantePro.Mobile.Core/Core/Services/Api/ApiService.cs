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
    private readonly HttpClient _httpClient;Encoder 

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
        // Lectura por stream + reintentos para evitar EOF en Android/OkHttp con respuestas chunked
        const int maxAttempts = 2;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                AddAuthHeader(token);
                if (!_httpClient.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/json"))
                {
                    _httpClient.DefaultRequestHeaders.Accept.Clear();
                    _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                }

                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.ConnectionClose = true; // Evita mantener viva la conexión (mitiga EOF en Android)
                try { request.Headers.AcceptEncoding.Clear(); request.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("identity")); } catch { }
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {
                    await using var stream = await response.Content.ReadAsStreamAsync();
                    var result = await JsonSerializer.DeserializeAsync<ApiResponse<T>>(stream, GetJsonOptions());
                    return result ?? ApiResponse<T>.ErrorResponse("Respuesta vacía del servidor");
                }

                // Intentar leer cuerpo de error (si existe)
                string errorBody = string.Empty;
                try { errorBody = await response.Content.ReadAsStringAsync(); } catch { /* ignorar */ }
                return ApiResponse<T>.ErrorResponse(
                    new List<string> { $"Error HTTP: {response.StatusCode} - {errorBody}" },
                    "Error de conexión",
                    (int)response.StatusCode);
            }
            catch (IOException ioEx) when (attempt < maxAttempts)
            {
                // Reintentar una vez ante EOF/transitorios
                System.Diagnostics.Debug.WriteLine($"[ApiService] Reintentando GET (IO) intento {attempt}: {ioEx.Message}");
                await Task.Delay(150);
                continue;
            }
            catch (HttpRequestException httpEx) when (attempt < maxAttempts)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] Reintentando GET (HTTP) intento {attempt}: {httpEx.Message}");
                await Task.Delay(150);
                continue;
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error inesperado",
                    500);
            }
        }

        // Si por alguna razón salimos del bucle sin retornar, responder genérico
        return ApiResponse<T>.ErrorResponse("Error de conexión", "Error inesperado", 500);
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data, string? token = null)
    {
        const int maxAttempts = 2;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                AddAuthHeader(token);
                var requestJson = JsonSerializer.Serialize(data, GetJsonOptions());
                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
                };
                request.Headers.ConnectionClose = true;
                try { request.Headers.AcceptEncoding.Clear(); request.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("identity")); } catch { }
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {
                    await using var stream = await response.Content.ReadAsStreamAsync();
                    var result = await JsonSerializer.DeserializeAsync<ApiResponse<T>>(stream, GetJsonOptions());
                    return result ?? ApiResponse<T>.ErrorResponse("Respuesta vacía del servidor");
                }

                string errorBody = string.Empty;
                try { errorBody = await response.Content.ReadAsStringAsync(); } catch { }
                return ApiResponse<T>.ErrorResponse(
                    new List<string> { $"Error HTTP: {response.StatusCode} - {errorBody}" },
                    "Error de conexión",
                    (int)response.StatusCode);
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                await Task.Delay(150);
                continue;
            }
            catch (HttpRequestException) when (attempt < maxAttempts)
            {
                await Task.Delay(150);
                continue;
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.ErrorResponse(new List<string> { ex.Message }, "Error inesperado", 500);
            }
        }
        return ApiResponse<T>.ErrorResponse("Error de conexión", "Error inesperado", 500);
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data, string? token = null)
    {
        const int maxAttempts = 2;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                AddAuthHeader(token);
                var requestJson = JsonSerializer.Serialize(data, GetJsonOptions());
                using var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
                {
                    Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
                };
                request.Headers.ConnectionClose = true;
                try { request.Headers.AcceptEncoding.Clear(); request.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("identity")); } catch { }
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {
                    await using var stream = await response.Content.ReadAsStreamAsync();
                    var result = await JsonSerializer.DeserializeAsync<ApiResponse<T>>(stream, GetJsonOptions());
                    return result ?? ApiResponse<T>.ErrorResponse("Respuesta vacía del servidor");
                }

                string errorBody = string.Empty;
                try { errorBody = await response.Content.ReadAsStringAsync(); } catch { }
                return ApiResponse<T>.ErrorResponse(
                    new List<string> { $"Error HTTP: {response.StatusCode} - {errorBody}" },
                    "Error de conexión",
                    (int)response.StatusCode);
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                await Task.Delay(150);
                continue;
            }
            catch (HttpRequestException) when (attempt < maxAttempts)
            {
                await Task.Delay(150);
                continue;
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.ErrorResponse(new List<string> { ex.Message }, "Error inesperado", 500);
            }
        }
        return ApiResponse<T>.ErrorResponse("Error de conexión", "Error inesperado", 500);
    }

    public async Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object data, string? token = null)
    {
        const int maxAttempts = 2;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                AddAuthHeader(token);
                var requestJson = JsonSerializer.Serialize(data, GetJsonOptions());
                using var request = new HttpRequestMessage(HttpMethod.Patch, endpoint)
                {
                    Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
                };
                request.Headers.ConnectionClose = true;
                try { request.Headers.AcceptEncoding.Clear(); request.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("identity")); } catch { }
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {
                    await using var stream = await response.Content.ReadAsStreamAsync();
                    var result = await JsonSerializer.DeserializeAsync<ApiResponse<T>>(stream, GetJsonOptions());
                    return result ?? ApiResponse<T>.ErrorResponse("Respuesta vacía del servidor");
                }

                string errorBody = string.Empty;
                try { errorBody = await response.Content.ReadAsStringAsync(); } catch { }
                return ApiResponse<T>.ErrorResponse(
                    new List<string> { $"Error HTTP: {response.StatusCode} - {errorBody}" },
                    "Error de conexión",
                    (int)response.StatusCode);
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                await Task.Delay(150);
                continue;
            }
            catch (HttpRequestException) when (attempt < maxAttempts)
            {
                await Task.Delay(150);
                continue;
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.ErrorResponse(new List<string> { ex.Message }, "Error inesperado", 500);
            }
        }
        return ApiResponse<T>.ErrorResponse("Error de conexión", "Error inesperado", 500);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string endpoint, string? token = null)
    {
        const int maxAttempts = 2;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                AddAuthHeader(token);
                using var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
                request.Headers.ConnectionClose = true;
                try { request.Headers.AcceptEncoding.Clear(); request.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("identity")); } catch { }
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {
                    return ApiResponse<bool>.SuccessResponse(true, "Eliminado exitosamente");
                }

                string errorBody = string.Empty;
                try { errorBody = await response.Content.ReadAsStringAsync(); } catch { }
                return ApiResponse<bool>.ErrorResponse(
                    new List<string> { $"Error HTTP: {response.StatusCode} - {errorBody}" },
                    "Error de conexión",
                    (int)response.StatusCode);
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                await Task.Delay(150);
                continue;
            }
            catch (HttpRequestException) when (attempt < maxAttempts)
            {
                await Task.Delay(150);
                continue;
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse(new List<string> { ex.Message }, "Error inesperado", 500);
            }
        }
        return ApiResponse<bool>.ErrorResponse("Error de conexión", "Error inesperado", 500);
    }

    private void AddAuthHeader(string? token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            // Mantener Authorization: Basic configurado en HttpClient (hosting SmarterASP)
            // Enviar el JWT en un header separado para que el backend lo lea explícitamente
            _httpClient.DefaultRequestHeaders.Remove("X-Bearer-Token");
            _httpClient.DefaultRequestHeaders.Add("X-Bearer-Token", token);
        }
        // Si no hay token, mantenemos únicamente el Authorization existente (Basic)
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
