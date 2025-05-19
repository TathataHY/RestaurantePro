namespace RestaurantePro.Domain.Core.Base.Events.Dispatcher
{
    /// <summary>
    /// Implementación del despachador de eventos de dominio con soporte para registro
    /// </summary>
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDomainEventRegistry? _eventRegistry;

        /// <summary>
        /// Constructor del despachador de eventos
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para resolver manejadores</param>
        /// <param name="eventRegistry">Registro de eventos (opcional)</param>
        public DomainEventDispatcher(IServiceProvider serviceProvider, IDomainEventRegistry? eventRegistry = null)
        {
            _serviceProvider = serviceProvider;
            _eventRegistry = eventRegistry;
        }

        /// <summary>
        /// Distribuye un evento de dominio a todos sus manejadores registrados
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
            
            foreach (var handler in handlers)
            {
                // Invocamos el método Handle en cada handler
                var method = handlerType.GetMethod("Handle");
                if (method != null)
                {
                    // Creamos una tarea para el manejador
                    var task = (Task)method.Invoke(handler, new object[] { evento, cancellationToken });
                    if (task != null)
                    {
                        await task;
                    }
                }
            }
        }

        /// <summary>
        /// Distribuye una colección de eventos de dominio a sus respectivos manejadores
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