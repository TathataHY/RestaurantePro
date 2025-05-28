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
            
        /// <summary>
        /// Sincroniza información de un proveedor entre el contexto de Proveedores y Comercial
        /// </summary>
        /// <param name="proveedorId">ID del proveedor a sincronizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con información del procesamiento</returns>
        Task<Result<bool>> SincronizarInformacionProveedorAsync(
            Guid proveedorId,
            CancellationToken cancellationToken = default);
    }
} 