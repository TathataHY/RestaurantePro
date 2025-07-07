using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using System.Text.Json;

namespace RestaurantePro.Mobile.Core.Services.Comandas;

/// <summary>
/// Servicio para gestión de comandas - Operaciones críticas
/// </summary>
public class ComandasService : IComandasService
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;
    private const string BaseEndpoint = "api/comandas";

    public ComandasService(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
    }

    /// <summary>
    /// Obtener todas las comandas activas
    /// </summary>
    public async Task<ApiResponse<List<ComandaDto>>> ObtenerComandasActivasAsync()
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            return await _apiService.GetAsync<List<ComandaDto>>($"{BaseEndpoint}/activas", token);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ComandaDto>>.ErrorResponse("Error al obtener comandas activas", "Error al obtener comandas activas");
        }
    }

    /// <summary>
    /// Obtener comandas por mesa específica
    /// </summary>
    public async Task<ApiResponse<List<ComandaDto>>> ObtenerComandasPorMesaAsync(Guid mesaId)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            return await _apiService.GetAsync<List<ComandaDto>>($"{BaseEndpoint}/mesa/{mesaId}", token);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ComandaDto>>.ErrorResponse($"Error al obtener comandas por mesa: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtener una comanda específica por ID
    /// </summary>
    public async Task<ApiResponse<ComandaDto>> ObtenerComandaPorIdAsync(Guid comandaId)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            return await _apiService.GetAsync<ComandaDto>($"{BaseEndpoint}/{comandaId}", token);
        }
        catch (Exception ex)
        {
            return ApiResponse<ComandaDto>.ErrorResponse($"Error al obtener comanda: {ex.Message}");
        }
    }

    /// <summary>
    /// Crear una nueva comanda
    /// </summary>
    public async Task<ApiResponse<ComandaDto>> CrearComandaAsync(CrearComandaRequest request)
    {
        try
        {
            if (request.MesaId == Guid.Empty)
            {
                return ApiResponse<ComandaDto>.ErrorResponse("La mesa es requerida", "La mesa es requerida");
            }
            var token = await _authService.GetTokenAsync();
            return await _apiService.PostAsync<ComandaDto>(BaseEndpoint, request, token);
        }
        catch (Exception ex)
        {
            return ApiResponse<ComandaDto>.ErrorResponse($"Error al crear comanda: {ex.Message}");
        }
    }

    /// <summary>
    /// Agregar productos a una comanda existente
    /// </summary>
    public async Task<ApiResponse<ComandaDto>> AgregarProductosAsync(Guid comandaId, List<ComandaProductoRequest> productos)
    {
        try
        {
            if (comandaId == Guid.Empty)
            {
                return ApiResponse<ComandaDto>.ErrorResponse("ID de comanda es requerido", "ID de comanda es requerido");
            }

            if (productos == null || productos.Count == 0)
            {
                return ApiResponse<ComandaDto>.ErrorResponse("Se requiere al menos un producto", "Se requiere al menos un producto");
            }
            var token = await _authService.GetTokenAsync();
            return await _apiService.PostAsync<ComandaDto>($"{BaseEndpoint}/{comandaId}/productos", productos, token);
        }
        catch (Exception ex)
        {
            return ApiResponse<ComandaDto>.ErrorResponse($"Error al agregar productos: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualizar cantidad de un producto en la comanda
    /// </summary>
    public async Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid productoId, int nuevaCantidad)
    {
        try
        {
            if (comandaId == Guid.Empty || productoId == Guid.Empty)
            {
                return ApiResponse<ComandaDto>.ErrorResponse("ID de comanda y producto son requeridos", "ID de comanda y producto son requeridos");
            }

            if (nuevaCantidad <= 0)
            {
                return ApiResponse<ComandaDto>.ErrorResponse("La cantidad debe ser mayor a 0", "La cantidad debe ser mayor a 0");
            }

            var request = new { ProductoId = productoId, Cantidad = nuevaCantidad };
            var token = await _authService.GetTokenAsync();
            return await _apiService.PutAsync<ComandaDto>($"{BaseEndpoint}/{comandaId}/productos/{productoId}/cantidad", request, token);
        }
        catch (Exception ex)
        {
            return ApiResponse<ComandaDto>.ErrorResponse($"Error al actualizar cantidad: {ex.Message}");
        }
    }

    /// <summary>
    /// Remover producto de la comanda
    /// </summary>
    public async Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid productoId)
    {
        try
        {
            if (comandaId == Guid.Empty || productoId == Guid.Empty)
            {
                return ApiResponse<ComandaDto>.ErrorResponse("ID de comanda y producto son requeridos", "ID de comanda y producto son requeridos");
            }
            var token = await _authService.GetTokenAsync();
            await _apiService.DeleteAsync($"{BaseEndpoint}/{comandaId}/productos/{productoId}", token);
            return ApiResponse<ComandaDto>.SuccessResponse(null!, "Producto removido exitosamente");
        }
        catch (Exception ex)
        {
            return ApiResponse<ComandaDto>.ErrorResponse($"Error al remover producto: {ex.Message}");
        }
    }

    /// <summary>
    /// Cambiar estado de la comanda
    /// </summary>
    public async Task<ApiResponse<ComandaDto>> CambiarEstadoComandaAsync(Guid comandaId, string nuevoEstado, string? observaciones = null)
    {
        try
        {
            if (comandaId == Guid.Empty)
            {
                return ApiResponse<ComandaDto>.ErrorResponse("ID de comanda es requerido", "ID de comanda es requerido");
            }

            if (string.IsNullOrWhiteSpace(nuevoEstado))
            {
                return ApiResponse<ComandaDto>.ErrorResponse("El nuevo estado es requerido", "El nuevo estado es requerido");
            }

            var request = new { Estado = nuevoEstado, Observaciones = observaciones };
            var token = await _authService.GetTokenAsync();
            return await _apiService.PutAsync<ComandaDto>($"{BaseEndpoint}/{comandaId}/estado", request, token);
        }
        catch (Exception ex)
        {
            return ApiResponse<ComandaDto>.ErrorResponse($"Error al cambiar estado: {ex.Message}");
        }
    }

    /// <summary>
    /// Finalizar comanda (cerrarla)
    /// </summary>
    public async Task<ApiResponse<ComandaDto>> FinalizarComandaAsync(Guid comandaId, string metodoPago, string? observaciones = null)
    {
        try
        {
            if (comandaId == Guid.Empty)
            {
                return ApiResponse<ComandaDto>.ErrorResponse("ID de comanda es requerido", "ID de comanda es requerido");
            }

            if (string.IsNullOrWhiteSpace(metodoPago))
            {
                return ApiResponse<ComandaDto>.ErrorResponse("El método de pago es requerido", "El método de pago es requerido");
            }

            var request = new { MetodoPago = metodoPago, Observaciones = observaciones };
            var token = await _authService.GetTokenAsync();
            return await _apiService.PostAsync<ComandaDto>($"{BaseEndpoint}/{comandaId}/finalizar", request, token);
        }
        catch (Exception ex)
        {
            return ApiResponse<ComandaDto>.ErrorResponse($"Error al finalizar comanda: {ex.Message}");
        }
    }

    /// <summary>
    /// Cancelar comanda
    /// </summary>
    public async Task<ApiResponse<ComandaDto>> CancelarComandaAsync(Guid comandaId, string motivo)
    {
        try
        {
            if (comandaId == Guid.Empty)
            {
                return ApiResponse<ComandaDto>.ErrorResponse("ID de comanda es requerido", "ID de comanda es requerido");
            }

            if (string.IsNullOrWhiteSpace(motivo))
            {
                return ApiResponse<ComandaDto>.ErrorResponse("El motivo de cancelación es requerido", "El motivo de cancelación es requerido");
            }

            var request = new { Motivo = motivo };
            var token = await _authService.GetTokenAsync();
            return await _apiService.PostAsync<ComandaDto>($"{BaseEndpoint}/{comandaId}/cancelar", request, token);
        }
        catch (Exception ex)
        {
            return ApiResponse<ComandaDto>.ErrorResponse($"Error al cancelar comanda: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtener estadísticas de comandas
    /// </summary>
    public async Task<ApiResponse<EstadisticasComandasDto>> ObtenerEstadisticasAsync()
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            return await _apiService.GetAsync<EstadisticasComandasDto>($"{BaseEndpoint}/estadisticas", token);
        }
        catch (Exception ex)
        {
            return ApiResponse<EstadisticasComandasDto>.ErrorResponse($"Error al obtener estadísticas: {ex.Message}");
        }
    }

    /// <summary>
    /// Búsqueda de comandas con filtros
    /// </summary>
    public async Task<ApiResponse<List<ComandaDto>>> BuscarComandasAsync(
        string? estado = null,
        Guid? mesaId = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        string? clienteNombre = null)
    {
        try
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(estado))
                queryParams.Add($"estado={Uri.EscapeDataString(estado)}");

            if (mesaId.HasValue && mesaId.Value != Guid.Empty)
                queryParams.Add($"mesaId={mesaId.Value}");

            if (fechaDesde.HasValue)
                queryParams.Add($"fechaDesde={fechaDesde.Value:yyyy-MM-dd}");

            if (fechaHasta.HasValue)
                queryParams.Add($"fechaHasta={fechaHasta.Value:yyyy-MM-dd}");

            if (!string.IsNullOrWhiteSpace(clienteNombre))
                queryParams.Add($"clienteNombre={Uri.EscapeDataString(clienteNombre)}");

            var queryString = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : string.Empty;
            var token = await _authService.GetTokenAsync();
            return await _apiService.GetAsync<List<ComandaDto>>($"{BaseEndpoint}/buscar{queryString}", token);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ComandaDto>>.ErrorResponse($"Error al buscar comandas: {ex.Message}");
        }
    }
} 