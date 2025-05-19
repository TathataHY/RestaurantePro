namespace RestaurantePro.Domain.Core.Base.Events.Subscription
{
    /// <summary>
    /// Representa una suscripción a eventos
    /// </summary>
    internal class EventSubscription
    {
        /// <summary>
        /// Identificador único de la suscripción
        /// </summary>
        public Guid Id { get; }
        
        /// <summary>
        /// Criterios para filtrar eventos
        /// </summary>
        public EventSubscriptionCriteria Criteria { get; }
        
        /// <summary>
        /// Manejador a invocar cuando se recibe un evento que cumple los criterios
        /// </summary>
        public EventSubscriptionHandler Handler { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public EventSubscription(Guid id, EventSubscriptionCriteria criteria, EventSubscriptionHandler handler)
        {
            Id = id;
            Criteria = criteria ?? throw new ArgumentNullException(nameof(criteria));
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }
    }
    
    /// <summary>
    /// Implementación del administrador de suscripciones a eventos entre agregados
    /// </summary>
    public class EventSubscriptionManager : IEventSubscriptionManager
    {
        // Lista de suscripciones activas
        private readonly List<EventSubscription> _subscriptions = new List<EventSubscription>();
        
        // Lock para sincronización de acceso a la lista de suscripciones
        private readonly object _subscriptionLock = new object();
        
        // Logger (en una implementación real, se inyectaría un logger)
        private readonly Action<string>? _logger;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Logger opcional para registrar actividad</param>
        public EventSubscriptionManager(Action<string>? logger = null)
        {
            _logger = logger;
        }
        
        /// <inheritdoc />
        public Guid Subscribe(EventSubscriptionCriteria criteria, EventSubscriptionHandler handler)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
                
            var subscriptionId = Guid.NewGuid();
            var subscription = new EventSubscription(subscriptionId, criteria, handler);
            
            lock (_subscriptionLock)
            {
                _subscriptions.Add(subscription);
            }
            
            _logger?.Invoke($"Creada suscripción {subscriptionId} para eventos de tipo {criteria.EventType.Name}");
            
            return subscriptionId;
        }
        
        /// <inheritdoc />
        public Guid Subscribe<TEvent>(EventSubscriptionHandler handler) where TEvent : DomainEvent
        {
            var criteria = new EventSubscriptionCriteria(typeof(TEvent));
            return Subscribe(criteria, handler);
        }
        
        /// <inheritdoc />
        public Guid Subscribe<TEvent>(Guid sourceEntityId, EventSubscriptionHandler handler) where TEvent : DomainEvent
        {
            var criteria = new EventSubscriptionCriteria(typeof(TEvent), sourceEntityId);
            return Subscribe(criteria, handler);
        }
        
        /// <inheritdoc />
        public Guid Subscribe<TEvent>(string sourceBoundedContext, EventSubscriptionHandler handler) where TEvent : DomainEvent
        {
            var criteria = new EventSubscriptionCriteria(typeof(TEvent), null, sourceBoundedContext);
            return Subscribe(criteria, handler);
        }
        
        /// <inheritdoc />
        public bool Unsubscribe(Guid subscriptionId)
        {
            lock (_subscriptionLock)
            {
                var index = _subscriptions.FindIndex(s => s.Id == subscriptionId);
                if (index >= 0)
                {
                    _subscriptions.RemoveAt(index);
                    _logger?.Invoke($"Eliminada suscripción {subscriptionId}");
                    return true;
                }
                
                return false;
            }
        }
        
        /// <inheritdoc />
        public async Task NotifySubscribersAsync(DomainEvent evento, CancellationToken cancellationToken = default)
        {
            if (evento == null)
                return;
                
            List<EventSubscription> matchingSubscriptions;
            
            // Obtenemos una copia de las suscripciones que coinciden con el evento
            lock (_subscriptionLock)
            {
                matchingSubscriptions = _subscriptions
                    .Where(s => s.Criteria.Matches(evento))
                    .ToList();
            }
            
            if (matchingSubscriptions.Count == 0)
                return;
                
            _logger?.Invoke($"Notificando evento {evento.GetType().Name} a {matchingSubscriptions.Count} suscriptores");
            
            // Notificamos a todos los suscriptores que coinciden
            var tasks = matchingSubscriptions.Select(s => 
                SafeInvokeHandlerAsync(s.Handler, evento, cancellationToken));
                
            await Task.WhenAll(tasks);
        }
        
        /// <inheritdoc />
        public async Task NotifySubscribersAsync(IEnumerable<DomainEvent> eventos, CancellationToken cancellationToken = default)
        {
            if (eventos == null)
                return;
                
            foreach (var evento in eventos)
            {
                await NotifySubscribersAsync(evento, cancellationToken);
            }
        }
        
        /// <summary>
        /// Invoca un manejador de forma segura, capturando excepciones
        /// </summary>
        private async Task SafeInvokeHandlerAsync(EventSubscriptionHandler handler, DomainEvent evento, CancellationToken cancellationToken)
        {
            try
            {
                await handler.Invoke(evento, cancellationToken);
            }
            catch (Exception ex)
            {
                // En una implementación real, se registraría este error
                // y potencialmente se tomarían acciones de recuperación o reintentos
                _logger?.Invoke($"Error al manejar evento {evento.GetType().Name}: {ex.Message}");
            }
        }
    }
} 