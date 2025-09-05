using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using ComandaModels = RestaurantePro.Mobile.Core.Features.Operations.Comandas.Models;
using System.Text.Json;

namespace RestaurantePro.Mobile.Core.Services.Comandas;

/// <summary>
/// Servicio para gestión de comandas - Operaciones críticas
/// </summary>
public class ComandasService : IComandasService
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;
    private const string BaseEndpoint = "api/operaciones/comandas";

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
            // Usar el endpoint principal con parámetros para obtener comandas activas
            var queryParams = "pageNumber=1&pageSize=100&soloActivas=true&incluirItems=true";
            var result = await _apiService.GetAsync<PaginatedList<ComandaDto>>($"{BaseEndpoint}?{queryParams}", token);
            
            if (result.Success && result.Data != null)
            {
                return ApiResponse<List<ComandaDto>>.SuccessResponse(result.Data.Items, result.Message);
            }
            else
            {
                return ApiResponse<List<ComandaDto>>.ErrorResponse(result.Message ?? "Error al obtener comandas activas");
            }
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ComandaDto>>.ErrorResponse($"Error al obtener comandas activas: {ex.Message}");
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
            // Usar el endpoint principal con parámetros de consulta
            var queryParams = $"mesaId={mesaId}&soloActivas=true&pageSize=100&incluirItems=true";
            var result = await _apiService.GetAsync<PaginatedList<ComandaDto>>($"{BaseEndpoint}?{queryParams}", token);
            
            if (result.Success && result.Data != null)
            {
                return ApiResponse<List<ComandaDto>>.SuccessResponse(result.Data.Items, result.Message);
            }
            else
            {
                return ApiResponse<List<ComandaDto>>.ErrorResponse(result.Message ?? "Error al obtener comandas por mesa");
            }
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
            // Forzamos incluirItems=true para asegurarnos de traer nombres de productos
            return await _apiService.GetAsync<ComandaDto>($"{BaseEndpoint}/{comandaId}?incluirItems=true", token);
        }
        catch (Exception ex)
        {
            return ApiResponse<ComandaDto>.ErrorResponse($"Error al obtener comanda: {ex.Message}");
        }
    }

    /// <summary>
    /// Crear una nueva comanda
    /// </summary>
    public async Task<ApiResponse<ComandaDto>> CrearComandaAsync(ComandaModels.CrearComandaRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.MesaId))
            {
                return ApiResponse<ComandaDto>.ErrorResponse("La mesa es requerida", "La mesa es requerida");
            }
            
            if (request.ProductosIniciales == null || request.ProductosIniciales.Count == 0)
            {
                return ApiResponse<ComandaDto>.ErrorResponse("Se requiere al menos un producto", "Se requiere al menos un producto");
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
            ApiResponse<ComandaDto>? lastResponse = null;

            foreach (var p in productos)
            {
                var payload = new { ProductoId = p.ProductoId, Cantidad = p.Cantidad, Observaciones = p.Observaciones };
                var resp = await _apiService.PostAsync<ComandaDto>($"{BaseEndpoint}/{comandaId}/productos", payload, token);
                if (!resp.Success)
                {
                    return resp;
                }
                lastResponse = resp;
            }

            return lastResponse ?? ApiResponse<ComandaDto>.ErrorResponse("No se pudo agregar ningún producto");
        }
        catch (Exception ex)
        {
            return ApiResponse<ComandaDto>.ErrorResponse($"Error al agregar productos: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualizar cantidad de un producto en la comanda
    /// </summary>
    public async Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid itemId, int nuevaCantidad)
    {
        try
        {
            if (comandaId == Guid.Empty || itemId == Guid.Empty)
            {
                return ApiResponse<ComandaDto>.ErrorResponse("ID de comanda y producto son requeridos", "ID de comanda y producto son requeridos");
            }

            var request = new { NuevaCantidad = nuevaCantidad };
            var token = await _authService.GetTokenAsync();
            return await _apiService.PutAsync<ComandaDto>($"{BaseEndpoint}/{comandaId}/productos/{itemId}/cantidad", request, token);
        }
        catch (Exception ex)
        {
            return ApiResponse<ComandaDto>.ErrorResponse($"Error al actualizar cantidad: {ex.Message}");
        }
    }

    /// <summary>
    /// Remover producto de la comanda
    /// </summary>
    public async Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid itemId)
    {
        try
        {
            if (comandaId == Guid.Empty || itemId == Guid.Empty)
            {
                return ApiResponse<ComandaDto>.ErrorResponse("ID de comanda y producto son requeridos", "ID de comanda y producto son requeridos");
            }
            var token = await _authService.GetTokenAsync();
            await _apiService.DeleteAsync($"{BaseEndpoint}/{comandaId}/productos/{itemId}", token);
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

            var request = new { NuevoEstado = nuevoEstado, Observaciones = observaciones };
            var token = await _authService.GetTokenAsync();
            return await _apiService.PatchAsync<ComandaDto>($"{BaseEndpoint}/{comandaId}/estado", request, token);
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

            // Backend exige UsuarioId. Lo obtenemos del JWT.
            var userIdStr = await _authService.GetUserIdAsync();
            if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId) || userId == Guid.Empty)
            {
                return ApiResponse<ComandaDto>.ErrorResponse("No se pudo identificar al usuario (UserId)", "UsuarioId requerido");
            }

            var request = new {
                UsuarioId = userId,
                ObservacionesFinalizacion = observaciones,
                ValidarTodosItemsListos = true,
                NotificarMesero = true
            };
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
            // Por ahora, calculamos las estadísticas basándonos en las comandas existentes
            var response = await BuscarComandasAsync();
            
            if (!response.Success || response.Data == null)
            {
                return ApiResponse<EstadisticasComandasDto>.ErrorResponse("Error al obtener comandas para calcular estadísticas");
            }

            var comandas = response.Data;
            var estadisticas = new EstadisticasComandasDto
            {
                TotalComandasActivas = comandas.Count(c => c.EstaActiva),
                ComandasPendientes = comandas.Count(c => c.Estado.ToLowerInvariant() == "pendiente"),
                ComandasEnPreparacion = comandas.Count(c => c.Estado.ToLowerInvariant() == "en_preparacion"),
                ComandasListas = comandas.Count(c => c.Estado.ToLowerInvariant() == "lista"),
                ComandasCompletadasHoy = comandas.Count(c => c.Estado.ToLowerInvariant() == "finalizada" && c.FechaCreacion.Date == DateTime.Today),
                ComandasCanceladasHoy = comandas.Count(c => c.Estado.ToLowerInvariant() == "cancelada" && c.FechaCreacion.Date == DateTime.Today),
                TiempoPromedioPreparacion = 0, // No tenemos esta información en el DTO actual
                VentasTotalDia = comandas.Where(c => c.Estado.ToLowerInvariant() == "finalizada" && c.FechaCreacion.Date == DateTime.Today).Sum(c => c.Total),
                ValorPromedioPorComanda = comandas.Where(c => c.Estado.ToLowerInvariant() == "finalizada").Any() 
                    ? comandas.Where(c => c.Estado.ToLowerInvariant() == "finalizada").Average(c => c.Total)
                    : 0
            };

            return ApiResponse<EstadisticasComandasDto>.SuccessResponse(estadisticas, "Estadísticas calculadas exitosamente");
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

            // Agregar parámetros de paginación por defecto
            queryParams.Add("pageNumber=1");
            queryParams.Add("pageSize=100");
            // Pedir también items para rellenar la sección de Items en la lista
            queryParams.Add("incluirItems=true");

            var queryString = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : string.Empty;
            var token = await _authService.GetTokenAsync();
            
            // El endpoint devuelve una PaginatedList, necesitamos extraer los items
            var response = await _apiService.GetAsync<PaginatedList<ComandaDto>>($"{BaseEndpoint}{queryString}", token);
            
            if (response.Success && response.Data != null)
            {
                return ApiResponse<List<ComandaDto>>.SuccessResponse(response.Data.Items, response.Message);
            }
            
            return ApiResponse<List<ComandaDto>>.ErrorResponse(response.Message ?? "Error al buscar comandas");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ComandaDto>>.ErrorResponse($"Error al buscar comandas: {ex.Message}");
        }
    }
} 