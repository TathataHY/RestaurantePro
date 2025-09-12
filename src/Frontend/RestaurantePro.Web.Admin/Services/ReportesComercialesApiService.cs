using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public class ReportesComercialesApiService : IReportesComercialesApiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenStore _tokenStore;

    public ReportesComercialesApiService(IHttpClientFactory httpFactory, TokenStore tokenStore)
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
    /// Obtiene reporte de análisis de clientes
    /// </summary>
    public async Task<object?> ObtenerAnalisisClientesAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var http = CreateClient();
            var url = $"api/comercial/reportes/analisis-clientes?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}";
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>(url);
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener análisis de clientes: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de segmentación de clientes
    /// </summary>
    public async Task<object?> ObtenerSegmentacionClientesAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>("api/comercial/reportes/segmentacion-clientes");
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener segmentación de clientes: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de análisis de productos
    /// </summary>
    public async Task<object?> ObtenerAnalisisProductosAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var http = CreateClient();
            var url = $"api/comercial/reportes/analisis-productos?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}";
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>(url);
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener análisis de productos: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de rentabilidad por producto
    /// </summary>
    public async Task<object?> ObtenerRentabilidadProductosAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var http = CreateClient();
            var url = $"api/comercial/reportes/rentabilidad-productos?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}";
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>(url);
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener rentabilidad de productos: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de análisis de promociones
    /// </summary>
    public async Task<object?> ObtenerAnalisisPromocionesAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var http = CreateClient();
            var url = $"api/comercial/reportes/analisis-promociones?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}";
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>(url);
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener análisis de promociones: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de tendencias de ventas
    /// </summary>
    public async Task<object?> ObtenerTendenciasVentasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var http = CreateClient();
            var url = $"api/comercial/reportes/tendencias-ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}";
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>(url);
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener tendencias de ventas: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de análisis de canales
    /// </summary>
    public async Task<object?> ObtenerAnalisisCanalesAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var http = CreateClient();
            var url = $"api/comercial/reportes/analisis-canales?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}";
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>(url);
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener análisis de canales: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de análisis de estacionalidad
    /// </summary>
    public async Task<object?> ObtenerAnalisisEstacionalidadAsync(int año)
    {
        try
        {
            var http = CreateClient();
            var url = $"api/comercial/reportes/analisis-estacionalidad?año={año}";
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>(url);
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener análisis de estacionalidad: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de análisis de competencia
    /// </summary>
    public async Task<object?> ObtenerAnalisisCompetenciaAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>("api/comercial/reportes/analisis-competencia");
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener análisis de competencia: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene reporte de proyecciones comerciales
    /// </summary>
    public async Task<object?> ObtenerProyeccionesComercialesAsync(int meses)
    {
        try
        {
            var http = CreateClient();
            var url = $"api/comercial/reportes/proyecciones?meses={meses}";
            var resp = await http.GetFromJsonAsync<ApiResponse<object>>(url);
            return resp?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener proyecciones comerciales: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Exporta reporte comercial a Excel
    /// </summary>
    public async Task<byte[]?> ExportarReporteComercialAsync(string tipoReporte, DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var http = CreateClient();
            var url = $"api/comercial/reportes/exportar/{tipoReporte}?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}";
            var resp = await http.GetAsync(url);
            
            if (resp.IsSuccessStatusCode)
            {
                return await resp.Content.ReadAsByteArrayAsync();
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al exportar reporte comercial {tipoReporte}: {ex.Message}");
            return null;
        }
    }
}
