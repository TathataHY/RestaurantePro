using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Models.Common;
using System.Text.Json;
using System.Net.Sockets;

namespace RestaurantePro.Mobile.Core.Services.Mesas;

/// <summary>
/// Implementación del servicio de mesas
/// </summary>
public class MesasService : IMesasService
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;
    private readonly RestaurantePro.Mobile.Core.Services.Caching.ICacheService? _cache;
    private const string BasePath = "api/operaciones/mesas";

    public MesasService(IApiService apiService, IAuthService authService, RestaurantePro.Mobile.Core.Services.Caching.ICacheService? cache = null)
    {
        _apiService = apiService;
        _authService = authService;
        _cache = cache;
    }

    /// <summary>
    /// Obtiene todas las mesas con filtros opcionales
    /// </summary>
    public async Task<ApiResponse<List<MesaDto>>> ObtenerMesasAsync(
        string? estado = null, 
        string? ubicacion = null, 
        int? capacidadMinima = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<List<MesaDto>>.ErrorResponse("Operación cancelada por el usuario");
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
        catch (HttpRequestException ex)
        {
            return ApiResponse<List<MesaDto>>.ErrorResponse(ex.Message, "Error de comunicación con el servidor", 500);
        }
        catch (TaskCanceledException ex)
        {
            return ApiResponse<List<MesaDto>>.ErrorResponse(ex.Message, "Timeout de la operación", 408);
        }
        catch (IOException ex)
        {
            return ApiResponse<List<MesaDto>>.ErrorResponse(ex.Message, "Error de entrada/salida", 500);
        }
        catch (SocketException ex)
        {
            return ApiResponse<List<MesaDto>>.ErrorResponse(ex.Message, "Error de conexión de red", 500);
        }
        catch (AggregateException ex)
        {
            return ApiResponse<List<MesaDto>>.ErrorResponse(ex.Message, "Error de red", 500);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<MesaDto>>.ErrorResponse(ex.Message, "Error inesperado", 500);
        }
    }

    /// <summary>
    /// Obtiene una mesa específica por ID
    /// </summary>
    public async Task<ApiResponse<MesaDto>> ObtenerMesaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ApiResponse<MesaDto>.ErrorResponse("Operación cancelada por el usuario");
        var endpoint = $"{BasePath}/{id}";
        var token = await _authService.GetTokenAsync();
        return await _apiService.GetAsync<MesaDto>(endpoint, token);
    }

    /// <summary>
    /// Obtiene las mesas disponibles
    /// </summary>
    public async Task<ApiResponse<List<MesaDto>>> ObtenerMesasDisponiblesAsync(
        int? capacidadMinima = null, 
        string? ubicacion = null,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ApiResponse<List<MesaDto>>.ErrorResponse("Operación cancelada por el usuario");
        var queryParams = new List<string>();
        
        if (capacidadMinima.HasValue)
            queryParams.Add($"capacidadMinima={capacidadMinima.Value}");
        
        if (!string.IsNullOrWhiteSpace(ubicacion))
            queryParams.Add($"ubicacion={Uri.EscapeDataString(ubicacion)}");

        // Limitar tamaño por defecto a 50 para reducir latencia
        queryParams.Add("pageNumber=1");
        queryParams.Add("pageSize=50");
        var query = "?" + string.Join("&", queryParams);
        var endpoint = $"{BasePath}/disponibles{query}";
        var token = await _authService.GetTokenAsync();
        
        // El endpoint retorna PaginatedList<MesaDto>, necesitamos extraer los Items
        ApiResponse<PaginatedList<MesaDto>> response;
        if (_cache != null)
        {
            response = await _cache.GetOrSetAsync(
                key: $"mesas_disponibles:{query}",
                factory: async () => await _apiService.GetAsync<PaginatedList<MesaDto>>(endpoint, token),
                expiration: TimeSpan.FromSeconds(30));
        }
        else
        {
            response = await _apiService.GetAsync<PaginatedList<MesaDto>>(endpoint, token);
        }
        
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
    public async Task<ApiResponse<EstadoMesasDto>> ObtenerEstadoOcupacionAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ApiResponse<EstadoMesasDto>.ErrorResponse("Operación cancelada por el usuario");
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
        string? observaciones = null,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ApiResponse<object>.ErrorResponse("Operación cancelada por el usuario");
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
        string? observaciones = null,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ApiResponse<MesaDto>.ErrorResponse("Operación cancelada por el usuario");
        System.Diagnostics.Debug.WriteLine($"[DEBUG] MesasService.LiberarMesaAsync -> mesaId={mesaId}, motivo='{motivo}', observaciones='{observaciones}'");
        var request = new
        {
            Motivo = motivo,
            Observaciones = observaciones ?? string.Empty,
            FechaLiberacion = DateTime.Now
        };

        var endpoint = $"{BasePath}/{mesaId}/liberar";
        var token = await _authService.GetTokenAsync();
        var response = await _apiService.PostAsync<MesaDto>(endpoint, request, token);
        System.Diagnostics.Debug.WriteLine($"[DEBUG] MesasService.LiberarMesaAsync <- Success={response.Success}, Message={response.Message}");
        return response;
    }

    /// <summary>
    /// Cambia el estado de una mesa
    /// </summary>
    public async Task<ApiResponse<MesaDto>> CambiarEstadoMesaAsync(
        Guid mesaId, 
        string nuevoEstado, 
        string? motivo = null,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ApiResponse<MesaDto>.ErrorResponse("Operación cancelada por el usuario");
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
        string? ubicacionPreferida = null,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ApiResponse<MesaDto>.ErrorResponse("Operación cancelada por el usuario");
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