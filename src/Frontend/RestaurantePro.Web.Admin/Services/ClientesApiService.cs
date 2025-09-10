using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

/// <summary>
/// Servicio para gestionar clientes del restaurante
/// </summary>
public class ClientesApiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenStore _tokenStore;

    public ClientesApiService(IHttpClientFactory httpFactory, TokenStore tokenStore)
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
    /// Obtiene la lista paginada de clientes con filtros
    /// </summary>
    public async Task<PaginatedList<ClienteDto>?> ObtenerClientesAsync(ClienteFiltrosDto filtros)
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
            
            if (!string.IsNullOrEmpty(filtros.Segmento))
                queryParams.Add($"segmento={Uri.EscapeDataString(filtros.Segmento)}");
            
            if (!string.IsNullOrEmpty(filtros.Ciudad))
                queryParams.Add($"ciudad={Uri.EscapeDataString(filtros.Ciudad)}");
            
            if (!string.IsNullOrEmpty(filtros.Estado))
                queryParams.Add($"estado={Uri.EscapeDataString(filtros.Estado)}");
            
            if (!string.IsNullOrEmpty(filtros.NivelFidelizacion))
                queryParams.Add($"nivelFidelizacion={Uri.EscapeDataString(filtros.NivelFidelizacion)}");
            
            if (filtros.FechaInicio.HasValue)
                queryParams.Add($"fechaInicio={filtros.FechaInicio.Value:yyyy-MM-dd}");
            
            if (filtros.FechaFin.HasValue)
                queryParams.Add($"fechaFin={filtros.FechaFin.Value:yyyy-MM-dd}");
            
            if (filtros.GastoMinimo.HasValue)
                queryParams.Add($"gastoMinimo={filtros.GastoMinimo.Value}");
            
            if (filtros.GastoMaximo.HasValue)
                queryParams.Add($"gastoMaximo={filtros.GastoMaximo.Value}");
            
            if (filtros.VisitasMinimas.HasValue)
                queryParams.Add($"visitasMinimas={filtros.VisitasMinimas.Value}");
            
            if (filtros.AceptaMarketing.HasValue)
                queryParams.Add($"aceptaMarketing={filtros.AceptaMarketing.Value}");

            var queryString = string.Join("&", queryParams);
            var response = await http.GetFromJsonAsync<ApiResponse<PaginatedList<ClienteDto>>>($"api/comercial/clientes?{queryString}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener clientes: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene un cliente específico por ID
    /// </summary>
    public async Task<ClienteDto?> ObtenerClienteAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<ClienteDto>>($"api/comercial/clientes/{id}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener cliente: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Crea un nuevo cliente
    /// </summary>
    public async Task<ApiResponse<ClienteDto>?> CrearClienteAsync(CrearClienteRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync("api/comercial/clientes", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear cliente: {ex.Message}");
            return new ApiResponse<ClienteDto>
            {
                Success = false,
                Message = $"Error al crear cliente: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Actualiza un cliente existente
    /// </summary>
    public async Task<ApiResponse<ClienteDto>?> ActualizarClienteAsync(ActualizarClienteRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PutAsJsonAsync($"api/comercial/clientes/{request.Id}", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar cliente: {ex.Message}");
            return new ApiResponse<ClienteDto>
            {
                Success = false,
                Message = $"Error al actualizar cliente: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Elimina un cliente (soft delete)
    /// </summary>
    public async Task<ApiResponse<bool>?> EliminarClienteAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var response = await http.DeleteAsync($"api/comercial/clientes/{id}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar cliente: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al eliminar cliente: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Activa o desactiva un cliente
    /// </summary>
    public async Task<ApiResponse<bool>?> ToggleActivarClienteAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PatchAsync($"api/comercial/clientes/{id}/toggle-activar", null);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cambiar estado del cliente: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al cambiar estado del cliente: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Obtiene las estadísticas de clientes
    /// </summary>
    public async Task<ClienteEstadisticasDto?> ObtenerEstadisticasAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<ClienteEstadisticasDto>>("api/comercial/clientes/estadisticas");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener estadísticas de clientes: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene el historial de compras de un cliente
    /// </summary>
    public async Task<List<ClienteComprasDto>?> ObtenerHistorialComprasAsync(Guid clienteId, int limite = 10)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<ClienteComprasDto>>>($"api/comercial/clientes/{clienteId}/compras?limite={limite}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener historial de compras: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene las reservaciones de un cliente
    /// </summary>
    public async Task<List<ClienteReservacionDto>?> ObtenerReservacionesAsync(Guid clienteId, int limite = 10)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<ClienteReservacionDto>>>($"api/comercial/clientes/{clienteId}/reservaciones?limite={limite}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener reservaciones: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene las tarjetas de fidelización de un cliente
    /// </summary>
    public async Task<List<ClienteTarjetaFidelizacionDto>?> ObtenerTarjetasFidelizacionAsync(Guid clienteId)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<ClienteTarjetaFidelizacionDto>>>($"api/comercial/clientes/{clienteId}/tarjetas-fidelizacion");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener tarjetas de fidelización: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Busca clientes por término de búsqueda
    /// </summary>
    public async Task<BuscarClienteResponse?> BuscarClientesAsync(BuscarClienteRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync("api/comercial/clientes/buscar", request);
            return await response.Content.ReadFromJsonAsync<BuscarClienteResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al buscar clientes: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene los segmentos de clientes disponibles
    /// </summary>
    public async Task<List<string>?> ObtenerSegmentosAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<string>>>("api/comercial/clientes/segmentos");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener segmentos: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene las ciudades de clientes
    /// </summary>
    public async Task<List<string>?> ObtenerCiudadesAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<string>>>("api/comercial/clientes/ciudades");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener ciudades: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Valida si un email ya existe
    /// </summary>
    public async Task<ApiResponse<bool>?> ValidarEmailAsync(string email, Guid? clienteIdExcluir = null)
    {
        try
        {
            var http = CreateClient();
            var queryParams = $"email={Uri.EscapeDataString(email)}";
            if (clienteIdExcluir.HasValue)
                queryParams += $"&excluirId={clienteIdExcluir.Value}";
            
            var response = await http.GetFromJsonAsync<ApiResponse<bool>>($"api/comercial/clientes/validar-email?{queryParams}");
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al validar email: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al validar email: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Exporta la lista de clientes a Excel
    /// </summary>
    public async Task<ApiResponse<byte[]>?> ExportarClientesAsync(ClienteFiltrosDto filtros, string formato = "Excel")
    {
        try
        {
            var http = CreateClient();
            var request = new { Filtros = filtros, Formato = formato };
            var response = await http.PostAsJsonAsync("api/comercial/clientes/exportar", request);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                return new ApiResponse<byte[]>
                {
                    Success = true,
                    Data = content,
                    Message = "Clientes exportados correctamente"
                };
            }
            else
            {
                return new ApiResponse<byte[]>
                {
                    Success = false,
                    Message = $"Error al exportar clientes: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al exportar clientes: {ex.Message}");
            return new ApiResponse<byte[]>
            {
                Success = false,
                Message = $"Error al exportar clientes: {ex.Message}"
            };
        }
    }
}
