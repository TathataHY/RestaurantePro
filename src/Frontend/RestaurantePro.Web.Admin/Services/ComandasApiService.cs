using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

/// <summary>
/// Servicio para gestionar comandas del restaurante
/// </summary>
public class ComandasApiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenStore _tokenStore;

    public ComandasApiService(IHttpClientFactory httpFactory, TokenStore tokenStore)
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
    /// Obtiene la lista paginada de comandas con filtros
    /// </summary>
    public async Task<PaginatedList<ComandaDto>?> ObtenerComandasAsync(ComandaFiltrosDto filtros)
    {
        try
        {
            var http = CreateClient();
            var queryParams = new List<string>
            {
                $"pageNumber={filtros.PageNumber}",
                $"pageSize={filtros.PageSize}",
                $"ordenarPor={filtros.OrdenarPor}",
                $"direccionOrden={filtros.DireccionOrden}"
            };

            if (!string.IsNullOrEmpty(filtros.Busqueda))
                queryParams.Add($"busqueda={Uri.EscapeDataString(filtros.Busqueda)}");
            
            if (!string.IsNullOrEmpty(filtros.Estado))
                queryParams.Add($"estado={Uri.EscapeDataString(filtros.Estado)}");
            
            if (!string.IsNullOrEmpty(filtros.Prioridad))
                queryParams.Add($"prioridad={Uri.EscapeDataString(filtros.Prioridad)}");
            
            if (!string.IsNullOrEmpty(filtros.TipoComanda))
                queryParams.Add($"tipoComanda={Uri.EscapeDataString(filtros.TipoComanda)}");
            
            if (filtros.MesaId.HasValue)
                queryParams.Add($"mesaId={filtros.MesaId.Value}");
            
            if (filtros.MeseroId.HasValue)
                queryParams.Add($"meseroId={filtros.MeseroId.Value}");
            
            if (filtros.ClienteId.HasValue)
                queryParams.Add($"clienteId={filtros.ClienteId.Value}");
            
            if (filtros.FechaInicio.HasValue)
                queryParams.Add($"fechaInicio={filtros.FechaInicio.Value:yyyy-MM-dd}");
            
            if (filtros.FechaFin.HasValue)
                queryParams.Add($"fechaFin={filtros.FechaFin.Value:yyyy-MM-dd}");
            
            if (filtros.EsUrgente.HasValue)
                queryParams.Add($"esUrgente={filtros.EsUrgente.Value}");
            
            if (filtros.EsDomicilio.HasValue)
                queryParams.Add($"esDomicilio={filtros.EsDomicilio.Value}");
            
            if (filtros.EsLenta.HasValue)
                queryParams.Add($"esLenta={filtros.EsLenta.Value}");
            
            if (filtros.TiempoMinimo.HasValue)
                queryParams.Add($"tiempoMinimo={filtros.TiempoMinimo.Value}");
            
            if (filtros.TiempoMaximo.HasValue)
                queryParams.Add($"tiempoMaximo={filtros.TiempoMaximo.Value}");

            var queryString = string.Join("&", queryParams);
            var response = await http.GetFromJsonAsync<ApiResponse<PaginatedList<ComandaDto>>>($"api/operaciones/comandas?{queryString}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener comandas: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene una comanda específica por ID
    /// </summary>
    public async Task<ComandaDto?> ObtenerComandaAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<ComandaDto>>($"api/operaciones/comandas/{id}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener comanda: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Crea una nueva comanda
    /// </summary>
    public async Task<ApiResponse<ComandaDto>?> CrearComandaAsync(CrearComandaRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync("api/operaciones/comandas", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear comanda: {ex.Message}");
            return new ApiResponse<ComandaDto>
            {
                Success = false,
                Message = $"Error al crear comanda: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Actualiza una comanda existente
    /// </summary>
    public async Task<ApiResponse<ComandaDto>?> ActualizarComandaAsync(ActualizarComandaRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PutAsJsonAsync($"api/operaciones/comandas/{request.Id}", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar comanda: {ex.Message}");
            return new ApiResponse<ComandaDto>
            {
                Success = false,
                Message = $"Error al actualizar comanda: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Elimina una comanda (soft delete)
    /// </summary>
    public async Task<ApiResponse<bool>?> EliminarComandaAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var response = await http.DeleteAsync($"api/operaciones/comandas/{id}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar comanda: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al eliminar comanda: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Cambia el estado de una comanda
    /// </summary>
    public async Task<ApiResponse<bool>?> CambiarEstadoAsync(CambiarEstadoComandaRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/operaciones/comandas/{request.ComandaId}/cambiar-estado", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cambiar estado: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al cambiar estado: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Cambia la prioridad de una comanda
    /// </summary>
    public async Task<ApiResponse<bool>?> CambiarPrioridadAsync(CambiarPrioridadComandaRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/operaciones/comandas/{request.ComandaId}/cambiar-prioridad", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cambiar prioridad: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al cambiar prioridad: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Asigna un cocinero a una comanda
    /// </summary>
    public async Task<ApiResponse<bool>?> AsignarCocineroAsync(AsignarCocineroRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/operaciones/comandas/{request.ComandaId}/asignar-cocinero", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al asignar cocinero: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al asignar cocinero: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Reasigna una comanda a otra mesa
    /// </summary>
    public async Task<ApiResponse<bool>?> ReasignarMesaAsync(ReasignarMesaRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/operaciones/comandas/{request.ComandaId}/reasignar-mesa", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al reasignar mesa: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al reasignar mesa: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Divide una comanda en dos
    /// </summary>
    public async Task<ApiResponse<ComandaDto>?> DividirComandaAsync(DividirComandaRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/operaciones/comandas/{request.ComandaId}/dividir", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al dividir comanda: {ex.Message}");
            return new ApiResponse<ComandaDto>
            {
                Success = false,
                Message = $"Error al dividir comanda: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Fusiona múltiples comandas
    /// </summary>
    public async Task<ApiResponse<ComandaDto>?> FusionarComandasAsync(FusionarComandasRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/operaciones/comandas/{request.ComandaPrincipalId}/fusionar", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al fusionar comandas: {ex.Message}");
            return new ApiResponse<ComandaDto>
            {
                Success = false,
                Message = $"Error al fusionar comandas: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Obtiene las estadísticas de comandas
    /// </summary>
    public async Task<ComandaEstadisticasDto?> ObtenerEstadisticasAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<ComandaEstadisticasDto>>("api/operaciones/comandas/estadisticas");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener estadísticas de comandas: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene el historial de estados de una comanda
    /// </summary>
    public async Task<List<ComandaEstadoDto>?> ObtenerHistorialEstadosAsync(Guid comandaId)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<ComandaEstadoDto>>>($"api/operaciones/comandas/{comandaId}/historial-estados");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener historial de estados: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene las comandas activas (no entregadas)
    /// </summary>
    public async Task<List<ComandaDto>?> ObtenerComandasActivasAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<ComandaDto>>>("api/operaciones/comandas/activas");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener comandas activas: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene las comandas urgentes
    /// </summary>
    public async Task<List<ComandaDto>?> ObtenerComandasUrgentesAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<ComandaDto>>>("api/operaciones/comandas/urgentes");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener comandas urgentes: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene las comandas lentas (más de 30 minutos)
    /// </summary>
    public async Task<List<ComandaDto>?> ObtenerComandasLentasAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<ComandaDto>>>("api/operaciones/comandas/lentas");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener comandas lentas: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene los estados de comanda disponibles
    /// </summary>
    public async Task<List<string>?> ObtenerEstadosAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<string>>>("api/operaciones/comandas/estados");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener estados: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene las prioridades disponibles
    /// </summary>
    public async Task<List<string>?> ObtenerPrioridadesAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<string>>>("api/operaciones/comandas/prioridades");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener prioridades: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene los tipos de comanda disponibles
    /// </summary>
    public async Task<List<string>?> ObtenerTiposComandaAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<string>>>("api/operaciones/comandas/tipos");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener tipos de comanda: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene el siguiente número de comanda
    /// </summary>
    public async Task<ApiResponse<string>?> ObtenerSiguienteNumeroComandaAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<string>>("api/operaciones/comandas/siguiente-numero");
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener siguiente número de comanda: {ex.Message}");
            return new ApiResponse<string>
            {
                Success = false,
                Message = $"Error al obtener siguiente número de comanda: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Valida si un número de comanda ya existe
    /// </summary>
    public async Task<ApiResponse<bool>?> ValidarNumeroComandaAsync(string numeroComanda, Guid? comandaIdExcluir = null)
    {
        try
        {
            var http = CreateClient();
            var queryParams = $"numeroComanda={Uri.EscapeDataString(numeroComanda)}";
            if (comandaIdExcluir.HasValue)
                queryParams += $"&excluirId={comandaIdExcluir.Value}";
            
            var response = await http.GetFromJsonAsync<ApiResponse<bool>>($"api/operaciones/comandas/validar-numero?{queryParams}");
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al validar número de comanda: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al validar número de comanda: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Exporta la lista de comandas a Excel
    /// </summary>
    public async Task<ApiResponse<byte[]>?> ExportarComandasAsync(ComandaFiltrosDto filtros, string formato = "Excel")
    {
        try
        {
            var http = CreateClient();
            var request = new { Filtros = filtros, Formato = formato };
            var response = await http.PostAsJsonAsync("api/operaciones/comandas/exportar", request);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                return new ApiResponse<byte[]>
                {
                    Success = true,
                    Data = content,
                    Message = "Comandas exportadas correctamente"
                };
            }
            else
            {
                return new ApiResponse<byte[]>
                {
                    Success = false,
                    Message = $"Error al exportar comandas: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al exportar comandas: {ex.Message}");
            return new ApiResponse<byte[]>
            {
                Success = false,
                Message = $"Error al exportar comandas: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Obtiene las comandas por mesa
    /// </summary>
    public async Task<List<ComandaDto>?> ObtenerComandasPorMesaAsync(Guid mesaId)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<ComandaDto>>>($"api/operaciones/comandas/mesa/{mesaId}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener comandas por mesa: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene las comandas por mesero
    /// </summary>
    public async Task<List<ComandaDto>?> ObtenerComandasPorMeseroAsync(Guid meseroId)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<ComandaDto>>>($"api/operaciones/comandas/mesero/{meseroId}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener comandas por mesero: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene las comandas por cliente
    /// </summary>
    public async Task<List<ComandaDto>?> ObtenerComandasPorClienteAsync(Guid clienteId)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<ComandaDto>>>($"api/operaciones/comandas/cliente/{clienteId}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener comandas por cliente: {ex.Message}");
            return null;
        }
    }
}
