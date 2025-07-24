using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Models.Common;
using System.Text.Json;

namespace RestaurantePro.Mobile.Core.Services.Mesas;

/// <summary>
/// Implementación del servicio de mesas
/// </summary>
public class MesasService : IMesasService
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;
    private const string BasePath = "api/operaciones/mesas";

    public MesasService(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
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
        var token = await _authService.GetTokenAsync();
        return await _apiService.GetAsync<List<MesaDto>>(endpoint, token);
    }

    /// <summary>
    /// Obtiene una mesa específica por ID
    /// </summary>
    public async Task<ApiResponse<MesaDto>> ObtenerMesaAsync(Guid id)
    {
        var endpoint = $"{BasePath}/{id}";
        var token = await _authService.GetTokenAsync();
        return await _apiService.GetAsync<MesaDto>(endpoint, token);
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
        var token = await _authService.GetTokenAsync();
        
        // El endpoint retorna PaginatedList<MesaDto>, necesitamos extraer los Items
        var response = await _apiService.GetAsync<PaginatedList<MesaDto>>(endpoint, token);
        
        if (response.Success && response.Data != null)
        {
            // Convertir PaginatedList a List
            return ApiResponse<List<MesaDto>>.SuccessResponse(response.Data.Items, response.Message);
        }
        
        return ApiResponse<List<MesaDto>>.ErrorResponse(response.Message ?? "Error al obtener mesas disponibles");
    }

    /// <summary>
    /// Obtiene el estado general de ocupación de las mesas
    /// </summary>
    public async Task<ApiResponse<EstadoMesasDto>> ObtenerEstadoOcupacionAsync()
    {
        var endpoint = $"{BasePath}/estado-ocupacion";
        var token = await _authService.GetTokenAsync();
        return await _apiService.GetAsync<EstadoMesasDto>(endpoint, token);
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
        var token = await _authService.GetTokenAsync();
        return await _apiService.PostAsync<object>(endpoint, request, token);
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
        var token = await _authService.GetTokenAsync();
        return await _apiService.PostAsync<MesaDto>(endpoint, request, token);
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
        var token = await _authService.GetTokenAsync();
        return await _apiService.PutAsync<MesaDto>(endpoint, request, token);
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
        var token = await _authService.GetTokenAsync();
        return await _apiService.GetAsync<MesaDto>(endpoint, token);
    }
} 