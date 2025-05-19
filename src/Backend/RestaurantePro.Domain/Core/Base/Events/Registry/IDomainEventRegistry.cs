namespace RestaurantePro.Domain.Core.Base.Events.Registry
{
    /// <summary>
    /// Interfaz para el servicio de registro centralizado de eventos de dominio.
    /// Permite almacenar eventos para auditoría, reconstrucción de estado
    /// y procesamiento posterior (Event Sourcing pattern).
    /// </summary>
    public interface IDomainEventRegistry
    {
        /// <summary>
        /// Registra un evento de dominio para su persistencia y posible procesamiento posterior
        /// </summary>
        /// <param name="evento">Evento de dominio a registrar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task RegisterAsync(DomainEvent evento, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Registra múltiples eventos de dominio en una sola operación
        /// </summary>
        /// <param name="eventos">Colección de eventos de dominio a registrar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task RegisterAllAsync(IEnumerable<DomainEvent> eventos, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene los eventos de dominio registrados para una entidad específica
        /// </summary>
        /// <param name="entityId">Identificador de la entidad</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de eventos ordenados cronológicamente</returns>
        Task<IReadOnlyList<DomainEvent>> GetEventsForEntityAsync(Guid entityId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todos los eventos de un tipo específico
        /// </summary>
        /// <typeparam name="T">Tipo de evento a obtener</typeparam>
        /// <param name="fromDate">Fecha desde la que buscar eventos (opcional)</param>
        /// <param name="toDate">Fecha hasta la que buscar eventos (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de eventos ordenados cronológicamente</returns>
        Task<IReadOnlyList<T>> GetEventsByTypeAsync<T>(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default) where T : DomainEvent;
    }
} 