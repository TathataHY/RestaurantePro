using Microsoft.Extensions.DependencyInjection;

namespace RestaurantePro.Domain.Core.Base.Events.Dispatcher
{
    /// <summary>
    /// Servicio que se encarga de distribuir eventos de dominio a sus manejadores correspondientes
    /// </summary>
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDomainEventLog _eventLog;

        /// <summary>
        /// Constructor
        /// </summary>
        public DomainEventDispatcher(IServiceProvider serviceProvider, IDomainEventLog eventLog)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _eventLog = eventLog ?? throw new ArgumentNullException(nameof(eventLog));
        }

        /// <summary>
        /// Distribuye un evento de dominio a todos sus manejadores registrados
        /// </summary>
        public async Task Dispatch(DomainEvent evento, CancellationToken cancellationToken = default)
        {
            if (evento == null)
                throw new ArgumentNullException(nameof(evento));

            var eventType = evento.GetType();
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

            // Obtener todos los manejadores registrados para este tipo de evento
            var handlers = _serviceProvider.GetServices(handlerType) as IEnumerable<object>;

            if (handlers == null || !handlers.Any())
            {
                // Si no hay manejadores, registrar en el log y salir
                await _eventLog.LogEvent(evento, 
                    $"No se encontraron manejadores para el evento {eventType.Name}", 
                    cancellationToken);
                return;
            }

            // Llamar a cada manejador
            foreach (dynamic handler in handlers)
            {
                try
                {
                    await handler.Handle((dynamic)evento, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Registrar error pero continuar con los demás manejadores
                    await _eventLog.LogEvent(evento, 
                        $"Error en manejador {handler.GetType().Name}: {ex.Message}", 
                        cancellationToken);
                }
            }
        }

        /// <summary>
        /// Distribuye una colección de eventos de dominio a sus respectivos manejadores
        /// </summary>
        public async Task DispatchAll(IEnumerable<DomainEvent> eventos, CancellationToken cancellationToken = default)
        {
            if (eventos == null)
                throw new ArgumentNullException(nameof(eventos));
                
            // Procesar cada evento en secuencia
            foreach (var evento in eventos)
            {
                await Dispatch(evento, cancellationToken);
            }
        }
    }
} 