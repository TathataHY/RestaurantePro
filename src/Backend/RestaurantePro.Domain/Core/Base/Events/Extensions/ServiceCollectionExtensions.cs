namespace RestaurantePro.Domain.Core.Base.Events.Extensions
{
    /// <summary>
    /// Extensiones para configurar servicios relacionados con eventos de dominio
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registra los servicios core de eventos de dominio
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
} 