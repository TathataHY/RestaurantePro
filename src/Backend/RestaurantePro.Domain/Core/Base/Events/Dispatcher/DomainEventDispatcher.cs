using System.Reflection;

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
        /// Despacha un evento de dominio a todos sus manejadores registrados
        /// </summary>
        /// <param name="evento">Evento de dominio a despachar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        public async Task Dispatch(DomainEvent evento, CancellationToken cancellationToken = default)
        {
            if (evento == null)
                throw new ArgumentNullException(nameof(evento));
            
            try
            {    
                // Obtener el tipo del evento
                var eventType = evento.GetType();
                
                // Construir el tipo genérico IDomainEventHandler<T> específico para este evento
                var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
                
                // Resolver los manejadores registrados para este tipo de evento
                var handlerWrapperType = typeof(IEnumerable<>).MakeGenericType(handlerType);
                var handlers = _serviceProvider.GetService(handlerWrapperType) as IEnumerable<object>;
                
                var handlersFound = false;
                bool hasExceptionOccurred = false;
                
                // Si hay manejadores, despacharlos
                if (handlers != null && handlers.Any())
                {
                    handlersFound = true;
                    
                    // Si solo hay un handler y lanza excepción, la propagaremos
                    bool hasOnlySingleHandler = handlers.Count() == 1;
                    
                    foreach (var handler in handlers)
                    {
                        try
                        {
                            // Obtener método Handle del manejador mediante reflexión
                            var method = handler.GetType().GetMethod("Handle");
                            
                            if (method != null)
                            {
                                // Invocar el método Handle con los parámetros correctos
                                var task = (Task)method.Invoke(handler, new object[] { evento, cancellationToken });
                                await task.ConfigureAwait(false);
                            }
                        }
                        catch (TargetInvocationException ex)
                        {
                            hasExceptionOccurred = true;
                            
                            // Registrar el evento si ocurre un error
                            await RegisterEventAsync(evento, cancellationToken);
                            
                            // Si hay un solo handler, propagar la excepción
                            if (hasOnlySingleHandler)
                            {
                                throw ex.InnerException ?? ex;
                            }
                        }
                        catch (Exception)
                        {
                            hasExceptionOccurred = true;
                            
                            // Registrar el evento en caso de cualquier otra excepción
                            await RegisterEventAsync(evento, cancellationToken);
                            
                            // Si hay un solo handler, propagar la excepción
                            if (hasOnlySingleHandler)
                            {
                                throw;
                            }
                        }
                    }
                }
                
                // Si no hay manejadores o después de procesarlos, notificar a los suscriptores
                if (_subscriptionManager != null)
                {
                    await _subscriptionManager.NotifySubscribersAsync(evento, cancellationToken);
                }
                
                // Si no hay manejadores, registrar el evento para su procesamiento posterior
                if ((!handlersFound || hasExceptionOccurred) && _eventRegistry != null)
                {
                    await RegisterEventAsync(evento, cancellationToken);
                }
            }
            catch (Exception)
            {
                // Asegurarse de que el evento se registre incluso si hay una excepción
                await RegisterEventAsync(evento, cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Despacha una colección de eventos de dominio en secuencia
        /// </summary>
        /// <param name="eventos">Eventos de dominio a despachar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        public async Task DispatchAll(IEnumerable<DomainEvent> eventos, CancellationToken cancellationToken = default)
        {
            if (eventos == null)
                throw new ArgumentNullException(nameof(eventos));
                
            foreach (var evento in eventos)
            {
                await Dispatch(evento, cancellationToken);
            }
        }
        
        private async Task RegisterEventAsync(DomainEvent evento, CancellationToken cancellationToken)
        {
            if (_eventRegistry != null)
            {
                await _eventRegistry.RegisterAsync(evento, cancellationToken);
            }
        }
    }
} 