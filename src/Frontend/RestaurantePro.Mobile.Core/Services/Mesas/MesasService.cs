using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using System.Text.Json;

namespace RestaurantePro.Mobile.Core.Services.Mesas;

/// <summary>
/// Implementación del servicio de mesas
/// </summary>
public class MesasService : IMesasService
{
    private readonly IApiService _apiService;
    private const string BasePath = "api/operaciones/mesas";

    public MesasService(IApiService apiService)
    {
        _apiService = apiService;
    }

    /// <summary>
    /// Obtiene todas las mesas con filtros opcionales
    /// </summary>
    public async Task<ApiResponse<List<MesaDto>>> ObtenerMesasAsync(
        string? estado = null, 
        string? ubicacion = null, 
        int? capacidadMinima = null)
    {
        var queryParams = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(estado))
            queryParams.Add($"estado={Uri.EscapeDataString(estado)}");
        
        if (!string.IsNullOrWhiteSpace(ubicacion))
            queryParams.Add($"ubicacion={Uri.EscapeDataString(ubicacion)}");
        
        if (capacidadMinima.HasValue)
            queryParams.Add($"capacidadMinima={capacidadMinima.Value}");

        var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        var endpoint = $"{BasePath}{query}";

        return await _apiService.GetAsync<List<MesaDto>>(endpoint);
    }

    /// <summary>
    /// Obtiene una mesa específica por ID
    /// </summary>
    public async Task<ApiResponse<MesaDto>> ObtenerMesaAsync(Guid id)
    {
        var endpoint = $"{BasePath}/{id}";
        return await _apiService.GetAsync<MesaDto>(endpoint);
    }

    /// <summary>
    /// Obtiene las mesas disponibles
    /// </summary>
    public async Task<ApiResponse<List<MesaDto>>> ObtenerMesasDisponiblesAsync(
        int? capacidadMinima = null, 
        string? ubicacion = null)
    {
        var queryParams = new List<string>();
        
        if (capacidadMinima.HasValue)
            queryParams.Add($"capacidadMinima={capacidadMinima.Value}");
        
        if (!string.IsNullOrWhiteSpace(ubicacion))
            queryParams.Add($"ubicacion={Uri.EscapeDataString(ubicacion)}");

        var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        var endpoint = $"{BasePath}/disponibles{query}";

        return await _apiService.GetAsync<List<MesaDto>>(endpoint);
    }

    /// <summary>
    /// Obtiene el estado general de ocupación de las mesas
    /// </summary>
    public async Task<ApiResponse<EstadoMesasDto>> ObtenerEstadoOcupacionAsync()
    {
        var endpoint = $"{BasePath}/estado-ocupacion";
        return await _apiService.GetAsync<EstadoMesasDto>(endpoint);
    }

    /// <summary>
    /// Asigna (ocupa) una mesa
    /// </summary>
    public async Task<ApiResponse<object>> AsignarMesaAsync(
        Guid mesaId, 
        Guid? clienteId = null, 
        int? numeroPersonas = null, 
        string? observaciones = null)
    {
        var request = new
        {
            ClienteId = clienteId,
            NumeroPersonas = numeroPersonas,
            Observaciones = observaciones ?? string.Empty,
            FechaAsignacion = DateTime.Now
        };

        var endpoint = $"{BasePath}/{mesaId}/asignar";
        return await _apiService.PostAsync<object>(endpoint, request);
    }

    /// <summary>
    /// Libera una mesa
    /// </summary>
    public async Task<ApiResponse<MesaDto>> LiberarMesaAsync(
        Guid mesaId, 
        string motivo = "Mesa liberada desde móvil", 
        string? observaciones = null)
    {
        var request = new
        {
            Motivo = motivo,
            Observaciones = observaciones ?? string.Empty,
            FechaLiberacion = DateTime.Now
        };

        var endpoint = $"{BasePath}/{mesaId}/liberar";
        return await _apiService.PostAsync<MesaDto>(endpoint, request);
    }

    /// <summary>
    /// Cambia el estado de una mesa
    /// </summary>
    public async Task<ApiResponse<MesaDto>> CambiarEstadoMesaAsync(
        Guid mesaId, 
        string nuevoEstado, 
        string? motivo = null)
    {
        var request = new
        {
            NuevoEstado = nuevoEstado,
            Motivo = motivo ?? $"Estado cambiado a {nuevoEstado} desde móvil",
            FechaCambio = DateTime.Now
        };

        var endpoint = $"{BasePath}/{mesaId}/estado";
        return await _apiService.PutAsync<MesaDto>(endpoint, request);
    }

    /// <summary>
    /// Busca la mejor mesa disponible para un número de personas
    /// </summary>
    public async Task<ApiResponse<MesaDto>> BuscarMejorMesaAsync(
        int numeroPersonas, 
        string? ubicacionPreferida = null)
    {
        var queryParams = new List<string>
        {
            $"numeroPersonas={numeroPersonas}"
        };
        
        if (!string.IsNullOrWhiteSpace(ubicacionPreferida))
            queryParams.Add($"ubicacionPreferida={Uri.EscapeDataString(ubicacionPreferida)}");

        var query = "?" + string.Join("&", queryParams);
        var endpoint = $"{BasePath}/buscar-mejor{query}";

        return await _apiService.GetAsync<MesaDto>(endpoint);
    }
} 