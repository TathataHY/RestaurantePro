namespace RestaurantePro.Domain.Core.Base.Events.Registry
{
    /// <summary>
    /// Implementación en memoria del registro de eventos de dominio.
    /// Útil para pruebas y desarrollo.
    /// </summary>
    public class InMemoryDomainEventRegistry : IDomainEventRegistry
    {
        private readonly List<DomainEventRecord> _events = new List<DomainEventRecord>();
        private readonly object _lock = new object();

        /// <summary>
        /// Registra un evento de dominio en memoria
        /// </summary>
        /// <param name="evento">Evento de dominio a registrar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        public Task RegisterAsync(DomainEvent evento, CancellationToken cancellationToken = default)
        {
            if (evento == null)
                throw new ArgumentNullException(nameof(evento));

            // Extraer información de la entidad
            // En un contexto real, esta información vendría de la entidad que generó el evento
            // Para este ejemplo, usamos inferencia basada en convenciones de nombres
            var eventType = evento.GetType();
            
            // Intentamos extraer el EntityId y EntityType de las propiedades del evento
            // Asumimos que muchos eventos tienen una propiedad con el Id de la entidad
            var entityIdProperty = eventType.GetProperties()
                .FirstOrDefault(p => p.Name.EndsWith("Id") && p.PropertyType == typeof(Guid));
                
            // Para la entidad, asumimos que está en el nombre del evento
            // Por ejemplo: ProductoCreado => Producto
            var entityType = eventType.Name;
            if (entityType.EndsWith("do") || entityType.EndsWith("da"))
            {
                entityType = entityType.Substring(0, entityType.Length - 2);
            }
            
            // Para el contexto, utilizamos el espacio de nombres
            // Por ejemplo: RestaurantePro.Domain.Core.Productos.Events => Core
            var domainContext = eventType.Namespace?.Split('.')[2] ?? "Core";
            
            // Obtenemos el EntityId o usamos uno nuevo si no podemos extraerlo
            var entityId = entityIdProperty != null 
                ? (Guid)entityIdProperty.GetValue(evento) 
                : Guid.NewGuid();
            
            // Crear el registro
            var record = new DomainEventRecord(evento, entityId, entityType, domainContext);
            
            // Agregar a la lista en memoria (thread-safe)
            lock (_lock)
            {
                _events.Add(record);
            }
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Registra múltiples eventos de dominio en memoria
        /// </summary>
        /// <param name="eventos">Colección de eventos de dominio a registrar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        public async Task RegisterAllAsync(IEnumerable<DomainEvent> eventos, CancellationToken cancellationToken = default)
        {
            if (eventos == null)
                throw new ArgumentNullException(nameof(eventos));
                
            foreach (var evento in eventos)
            {
                await RegisterAsync(evento, cancellationToken);
            }
        }

        /// <summary>
        /// Obtiene los eventos de dominio registrados para una entidad específica
        /// </summary>
        /// <param name="entityId">Identificador de la entidad</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de eventos ordenados cronológicamente</returns>
        public Task<IReadOnlyList<DomainEvent>> GetEventsForEntityAsync(Guid entityId, CancellationToken cancellationToken = default)
        {
            if (entityId == Guid.Empty)
                throw new ArgumentException("El ID de entidad no puede estar vacío", nameof(entityId));
                
            // Obtener los eventos para la entidad específica (thread-safe)
            List<DomainEventRecord> entityEvents;
            lock (_lock)
            {
                entityEvents = _events
                    .Where(e => e.EntityId == entityId)
                    .OrderBy(e => e.TimeStamp)
                    .ToList();
            }
            
            // Deserializar los eventos
            var result = entityEvents
                .Select(record => 
                {
                    // Obtener el tipo de evento
                    var eventType = Type.GetType(record.EventType);
                    if (eventType == null)
                        throw new InvalidOperationException($"No se pudo encontrar el tipo {record.EventType}");
                        
                    // Usar un método genérico para deserializar el evento
                    var method = typeof(DomainEventRecord)
                        .GetMethod(nameof(DomainEventRecord.DeserializeEvent))
                        .MakeGenericMethod(eventType);
                        
                    // Invocar el método
                    return (DomainEvent)method.Invoke(record, null);
                })
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
            var eventTypeName = typeof(T).FullName;
            
            // Aplicar filtros de fecha si se proporcionaron
            var startDate = fromDate ?? DateTime.MinValue;
            var endDate = toDate ?? DateTime.MaxValue;
            
            // Obtener los eventos del tipo solicitado (thread-safe)
            List<DomainEventRecord> typeEvents;
            lock (_lock)
            {
                typeEvents = _events
                    .Where(e => e.EventType == eventTypeName)
                    .Where(e => e.TimeStamp >= startDate && e.TimeStamp <= endDate)
                    .OrderBy(e => e.TimeStamp)
                    .ToList();
            }
            
            // Deserializar los eventos
            var result = typeEvents
                .Select(record => record.DeserializeEvent<T>())
                .ToList();
                
            return Task.FromResult<IReadOnlyList<T>>(result);
        }
        
        /// <summary>
        /// Limpia todos los eventos almacenados (útil para pruebas)
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _events.Clear();
            }
        }
    }
} 