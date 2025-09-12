using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

/// <summary>
/// Servicio para gestionar reportes del restaurante
/// </summary>
public class ReportesApiService : IReportesApiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenStore _tokenStore;

    public ReportesApiService(IHttpClientFactory httpFactory, TokenStore tokenStore)
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
    /// Obtiene estadísticas generales de reportes
    /// </summary>
    public virtual async Task<ReporteEstadisticasDto?> ObtenerEstadisticasAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<ReporteEstadisticasDto>>("api/operaciones/reportes/estadisticas");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener estadísticas de reportes: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Genera un reporte de ventas por período
    /// </summary>
    public async Task<ReporteVentasDto?> GenerarReporteVentasAsync(ReporteFiltrosDto filtros)
    {
        try
        {
            var http = CreateClient();
            var queryParams = new List<string>
            {
                $"fechaInicio={filtros.FechaInicio:yyyy-MM-dd}",
                $"fechaFin={filtros.FechaFin:yyyy-MM-dd}"
            };

            if (filtros.MeseroId.HasValue)
                queryParams.Add($"meseroId={filtros.MeseroId.Value}");
            
            if (filtros.MesaId.HasValue)
                queryParams.Add($"mesaId={filtros.MesaId.Value}");

            var queryString = string.Join("&", queryParams);
            var response = await http.GetFromJsonAsync<ApiResponse<ReporteVentasDto>>($"api/operaciones/reportes/ventas?{queryString}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al generar reporte de ventas: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Genera un reporte de productos más vendidos
    /// </summary>
    public async Task<ReporteProductosDto?> GenerarReporteProductosAsync(ReporteFiltrosDto filtros)
    {
        try
        {
            var http = CreateClient();
            var queryParams = new List<string>
            {
                $"fechaInicio={filtros.FechaInicio:yyyy-MM-dd}",
                $"fechaFin={filtros.FechaFin:yyyy-MM-dd}"
            };

            if (filtros.CategoriaId.HasValue)
                queryParams.Add($"categoriaId={filtros.CategoriaId.Value}");

            if (filtros.LimiteResultados.HasValue)
                queryParams.Add($"limite={filtros.LimiteResultados.Value}");

            var queryString = string.Join("&", queryParams);
            var response = await http.GetFromJsonAsync<ApiResponse<ReporteProductosDto>>($"api/operaciones/reportes/productos?{queryString}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al generar reporte de productos: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Genera un reporte de rendimiento de mesas
    /// </summary>
    public async Task<ReporteMesasDto?> GenerarReporteMesasAsync(ReporteFiltrosDto filtros)
    {
        try
        {
            var http = CreateClient();
            var queryParams = new List<string>
            {
                $"fechaInicio={filtros.FechaInicio:yyyy-MM-dd}",
                $"fechaFin={filtros.FechaFin:yyyy-MM-dd}"
            };

            if (filtros.MesaId.HasValue)
                queryParams.Add($"mesaId={filtros.MesaId.Value}");

            var queryString = string.Join("&", queryParams);
            var response = await http.GetFromJsonAsync<ApiResponse<ReporteMesasDto>>($"api/operaciones/reportes/mesas?{queryString}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al generar reporte de mesas: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Genera un reporte de resumen de comandas
    /// </summary>
    public async Task<ReporteComandasDto?> GenerarReporteComandasAsync(ReporteFiltrosDto filtros)
    {
        try
        {
            var http = CreateClient();
            var queryParams = new List<string>
            {
                $"fechaInicio={filtros.FechaInicio:yyyy-MM-dd}",
                $"fechaFin={filtros.FechaFin:yyyy-MM-dd}"
            };

            if (filtros.MeseroId.HasValue)
                queryParams.Add($"meseroId={filtros.MeseroId.Value}");

            if (filtros.MesaId.HasValue)
                queryParams.Add($"mesaId={filtros.MesaId.Value}");

            var queryString = string.Join("&", queryParams);
            var response = await http.GetFromJsonAsync<ApiResponse<ReporteComandasDto>>($"api/operaciones/reportes/comandas?{queryString}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al generar reporte de comandas: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Genera un reporte personalizado
    /// </summary>
    public async Task<ApiResponse<object>?> GenerarReportePersonalizadoAsync(SolicitarReporteRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync("api/operaciones/reportes/generar", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al generar reporte personalizado: {ex.Message}");
            return new ApiResponse<object>
            {
                Success = false,
                Message = $"Error al generar reporte: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Exporta un reporte en formato específico
    /// </summary>
    public async Task<ApiResponse<byte[]>?> ExportarReporteAsync(ExportarReporteRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync("api/operaciones/reportes/exportar", request);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                return new ApiResponse<byte[]>
                {
                    Success = true,
                    Data = content,
                    Message = "Reporte exportado correctamente"
                };
            }
            else
            {
                return new ApiResponse<byte[]>
                {
                    Success = false,
                    Message = $"Error al exportar reporte: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al exportar reporte: {ex.Message}");
            return new ApiResponse<byte[]>
            {
                Success = false,
                Message = $"Error al exportar reporte: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Obtiene el historial de reportes generados
    /// </summary>
    public async Task<List<ReporteDto>?> ObtenerHistorialReportesAsync(int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<ReporteDto>>>($"api/operaciones/reportes/historial?pageNumber={pageNumber}&pageSize={pageSize}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener historial de reportes: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene un reporte específico por ID
    /// </summary>
    public async Task<ReporteDto?> ObtenerReporteAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<ReporteDto>>($"api/operaciones/reportes/{id}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener reporte: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Elimina un reporte del historial
    /// </summary>
    public async Task<ApiResponse<bool>?> EliminarReporteAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var response = await http.DeleteAsync($"api/operaciones/reportes/{id}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar reporte: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al eliminar reporte: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Obtiene los tipos de reportes disponibles
    /// </summary>
    public async Task<List<TipoReporte>?> ObtenerTiposReportesAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<TipoReporte>>>("api/operaciones/reportes/tipos");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener tipos de reportes: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Valida los filtros de un reporte
    /// </summary>
    public async Task<ApiResponse<bool>?> ValidarFiltrosAsync(ReporteFiltrosDto filtros)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync("api/operaciones/reportes/validar-filtros", filtros);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al validar filtros: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al validar filtros: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Obtiene todos los reportes
    /// </summary>
    public async Task<List<ReporteDto>> ObtenerReportesAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<ReporteDto>>>("api/operaciones/reportes");
            return response?.Data ?? new List<ReporteDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener reportes: {ex.Message}");
            return new List<ReporteDto>();
        }
    }

    /// <summary>
    /// Obtiene un reporte por ID
    /// </summary>
    public async Task<ReporteDto?> ObtenerReportePorIdAsync(Guid id)
    {
        return await ObtenerReporteAsync(id);
    }

    /// <summary>
    /// Genera un reporte
    /// </summary>
    public async Task<byte[]?> GenerarReporteAsync(Guid reporteId, Dictionary<string, object> parametros)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/operaciones/reportes/{reporteId}/generar", parametros);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al generar reporte {reporteId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene los tipos de reporte disponibles
    /// </summary>
    public async Task<List<TipoReporte>> ObtenerTiposReporteAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<TipoReporte>>>("api/operaciones/reportes/tipos");
            return response?.Data ?? new List<TipoReporte>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener tipos de reporte: {ex.Message}");
            return new List<TipoReporte>();
        }
    }
}
