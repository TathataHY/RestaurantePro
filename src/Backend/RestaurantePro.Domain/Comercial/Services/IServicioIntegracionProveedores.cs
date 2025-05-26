namespace RestaurantePro.Domain.Comercial.Services
{
    /// <summary>
    /// Interfaz para el servicio de integración entre Proveedores y Comercial
    /// </summary>
    public interface IServicioIntegracionProveedores
    {
        /// <summary>
        /// Procesa una orden de compra aprobada
        /// </summary>
        /// <param name="evento">Evento de orden aprobada</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con información del procesamiento</returns>
        Task<Result<bool>> ProcesarOrdenCompraAprobadaAsync(
            OrdenCompraAprobada evento,
            CancellationToken cancellationToken = default);
    }
} 