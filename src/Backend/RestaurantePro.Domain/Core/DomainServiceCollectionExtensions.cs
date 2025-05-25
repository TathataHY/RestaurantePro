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
            services.AddScoped<IEventBasedNotificationService, EventBasedNotificationService>();
            
            // Registrar servicio de caché
            services.AddSingleton<ICacheService, MemoryCacheService>();
            
            // Registrar servicios de dominio decorados con caché
            // 1. Verificador de Stock
            services.AddScoped<VerificadorStock>(); // Implementación original
            services.AddScoped<IVerificadorStock>(sp => 
            {
                var original = sp.GetRequiredService<VerificadorStock>();
                var cacheService = sp.GetRequiredService<ICacheService>();
                return new VerificadorStockCached(original, cacheService);
            });
            services.AddScoped<IVerificadorStockCached>(sp => 
                (IVerificadorStockCached)sp.GetRequiredService<IVerificadorStock>());
                
            // 2. Servicio de Fidelización
            services.AddScoped<ServicioFidelizacion>(); // Implementación original
            services.AddScoped<IServicioFidelizacion>(sp => 
            {
                var original = sp.GetRequiredService<ServicioFidelizacion>();
                var cacheService = sp.GetRequiredService<ICacheService>();
                return new ServicioFidelizacionCached(original, cacheService);
            });
            services.AddScoped<IServicioFidelizacionCached>(sp => 
                (IServicioFidelizacionCached)sp.GetRequiredService<IServicioFidelizacion>());
            
            // Registrar políticas de dominio
            services.AddTransient<IClientesFrecuentesPolicy, ClientesFrecuentesPolicy>();
            services.AddTransient<IStockBajoPolicy, StockBajoPolicy>();
            services.AddTransient<IProductoRecomendadoPolicy, ProductoRecomendadoPolicy>();
            services.AddTransient<IVisibilidadCategoriasPolicy, VisibilidadCategoriasPolicy>();
            
            // Registrar especificaciones reutilizables
            services.AddTransient<Comercial.Clientes.Specifications.ClienteFrecuenteSpecification>();
            services.AddTransient<Inventario.Ingredientes.Specifications.IngredienteRotacionAltaSpecification>();
            services.AddTransient<Core.Productos.Specifications.ProductoDisponibleSpecification>();
            
            // Registrar interfaces de fachada para la capa de aplicación
            services.AddScoped<Comercial.Services.IComercialServiceFacade, Comercial.Services.ComercialServiceFacade>();
            services.AddScoped<Operaciones.Services.IOperacionesServiceFacade, Operaciones.Services.OperacionesServiceFacade>();
            services.AddScoped<Inventario.Services.IInventarioServiceFacade, Inventario.Services.InventarioServiceFacade>();
            services.AddScoped<Proveedores.Services.IProveedoresServiceFacade, Proveedores.Services.ProveedoresServiceFacade>();
            
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
            services.AddScoped<IEventBasedNotificationService, EventBasedNotificationService>();
            
            // Registrar servicio de caché para pruebas (singleton para mantenerlo en memoria durante las pruebas)
            services.AddSingleton<ICacheService, MemoryCacheService>();
            
            // Registrar servicios de dominio decorados con caché para pruebas
            // 1. Verificador de Stock
            services.AddScoped<VerificadorStock>(); // Implementación original
            services.AddScoped<IVerificadorStock>(sp => 
            {
                var original = sp.GetRequiredService<VerificadorStock>();
                var cacheService = sp.GetRequiredService<ICacheService>();
                return new VerificadorStockCached(original, cacheService);
            });
            services.AddScoped<IVerificadorStockCached>(sp => 
                (IVerificadorStockCached)sp.GetRequiredService<IVerificadorStock>());
                
            // 2. Servicio de Fidelización
            services.AddScoped<ServicioFidelizacion>(); // Implementación original
            services.AddScoped<IServicioFidelizacion>(sp => 
            {
                var original = sp.GetRequiredService<ServicioFidelizacion>();
                var cacheService = sp.GetRequiredService<ICacheService>();
                return new ServicioFidelizacionCached(original, cacheService);
            });
            services.AddScoped<IServicioFidelizacionCached>(sp => 
                (IServicioFidelizacionCached)sp.GetRequiredService<IServicioFidelizacion>());
            
            // Registrar políticas de dominio
            services.AddTransient<IClientesFrecuentesPolicy, ClientesFrecuentesPolicy>();
            services.AddTransient<IStockBajoPolicy, StockBajoPolicy>();
            services.AddTransient<IProductoRecomendadoPolicy, ProductoRecomendadoPolicy>();
            services.AddTransient<IVisibilidadCategoriasPolicy, VisibilidadCategoriasPolicy>();
            
            // Registrar especificaciones reutilizables para pruebas
            services.AddTransient<Comercial.Clientes.Specifications.ClienteFrecuenteSpecification>();
            services.AddTransient<Inventario.Ingredientes.Specifications.IngredienteRotacionAltaSpecification>();
            services.AddTransient<Core.Productos.Specifications.ProductoDisponibleSpecification>();
            
            // Registrar mocks de interfaces de fachada para pruebas
            // Aquí se pueden usar implementaciones simuladas para pruebas
            
            return services;
        }
    }
} 
