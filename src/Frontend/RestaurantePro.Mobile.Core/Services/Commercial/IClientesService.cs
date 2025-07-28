using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Commercial;

public interface IClientesService
{
    Task<ApiResponse<List<ClienteSummaryDto>>> ObtenerClientesAsync(bool soloActivos = true);
    Task<ApiResponse<ClienteDto>> ObtenerClienteAsync(Guid id);
    Task<ApiResponse<List<ClienteSummaryDto>>> BuscarClientesAsync(string terminoBusqueda);
    Task<ApiResponse<ClienteDto>> CrearClienteAsync(ClienteDto cliente);
    Task<ApiResponse<ClienteDto>> ActualizarClienteAsync(Guid id, ClienteDto cliente);
    Task<ApiResponse<bool>> EliminarClienteAsync(Guid id);
    Task<ApiResponse<EstadisticasClientesDto>> ObtenerEstadisticasAsync();
    Task<ApiResponse<List<ClienteSummaryDto>>> ObtenerClientesFrecuentesAsync(int cantidad = 10);
    
    // Métodos adicionales para ViewModels
    Task<ApiResponse<List<ClienteSummaryDto>>> ObtenerClientesAsync(FiltroClientesDto filtro);
    Task<ApiResponse<List<ClienteSummaryDto>>> ObtenerClientesPorSegmentoAsync(string segmento);
    Task<ApiResponse<List<ClienteSummaryDto>>> ObtenerClientesConTarjetaFidelizacionAsync();
    Task<ApiResponse<List<ClienteSummaryDto>>> ObtenerClientesPorFechaRegistroAsync(DateTime fechaDesde, DateTime fechaHasta);
    Task<ApiResponse<bool>> DesactivarClienteAsync(Guid clienteId);
    /// <summary>
    /// Obtener historial de comandas de un cliente
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <returns>Lista de comandas del cliente</returns>
    Task<ApiResponse<List<ComandaDto>>> ObtenerHistorialComandasAsync(Guid clienteId);
} 