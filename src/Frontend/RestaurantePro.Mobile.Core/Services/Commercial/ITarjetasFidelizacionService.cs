using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Commercial;

public interface ITarjetasFidelizacionService
{
    Task<ApiResponse<TarjetaFidelizacionDto>> BuscarTarjetaAsync(string numeroTarjeta);
    Task<ApiResponse<TarjetaFidelizacionDto>> ActivarTarjetaAsync(string numeroTarjeta, string nombreCliente);
    /// <summary>
    /// Obtener tarjeta por código
    /// </summary>
    /// <param name="codigo">Código de la tarjeta</param>
    /// <returns>Tarjeta de fidelización</returns>
    Task<ApiResponse<TarjetaFidelizacionDto>> ObtenerTarjetaPorCodigoAsync(string codigo);

    /// <summary>
    /// Obtener tarjeta por ID
    /// </summary>
    /// <param name="id">ID de la tarjeta</param>
    /// <returns>Tarjeta de fidelización</returns>
    Task<ApiResponse<TarjetaFidelizacionDto>> ObtenerTarjetaAsync(Guid id);
    Task<ApiResponse<List<TransaccionPuntosDto>>> ObtenerHistorialTransaccionesAsync(Guid tarjetaId);
    Task<ApiResponse<TarjetaFidelizacionDto>> AcumularPuntosAsync(Guid tarjetaId, decimal montoCompra);
    Task<ApiResponse<TarjetaFidelizacionDto>> CanjearPuntosAsync(Guid tarjetaId, int puntosACanjear, decimal descuento);
    Task<ApiResponse<List<TarjetaFidelizacionDto>>> ObtenerTarjetasActivasAsync();
    Task<ApiResponse<bool>> DesactivarTarjetaAsync(Guid id);
    
    // Métodos adicionales para ViewModels
    Task<ApiResponse<List<TarjetaFidelizacionDto>>> ObtenerTarjetasAsync(FiltroTarjetasFidelizacionDto filtro);
    Task<ApiResponse<List<TransaccionPuntosDto>>> ObtenerHistorialAsync(Guid tarjetaId);
    Task<ApiResponse<bool>> BloquearTarjetaAsync(Guid tarjetaId);
    Task<ApiResponse<HistorialPuntosDto>> ObtenerHistorialPuntosAsync(Guid tarjetaId);
    Task<ApiResponse<EstadisticasTarjetaDto>> ObtenerEstadisticasAsync(Guid tarjetaId);
    Task<ApiResponse<bool>> EliminarTarjetaAsync(Guid tarjetaId);
} 