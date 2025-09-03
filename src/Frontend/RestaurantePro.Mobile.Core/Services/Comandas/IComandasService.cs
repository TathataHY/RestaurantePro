using RestaurantePro.Mobile.Core.Models.DTOs;
using ComandaModels = RestaurantePro.Mobile.Core.Features.Operations.Comandas.Models;

namespace RestaurantePro.Mobile.Core.Services.Comandas;

/// <summary>
/// Servicio para gestión de comandas y órdenes del restaurante
/// </summary>
public interface IComandasService
{
    /// <summary>
    /// Obtener todas las comandas activas
    /// </summary>
    Task<ApiResponse<List<ComandaDto>>> ObtenerComandasActivasAsync();

    /// <summary>
    /// Obtener comandas por mesa específica
    /// </summary>
    Task<ApiResponse<List<ComandaDto>>> ObtenerComandasPorMesaAsync(Guid mesaId);

    /// <summary>
    /// Obtener una comanda específica por ID
    /// </summary>
    Task<ApiResponse<ComandaDto>> ObtenerComandaPorIdAsync(Guid comandaId);

    /// <summary>
    /// Crear una nueva comanda
    /// </summary>
    Task<ApiResponse<ComandaDto>> CrearComandaAsync(ComandaModels.CrearComandaRequest request);

    /// <summary>
    /// Agregar productos a una comanda existente
    /// </summary>
    Task<ApiResponse<ComandaDto>> AgregarProductosAsync(Guid comandaId, List<ComandaProductoRequest> productos);

    /// <summary>
    /// Actualizar cantidad de un producto en la comanda
    /// </summary>
    Task<ApiResponse<ComandaDto>> ActualizarCantidadProductoAsync(Guid comandaId, Guid productoId, int nuevaCantidad);

    /// <summary>
    /// Remover producto de la comanda
    /// </summary>
    Task<ApiResponse<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid productoId);

    /// <summary>
    /// Cambiar estado de la comanda
    /// </summary>
    Task<ApiResponse<ComandaDto>> CambiarEstadoComandaAsync(Guid comandaId, string nuevoEstado, string? observaciones = null);

    /// <summary>
    /// Finalizar comanda (cerrarla)
    /// </summary>
    Task<ApiResponse<ComandaDto>> FinalizarComandaAsync(Guid comandaId, string metodoPago, string? observaciones = null);

    /// <summary>
    /// Cancelar comanda
    /// </summary>
    Task<ApiResponse<ComandaDto>> CancelarComandaAsync(Guid comandaId, string motivo);

    /// <summary>
    /// Obtener estadísticas de comandas
    /// </summary>
    Task<ApiResponse<EstadisticasComandasDto>> ObtenerEstadisticasAsync();

    /// <summary>
    /// Búsqueda de comandas con filtros
    /// </summary>
    Task<ApiResponse<List<ComandaDto>>> BuscarComandasAsync(
        string? estado = null,
        Guid? mesaId = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        string? clienteNombre = null);
} 