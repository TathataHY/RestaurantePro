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
	public async Task<ApiResponse<List<ComandaDto>>> ObtenerComandasActivasAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			if (cancellationToken.IsCancellationRequested)
				return ApiResponse<List<ComandaDto>>.ErrorResponse("Operación cancelada por el usuario");
			var token = await _authService.GetTokenAsync();
			var queryParams = "pageNumber=1&pageSize=12&soloActivas=true&incluirItems=true";
			var result = await _apiService.GetAsync<PaginatedList<ComandaDto>>($"{BaseEndpoint}?{queryParams}", token);
			if (result.Success && result.Data != null)
			{
				return ApiResponse<List<ComandaDto>>.SuccessResponse(result.Data.Items, result.Message);
			}
			else
			{
				var msg = result.Message ?? "Error al obtener comandas activas";
				return ApiResponse<List<ComandaDto>>.ErrorResponse("Error en la operación", "Error en la operación");
			}
		}
		catch (Exception ex)
		{
			return ApiResponse<List<ComandaDto>>.ErrorResponse($"Error al obtener comandas activas: {ex.Message}");
		}
	}

	public async Task<ApiResponse<List<ComandaDto>>> ObtenerComandasPorMesaAsync(Guid mesaId, CancellationToken cancellationToken = default)
	{
		try
		{
			if (cancellationToken.IsCancellationRequested)
				return ApiResponse<List<ComandaDto>>.ErrorResponse("Operación cancelada por el usuario");
			var token = await _authService.GetTokenAsync();
			var queryParams = $"mesaId={mesaId}&soloActivas=true&pageSize=12&incluirItems=true";
			var result = await _apiService.GetAsync<PaginatedList<ComandaDto>>($"{BaseEndpoint}?{queryParams}", token);
			if (result.Success && result.Data != null)
			{
				return ApiResponse<List<ComandaDto>>.SuccessResponse(result.Data.Items, result.Message);
			}
			else
			{
				var msg = result.Message ?? "Error al obtener comandas por mesa";
				return ApiResponse<List<ComandaDto>>.ErrorResponse(msg, msg);
			}
		}
		catch (Exception ex)
		{
			return ApiResponse<List<ComandaDto>>.ErrorResponse($"Error al obtener comandas por mesa: {ex.Message}");
		}
	}

	public async Task<ApiResponse<ComandaDto>> ObtenerComandaPorIdAsync(Guid comandaId, CancellationToken cancellationToken = default)
	{
		try
		{
			if (cancellationToken.IsCancellationRequested)
				return ApiResponse<ComandaDto>.ErrorResponse("Operación cancelada por el usuario");
			var token = await _authService.GetTokenAsync();
			return await _apiService.GetAsync<ComandaDto>($"{BaseEndpoint}/{comandaId}?incluirItems=true", token);
		}
		catch (Exception ex)
		{
			return ApiResponse<ComandaDto>.ErrorResponse($"Error al obtener comanda: {ex.Message}");
		}
	}

	public async Task<ApiResponse<ComandaDto>> CrearComandaAsync(ComandaModels.CrearComandaRequest request, CancellationToken cancellationToken = default)
	{
		try
		{
			if (cancellationToken.IsCancellationRequested)
				return ApiResponse<ComandaDto>.ErrorResponse("Operación cancelada por el usuario");
			// Validar mesa sólo si el tipo es Mesa (por defecto si no se envía)
			var tipo = (request.Tipo ?? "Mesa").Trim();
			var requiereMesa = string.Equals(tipo, "Mesa", StringComparison.OrdinalIgnoreCase);
			if (requiereMesa && string.IsNullOrEmpty(request.MesaId))
			{
				return ApiResponse<ComandaDto>.ErrorResponse("La mesa es requerida para comandas de tipo Mesa", "La mesa es requerida");
			}
			if (request.ProductosIniciales == null || request.ProductosIniciales.Count == 0)
			{
				return ApiResponse<ComandaDto>.ErrorResponse("Se requiere al menos un producto", "Se requiere al menos un producto");
			}
			if (string.IsNullOrWhiteSpace(request.MeseroId))
			{
				var userIdStr = await _authService.GetUserIdAsync();
				if (string.IsNullOrWhiteSpace(userIdStr))
				{
					return ApiResponse<ComandaDto>.ErrorResponse("No autenticado", "Debe iniciar sesión para crear una comanda");
				}
				request.MeseroId = userIdStr;
			}
			var token = await _authService.GetTokenAsync();
			return await _apiService.PostAsync<ComandaDto>(BaseEndpoint, request, token);
		}
		catch (Exception ex)
		{
			return ApiResponse<ComandaDto>.ErrorResponse($"Error al crear comanda: {ex.Message}");
		}
	}

	public async Task<ApiResponse<ComandaDto>> AgregarProductosAsync(Guid comandaId, List<ComandaProductoRequest> productos, CancellationToken cancellationToken = default)
	{
		try
		{
			if (cancellationToken.IsCancellationRequested)
				return ApiResponse<ComandaDto>.ErrorResponse("Operación cancelada por el usuario");
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
				if (cancellationToken.IsCancellationRequested)
					return ApiResponse<ComandaDto>.ErrorResponse("Operación cancelada por el usuario");
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

	public async Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid itemId, int nuevaCantidad, CancellationToken cancellationToken = default)
	{
		try
		{
			if (cancellationToken.IsCancellationRequested)
				return ApiResponse<ComandaDto>.ErrorResponse("Operación cancelada por el usuario");
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

	public async Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid itemId, CancellationToken cancellationToken = default)
	{
		try
		{
			if (cancellationToken.IsCancellationRequested)
				return ApiResponse<ComandaDto>.ErrorResponse("Operación cancelada por el usuario");
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

	public async Task<ApiResponse<ComandaDto>> CambiarEstadoComandaAsync(Guid comandaId, string nuevoEstado, string? observaciones = null, CancellationToken cancellationToken = default)
	{
		try
		{
			if (cancellationToken.IsCancellationRequested)
				return ApiResponse<ComandaDto>.ErrorResponse("Operación cancelada por el usuario");
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

	public async Task<ApiResponse<ComandaDto>> FinalizarComandaAsync(Guid comandaId, string metodoPago, string? observaciones = null, CancellationToken cancellationToken = default)
	{
		try
		{
			if (cancellationToken.IsCancellationRequested)
				return ApiResponse<ComandaDto>.ErrorResponse("Operación cancelada por el usuario");
			if (comandaId == Guid.Empty)
			{
				return ApiResponse<ComandaDto>.ErrorResponse("ID de comanda es requerido", "ID de comanda es requerido");
			}
			if (string.IsNullOrWhiteSpace(metodoPago))
			{
				return ApiResponse<ComandaDto>.ErrorResponse("El método de pago es requerido", "El método de pago es requerido");
			}
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

	public async Task<ApiResponse<ComandaDto>> CancelarComandaAsync(Guid comandaId, string motivo, CancellationToken cancellationToken = default)
	{
		try
		{
			if (cancellationToken.IsCancellationRequested)
				return ApiResponse<ComandaDto>.ErrorResponse("Operación cancelada por el usuario");
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

	public async Task<ApiResponse<EstadisticasComandasDto>> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			if (cancellationToken.IsCancellationRequested)
				return ApiResponse<EstadisticasComandasDto>.ErrorResponse("Operación cancelada por el usuario");
			var response = await BuscarComandasAsync(cancellationToken: cancellationToken);
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
				TiempoPromedioPreparacion = 0,
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

	public async Task<ApiResponse<List<ComandaDto>>> BuscarComandasAsync(
		string? estado = null,
		Guid? mesaId = null,
		DateTime? fechaDesde = null,
		DateTime? fechaHasta = null,
		string? clienteNombre = null,
		CancellationToken cancellationToken = default)
	{
		try
		{
			if (cancellationToken.IsCancellationRequested)
				return ApiResponse<List<ComandaDto>>.ErrorResponse("Operación cancelada por el usuario");
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
			queryParams.Add("pageNumber=1");
			queryParams.Add("pageSize=12");
			queryParams.Add("incluirItems=true");
			var queryString = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : string.Empty;
			var token = await _authService.GetTokenAsync();
			
			// Si no hay token, devolver error de autenticación
			if (string.IsNullOrEmpty(token))
			{
				return ApiResponse<List<ComandaDto>>.ErrorResponse("No se pudo obtener el token de autenticación", "Error de autenticación");
			}
			
			var response = await _apiService.GetAsync<PaginatedList<ComandaDto>>($"{BaseEndpoint}{queryString}", token);
			if (response.Success && response.Data != null)
			{
				return ApiResponse<List<ComandaDto>>.SuccessResponse(response.Data.Items, response.Message);
			}
			var msg = response.Message ?? "Error al buscar comandas";
			return ApiResponse<List<ComandaDto>>.ErrorResponse(msg, msg);
		}
		catch (Exception ex)
		{
			return ApiResponse<List<ComandaDto>>.ErrorResponse($"Error al buscar comandas: {ex.Message}");
		}
	}
} 