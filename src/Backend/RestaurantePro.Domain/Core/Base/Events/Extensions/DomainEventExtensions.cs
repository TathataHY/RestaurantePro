namespace RestaurantePro.Domain.Core.Base.Events.Extensions
{
    /// <summary>
    /// Extensiones para configurar los servicios relacionados con eventos de dominio
    /// </summary>
    public static class DomainEventExtensions
    {
        /// <summary>
        /// Agrega los servicios básicos de eventos de dominio al contenedor
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <returns>La misma colección para encadenamiento</returns>
        public static IServiceCollection AddDomainEvents(this IServiceCollection services)
        {
            // Registrar el despachador de eventos
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            
            return services;
        }
        
        /// <summary>
        /// Agrega el registro de eventos de dominio en memoria
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
        /// Escanea el ensamblado indicado para registrar todos los manejadores de eventos
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="assembly">Ensamblado a escanear (opcional, por defecto el ensamblado que llama)</param>
        /// <returns>La misma colección para encadenamiento</returns>
        public static IServiceCollection AddAllDomainEventHandlers(this IServiceCollection services, System.Reflection.Assembly? assembly = null)
        {
            // Si no se especificó un ensamblado, usar el ensamblado que llama
            assembly ??= System.Reflection.Assembly.GetCallingAssembly();
            
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
} 