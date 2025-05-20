namespace RestaurantePro.Domain.Core.Base.Events.Registry
{
    /// <summary>
    /// Implementación en memoria del registro de eventos de dominio para pruebas.
    /// Extiende la implementación interna para agregar métodos adicionales para pruebas.
    /// </summary>
    public class InMemoryDomainEventRegistry : IDomainEventRegistry
    {
        private readonly List<DomainEvent> _events = new List<DomainEvent>();
        
        /// <summary>
        /// Registra un evento de dominio en memoria
        /// </summary>
        /// <param name="evento">Evento de dominio a registrar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        public Task RegisterAsync(DomainEvent evento, CancellationToken cancellationToken = default)
        {
            _events.Add(evento);
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Registra múltiples eventos de dominio en memoria
        /// </summary>
        /// <param name="eventos">Colección de eventos de dominio a registrar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        public Task RegisterAllAsync(IEnumerable<DomainEvent> eventos, CancellationToken cancellationToken = default)
        {
            _events.AddRange(eventos);
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Obtiene los eventos de dominio registrados para una entidad específica
        /// </summary>
        /// <param name="entityId">Identificador de la entidad</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de eventos ordenados cronológicamente</returns>
        public Task<IReadOnlyList<DomainEvent>> GetEventsForEntityAsync(Guid entityId, CancellationToken cancellationToken = default)
        {
            var result = _events
                .Where(e => e.EntityId == entityId)
                .OrderBy(e => e.OccurredOn)
                .ToList();
                
            return Task.FromResult<IReadOnlyList<DomainEvent>>(result);
        }
        
        /// <summary>
        /// Obtiene todos los eventos de un tipo específico
        /// </summary>
        /// <typeparam name="T">Tipo de evento a obtener</typeparam>
        /// <param name="fromDate">Fecha desde la que buscar eventos (opcional)</param>
        /// <param name="toDate">Fecha hasta la que buscar eventos (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de eventos ordenados cronológicamente</returns>
        public Task<IReadOnlyList<T>> GetEventsByTypeAsync<T>(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default) where T : DomainEvent
        {
            var query = _events.OfType<T>();
            
            if (fromDate.HasValue)
            {
                query = query.Where(e => e.OccurredOn >= fromDate.Value);
            }
            
            if (toDate.HasValue)
            {
                query = query.Where(e => e.OccurredOn <= toDate.Value);
            }
            
            var result = query.OrderBy(e => e.OccurredOn).ToList();
            return Task.FromResult<IReadOnlyList<T>>(result);
        }
        
        /// <summary>
        /// Elimina todos los eventos almacenados.
        /// Este método es útil para pruebas unitarias.
        /// </summary>
        public void Clear()
        {
            _events.Clear();
        }
    }
} 