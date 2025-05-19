namespace RestaurantePro.Domain.Core.Base.Events.Dispatcher
{
    /// <summary>
    /// Implementación del despachador de eventos de dominio con soporte para registro y suscripciones
    /// </summary>
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDomainEventRegistry? _eventRegistry;
        private readonly IEventSubscriptionManager? _subscriptionManager;

        /// <summary>
        /// Constructor del despachador de eventos
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para resolver manejadores</param>
        /// <param name="eventRegistry">Registro de eventos (opcional)</param>
        /// <param name="subscriptionManager">Administrador de suscripciones (opcional)</param>
        public DomainEventDispatcher(
            IServiceProvider serviceProvider, 
            IDomainEventRegistry? eventRegistry = null,
            IEventSubscriptionManager? subscriptionManager = null)
        {
            _serviceProvider = serviceProvider;
            _eventRegistry = eventRegistry;
            _subscriptionManager = subscriptionManager;
        }

        /// <summary>
        /// Distribuye un evento de dominio a todos sus manejadores registrados y suscriptores
        /// </summary>
        /// <param name="evento">Evento a distribuir</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        public async Task Dispatch(DomainEvent evento, CancellationToken cancellationToken = default)
        {
            // Si hay un registro de eventos, registramos el evento
            if (_eventRegistry != null)
            {
                try
                {
                    await _eventRegistry.RegisterAsync(evento, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log error but continue with dispatch
                    // En una implementación real, se registraría este error
                    System.Diagnostics.Debug.WriteLine($"Error registering event: {ex.Message}");
                }
            }

            // Obtenemos el tipo del evento
            var eventType = evento.GetType();
            
            // Construimos el tipo del handler genérico
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            
            // Obtenemos todos los handlers para este tipo de evento
            var handlers = _serviceProvider.GetServices(handlerType);
            
            // Lista para almacenar las tareas de los manejadores
            var handlerTasks = new List<Task>();
            
            // Invocamos cada manejador registrado en el contenedor
            foreach (var handler in handlers)
            {
                // Invocamos el método Handle en cada handler
                var method = handlerType.GetMethod("Handle");
                if (method != null)
                {
                    try
                    {
                        // Creamos una tarea para el manejador
                        var task = (Task)method.Invoke(handler, new object[] { evento, cancellationToken });
                        if (task != null)
                        {
                            handlerTasks.Add(task);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue with other handlers
                        System.Diagnostics.Debug.WriteLine($"Error invoking handler: {ex.Message}");
                    }
                }
            }
            
            // Esperar a que todos los manejadores registrados terminen
            if (handlerTasks.Count > 0)
            {
                await Task.WhenAll(handlerTasks);
            }
            
            // Notificar a los suscriptores si hay un administrador de suscripciones
            if (_subscriptionManager != null)
            {
                await _subscriptionManager.NotifySubscribersAsync(evento, cancellationToken);
            }
        }

        /// <summary>
        /// Distribuye una colección de eventos de dominio a sus respectivos manejadores y suscriptores
        /// </summary>
        /// <param name="eventos">Colección de eventos a distribuir</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        public async Task DispatchAll(IEnumerable<DomainEvent> eventos, CancellationToken cancellationToken = default)
        {
            // Si hay un registro de eventos, registramos todos los eventos en una sola operación
            if (_eventRegistry != null)
            {
                try
                {
                    await _eventRegistry.RegisterAllAsync(eventos, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log error but continue with dispatch
                    // En una implementación real, se registraría este error
                    System.Diagnostics.Debug.WriteLine($"Error registering events: {ex.Message}");
                }
            }

            // Distribuimos cada evento individualmente
            foreach (var evento in eventos)
            {
                await Dispatch(evento, cancellationToken);
            }
        }
    }
} 