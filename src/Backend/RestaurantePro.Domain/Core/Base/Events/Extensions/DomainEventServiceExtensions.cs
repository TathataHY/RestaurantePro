namespace RestaurantePro.Domain.Core.Base.Events.Extensions
{
    /// <summary>
    /// Extensiones unificadas para configurar servicios relacionados con eventos de dominio
    /// </summary>
    public static class DomainEventServiceExtensions
    {
        /// <summary>
        /// Registra los servicios base de eventos de dominio
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <returns>La colección de servicios con los servicios agregados</returns>
        public static IServiceCollection AddDomainEventServices(this IServiceCollection services)
        {
            // Registrar el despachador de eventos
            services.AddTransient<IDomainEventDispatcher, DomainEventDispatcher>();
            
            return services;
        }
        
        /// <summary>
        /// Registra los servicios de eventos de dominio con soporte para registro
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <returns>La colección de servicios con los servicios agregados</returns>
        public static IServiceCollection AddDomainEventServicesWithRegistry(this IServiceCollection services)
        {
            // Registrar los servicios core
            services.AddDomainEventServices();
            
            // Registrar el registro de eventos
            // Por defecto se registra como Transient, pero puede cambiarse a Singleton si se desea
            services.AddTransient<IDomainEventRegistry, NullDomainEventRegistry>();
            
            return services;
        }
        
        /// <summary>
        /// Registra el registro de eventos de dominio en memoria
        /// Útil para entornos de desarrollo y pruebas
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <returns>La misma colección para encadenamiento</returns>
        public static IServiceCollection AddInMemoryDomainEventRegistry(this IServiceCollection services)
        {
            // Registrar el registro de eventos en memoria como singleton
            services.AddSingleton<IDomainEventRegistry, InMemoryDomainEventRegistry>();
            
            return services;
        }
        
        /// <summary>
        /// Registra los servicios de eventos de dominio con soporte para suscripciones entre agregados
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <returns>La colección de servicios con los servicios agregados</returns>
        public static IServiceCollection AddDomainEventServicesWithSubscriptions(this IServiceCollection services)
        {
            // Registrar los servicios core
            services.AddDomainEventServices();
            
            // Registrar el administrador de suscripciones como singleton para mantener las suscripciones
            services.AddSingleton<IEventSubscriptionManager, EventSubscriptionManager>();
            
            return services;
        }
        
        /// <summary>
        /// Registra los servicios de eventos de dominio con soporte completo
        /// (registro y suscripciones entre agregados)
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <returns>La colección de servicios con los servicios agregados</returns>
        public static IServiceCollection AddDomainEventServicesComplete(this IServiceCollection services)
        {
            // Registrar los servicios core
            services.AddDomainEventServices();
            
            // Registrar el registro de eventos
            services.AddTransient<IDomainEventRegistry, NullDomainEventRegistry>();
            
            // Registrar el administrador de suscripciones
            services.AddSingleton<IEventSubscriptionManager, EventSubscriptionManager>();
            
            return services;
        }
        
        /// <summary>
        /// Escanea el ensamblado indicado para registrar todos los manejadores de eventos
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="assembly">Ensamblado a escanear (opcional, por defecto el ensamblado que llama)</param>
        /// <returns>La misma colección para encadenamiento</returns>
        public static IServiceCollection AddAllDomainEventHandlers(this IServiceCollection services, Assembly? assembly = null)
        {
            // Si no se especificó un ensamblado, usar el ensamblado que llama
            assembly ??= Assembly.GetCallingAssembly();
            
            // Buscar todos los tipos que implementan IDomainEventHandler<>
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.GetInterfaces().Any(i => 
                    i.IsGenericType && 
                    i.GetGenericTypeDefinition() == typeof(Handlers.IDomainEventHandler<>)))
                .ToList();
                
            // Registrar cada manejador con su interfaz
            foreach (var handlerType in handlerTypes)
            {
                // Obtener la interfaz IDomainEventHandler<T> que implementa
                var handlerInterface = handlerType.GetInterfaces()
                    .First(i => i.IsGenericType && 
                           i.GetGenericTypeDefinition() == typeof(Handlers.IDomainEventHandler<>));
                           
                // Registrar el manejador
                services.AddScoped(handlerInterface, handlerType);
            }
            
            return services;
        }
    }
    
    /// <summary>
    /// Implementación nula del registro de eventos que no hace nada.
    /// Útil como implementación por defecto cuando no se necesita persistir eventos.
    /// </summary>
    internal class NullDomainEventRegistry : IDomainEventRegistry
    {
        /// <inheritdoc />
        public Task RegisterAsync(DomainEvent evento, CancellationToken cancellationToken = default)
        {
            // No hace nada
            return Task.CompletedTask;
        }
        
        /// <inheritdoc />
        public Task RegisterAllAsync(IEnumerable<DomainEvent> eventos, CancellationToken cancellationToken = default)
        {
            // No hace nada
            return Task.CompletedTask;
        }
        
        /// <inheritdoc />
        public Task<IReadOnlyList<DomainEvent>> GetEventsForEntityAsync(Guid entityId, CancellationToken cancellationToken = default)
        {
            // Devuelve lista vacía
            return Task.FromResult<IReadOnlyList<DomainEvent>>(new List<DomainEvent>());
        }
        
        /// <inheritdoc />
        public Task<IReadOnlyList<T>> GetEventsByTypeAsync<T>(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default) where T : DomainEvent
        {
            // Devuelve lista vacía
            return Task.FromResult<IReadOnlyList<T>>(new List<T>());
        }
    }
    
    /// <summary>
    /// Implementación en memoria del registro de eventos.
    /// Útil para entornos de desarrollo y pruebas.
    /// </summary>
    internal class InMemoryDomainEventRegistry : IDomainEventRegistry
    {
        private readonly List<DomainEvent> _events = new List<DomainEvent>();
        
        /// <inheritdoc />
        public Task RegisterAsync(DomainEvent evento, CancellationToken cancellationToken = default)
        {
            _events.Add(evento);
            return Task.CompletedTask;
        }
        
        /// <inheritdoc />
        public Task RegisterAllAsync(IEnumerable<DomainEvent> eventos, CancellationToken cancellationToken = default)
        {
            _events.AddRange(eventos);
            return Task.CompletedTask;
        }
        
        /// <inheritdoc />
        public Task<IReadOnlyList<DomainEvent>> GetEventsForEntityAsync(Guid entityId, CancellationToken cancellationToken = default)
        {
            var result = _events
                .Where(e => e.EntityId == entityId)
                .OrderBy(e => e.OccurredOn)
                .ToList();
                
            return Task.FromResult<IReadOnlyList<DomainEvent>>(result);
        }
        
        /// <inheritdoc />
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
    }
} 