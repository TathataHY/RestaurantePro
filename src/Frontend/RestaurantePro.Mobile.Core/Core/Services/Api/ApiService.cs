using System.Net.Http.Headers;
using System.Net.Sockets;
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

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, string? token = null, CancellationToken cancellationToken = default)
    {
        System.Diagnostics.Debug.WriteLine($"🔍 [ApiService] Iniciando GetAsync - endpoint: {endpoint}, token: {!string.IsNullOrEmpty(token)}");
        
        // Lectura por stream + reintentos para evitar EOF en Android/OkHttp con respuestas chunked
        const int maxAttempts = 3;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 [ApiService] Intento {attempt}/{maxAttempts}");
                
                AddAuthHeader(token);
                if (!_httpClient.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/json"))
                {
                    _httpClient.DefaultRequestHeaders.Accept.Clear();
                    _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                }

                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.ConnectionClose = true; // Evita mantener viva la conexión (mitiga EOF en Android)
                try { request.Headers.AcceptEncoding.Clear(); request.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("identity")); } catch { }
                
                System.Diagnostics.Debug.WriteLine($"🔍 [ApiService] Enviando request a: {endpoint}");
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
                System.Diagnostics.Debug.WriteLine($"🔍 [ApiService] Respuesta recibida - StatusCode: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"🔍 [ApiService] StatusCode exitoso, leyendo contenido...");
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    
                    System.Diagnostics.Debug.WriteLine($"🔍 [ApiService] Iniciando deserialización JSON... (Content length: {content?.Length ?? 0})");
                    var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, GetJsonOptions());
                    System.Diagnostics.Debug.WriteLine($"🔍 [ApiService] Deserialización completada - result: {result != null}");
                    
                    // Verificar si el resultado es válido (no null y tiene datos o es un error válido)
                    if (result == null || (result.Data == null && string.IsNullOrEmpty(result.Message) && string.IsNullOrEmpty(result.Error)))
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ [ApiService] Respuesta vacía del servidor");
                        return ApiResponse<T>.ErrorResponse("Respuesta vacía del servidor");
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"✅ [ApiService] Respuesta válida - Success: {result.Success}");
                    return result;
                }

                // Intentar leer cuerpo de error (si existe)
                string errorBody = string.Empty;
                try { errorBody = await response.Content.ReadAsStringAsync(cancellationToken); } catch { /* ignorar */ }
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
            catch (TaskCanceledException tcEx) when (attempt < maxAttempts)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] Reintentando GET (Timeout) intento {attempt}: {tcEx.Message}");
                await Task.Delay(150);
                continue;
            }
            catch (AggregateException aggEx) when (attempt < maxAttempts)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] Reintentando GET (Aggregate) intento {attempt}: {aggEx.Message}");
                await Task.Delay(150);
                continue;
            }
            catch (SocketException sockEx) when (attempt < maxAttempts)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] Reintentando GET (Socket) intento {attempt}: {sockEx.Message}");
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

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data, string? token = null, CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 3;
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
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    var result = await JsonSerializer.DeserializeAsync<ApiResponse<T>>(stream, GetJsonOptions(), cancellationToken);
                    return result ?? ApiResponse<T>.ErrorResponse("Respuesta vacía del servidor");
                }

                string errorBody = string.Empty;
                try { errorBody = await response.Content.ReadAsStringAsync(cancellationToken); } catch { }
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
            catch (TaskCanceledException) when (attempt < maxAttempts)
            {
                await Task.Delay(150);
                continue;
            }
            catch (AggregateException) when (attempt < maxAttempts)
            {
                await Task.Delay(150);
                continue;
            }
            catch (SocketException) when (attempt < maxAttempts)
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

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data, string? token = null, CancellationToken cancellationToken = default)
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
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    var result = await JsonSerializer.DeserializeAsync<ApiResponse<T>>(stream, GetJsonOptions(), cancellationToken);
                    return result ?? ApiResponse<T>.ErrorResponse("Respuesta vacía del servidor");
                }

                string errorBody = string.Empty;
                try { errorBody = await response.Content.ReadAsStringAsync(cancellationToken); } catch { }
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

    public async Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object data, string? token = null, CancellationToken cancellationToken = default)
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
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    var result = await JsonSerializer.DeserializeAsync<ApiResponse<T>>(stream, GetJsonOptions(), cancellationToken);
                    return result ?? ApiResponse<T>.ErrorResponse("Respuesta vacía del servidor");
                }

                string errorBody = string.Empty;
                try { errorBody = await response.Content.ReadAsStringAsync(cancellationToken); } catch { }
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

    public async Task<ApiResponse<bool>> DeleteAsync(string endpoint, string? token = null, CancellationToken cancellationToken = default)
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
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return ApiResponse<bool>.SuccessResponse(true, "Eliminado exitosamente");
                }

                string errorBody = string.Empty;
                try { errorBody = await response.Content.ReadAsStringAsync(cancellationToken); } catch { }
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
        // Siempre remover el header X-Bearer-Token para asegurar limpieza
        _httpClient.DefaultRequestHeaders.Remove("X-Bearer-Token");
        
        if (!string.IsNullOrEmpty(token))
        {
            // Mantener Authorization: Basic configurado en HttpClient (hosting SmarterASP)
            // Enviar el JWT en un header separado para que el backend lo lea explícitamente
            _httpClient.DefaultRequestHeaders.Add("X-Bearer-Token", token);
        }
        // Si no hay token, el header X-Bearer-Token se habrá removido, y solo quedará el Basic si existe.
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
