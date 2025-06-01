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
            
            // Registrar manejador de eventos para invalidación automática de caché
            services.AddScoped<IDomainEventHandler<DomainEvent>, CacheInvalidationEventHandler>();
            
            // Registrar servicios compartidos
            // IDateTimeService para producción - se registra aquí como fallback
            services.AddScoped<Core.Base.Services.IDateTimeService, Core.Base.Services.DateTimeService>();
            services.AddScoped<IEventBasedNotificationService, EventBasedNotificationService>();
            
            // Registrar servicios de caché con telemetría y TTL dinámico
            services.AddSingleton<ICacheTelemetry, InMemoryCacheTelemetry>();
            services.AddSingleton<IDynamicTtlStrategy, UsageBasedTtlStrategy>();
            services.AddSingleton<ICacheService>(sp => 
            {
                var baseCacheService = new MemoryCacheService();
                var telemetry = sp.GetRequiredService<ICacheTelemetry>();
                var ttlStrategy = sp.GetRequiredService<IDynamicTtlStrategy>();
                return new SmartCacheDecorator(baseCacheService, telemetry, ttlStrategy);
            });
            
            // Registrar servicio de notificaciones con caché
            services.AddScoped<ServicioNotificaciones>(); // Implementación original
            services.AddScoped<IServicioNotificaciones>(sp => 
            {
                var original = sp.GetRequiredService<ServicioNotificaciones>();
                var cacheService = sp.GetRequiredService<ICacheService>();
                return new ServicioNotificacionesCached(original, cacheService);
            });
            services.AddScoped<IServicioNotificacionesCached>(sp => 
                (IServicioNotificacionesCached)sp.GetRequiredService<IServicioNotificaciones>());
            
            // Registrar servicio de usuarios con caché
            services.AddScoped<Usuarios.Services.UsuarioService>(); // Implementación original
            services.AddScoped<Usuarios.Services.IUsuarioService>(sp => 
            {
                var original = sp.GetRequiredService<Usuarios.Services.UsuarioService>();
                var cacheService = sp.GetRequiredService<ICacheService>();
                return new Usuarios.Services.UsuarioServiceCached(original, cacheService);
            });
            services.AddScoped<Usuarios.Services.IUsuarioServiceCached>(sp => 
                (Usuarios.Services.IUsuarioServiceCached)sp.GetRequiredService<Usuarios.Services.IUsuarioService>());
                
            // Registrar manejador de eventos específico para invalidación de caché de usuarios
            services.AddScoped<IDomainEventHandler<Usuarios.Events.Usuario.UsuarioCreado>, Usuarios.EventHandlers.CacheInvalidationUsuarioEventHandler>();
            services.AddScoped<IDomainEventHandler<Usuarios.Events.Usuario.UsuarioActualizado>, Usuarios.EventHandlers.CacheInvalidationUsuarioEventHandler>();
            services.AddScoped<IDomainEventHandler<Usuarios.Events.Usuario.UsuarioDesactivado>, Usuarios.EventHandlers.CacheInvalidationUsuarioEventHandler>();
            services.AddScoped<IDomainEventHandler<Usuarios.Events.Usuario.UsuarioActivado>, Usuarios.EventHandlers.CacheInvalidationUsuarioEventHandler>();
            services.AddScoped<IDomainEventHandler<Usuarios.Events.Usuario.UsuarioBloqueado>, Usuarios.EventHandlers.CacheInvalidationUsuarioEventHandler>();
            services.AddScoped<IDomainEventHandler<Usuarios.Events.Usuario.UsuarioDesbloqueado>, Usuarios.EventHandlers.CacheInvalidationUsuarioEventHandler>();
            services.AddScoped<IDomainEventHandler<Usuarios.Events.Usuario.RolAsignado>, Usuarios.EventHandlers.CacheInvalidationUsuarioEventHandler>();
            services.AddScoped<IDomainEventHandler<Usuarios.Events.Usuario.RolRemovido>, Usuarios.EventHandlers.CacheInvalidationUsuarioEventHandler>();
            
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
                
            // ✨ Nuevos servicios de dominio agregados durante reorganización de arquitectura
            // 6. Calculadora de Puntos
            services.AddScoped<Comercial.Services.ICalculadoraPuntosService, Comercial.Services.CalculadoraPuntosService>();
            
            // 7. Calculadora de Promociones
            services.AddScoped<Comercial.Promociones.Services.ICalculadoraPromocionesService, Comercial.Promociones.Services.CalculadoraPromocionesService>();
            
            // 8. Generador de Número de Tarjeta
            services.AddScoped<Comercial.Services.IGeneradorNumeroTarjetaService, Comercial.Services.GeneradorNumeroTarjetaService>();
            
            // 9. Generador de Número de Comanda
            services.AddScoped<Operaciones.Services.IGeneradorNumeroComandaService, Operaciones.Services.GeneradorNumeroComandaService>();
            
            // 10. Builder de Tarjeta de Fidelización (Domain Builder Pattern)
            services.AddScoped<Comercial.Clientes.Builders.TarjetaFidelizacionBuilder>();
            
            // 3. Servicio de Recetas
            services.AddScoped<RecetaService>(); // Implementación original
            services.AddScoped<IRecetaService>(sp => 
            {
                var original = sp.GetRequiredService<RecetaService>();
                var cacheService = sp.GetRequiredService<ICacheService>();
                return new RecetaServiceCached(original, cacheService, sp.GetRequiredService<INotificationManager>());
            });
            services.AddScoped<IRecetaServiceCached>(sp => 
                (IRecetaServiceCached)sp.GetRequiredService<IRecetaService>());
                
            // 4. Servicio de Productos y Categorías
            services.AddScoped<ProductoCategoriaService>(); // Implementación original
            services.AddScoped<IProductoCategoriaService>(sp => 
            {
                var original = sp.GetRequiredService<ProductoCategoriaService>();
                var cacheService = sp.GetRequiredService<ICacheService>();
                return new ProductoCategoriaServiceCached(original, cacheService);
            });
            services.AddScoped<IProductoCategoriaServiceCached>(sp => 
                (IProductoCategoriaServiceCached)sp.GetRequiredService<IProductoCategoriaService>());
            
            // Registrar políticas de dominio
            services.AddTransient<IClientesFrecuentesPolicy, ClientesFrecuentesPolicy>();
            services.AddTransient<IStockBajoPolicy, StockBajoPolicy>();
            services.AddTransient<IProductoRecomendadoPolicy, ProductoRecomendadoPolicy>();
            services.AddTransient<IVisibilidadCategoriasPolicy, VisibilidadCategoriasPolicy>();
            
            // Registrar especificaciones reutilizables
            services.AddTransient<Comercial.Clientes.Specifications.ClienteFrecuenteSpecification>();
            services.AddTransient<Inventario.Ingredientes.Specifications.IngredienteRotacionAltaSpecification>();
            services.AddTransient<Core.Productos.Specifications.ProductoDisponibleSpecification>();
            
            // 5. Generador de Órdenes de Compra
            services.AddScoped<Inventario.Services.GeneradorOrdenesCompra>(); // Implementación original
            services.AddScoped<Inventario.Services.IGeneradorOrdenesCompra>(sp => 
            {
                var original = sp.GetRequiredService<Inventario.Services.GeneradorOrdenesCompra>();
                var cacheService = sp.GetRequiredService<ICacheService>();
                return new Inventario.Services.GeneradorOrdenesCompraCached(original, cacheService);
            });
            services.AddScoped<Inventario.Services.IGeneradorOrdenesCompraCached>(sp => 
                (Inventario.Services.IGeneradorOrdenesCompraCached)sp.GetRequiredService<Inventario.Services.IGeneradorOrdenesCompra>());
            
            // Registrar interfaces de fachada para la capa de aplicación
            services.AddScoped<Comercial.Services.IComercialServiceFacade, Comercial.Services.ComercialServiceFacade>();
            services.AddScoped<Operaciones.Services.IOperacionesServiceFacade, Operaciones.Services.OperacionesServiceFacade>();
            services.AddScoped<Inventario.Services.IInventarioServiceFacade, Inventario.Services.InventarioServiceFacade>();
            services.AddScoped<Proveedores.Services.IProveedoresServiceFacade, Proveedores.Services.ProveedoresServiceFacade>();
            
            // Registrar servicios de integración entre contextos (ACL)
            // Registramos el servicio de integración de proveedores-comercial (trasladado desde Core a Comercial)
            services.AddScoped<Comercial.Services.IServicioIntegracionProveedores, Comercial.Services.ServicioIntegracionProveedores>();
            
            // Registro de servicio de integración entre Core y Operaciones
            services.AddScoped<Core.Services.ICoreOperacionesIntegrationService, Core.Services.CoreOperacionesIntegrationService>();
            
            // Registro de servicio de integración entre Operaciones e Inventario
            services.AddScoped<Operaciones.Services.IOperacionesInventarioIntegrationService, Operaciones.Services.OperacionesInventarioIntegrationService>();
            
            // 🍳 Registro del servicio de preparaciones diarias
            services.AddScoped<Operaciones.Preparaciones.Services.IServicioPreparaciones, Operaciones.Preparaciones.Services.ServicioPreparaciones>();
            
            // ✨ NUEVOS SERVICIOS DE INVENTARIO - Agregados durante resolución de errores de compilación
            // 11. Servicio de Validación de Inventario (Domain Service)
            services.AddScoped<Inventario.Services.IValidacionInventarioService, Inventario.Services.ValidacionInventarioService>();
            
            // 12. Servicio de Alertas de Stock (Domain Service)
            services.AddScoped<Inventario.Services.IAlertaStockService, Inventario.Services.AlertaStockService>();
            
            // 13. Servicio de Productos (Domain Service)
            services.AddScoped<Core.Productos.Services.IProductoService, Core.Productos.Services.ProductoService>();
            
            // Registrar manejadores de eventos de integración
            services.AddScoped<IDomainEventHandler<OrdenCompraAprobada>, Comercial.EventHandlers.OrdenCompraAprobada_NotificacionProveedorHandler>();
            services.AddScoped<IDomainEventHandler<OrdenCompraAprobada>, Comercial.EventHandlers.OrdenCompraAprobada_ActualizarEstadisticasProveedorHandler>();
            
            // 🍳 Registrar event handler de preparaciones
            services.AddScoped<IDomainEventHandler<Operaciones.Comandas.Events.Comanda.ComandaCreada>, Operaciones.EventHandlers.ComandaCreada_VerificarPreparacionesHandler>();
            
            // Registrar servicio de notificación
            services.AddScoped<INotification, Notification>();
            
            // Core services
            // IDateTimeService se implementará en Infrastructure
            // services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddScoped<IEventSubscriptionManager, EventSubscriptionManager>();
            services.AddScoped<IEventBasedNotificationService, EventBasedNotificationService>();
            
            // Core services - Product            
            // ... existing code ...
            
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
            
            // Registrar manejador de eventos para invalidación automática de caché en pruebas
            services.AddScoped<IDomainEventHandler<DomainEvent>, CacheInvalidationEventHandler>();
            
            // Usar MockDateTimeService para tests con fecha fija
            services.AddSingleton<IDateTimeService>(new Base.Services.MockDateTimeService(new DateTime(2024, 1, 15, 10, 0, 0)));
            services.AddScoped<IEventBasedNotificationService, EventBasedNotificationService>();
            
            // Registrar servicios de caché con telemetría y TTL dinámico para pruebas
            services.AddSingleton<ICacheTelemetry, InMemoryCacheTelemetry>();
            services.AddSingleton<IDynamicTtlStrategy, UsageBasedTtlStrategy>();
            services.AddSingleton<ICacheService>(sp => 
            {
                var baseCacheService = new MemoryCacheService();
                var telemetry = sp.GetRequiredService<ICacheTelemetry>();
                var ttlStrategy = sp.GetRequiredService<IDynamicTtlStrategy>();
                return new SmartCacheDecorator(baseCacheService, telemetry, ttlStrategy);
            });
            
            // Registrar servicio de notificaciones con caché para pruebas
            services.AddScoped<ServicioNotificaciones>(); // Implementación original
            services.AddScoped<IServicioNotificaciones>(sp => 
            {
                var original = sp.GetRequiredService<ServicioNotificaciones>();
                var cacheService = sp.GetRequiredService<ICacheService>();
                return new ServicioNotificacionesCached(original, cacheService);
            });
            services.AddScoped<IServicioNotificacionesCached>(sp => 
                (IServicioNotificacionesCached)sp.GetRequiredService<IServicioNotificaciones>());
            
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
                
            // ✨ Nuevos servicios de dominio agregados durante reorganización de arquitectura
            // 6. Calculadora de Puntos
            services.AddScoped<Comercial.Services.ICalculadoraPuntosService, Comercial.Services.CalculadoraPuntosService>();
            
            // 7. Calculadora de Promociones
            services.AddScoped<Comercial.Promociones.Services.ICalculadoraPromocionesService, Comercial.Promociones.Services.CalculadoraPromocionesService>();
            
            // 8. Generador de Número de Tarjeta
            services.AddScoped<Comercial.Services.IGeneradorNumeroTarjetaService, Comercial.Services.GeneradorNumeroTarjetaService>();
            
            // 9. Generador de Número de Comanda
            services.AddScoped<Operaciones.Services.IGeneradorNumeroComandaService, Operaciones.Services.GeneradorNumeroComandaService>();
            
            // 10. Builder de Tarjeta de Fidelización (Domain Builder Pattern)
            services.AddScoped<Comercial.Clientes.Builders.TarjetaFidelizacionBuilder>();
            
            // 3. Servicio de Recetas
            services.AddScoped<RecetaService>(); // Implementación original
            services.AddScoped<IRecetaService>(sp => 
            {
                var original = sp.GetRequiredService<RecetaService>();
                var cacheService = sp.GetRequiredService<ICacheService>();
                return new RecetaServiceCached(original, cacheService, sp.GetRequiredService<INotificationManager>());
            });
            services.AddScoped<IRecetaServiceCached>(sp => 
                (IRecetaServiceCached)sp.GetRequiredService<IRecetaService>());
                
            // 4. Servicio de Productos y Categorías
            services.AddScoped<ProductoCategoriaService>(); // Implementación original
            services.AddScoped<IProductoCategoriaService>(sp => 
            {
                var original = sp.GetRequiredService<ProductoCategoriaService>();
                var cacheService = sp.GetRequiredService<ICacheService>();
                return new ProductoCategoriaServiceCached(original, cacheService);
            });
            services.AddScoped<IProductoCategoriaServiceCached>(sp => 
                (IProductoCategoriaServiceCached)sp.GetRequiredService<IProductoCategoriaService>());
            
            // Registrar políticas de dominio
            services.AddTransient<IClientesFrecuentesPolicy, ClientesFrecuentesPolicy>();
            services.AddTransient<IStockBajoPolicy, StockBajoPolicy>();
            services.AddTransient<IProductoRecomendadoPolicy, ProductoRecomendadoPolicy>();
            services.AddTransient<IVisibilidadCategoriasPolicy, VisibilidadCategoriasPolicy>();
            
            // Registrar especificaciones reutilizables para pruebas
            services.AddTransient<Comercial.Clientes.Specifications.ClienteFrecuenteSpecification>();
            services.AddTransient<Inventario.Ingredientes.Specifications.IngredienteRotacionAltaSpecification>();
            services.AddTransient<Core.Productos.Specifications.ProductoDisponibleSpecification>();
            
            // 5. Generador de Órdenes de Compra para pruebas
            services.AddScoped<Inventario.Services.GeneradorOrdenesCompra>(); // Implementación original
            services.AddScoped<Inventario.Services.IGeneradorOrdenesCompra>(sp => 
            {
                var original = sp.GetRequiredService<Inventario.Services.GeneradorOrdenesCompra>();
                var cacheService = sp.GetRequiredService<ICacheService>();
                return new Inventario.Services.GeneradorOrdenesCompraCached(original, cacheService);
            });
            services.AddScoped<Inventario.Services.IGeneradorOrdenesCompraCached>(sp => 
                (Inventario.Services.IGeneradorOrdenesCompraCached)sp.GetRequiredService<Inventario.Services.IGeneradorOrdenesCompra>());
            
            // Registrar mocks de interfaces de fachada para pruebas
            // Aquí se pueden usar implementaciones simuladas para pruebas
            
            // Registro de servicio de integración entre Core y Operaciones para pruebas
            services.AddScoped<Core.Services.ICoreOperacionesIntegrationService, Core.Services.CoreOperacionesIntegrationService>();
            
            // Registro de servicio de integración entre Operaciones e Inventario para pruebas
            services.AddScoped<Operaciones.Services.IOperacionesInventarioIntegrationService, Operaciones.Services.OperacionesInventarioIntegrationService>();
            
            // 🍳 Registro del servicio de preparaciones diarias para pruebas
            services.AddScoped<Operaciones.Preparaciones.Services.IServicioPreparaciones, Operaciones.Preparaciones.Services.ServicioPreparaciones>();
            
            // 🍳 Registrar event handler de preparaciones para pruebas
            services.AddScoped<IDomainEventHandler<Operaciones.Comandas.Events.Comanda.ComandaCreada>, Operaciones.EventHandlers.ComandaCreada_VerificarPreparacionesHandler>();
            
            // Agregar el registro del servicio de usuarios con caché para pruebas
            services.AddScoped<Usuarios.Services.UsuarioService>(); // Implementación original
            services.AddScoped<Usuarios.Services.IUsuarioService>(sp => 
            {
                var original = sp.GetRequiredService<Usuarios.Services.UsuarioService>();
                var cacheService = sp.GetRequiredService<ICacheService>();
                return new Usuarios.Services.UsuarioServiceCached(original, cacheService);
            });
            services.AddScoped<Usuarios.Services.IUsuarioServiceCached>(sp => 
                (Usuarios.Services.IUsuarioServiceCached)sp.GetRequiredService<Usuarios.Services.IUsuarioService>());
                
            // Registrar manejador de eventos específico para invalidación de caché de usuarios en pruebas
            services.AddScoped<IDomainEventHandler<Usuarios.Events.Usuario.UsuarioCreado>, Usuarios.EventHandlers.CacheInvalidationUsuarioEventHandler>();
            services.AddScoped<IDomainEventHandler<Usuarios.Events.Usuario.UsuarioActualizado>, Usuarios.EventHandlers.CacheInvalidationUsuarioEventHandler>();
            services.AddScoped<IDomainEventHandler<Usuarios.Events.Usuario.UsuarioDesactivado>, Usuarios.EventHandlers.CacheInvalidationUsuarioEventHandler>();
            services.AddScoped<IDomainEventHandler<Usuarios.Events.Usuario.UsuarioActivado>, Usuarios.EventHandlers.CacheInvalidationUsuarioEventHandler>();
            services.AddScoped<IDomainEventHandler<Usuarios.Events.Usuario.UsuarioBloqueado>, Usuarios.EventHandlers.CacheInvalidationUsuarioEventHandler>();
            services.AddScoped<IDomainEventHandler<Usuarios.Events.Usuario.UsuarioDesbloqueado>, Usuarios.EventHandlers.CacheInvalidationUsuarioEventHandler>();
            services.AddScoped<IDomainEventHandler<Usuarios.Events.Usuario.RolAsignado>, Usuarios.EventHandlers.CacheInvalidationUsuarioEventHandler>();
            services.AddScoped<IDomainEventHandler<Usuarios.Events.Usuario.RolRemovido>, Usuarios.EventHandlers.CacheInvalidationUsuarioEventHandler>();
            
            // 🍳 Registro del servicio de preparaciones diarias para pruebas
            services.AddScoped<Operaciones.Preparaciones.Services.IServicioPreparaciones, Operaciones.Preparaciones.Services.ServicioPreparaciones>();
            
            // ✨ NUEVOS SERVICIOS DE INVENTARIO - Para pruebas
            // 11. Servicio de Validación de Inventario (Domain Service)
            services.AddScoped<Inventario.Services.IValidacionInventarioService, Inventario.Services.ValidacionInventarioService>();
            
            // 12. Servicio de Alertas de Stock (Domain Service)
            services.AddScoped<Inventario.Services.IAlertaStockService, Inventario.Services.AlertaStockService>();
            
            // 13. Servicio de Productos (Domain Service) - Para pruebas
            services.AddScoped<Core.Productos.Services.IProductoService, Core.Productos.Services.ProductoService>();
            
            return services;
        }
    }
} 
