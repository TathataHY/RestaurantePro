using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Commercial;

/// <summary>
/// Interfaz para el servicio de facturación
/// </summary>
public interface IFacturasService
{
    /// <summary>
    /// Obtener facturas por fecha
    /// </summary>
    /// <param name="fecha">Fecha de las facturas</param>
    /// <returns>Lista de facturas</returns>
    Task<ApiResponse<List<FacturaDto>>> ObtenerFacturasAsync(DateTime fecha);

    /// <summary>
    /// Buscar facturas por criterios
    /// </summary>
    /// <param name="busqueda">Término de búsqueda</param>
    /// <param name="fecha">Fecha de las facturas</param>
    /// <returns>Lista de facturas filtradas</returns>
    Task<ApiResponse<List<FacturaDto>>> BuscarFacturasAsync(string busqueda, DateTime fecha);

    /// <summary>
    /// Obtener estadísticas de facturas
    /// </summary>
    /// <param name="fecha">Fecha para las estadísticas</param>
    /// <returns>Estadísticas de facturas</returns>
    Task<ApiResponse<EstadisticasFacturasDto>> ObtenerEstadisticasAsync(DateTime fecha);

    /// <summary>
    /// Imprimir factura
    /// </summary>
    /// <param name="facturaId">ID de la factura</param>
    /// <returns>Resultado de la impresión</returns>
    Task<ApiResponse<bool>> ImprimirFacturaAsync(Guid facturaId);

    /// <summary>
    /// Obtener facturas pendientes
    /// </summary>
    /// <returns>Lista de facturas pendientes</returns>
    Task<ApiResponse<List<FacturaDto>>> ObtenerFacturasPendientesAsync();

    /// <summary>
    /// Registrar pago de factura
    /// </summary>
    /// <param name="facturaId">ID de la factura</param>
    /// <param name="pagoDto">Datos del pago</param>
    /// <returns>Resultado del registro</returns>
    Task<ApiResponse<bool>> RegistrarPagoAsync(Guid facturaId, RegistrarPagoDto pagoDto);

    /// <summary>
    /// Anular factura
    /// </summary>
    /// <param name="facturaId">ID de la factura</param>
    /// <param name="anulacionDto">Datos de la anulación</param>
    /// <returns>Resultado de la anulación</returns>
    Task<ApiResponse<bool>> AnularFacturaAsync(Guid facturaId, AnularFacturaDto anulacionDto);

    /// <summary>
    /// Descargar factura en PDF
    /// </summary>
    /// <param name="facturaId">ID de la factura</param>
    /// <returns>URL del PDF</returns>
    Task<ApiResponse<string>> DescargarFacturaPdfAsync(Guid facturaId);

    /// <summary>
    /// Enviar factura por email
    /// </summary>
    /// <param name="facturaId">ID de la factura</param>
    /// <param name="email">Email de destino</param>
    /// <returns>Resultado del envío</returns>
    Task<ApiResponse<string>> EnviarFacturaPorEmailAsync(Guid facturaId, string email);
} 