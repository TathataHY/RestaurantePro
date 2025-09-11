using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public class ReportesInventarioApiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenStore _tokenStore;

    public ReportesInventarioApiService(IHttpClientFactory httpFactory, TokenStore tokenStore)
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
    /// Obtiene reporte de análisis de stock
    /// </summary>
    public async Task<object?> ObtenerAnalisisStockAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>("api/inventario/reportes/analisis-stock");
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener análisis de stock: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de movimientos de inventario
    /// </summary>
    public async Task<object?> ObtenerMovimientosInventarioAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var http = CreateClient();
            var url = $"api/inventario/reportes/movimientos?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}";
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>(url);
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener movimientos de inventario: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de productos próximos a vencer
    /// </summary>
    public async Task<object?> ObtenerProductosProximosVencerAsync(int dias)
    {
        try
        {
            var http = CreateClient();
            var url = $"api/inventario/reportes/proximos-vencer?dias={dias}";
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>(url);
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener productos próximos a vencer: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de productos vencidos
    /// </summary>
    public async Task<object?> ObtenerProductosVencidosAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>("api/inventario/reportes/vencidos");
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener productos vencidos: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de rotación de inventario
    /// </summary>
    public async Task<object?> ObtenerRotacionInventarioAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var http = CreateClient();
            var url = $"api/inventario/reportes/rotacion?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}";
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>(url);
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener rotación de inventario: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de análisis de costos
    /// </summary>
    public async Task<object?> ObtenerAnalisisCostosAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var http = CreateClient();
            var url = $"api/inventario/reportes/analisis-costos?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}";
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>(url);
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener análisis de costos: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de eficiencia de proveedores
    /// </summary>
    public async Task<object?> ObtenerEficienciaProveedoresAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var http = CreateClient();
            var url = $"api/inventario/reportes/eficiencia-proveedores?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}";
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>(url);
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener eficiencia de proveedores: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de predicción de demanda
    /// </summary>
    public async Task<object?> ObtenerPrediccionDemandaAsync(int meses)
    {
        try
        {
            var http = CreateClient();
            var url = $"api/inventario/reportes/prediccion-demanda?meses={meses}";
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>(url);
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener predicción de demanda: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de optimización de inventario
    /// </summary>
    public async Task<object?> ObtenerOptimizacionInventarioAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>("api/inventario/reportes/optimizacion");
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener optimización de inventario: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de análisis de desperdicios
    /// </summary>
    public async Task<object?> ObtenerAnalisisDesperdiciosAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var http = CreateClient();
            var url = $"api/inventario/reportes/analisis-desperdicios?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}";
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>(url);
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener análisis de desperdicios: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de valorización de inventario
    /// </summary>
    public async Task<object?> ObtenerValorizacionInventarioAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>("api/inventario/reportes/valorizacion");
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener valorización de inventario: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Exporta reporte de inventario a Excel
    /// </summary>
    public async Task<byte[]?> ExportarReporteInventarioAsync(string tipoReporte, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        try
        {
            var http = CreateClient();
            var url = $"api/inventario/reportes/exportar/{tipoReporte}";
            
            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                url += $"?fechaInicio={fechaInicio.Value:yyyy-MM-dd}&fechaFin={fechaFin.Value:yyyy-MM-dd}";
            }
            
            var resp = await http.GetAsync(url);
            
            if (resp.IsSuccessStatusCode)
            {
                return await resp.Content.ReadAsByteArrayAsync();
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al exportar reporte de inventario {tipoReporte}: {ex.Message}");
            return null;
        }
    }
}
