namespace RestaurantePro.Domain.Operaciones.Services
{
    /// <summary>
    /// Interfaz que define los métodos para la integración entre los contextos de Operaciones e Inventario.
    /// Implementa el patrón Anticorruption Layer para mantener la integridad entre ambos contextos.
    /// </summary>
    public interface IOperacionesInventarioIntegrationService
    {
        /// <summary>
        /// Verifica la disponibilidad de ingredientes para una comanda.
        /// </summary>
        /// <param name="comandaId">Id de la comanda a verificar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con información de disponibilidad</returns>
        Task<Result<Results.DisponibilidadIngredientesResult>> VerificarDisponibilidadIngredientesComandaAsync(
            Guid comandaId, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Reserva los ingredientes necesarios para una comanda.
        /// Reduce temporalmente el stock disponible.
        /// </summary>
        /// <param name="comandaId">Id de la comanda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result<bool>> ReservarIngredientesComandaAsync(
            Guid comandaId, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Confirma el consumo definitivo de ingredientes cuando se entrega un ítem de comanda.
        /// </summary>
        /// <param name="comandaId">Id de la comanda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result<bool>> ConfirmarConsumoIngredientesAsync(
            Guid comandaId, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Libera la reserva de ingredientes cuando se cancela un ítem de comanda.
        /// </summary>
        /// <param name="comandaId">Id de la comanda</param>
        /// <param name="motivo">Motivo de la cancelación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result<bool>> LiberarReservaIngredientesAsync(
            Guid comandaId, 
            string motivo, 
            CancellationToken cancellationToken = default);
    }
} 