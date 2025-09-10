using RestaurantePro.Web.Admin.Models;
using System.Text;
using System.Text.Json;

namespace RestaurantePro.Web.Admin.Services
{
    /// <summary>
    /// Servicio para gestión de preparaciones de cocina
    /// </summary>
    public class PreparacionesApiService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenStore _tokenStore;
        private readonly JsonSerializerOptions _jsonOptions;

        public PreparacionesApiService(IHttpClientFactory httpClientFactory, TokenStore tokenStore)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
            _tokenStore = tokenStore;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private HttpClient CreateClient()
        {
            var client = _httpClient;
            var token = _tokenStore.Token;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        /// <summary>
        /// Obtiene preparaciones paginadas con filtros
        /// </summary>
        public async Task<PaginatedList<PreparacionDto>?> ObtenerPreparacionesPaginadasAsync(
            int pageNumber = 1, 
            int pageSize = 20, 
            PreparacionFiltrosDto? filtros = null)
        {
            try
            {
                var client = CreateClient();
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (filtros != null)
                {
                    if (filtros.Estado.HasValue)
                        queryParams.Add($"estado={(int)filtros.Estado.Value}");
                    if (filtros.Prioridad.HasValue)
                        queryParams.Add($"prioridad={(int)filtros.Prioridad.Value}");
                    if (filtros.CocineroId.HasValue)
                        queryParams.Add($"cocineroId={filtros.CocineroId.Value}");
                    if (filtros.ProductoId.HasValue)
                        queryParams.Add($"productoId={filtros.ProductoId.Value}");
                    if (filtros.ComandaId.HasValue)
                        queryParams.Add($"comandaId={filtros.ComandaId.Value}");
                    if (filtros.MesaNumero.HasValue)
                        queryParams.Add($"mesaNumero={filtros.MesaNumero.Value}");
                    if (filtros.FechaInicio.HasValue)
                        queryParams.Add($"fechaInicio={filtros.FechaInicio.Value:yyyy-MM-dd}");
                    if (filtros.FechaFin.HasValue)
                        queryParams.Add($"fechaFin={filtros.FechaFin.Value:yyyy-MM-dd}");
                    if (filtros.EstaAtrasada.HasValue)
                        queryParams.Add($"estaAtrasada={filtros.EstaAtrasada.Value}");
                    if (!string.IsNullOrEmpty(filtros.Busqueda))
                        queryParams.Add($"busqueda={Uri.EscapeDataString(filtros.Busqueda)}");
                }

                var queryString = string.Join("&", queryParams);
                var response = await client.GetAsync($"api/operaciones/preparaciones?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<PaginatedList<PreparacionDto>>(content, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener preparaciones: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene preparación por ID
        /// </summary>
        public async Task<PreparacionDetalleDto?> ObtenerPreparacionAsync(int id)
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync($"api/operaciones/preparaciones/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<PreparacionDetalleDto>(content, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener preparación: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene cola de preparaciones
        /// </summary>
        public async Task<ColaPreparacionesDto?> ObtenerColaPreparacionesAsync()
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync("api/operaciones/preparaciones/cola");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ColaPreparacionesDto>(content, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener cola de preparaciones: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene estadísticas de preparaciones
        /// </summary>
        public async Task<PreparacionEstadisticasDto?> ObtenerEstadisticasAsync()
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync("api/operaciones/preparaciones/estadisticas");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<PreparacionEstadisticasDto>(content, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener estadísticas de preparaciones: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene estados de preparación
        /// </summary>
        public async Task<List<string>?> ObtenerEstadosAsync()
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync("api/operaciones/preparaciones/estados");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<string>>(content, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener estados: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene prioridades de preparación
        /// </summary>
        public async Task<List<string>?> ObtenerPrioridadesAsync()
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync("api/operaciones/preparaciones/prioridades");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<string>>(content, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener prioridades: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene cocineros disponibles
        /// </summary>
        public async Task<List<dynamic>?> ObtenerCocinerosAsync()
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync("api/operaciones/preparaciones/cocineros");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<dynamic>>(content, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener cocineros: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Asigna cocinero a preparación
        /// </summary>
        public async Task<bool> AsignarCocineroAsync(AsignarCocineroRequest request)
        {
            try
            {
                var client = CreateClient();
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/operaciones/preparaciones/asignar-cocinero", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al asignar cocinero: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Actualiza estado de preparación
        /// </summary>
        public async Task<bool> ActualizarEstadoAsync(ActualizarEstadoPreparacionRequest request)
        {
            try
            {
                var client = CreateClient();
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync("api/operaciones/preparaciones/actualizar-estado", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar estado: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Actualiza tiempo de preparación
        /// </summary>
        public async Task<bool> ActualizarTiempoAsync(ActualizarTiempoPreparacionRequest request)
        {
            try
            {
                var client = CreateClient();
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync("api/operaciones/preparaciones/actualizar-tiempo", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar tiempo: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Inicia preparación
        /// </summary>
        public async Task<bool> IniciarPreparacionAsync(int preparacionId)
        {
            try
            {
                var client = CreateClient();
                var response = await client.PostAsync($"api/operaciones/preparaciones/{preparacionId}/iniciar", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al iniciar preparación: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Completa preparación
        /// </summary>
        public async Task<bool> CompletarPreparacionAsync(int preparacionId, string? notas = null)
        {
            try
            {
                var client = CreateClient();
                var request = new { notas };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"api/operaciones/preparaciones/{preparacionId}/completar", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al completar preparación: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cancela preparación
        /// </summary>
        public async Task<bool> CancelarPreparacionAsync(int preparacionId, string? motivo = null)
        {
            try
            {
                var client = CreateClient();
                var request = new { motivo };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"api/operaciones/preparaciones/{preparacionId}/cancelar", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cancelar preparación: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Exporta preparaciones a Excel
        /// </summary>
        public async Task<byte[]?> ExportarExcelAsync(PreparacionFiltrosDto? filtros = null)
        {
            try
            {
                var client = CreateClient();
                var queryParams = new List<string>();

                if (filtros != null)
                {
                    if (filtros.Estado.HasValue)
                        queryParams.Add($"estado={(int)filtros.Estado.Value}");
                    if (filtros.Prioridad.HasValue)
                        queryParams.Add($"prioridad={(int)filtros.Prioridad.Value}");
                    if (filtros.CocineroId.HasValue)
                        queryParams.Add($"cocineroId={filtros.CocineroId.Value}");
                    if (filtros.FechaInicio.HasValue)
                        queryParams.Add($"fechaInicio={filtros.FechaInicio.Value:yyyy-MM-dd}");
                    if (filtros.FechaFin.HasValue)
                        queryParams.Add($"fechaFin={filtros.FechaFin.Value:yyyy-MM-dd}");
                }

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await client.GetAsync($"api/operaciones/preparaciones/exportar-excel{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al exportar preparaciones: {ex.Message}");
                return null;
            }
        }
    }
}
