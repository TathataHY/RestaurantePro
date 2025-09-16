using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

/// <summary>
/// 📋 Implementación del servicio para gestionar el historial de comandas
/// </summary>
public class HistorialComandasApiService : IHistorialComandasApiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenStore _tokenStore;
    private readonly ILogger<HistorialComandasApiService> _logger;

    public HistorialComandasApiService(IHttpClientFactory httpFactory, TokenStore tokenStore, ILogger<HistorialComandasApiService> logger)
    {
        _httpFactory = httpFactory;
        _tokenStore = tokenStore;
        _logger = logger;
    }

    private HttpClient CreateClient()
    {
        var http = _httpFactory.CreateClient("Api");
        if (!string.IsNullOrWhiteSpace(_tokenStore.Token))
        {
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStore.Token);
        }
        return http;
    }

    /// <summary>
    /// 🔍 Obtener historial de comandas con filtros avanzados
    /// </summary>
    public async Task<PaginatedList<ComandaSummaryDto>?> ObtenerHistorialComandasAsync(
        int pageNumber = 1,
        int pageSize = 20,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        Guid? mesaId = null,
        Guid? meseroId = null,
        Guid? clienteId = null,
        string? estado = null,
        decimal? montoMinimo = null,
        decimal? montoMaximo = null,
        string? terminoBusqueda = null,
        bool incluirCanceladas = false,
        bool soloFinalizadas = true,
        string ordenarPor = "FechaMasReciente")
    {
        var http = CreateClient();
        var query = new List<string>();

        // Parámetros básicos
        query.Add($"pageNumber={pageNumber}");
        query.Add($"pageSize={pageSize}");
        query.Add($"incluirCanceladas={incluirCanceladas}");
        query.Add($"soloFinalizadas={soloFinalizadas}");
        query.Add($"ordenarPor={Uri.EscapeDataString(ordenarPor)}");

        // Filtros opcionales
        if (fechaDesde.HasValue)
            query.Add($"fechaDesde={fechaDesde.Value:yyyy-MM-ddTHH:mm:ss}");
        if (fechaHasta.HasValue)
            query.Add($"fechaHasta={fechaHasta.Value:yyyy-MM-ddTHH:mm:ss}");
        if (mesaId.HasValue)
            query.Add($"mesaId={mesaId.Value}");
        if (meseroId.HasValue)
            query.Add($"meseroId={meseroId.Value}");
        if (clienteId.HasValue)
            query.Add($"clienteId={clienteId.Value}");
        if (!string.IsNullOrWhiteSpace(estado))
            query.Add($"estado={Uri.EscapeDataString(estado)}");
        if (montoMinimo.HasValue)
            query.Add($"montoMinimo={montoMinimo.Value}");
        if (montoMaximo.HasValue)
            query.Add($"montoMaximo={montoMaximo.Value}");
        if (!string.IsNullOrWhiteSpace(terminoBusqueda))
            query.Add($"terminoBusqueda={Uri.EscapeDataString(terminoBusqueda)}");

        var queryString = "?" + string.Join("&", query);

        try
        {
            _logger.LogInformation("📋 Obteniendo historial de comandas con filtros avanzados");
            var response = await http.GetFromJsonAsync<ApiResponse<PaginatedList<ComandaSummaryDto>>>(
                $"api/operaciones/historial-comandas{queryString}");

            if (response?.Success == true)
            {
                _logger.LogInformation("✅ Historial de comandas obtenido exitosamente. Total: {Total}", 
                    response.Data?.TotalCount ?? 0);
                return response.Data;
            }

            _logger.LogWarning("⚠️ Error al obtener historial de comandas: {Error}", response?.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error inesperado al obtener historial de comandas");
            return null;
        }
    }

    /// <summary>
    /// 🔍 Obtener historial de comandas por mesa específica
    /// </summary>
    public async Task<PaginatedList<ComandaSummaryDto>?> ObtenerHistorialPorMesaAsync(
        Guid mesaId,
        DateTime? fechaDesde = null,
        int pageNumber = 1)
    {
        var http = CreateClient();
        var query = new List<string>
        {
            $"pageNumber={pageNumber}"
        };

        if (fechaDesde.HasValue)
            query.Add($"fechaDesde={fechaDesde.Value:yyyy-MM-ddTHH:mm:ss}");

        var queryString = query.Count > 1 ? "?" + string.Join("&", query) : string.Empty;

        try
        {
            _logger.LogInformation("📋 Obteniendo historial de comandas para mesa {MesaId}", mesaId);
            var response = await http.GetFromJsonAsync<ApiResponse<PaginatedList<ComandaSummaryDto>>>(
                $"api/operaciones/historial-comandas/mesa/{mesaId}{queryString}");

            if (response?.Success == true)
            {
                _logger.LogInformation("✅ Historial de mesa {MesaId} obtenido exitosamente. Total: {Total}", 
                    mesaId, response.Data?.TotalCount ?? 0);
                return response.Data;
            }

            _logger.LogWarning("⚠️ Error al obtener historial de mesa {MesaId}: {Error}", mesaId, response?.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error inesperado al obtener historial de mesa {MesaId}", mesaId);
            return null;
        }
    }

    /// <summary>
    /// 🔍 Obtener historial básico (últimos 30 días)
    /// </summary>
    public async Task<PaginatedList<ComandaSummaryDto>?> ObtenerHistorialBasicoAsync(
        int pageNumber = 1,
        int pageSize = 20)
    {
        var http = CreateClient();
        var queryString = $"?pageNumber={pageNumber}&pageSize={pageSize}";

        try
        {
            _logger.LogInformation("📋 Obteniendo historial básico de comandas");
            var response = await http.GetFromJsonAsync<ApiResponse<PaginatedList<ComandaSummaryDto>>>(
                $"api/operaciones/historial-comandas/basico{queryString}");

            if (response?.Success == true)
            {
                _logger.LogInformation("✅ Historial básico obtenido exitosamente. Total: {Total}", 
                    response.Data?.TotalCount ?? 0);
                return response.Data;
            }

            _logger.LogWarning("⚠️ Error al obtener historial básico: {Error}", response?.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error inesperado al obtener historial básico");
            return null;
        }
    }
}
