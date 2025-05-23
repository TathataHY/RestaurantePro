namespace RestaurantePro.Domain.Comercial.Facturacion.Services
{
    /// <summary>
    /// Interfaz del servicio para gestionar facturas vencidas
    /// </summary>
    public interface IServicioGestionFacturasVencidas
    {
        /// <summary>
        /// Procesa las facturas vencidas
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de facturas procesadas</returns>
        Task<int> ProcesarFacturasVencidasAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Genera un informe de facturas vencidas
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Informe de facturas vencidas</returns>
        Task<InformeFacturasVencidas> GenerarInformeFacturasVencidasAsync(CancellationToken cancellationToken = default);
    }
} 