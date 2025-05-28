namespace RestaurantePro.Domain.Operaciones.Services
{
    /// <summary>
    /// Interfaz para el servicio de integración entre los contextos de Operaciones e Inventario.
    /// Implementa el patrón Anticorruption Layer para la comunicación entre comandas e inventario.
    /// </summary>
    public interface IOperacionesInventarioIntegrationService
    {
        /// <summary>
        /// Verifica la disponibilidad de ingredientes para una comanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con información sobre la disponibilidad</returns>
        Task<Result<DisponibilidadIngredientesResult>> VerificarDisponibilidadIngredientesComandaAsync(
            Guid comandaId,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Reserva ingredientes para una comanda (reducción temporal del stock)
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result<bool>> ReservarIngredientesComandaAsync(
            Guid comandaId,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Confirma el consumo de ingredientes para una comanda finalizada
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result<bool>> ConfirmarConsumoIngredientesAsync(
            Guid comandaId,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Libera la reserva de ingredientes para una comanda cancelada
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="motivo">Motivo de la cancelación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result<bool>> LiberarReservaIngredientesAsync(
            Guid comandaId,
            string motivo,
            CancellationToken cancellationToken = default);
    }
    
    /// <summary>
    /// Resultado de la verificación de disponibilidad de ingredientes
    /// </summary>
    public class DisponibilidadIngredientesResult
    {
        /// <summary>
        /// Indica si todos los ingredientes están disponibles
        /// </summary>
        public bool TodosDisponibles { get; set; }
        
        /// <summary>
        /// Diccionario con productos que no se pueden preparar y la razón
        /// </summary>
        public Dictionary<Guid, string> ProductosNoDisponibles { get; set; }
        
        /// <summary>
        /// Diccionario con ingredientes faltantes y la cantidad necesaria
        /// </summary>
        public Dictionary<string, decimal> IngredientesFaltantes { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public DisponibilidadIngredientesResult()
        {
            TodosDisponibles = true;
            ProductosNoDisponibles = new Dictionary<Guid, string>();
            IngredientesFaltantes = new Dictionary<string, decimal>();
        }
    }
} 