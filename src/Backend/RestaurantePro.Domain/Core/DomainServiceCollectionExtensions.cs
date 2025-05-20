namespace RestaurantePro.Domain.Core
{
    /// <summary>
    /// Extensiones para configurar servicios de dominio
    /// </summary>
    public static class DomainServiceCollectionExtensions
    {
        /// <summary>
        /// Registra todos los servicios y políticas de dominio
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <returns>La colección de servicios con los servicios de dominio agregados</returns>
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            // Registrar eventos de dominio (con suscripciones y registro)
            services.AddDomainEventServicesComplete();
            
            // Registrar servicios compartidos
            services.AddTransient<IDateTimeService, DateTimeService>();
            
            // Registrar políticas de dominio
            services.AddTransient<IClientesFrecuentesPolicy, ClientesFrecuentesPolicy>();
            services.AddTransient<IStockBajoPolicy, StockBajoPolicy>();
            services.AddTransient<IProductoRecomendadoPolicy, ProductoRecomendadoPolicy>();
            services.AddTransient<IVisibilidadCategoriasPolicy, VisibilidadCategoriasPolicy>();
            
            return services;
        }
        
        /// <summary>
        /// Registra servicios de dominio para pruebas
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <returns>La colección de servicios con los servicios de dominio para pruebas</returns>
        public static IServiceCollection AddDomainServicesForTests(this IServiceCollection services)
        {
            // Registrar eventos de dominio pero con implementación nula del registro
            services.AddDomainEventServices();
            
            // Usar directamente el MockDateTimeService de Core/SharedKernel/Services
            services.AddSingleton<IDateTimeService>(new MockDateTimeService(DateTime.Now));
            
            // Registrar políticas de dominio
            services.AddTransient<IClientesFrecuentesPolicy, ClientesFrecuentesPolicy>();
            services.AddTransient<IStockBajoPolicy, StockBajoPolicy>();
            services.AddTransient<IProductoRecomendadoPolicy, ProductoRecomendadoPolicy>();
            services.AddTransient<IVisibilidadCategoriasPolicy, VisibilidadCategoriasPolicy>();
            
            return services;
        }
    }
} 