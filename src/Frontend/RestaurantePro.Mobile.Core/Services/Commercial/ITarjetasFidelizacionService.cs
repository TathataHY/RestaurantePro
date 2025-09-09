using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Commercial;

public interface ITarjetasFidelizacionService
{
    Task<ApiResponse<TarjetaFidelizacionDto>> BuscarTarjetaAsync(string numeroTarjeta, CancellationToken cancellationToken = default);
    Task<ApiResponse<TarjetaFidelizacionDto>> ActivarTarjetaAsync(string numeroTarjeta, string nombreCliente, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtener tarjeta por código
    /// </summary>
    /// <param name="codigo">Código de la tarjeta</param>
    /// <returns>Tarjeta de fidelización</returns>
    Task<ApiResponse<TarjetaFidelizacionDto>> ObtenerTarjetaPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtener tarjeta por ID
    /// </summary>
    /// <param name="id">ID de la tarjeta</param>
    /// <returns>Tarjeta de fidelización</returns>
    Task<ApiResponse<TarjetaFidelizacionDto>> ObtenerTarjetaAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<TransaccionPuntosDto>>> ObtenerHistorialTransaccionesAsync(Guid tarjetaId, CancellationToken cancellationToken = default);
    Task<ApiResponse<TarjetaFidelizacionDto>> AcumularPuntosAsync(Guid tarjetaId, decimal montoCompra, CancellationToken cancellationToken = default);
    Task<ApiResponse<TarjetaFidelizacionDto>> CanjearPuntosAsync(Guid tarjetaId, int puntosACanjear, decimal descuento, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<TarjetaFidelizacionDto>>> ObtenerTarjetasActivasAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DesactivarTarjetaAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Métodos adicionales para ViewModels
    Task<ApiResponse<List<TarjetaFidelizacionDto>>> ObtenerTarjetasAsync(FiltroTarjetasFidelizacionDto filtro, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<TransaccionPuntosDto>>> ObtenerHistorialAsync(Guid tarjetaId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> BloquearTarjetaAsync(Guid tarjetaId, CancellationToken cancellationToken = default);
    Task<ApiResponse<HistorialPuntosDto>> ObtenerHistorialPuntosAsync(Guid tarjetaId, CancellationToken cancellationToken = default);
    Task<ApiResponse<EstadisticasTarjetaDto>> ObtenerEstadisticasAsync(Guid tarjetaId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> EliminarTarjetaAsync(Guid tarjetaId, CancellationToken cancellationToken = default);
} 