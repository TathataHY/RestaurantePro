using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Config;

namespace RestaurantePro.Mobile.Core.Services.Api
{
    /// <summary>
    /// Cliente base para comunicación con la API
    /// </summary>
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ITokenService _tokenService;

        public ApiClient(ITokenService tokenService, IAppSettings appSettings)
        {
            _tokenService = tokenService;
            _baseUrl = appSettings.ApiBaseUrl;
            _httpClient = new HttpClient();
            
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Realiza una petición GET a la API
        /// </summary>
        public async Task<T> GetAsync<T>(string endpoint)
        {
            await SetAuthorizationHeader();
            
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/{endpoint}");
                await HandleResponseErrors(response);
                
                var content = await response.Content.ReadAsStringAsync();
                return DeserializeResponse<T>(content);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException("Error de conexión con el servidor", ex);
            }
            catch (Exception ex) when (!(ex is ApiException))
            {
                throw new ApiException("Error al procesar la respuesta", ex);
            }
        }

        /// <summary>
        /// Realiza una petición POST a la API
        /// </summary>
        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            await SetAuthorizationHeader();
            
            try
            {
                var jsonContent = JsonSerializer.Serialize(data);
                var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/{endpoint}", stringContent);
                await HandleResponseErrors(response);
                
                var content = await response.Content.ReadAsStringAsync();
                return DeserializeResponse<T>(content);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException("Error de conexión con el servidor", ex);
            }
            catch (Exception ex) when (!(ex is ApiException))
            {
                throw new ApiException("Error al procesar la respuesta", ex);
            }
        }

        /// <summary>
        /// Realiza una petición PUT a la API
        /// </summary>
        public async Task<T> PutAsync<T>(string endpoint, object data)
        {
            await SetAuthorizationHeader();
            
            try
            {
                var jsonContent = JsonSerializer.Serialize(data);
                var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"{_baseUrl}/{endpoint}", stringContent);
                await HandleResponseErrors(response);
                
                var content = await response.Content.ReadAsStringAsync();
                return DeserializeResponse<T>(content);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException("Error de conexión con el servidor", ex);
            }
            catch (Exception ex) when (!(ex is ApiException))
            {
                throw new ApiException("Error al procesar la respuesta", ex);
            }
        }

        /// <summary>
        /// Realiza una petición DELETE a la API
        /// </summary>
        public async Task DeleteAsync(string endpoint)
        {
            await SetAuthorizationHeader();
            
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/{endpoint}");
                await HandleResponseErrors(response);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException("Error de conexión con el servidor", ex);
            }
            catch (Exception ex) when (!(ex is ApiException))
            {
                throw new ApiException("Error al procesar la respuesta", ex);
            }
        }

        /// <summary>
        /// Establece el encabezado de autorización con el token JWT
        /// </summary>
        private async Task SetAuthorizationHeader()
        {
            var token = await _tokenService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                // Remover el encabezado de autorización si no hay token
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        /// <summary>
        /// Maneja errores de respuesta HTTP
        /// </summary>
        private async Task HandleResponseErrors(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                try
                {
                    // Intentar deserializar el error como una ApiResponse
                    var errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(content, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (errorResponse != null)
                    {
                        throw new ApiException(errorResponse.Message ?? "Error en la petición", 
                            errorResponse.Errors, (int)response.StatusCode);
                    }
                }
                catch
                {
                    // Si no se puede deserializar, usar el mensaje HTTP estándar
                    throw new ApiException(response.ReasonPhrase ?? "Error desconocido", (int)response.StatusCode);
                }
                
                // Si llegamos aquí, lanzar una excepción genérica
                throw new ApiException($"Error HTTP: {(int)response.StatusCode}", (int)response.StatusCode);
            }
        }

        /// <summary>
        /// Deserializa la respuesta a partir del formato estándar de ApiResponse
        /// </summary>
        private T DeserializeResponse<T>(string content)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            // Intentar deserializar como ApiResponse primero
            try
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, options);
                
                if (apiResponse != null)
                {
                    if (!apiResponse.Success)
                    {
                        throw new ApiException(apiResponse.Message ?? "La operación no fue exitosa", 
                            apiResponse.Errors, apiResponse.StatusCode);
                    }
                    
                    return apiResponse.Data;
                }
            }
            catch (JsonException)
            {
                // Si falla, intentar deserializar directamente al tipo T
                return JsonSerializer.Deserialize<T>(content, options);
            }
            
            // Si no se pudo deserializar, retornar el valor por defecto
            return default;
        }
    }

    /// <summary>
    /// Excepción lanzada por errores de API
    /// </summary>
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public string[] Errors { get; }

        public ApiException(string message, Exception innerException = null) 
            : base(message, innerException)
        {
            StatusCode = 0;
            Errors = Array.Empty<string>();
        }

        public ApiException(string message, int statusCode) 
            : base(message)
        {
            StatusCode = statusCode;
            Errors = Array.Empty<string>();
        }

        public ApiException(string message, string[] errors, int statusCode) 
            : base(message)
        {
            StatusCode = statusCode;
            Errors = errors ?? Array.Empty<string>();
        }
    }

    /// <summary>
    /// Modelo para deserializar la respuesta estándar de la API
    /// </summary>
    internal class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public string[] Errors { get; set; }
        public int StatusCode { get; set; }
    }

    /// <summary>
    /// Modelo para deserializar errores de la API
    /// </summary>
    internal class ApiErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string[] Errors { get; set; }
        public int StatusCode { get; set; }
    }
} 