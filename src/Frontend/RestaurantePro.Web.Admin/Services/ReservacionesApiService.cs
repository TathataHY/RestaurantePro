using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

/// <summary>
/// Servicio para gestionar reservaciones del restaurante
/// </summary>
public class ReservacionesApiService : IReservacionesApiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenStore _tokenStore;

    public ReservacionesApiService(IHttpClientFactory httpFactory, TokenStore tokenStore)
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
    /// Obtiene la lista paginada de reservaciones con filtros
    /// </summary>
    public async Task<PaginatedList<ReservacionDto>?> ObtenerReservacionesAsync(ReservacionFiltrosDto filtros)
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
            
            if (!string.IsNullOrEmpty(filtros.CanalReservacion))
                queryParams.Add($"canalReservacion={Uri.EscapeDataString(filtros.CanalReservacion)}");
            
            if (filtros.ClienteId.HasValue)
                queryParams.Add($"clienteId={filtros.ClienteId.Value}");
            
            if (filtros.MesaId.HasValue)
                queryParams.Add($"mesaId={filtros.MesaId.Value}");
            
            if (filtros.FechaInicio.HasValue)
                queryParams.Add($"fechaInicio={filtros.FechaInicio.Value:yyyy-MM-dd}");
            
            if (filtros.FechaFin.HasValue)
                queryParams.Add($"fechaFin={filtros.FechaFin.Value:yyyy-MM-dd}");
            
            if (filtros.HoraInicio.HasValue)
                queryParams.Add($"horaInicio={filtros.HoraInicio.Value:hh\\:mm}");
            
            if (filtros.HoraFin.HasValue)
                queryParams.Add($"horaFin={filtros.HoraFin.Value:hh\\:mm}");
            
            if (filtros.NumeroPersonasMinimo.HasValue)
                queryParams.Add($"numeroPersonasMinimo={filtros.NumeroPersonasMinimo.Value}");
            
            if (filtros.NumeroPersonasMaximo.HasValue)
                queryParams.Add($"numeroPersonasMaximo={filtros.NumeroPersonasMaximo.Value}");
            
            if (filtros.EsUrgente.HasValue)
                queryParams.Add($"esUrgente={filtros.EsUrgente.Value}");
            
            if (filtros.EsVIP.HasValue)
                queryParams.Add($"esVIP={filtros.EsVIP.Value}");
            
            if (filtros.EsGrupo.HasValue)
                queryParams.Add($"esGrupo={filtros.EsGrupo.Value}");
            
            if (filtros.EsRecurrente.HasValue)
                queryParams.Add($"esRecurrente={filtros.EsRecurrente.Value}");
            
            if (filtros.EsHoy.HasValue)
                queryParams.Add($"esHoy={filtros.EsHoy.Value}");
            
            if (filtros.EsFutura.HasValue)
                queryParams.Add($"esFutura={filtros.EsFutura.Value}");
            
            if (filtros.RequiereAtencion.HasValue)
                queryParams.Add($"requiereAtencion={filtros.RequiereAtencion.Value}");

            var queryString = string.Join("&", queryParams);
            var response = await http.GetFromJsonAsync<ApiResponse<PaginatedList<ReservacionDto>>>($"api/operaciones/reservaciones?{queryString}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener reservaciones: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene una reservación específica por ID
    /// </summary>
    public async Task<ReservacionDto?> ObtenerReservacionAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<ReservacionDto>>($"api/operaciones/reservaciones/{id}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener reservación: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Crea una nueva reservación
    /// </summary>
    public async Task<ApiResponse<ReservacionDto>?> CrearReservacionAsync(CrearReservacionRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync("api/operaciones/reservaciones", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<ReservacionDto>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear reservación: {ex.Message}");
            return new ApiResponse<ReservacionDto>
            {
                Success = false,
                Message = $"Error al crear reservación: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Actualiza una reservación existente
    /// </summary>
    public async Task<ApiResponse<ReservacionDto>?> ActualizarReservacionAsync(ActualizarReservacionRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PutAsJsonAsync($"api/operaciones/reservaciones/{request.Id}", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<ReservacionDto>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar reservación: {ex.Message}");
            return new ApiResponse<ReservacionDto>
            {
                Success = false,
                Message = $"Error al actualizar reservación: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Elimina una reservación (soft delete)
    /// </summary>
    public async Task<ApiResponse<bool>?> EliminarReservacionAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var response = await http.DeleteAsync($"api/operaciones/reservaciones/{id}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar reservación: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al eliminar reservación: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Confirma una reservación
    /// </summary>
    public async Task<ApiResponse<bool>?> ConfirmarReservacionAsync(ConfirmarReservacionRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/operaciones/reservaciones/{request.ReservacionId}/confirmar", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al confirmar reservación: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al confirmar reservación: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Cancela una reservación
    /// </summary>
    public async Task<ApiResponse<bool>?> CancelarReservacionAsync(CancelarReservacionRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/operaciones/reservaciones/{request.ReservacionId}/cancelar", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cancelar reservación: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al cancelar reservación: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Marca la llegada de una reservación
    /// </summary>
    public async Task<ApiResponse<bool>?> MarcarLlegadaAsync(MarcarLlegadaRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/operaciones/reservaciones/{request.ReservacionId}/marcar-llegada", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al marcar llegada: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al marcar llegada: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Marca la salida de una reservación
    /// </summary>
    public async Task<ApiResponse<bool>?> MarcarSalidaAsync(MarcarSalidaRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/operaciones/reservaciones/{request.ReservacionId}/marcar-salida", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al marcar salida: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al marcar salida: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Verifica la disponibilidad de mesas
    /// </summary>
    public async Task<DisponibilidadResponse?> VerificarDisponibilidadAsync(VerificarDisponibilidadRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync("api/operaciones/reservaciones/verificar-disponibilidad", request);
            return await response.Content.ReadFromJsonAsync<DisponibilidadResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al verificar disponibilidad: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Reasigna una reservación a otra mesa
    /// </summary>
    public async Task<ApiResponse<bool>?> ReasignarMesaAsync(ReasignarMesaReservacionRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/operaciones/reservaciones/{request.ReservacionId}/reasignar-mesa", request);
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
    /// Cambia la hora de una reservación
    /// </summary>
    public async Task<ApiResponse<bool>?> CambiarHoraAsync(CambiarHoraReservacionRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/operaciones/reservaciones/{request.ReservacionId}/cambiar-hora", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cambiar hora: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al cambiar hora: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Duplica una reservación
    /// </summary>
    public async Task<ApiResponse<ReservacionDto>?> DuplicarReservacionAsync(DuplicarReservacionRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/operaciones/reservaciones/{request.ReservacionId}/duplicar", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<ReservacionDto>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al duplicar reservación: {ex.Message}");
            return new ApiResponse<ReservacionDto>
            {
                Success = false,
                Message = $"Error al duplicar reservación: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Obtiene las estadísticas de reservaciones
    /// </summary>
    public async Task<ReservacionEstadisticasDto?> ObtenerEstadisticasAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<ReservacionEstadisticasDto>>("api/operaciones/reservaciones/estadisticas");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener estadísticas de reservaciones: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene el calendario de reservaciones para una fecha
    /// </summary>
    public async Task<CalendarioReservacionesDto?> ObtenerCalendarioAsync(DateTime fecha)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<CalendarioReservacionesDto>>($"api/operaciones/reservaciones/calendario?fecha={fecha:yyyy-MM-dd}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener calendario: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene las reservaciones de hoy
    /// </summary>
    public async Task<List<ReservacionDto>?> ObtenerReservacionesHoyAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<ReservacionDto>>>("api/operaciones/reservaciones/hoy");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener reservaciones de hoy: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene las reservaciones urgentes
    /// </summary>
    public async Task<List<ReservacionDto>?> ObtenerReservacionesUrgentesAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<ReservacionDto>>>("api/operaciones/reservaciones/urgentes");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener reservaciones urgentes: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene las reservaciones que requieren atención
    /// </summary>
    public async Task<List<ReservacionDto>?> ObtenerReservacionesRequierenAtencionAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<ReservacionDto>>>("api/operaciones/reservaciones/requieren-atencion");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener reservaciones que requieren atención: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene los estados de reservación disponibles
    /// </summary>
    public async Task<List<string>?> ObtenerEstadosAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<string>>>("api/operaciones/reservaciones/estados");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener estados: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene los canales de reservación disponibles
    /// </summary>
    public async Task<List<string>?> ObtenerCanalesAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<string>>>("api/operaciones/reservaciones/canales");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener canales: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene el siguiente número de reservación
    /// </summary>
    public async Task<ApiResponse<string>?> ObtenerSiguienteNumeroReservacionAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<string>>("api/operaciones/reservaciones/siguiente-numero");
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener siguiente número de reservación: {ex.Message}");
            return new ApiResponse<string>
            {
                Success = false,
                Message = $"Error al obtener siguiente número de reservación: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Valida si un número de reservación ya existe
    /// </summary>
    public async Task<ApiResponse<bool>?> ValidarNumeroReservacionAsync(string numeroReservacion, Guid? reservacionIdExcluir = null)
    {
        try
        {
            var http = CreateClient();
            var queryParams = $"numeroReservacion={Uri.EscapeDataString(numeroReservacion)}";
            if (reservacionIdExcluir.HasValue)
                queryParams += $"&excluirId={reservacionIdExcluir.Value}";
            
            var response = await http.GetFromJsonAsync<ApiResponse<bool>>($"api/operaciones/reservaciones/validar-numero?{queryParams}");
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al validar número de reservación: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al validar número de reservación: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Exporta la lista de reservaciones a Excel
    /// </summary>
    public async Task<ApiResponse<byte[]>?> ExportarReservacionesAsync(ReservacionFiltrosDto filtros, string formato = "Excel")
    {
        try
        {
            var http = CreateClient();
            var request = new { Filtros = filtros, Formato = formato };
            var response = await http.PostAsJsonAsync("api/operaciones/reservaciones/exportar", request);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                return new ApiResponse<byte[]>
                {
                    Success = true,
                    Data = content,
                    Message = "Reservaciones exportadas correctamente"
                };
            }
            else
            {
                return new ApiResponse<byte[]>
                {
                    Success = false,
                    Message = $"Error al exportar reservaciones: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al exportar reservaciones: {ex.Message}");
            return new ApiResponse<byte[]>
            {
                Success = false,
                Message = $"Error al exportar reservaciones: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Obtiene las reservaciones por cliente
    /// </summary>
    public async Task<List<ReservacionDto>?> ObtenerReservacionesPorClienteAsync(Guid clienteId)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<ReservacionDto>>>($"api/operaciones/reservaciones/cliente/{clienteId}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener reservaciones por cliente: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene las reservaciones por mesa
    /// </summary>
    public async Task<List<ReservacionDto>?> ObtenerReservacionesPorMesaAsync(Guid mesaId)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<ReservacionDto>>>($"api/operaciones/reservaciones/mesa/{mesaId}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener reservaciones por mesa: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene las reservaciones por fecha
    /// </summary>
    public async Task<List<ReservacionDto>?> ObtenerReservacionesPorFechaAsync(DateTime fecha)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<ReservacionDto>>>($"api/operaciones/reservaciones/fecha/{fecha:yyyy-MM-dd}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener reservaciones por fecha: {ex.Message}");
            return null;
        }
    }
}
