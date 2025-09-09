using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Commercial;

public interface IClientesService
{
    Task<ApiResponse<List<ClienteSummaryDto>>> ObtenerClientesAsync(bool soloActivos = true, CancellationToken cancellationToken = default);
    Task<ApiResponse<ClienteDto>> ObtenerClienteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<ClienteSummaryDto>>> BuscarClientesAsync(string terminoBusqueda, CancellationToken cancellationToken = default);
    Task<ApiResponse<ClienteDto>> CrearClienteAsync(ClienteDto cliente, CancellationToken cancellationToken = default);
    Task<ApiResponse<ClienteDto>> ActualizarClienteAsync(Guid id, ClienteDto cliente, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> EliminarClienteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<EstadisticasClientesDto>> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<List<ClienteSummaryDto>>> ObtenerClientesFrecuentesAsync(int cantidad = 10, CancellationToken cancellationToken = default);
    
    // Métodos adicionales para ViewModels
    Task<ApiResponse<List<ClienteSummaryDto>>> ObtenerClientesAsync(FiltroClientesDto filtro, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<ClienteSummaryDto>>> ObtenerClientesPorSegmentoAsync(string segmento, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<ClienteSummaryDto>>> ObtenerClientesConTarjetaFidelizacionAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<List<ClienteSummaryDto>>> ObtenerClientesPorFechaRegistroAsync(DateTime fechaDesde, DateTime fechaHasta, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DesactivarClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtener historial de comandas de un cliente
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <returns>Lista de comandas del cliente</returns>
    Task<ApiResponse<List<ComandaDto>>> ObtenerHistorialComandasAsync(Guid clienteId, CancellationToken cancellationToken = default);
}