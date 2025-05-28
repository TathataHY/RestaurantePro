namespace RestaurantePro.Domain.Core.Services
{
    /// <summary>
    /// Interfaz para el servicio de integración entre los contextos Core y Operaciones
    /// </summary>
    public interface ICoreOperacionesIntegrationService
    {
        /// <summary>
        /// Verifica la disponibilidad de un conjunto de productos para una comanda
        /// </summary>
        /// <param name="productosIdCantidad">Diccionario con IDs de productos y sus cantidades</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con información de disponibilidad</returns>
        Task<Result<DisponibilidadProductosResult>> VerificarDisponibilidadProductosAsync(
            Dictionary<Guid, int> productosIdCantidad,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Calcula el precio total de un conjunto de productos para una comanda
        /// </summary>
        /// <param name="productosIdCantidad">Diccionario con IDs de productos y sus cantidades</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con información de precios</returns>
        Task<Result<CalculoPreciosResult>> CalcularPreciosTotalesAsync(
            Dictionary<Guid, int> productosIdCantidad,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Actualiza la información de productos en stock basado en una comanda finalizada
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="productosIdCantidad">Diccionario con IDs de productos y sus cantidades</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado del procesamiento</returns>
        Task<Result<bool>> ProcesarComandaFinalizadaAsync(
            Guid comandaId,
            Dictionary<Guid, int> productosIdCantidad,
            CancellationToken cancellationToken = default);
    }
} 