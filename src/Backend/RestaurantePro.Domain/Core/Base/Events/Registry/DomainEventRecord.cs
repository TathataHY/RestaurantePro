namespace RestaurantePro.Domain.Core.Base.Events.Registry
{
    /// <summary>
    /// Representa un registro persistente de un evento de dominio.
    /// Esta clase se utiliza para la persistencia y auditoría de eventos.
    /// </summary>
    public class DomainEventRecord
    {
        /// <summary>
        /// Identificador único del registro de evento
        /// </summary>
        public Guid Id { get; private set; }
        
        /// <summary>
        /// Fecha y hora en que se registró el evento
        /// </summary>
        public DateTime TimeStamp { get; private set; }
        
        /// <summary>
        /// Nombre del tipo de evento (incluyendo namespace)
        /// </summary>
        public string EventType { get; private set; }
        
        /// <summary>
        /// Contenido del evento serializado como JSON
        /// </summary>
        public string EventData { get; private set; }
        
        /// <summary>
        /// Identificador de la entidad a la que pertenece el evento
        /// </summary>
        public Guid EntityId { get; private set; }
        
        /// <summary>
        /// Nombre de la entidad a la que pertenece el evento
        /// </summary>
        public string EntityType { get; private set; }
        
        /// <summary>
        /// Contexto del dominio al que pertenece el evento
        /// </summary>
        public string DomainContext { get; private set; }
        
        /// <summary>
        /// Indica si el evento ha sido procesado por todos los manejadores
        /// </summary>
        public bool Processed { get; private set; }
        
        // Constructor para ORM
        protected DomainEventRecord() { }
        
        /// <summary>
        /// Crea un nuevo registro de evento de dominio
        /// </summary>
        /// <param name="evento">Evento de dominio</param>
        /// <param name="entityId">ID de la entidad asociada</param>
        /// <param name="entityType">Nombre del tipo de entidad</param>
        /// <param name="domainContext">Contexto de dominio</param>
        public DomainEventRecord(DomainEvent evento, Guid entityId, string entityType, string domainContext)
        {
            if (evento == null)
                throw new ArgumentNullException(nameof(evento));
                
            Id = Guid.NewGuid();
            TimeStamp = DateTime.UtcNow;
            EventType = evento.GetType().FullName;
            EventData = JsonSerializer.Serialize(evento);
            EntityId = entityId;
            EntityType = entityType;
            DomainContext = domainContext;
            Processed = false;
        }
        
        /// <summary>
        /// Marca el evento como procesado
        /// </summary>
        public void MarkAsProcessed()
        {
            Processed = true;
        }
        
        /// <summary>
        /// Deserializa el evento de dominio almacenado
        /// </summary>
        /// <typeparam name="T">Tipo de evento a deserializar</typeparam>
        /// <returns>Evento de dominio deserializado</returns>
        public T DeserializeEvent<T>() where T : DomainEvent
        {
            return JsonSerializer.Deserialize<T>(EventData);
        }
    }
} 