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
        /// <returns>Resultado con el número de facturas procesadas</returns>
        Task<Result<int>> ProcesarFacturasVencidasAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Genera un informe de facturas vencidas
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con el informe de facturas vencidas</returns>
        Task<Result<InformeFacturasVencidas>> GenerarInformeFacturasVencidasAsync(CancellationToken cancellationToken = default);
    }
} 