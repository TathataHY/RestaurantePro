namespace RestaurantePro.Domain.Core.Base.Events.Subscription
{
    /// <summary>
    /// Define los criterios para filtrar eventos en una suscripción
    /// </summary>
    public class EventSubscriptionCriteria
    {
        /// <summary>
        /// Filtro por tipo de evento (obligatorio)
        /// </summary>
        public Type EventType { get; }
        
        /// <summary>
        /// Filtro opcional por ID de la entidad emisora
        /// </summary>
        public Guid? SourceEntityId { get; }
        
        /// <summary>
        /// Filtro opcional por contexto delimitado de origen
        /// </summary>
        public string? SourceBoundedContext { get; }
        
        /// <summary>
        /// Constructor para crear un criterio de suscripción
        /// </summary>
        /// <param name="eventType">Tipo de evento (obligatorio)</param>
        /// <param name="sourceEntityId">ID de la entidad emisora (opcional)</param>
        /// <param name="sourceBoundedContext">Contexto delimitado de origen (opcional)</param>
        public EventSubscriptionCriteria(Type eventType, Guid? sourceEntityId = null, string? sourceBoundedContext = null)
        {
            EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
            SourceEntityId = sourceEntityId;
            SourceBoundedContext = sourceBoundedContext;
        }
        
        /// <summary>
        /// Evalúa si un evento coincide con los criterios de suscripción
        /// </summary>
        /// <param name="evento">Evento a evaluar</param>
        /// <returns>True si el evento coincide con los criterios</returns>
        public bool Matches(DomainEvent evento)
        {
            if (evento == null)
                return false;
                
            // Verificar tipo de evento
            if (!EventType.IsAssignableFrom(evento.GetType()))
                return false;
                
            // Verificar ID de entidad si se especificó
            if (SourceEntityId.HasValue && evento.EntityId != SourceEntityId.Value)
                return false;
                
            // Verificar contexto delimitado si se especificó
            if (!string.IsNullOrEmpty(SourceBoundedContext) && 
                !string.Equals(evento.BoundedContext, SourceBoundedContext, StringComparison.OrdinalIgnoreCase))
                return false;
                
            return true;
        }
    }
    
    /// <summary>
    /// Delegado para manejadores de eventos por suscripción
    /// </summary>
    /// <param name="evento">Evento recibido</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    public delegate Task EventSubscriptionHandler(DomainEvent evento, CancellationToken cancellationToken);
    
    /// <summary>
    /// Interfaz para el administrador de suscripciones a eventos entre agregados y contextos.
    /// Permite a los agregados suscribirse a eventos específicos emitidos por otros agregados
    /// sin acoplarse directamente a ellos.
    /// </summary>
    public interface IEventSubscriptionManager
    {
        /// <summary>
        /// Suscribe un manejador a eventos que cumplan con los criterios especificados
        /// </summary>
        /// <param name="criteria">Criterios de suscripción</param>
        /// <param name="handler">Manejador de eventos</param>
        /// <returns>Identificador único de la suscripción</returns>
        Guid Subscribe(EventSubscriptionCriteria criteria, EventSubscriptionHandler handler);
        
        /// <summary>
        /// Suscribe un manejador a eventos de un tipo específico
        /// </summary>
        /// <typeparam name="TEvent">Tipo de evento</typeparam>
        /// <param name="handler">Manejador de eventos</param>
        /// <returns>Identificador único de la suscripción</returns>
        Guid Subscribe<TEvent>(EventSubscriptionHandler handler) where TEvent : DomainEvent;
        
        /// <summary>
        /// Suscribe un manejador a eventos de un tipo específico emitidos por una entidad concreta
        /// </summary>
        /// <typeparam name="TEvent">Tipo de evento</typeparam>
        /// <param name="sourceEntityId">ID de la entidad emisora</param>
        /// <param name="handler">Manejador de eventos</param>
        /// <returns>Identificador único de la suscripción</returns>
        Guid Subscribe<TEvent>(Guid sourceEntityId, EventSubscriptionHandler handler) where TEvent : DomainEvent;
        
        /// <summary>
        /// Suscribe un manejador a eventos de un tipo específico emitidos por un contexto delimitado
        /// </summary>
        /// <typeparam name="TEvent">Tipo de evento</typeparam>
        /// <param name="sourceBoundedContext">Contexto delimitado de origen</param>
        /// <param name="handler">Manejador de eventos</param>
        /// <returns>Identificador único de la suscripción</returns>
        Guid Subscribe<TEvent>(string sourceBoundedContext, EventSubscriptionHandler handler) where TEvent : DomainEvent;
        
        /// <summary>
        /// Cancela una suscripción existente
        /// </summary>
        /// <param name="subscriptionId">ID de la suscripción a cancelar</param>
        /// <returns>True si la suscripción fue cancelada exitosamente</returns>
        bool Unsubscribe(Guid subscriptionId);
        
        /// <summary>
        /// Notifica un evento a todos los manejadores suscritos que coincidan con los criterios
        /// </summary>
        /// <param name="evento">Evento a notificar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task NotifySubscribersAsync(DomainEvent evento, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Notifica múltiples eventos a sus respectivos manejadores suscritos
        /// </summary>
        /// <param name="eventos">Eventos a notificar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task NotifySubscribersAsync(IEnumerable<DomainEvent> eventos, CancellationToken cancellationToken = default);
    }
} 