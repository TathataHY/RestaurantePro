using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using AutoMapper;
using FluentValidation;
using MediatR;
using RestaurantePro.Application.Common.Behaviors;
using RestaurantePro.Application.Config.Mappings;
using RestaurantePro.Application.Config.Settings;
using RestaurantePro.Application.Comercial.Promociones.Commands.ActivarPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.PausarPromocion;
using System.Reflection;

namespace RestaurantePro.Application.Config.DependencyInjection;

/// <summary>
/// Configuración principal de servicios para la capa Application
/// Registra MediatR, AutoMapper, FluentValidation y servicios por contexto
/// </summary>
public static class ApplicationServiceCollection
{
    /// <summary>
    /// Registra todos los servicios de la capa Application
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar settings de aplicación
        services.AddAppSettings(configuration);
        
        // Configurar AutoMapper con todos los profiles
        services.AddAutoMapperServices();

        // Configurar MediatR para Commands y Queries
        services.AddMediatrServices();

        // Configurar FluentValidation
        services.AddFluentValidationServices();

        // Configurar servicios de caché
        services.AddCachingServices();

        // Registrar servicios específicos por contexto
        services.AddContextServices();

        return services;
    }

    /// <summary>
    /// Configura los settings de aplicación desde appsettings.json
    /// </summary>
    private static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
    {
        // Registrar AppSettings principal
        services.Configure<AppSettings>(configuration.GetSection(AppSettings.SectionName));
        
        // Registrar settings específicos para inyección directa si es necesario
        services.Configure<NotificationSettings>(configuration.GetSection($"{AppSettings.SectionName}:Notifications"));
        services.Configure<CacheSettings>(configuration.GetSection($"{AppSettings.SectionName}:Cache"));
        services.Configure<BackgroundJobSettings>(configuration.GetSection($"{AppSettings.SectionName}:BackgroundJobs"));
        services.Configure<BusinessRulesSettings>(configuration.GetSection($"{AppSettings.SectionName}:BusinessRules"));
        
        // 🆕 Registrar settings de behaviors
        services.Configure<BehaviorSettings>(configuration.GetSection($"{AppSettings.SectionName}:Behaviors"));
        services.Configure<PerformanceSettings>(configuration.GetSection($"{AppSettings.SectionName}:Behaviors:Performance"));
        services.Configure<RetrySettings>(configuration.GetSection($"{AppSettings.SectionName}:Behaviors:Retry"));
        services.Configure<AuditingSettings>(configuration.GetSection($"{AppSettings.SectionName}:Behaviors:Auditing"));
        services.Configure<TransactionSettings>(configuration.GetSection($"{AppSettings.SectionName}:Behaviors:Transaction"));
        
        // 🆕 Registrar settings de métricas
        services.Configure<MetricsSettings>(configuration.GetSection($"{AppSettings.SectionName}:Metrics"));
        
        return services;
    }

    /// <summary>
    /// Configura MediatR para Vertical Slices Architecture
    /// </summary>
    private static IServiceCollection AddMediatrServices(this IServiceCollection services)
    {
        // Registrar MediatR desde el assembly actual (versión 11.x compatible)
        services.AddMediatR(Assembly.GetExecutingAssembly());

        // 🔧 REGISTRO MANUAL DE HANDLERS PROBLEMÁTICOS PARA DEBUGGING
        services.AddTransient<ActivarPromocionHandler>();
        services.AddTransient<PausarPromocionHandler>();

        // Registrar behaviors del pipeline (en orden de ejecución)
        // 1. Exception Handling - Para capturar y convertir excepciones
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
        
        // 2. Auditing - Para auditoría automática de commands
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditingBehavior<,>));
        
        // 3. Transaction - Para manejo automático de transacciones
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        
        // 4. Retry - Para reintentos automáticos en errores transitorios
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
        
        // 5. Logging - Para trackear inicio/fin de operaciones
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        
        // 6. Performance - Para monitorear operaciones lentas (mejorado con métricas)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        
        // 7. Caching - Para optimizar consultas repetidas (solo queries)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        
        // 8. Validation - Para validar datos antes de procesar (último antes del handler)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }

    /// <summary>
    /// Configura AutoMapper con profiles por contexto
    /// </summary>
    private static IServiceCollection AddAutoMapperServices(this IServiceCollection services)
    {
        // Configurar AutoMapper manualmente para evitar ambigüedad de versiones
        services.AddSingleton<IMapper>(provider =>
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CoreMappingProfile>();
                cfg.AddProfile<ComercialMappingProfile>();
                cfg.AddProfile<OperacionesMappingProfile>();
                cfg.AddProfile<InventarioMappingProfile>();
                cfg.AddProfile<ProveedoresMappingProfile>();
            });
            return configuration.CreateMapper();
        });

        return services;
    }

    /// <summary>
    /// Configura FluentValidation para validación automática
    /// </summary>
    private static IServiceCollection AddFluentValidationServices(this IServiceCollection services)
    {
        // Registrar todos los validadores del assembly actual
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Configurar validación automática
        services.Configure<ValidationSettings>(options =>
        {
            options.ValidateOnCreate = true;
            options.ValidateOnUpdate = true;
            options.ValidateComplexTypes = true;
        });

        return services;
    }

    /// <summary>
    /// Configura servicios de caché para CachingBehavior
    /// </summary>
    private static IServiceCollection AddCachingServices(this IServiceCollection services)
    {
        // Agregar MemoryCache para CachingBehavior
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1024; // Limitar a 1024 entradas
            options.CompactionPercentage = 0.25; // Compactar cuando se alcance el límite
        });

        return services;
    }

    /// <summary>
    /// Registra servicios específicos por contexto
    /// </summary>
    private static IServiceCollection AddContextServices(this IServiceCollection services)
    {
        services.AddCoreServices();
        services.AddComercialServices();
        services.AddOperacionesServices();
        services.AddInventarioServices();
        services.AddProveedoresServices();

        // 🔥 ACTIVAR: Registrar todos los Domain Event Handlers
        RegisterDomainEventHandlers(services);

        return services;
    }

    /// <summary>
    /// Registra servicios específicos del contexto Core
    /// </summary>
    private static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // ✅ SERVICIOS DE PRODUCTOS
        // Servicios de negocio para gestión de productos
        // TODO: Agregar cuando se implementen servicios específicos como:
        // services.AddScoped<IProductoRecommendationService, ProductoRecommendationService>();
        // services.AddScoped<IRecetaCostCalculatorService, RecetaCostCalculatorService>();
        
        // ✅ SERVICIOS DE USUARIOS
        // Servicios de autenticación y autorización
        // TODO: Agregar cuando se implementen servicios específicos como:
        // services.AddScoped<IUserPermissionService, UserPermissionService>();
        // services.AddScoped<IPasswordHashingService, PasswordHashingService>();
        
        // ✅ SERVICIOS DE NOTIFICACIONES CORE
        // Ya registrados en Infrastructure:
        // - INotificationService 
        // - ISignalRService (para notificaciones tiempo real)
        
        // ✅ SERVICIOS DE RECETAS
        // Servicios para cálculo de costos y gestión de recetas
        // TODO: Agregar cuando se implementen servicios específicos como:
        // services.AddScoped<IRecetaAnalysisService, RecetaAnalysisService>();
        // services.AddScoped<IIngredientSubstitutionService, IngredientSubstitutionService>();
        
        return services;
    }

    /// <summary>
    /// Registra servicios específicos del contexto Comercial
    /// </summary>
    private static IServiceCollection AddComercialServices(this IServiceCollection services)
    {
        // ✅ SERVICIOS DE FIDELIZACIÓN
        // Ya tenemos el Domain Service IServicioFidelizacion registrado en Domain
        // TODO: Agregar servicios de aplicación específicos como:
        // services.AddScoped<IFidelizacionAnalyticsService, FidelizacionAnalyticsService>();
        // services.AddScoped<IPromocionEngineService, PromocionEngineService>();
        
        // ✅ SERVICIOS DE FACTURACIÓN
        // Ya tenemos el Domain Service IServicioFacturacion registrado en Domain
        // TODO: Agregar servicios de aplicación específicos como:
        // services.AddScoped<IFacturacionReportService, FacturacionReportService>();
        // services.AddScoped<IPagoProcessorService, PagoProcessorService>();
        
        // ✅ SERVICIOS DE CLIENTES
        // Servicios para gestión avanzada de clientes
        // TODO: Agregar cuando se implementen servicios específicos como:
        // services.AddScoped<IClienteSegmentationService, ClienteSegmentationService>();
        // services.AddScoped<IClienteAnalyticsService, ClienteAnalyticsService>();
        
        // ✅ SERVICIOS DE COMUNICACIÓN
        // Ya registrados en Infrastructure:
        // - IEmailService
        // - ISMSService
        
        // ✅ DOMAIN SERVICE FACADES
        // Ya registrado el ComercialServiceFacade en Domain para Commands avanzados
        
        return services;
    }

    /// <summary>
    /// Registra servicios específicos del contexto Operaciones
    /// </summary>
    private static IServiceCollection AddOperacionesServices(this IServiceCollection services)
    {
        // ✅ SERVICIOS DE COMANDAS
        // Servicios para gestión avanzada de comandas y pedidos
        // TODO: Agregar cuando se implementen servicios específicos como:
        // services.AddScoped<IComandaOptimizationService, ComandaOptimizationService>();
        // services.AddScoped<IKitchenWorkflowService, KitchenWorkflowService>();
        
        // ✅ SERVICIOS DE RESERVACIONES
        // Servicios para gestión de disponibilidad y reservaciones
        // TODO: Agregar cuando se implementen servicios específicos como:
        // services.AddScoped<IDisponibilidadCalculatorService, DisponibilidadCalculatorService>();
        // services.AddScoped<IReservacionOptimizationService, ReservacionOptimizationService>();
        
        // ✅ SERVICIOS DE MESAS
        // Servicios para gestión y optimización de mesas
        // TODO: Agregar cuando se implementen servicios específicos como:
        // services.AddScoped<IMesaAllocationService, MesaAllocationService>();
        // services.AddScoped<IMesaStatusTrackingService, MesaStatusTrackingService>();
        
        // ✅ SERVICIOS DE PREPARACIONES
        // Servicios para planificación de preparaciones diarias
        // TODO: Agregar cuando se implementen servicios específicos como:
        // services.AddScoped<IPreparacionPlanningService, PreparacionPlanningService>();
        // services.AddScoped<IPreparacionAnalyticsService, PreparacionAnalyticsService>();
        
        // ✅ DOMAIN SERVICE FACADES
        // Ya registrado el OperacionesServiceFacade en Domain para Commands avanzados
        
        // ✅ SERVICIOS DE TRABAJO EN SEGUNDO PLANO
        // Ya registrado en Infrastructure:
        // - IBackgroundJobService (para recordatorios y trabajos automáticos)
        
        return services;
    }

    /// <summary>
    /// Registra servicios específicos del contexto Inventario
    /// </summary>
    private static IServiceCollection AddInventarioServices(this IServiceCollection services)
    {
        // ✅ SERVICIOS DE ANÁLISIS DE INVENTARIO
        // Ya implementamos el Query ObtenerAnalisisInventario con Machine Learning
        // TODO: Agregar servicios de aplicación específicos como:
        // services.AddScoped<IInventarioMLPredictionService, InventarioMLPredictionService>();
        // services.AddScoped<IStockOptimizationService, StockOptimizationService>();
        
        // ✅ SERVICIOS DE ALERTAS
        // Servicios para gestión inteligente de alertas de stock
        // TODO: Agregar cuando se implementen servicios específicos como:
        // services.AddScoped<IInventarioAlertService, InventarioAlertService>();
        // services.AddScoped<IStockAlertPriorityService, StockAlertPriorityService>();
        
        // ✅ SERVICIOS DE MOVIMIENTOS
        // Servicios para tracking y análisis de movimientos
        // TODO: Agregar cuando se implementen servicios específicos como:
        // services.AddScoped<IMovimientoTrackingService, MovimientoTrackingService>();
        // services.AddScoped<IMovimientoAnalyticsService, MovimientoAnalyticsService>();
        
        // ✅ SERVICIOS DE ÓRDENES DE COMPRA
        // Servicios para automatización de órdenes
        // TODO: Agregar cuando se implementen servicios específicos como:
        // services.AddScoped<IOrdenCompraAutomationService, OrdenCompraAutomationService>();
        // services.AddScoped<IProveedorSelectorService, ProveedorSelectorService>();
        
        // ✅ SERVICIOS DE VALORACIÓN
        // Servicios para cálculo de costos y valoración
        // TODO: Agregar cuando se implementen servicios específicos como:
        // services.AddScoped<IInventarioValuationService, InventarioValuationService>();
        // services.AddScoped<ICostCalculatorService, CostCalculatorService>();
        
        return services;
    }

    /// <summary>
    /// Registra servicios específicos del contexto Proveedores
    /// </summary>
    private static IServiceCollection AddProveedoresServices(this IServiceCollection services)
    {
        // ✅ SERVICIOS DE EVALUACIÓN DE PROVEEDORES
        // Servicios para análisis y evaluación de desempeño
        // TODO: Agregar cuando se implementen servicios específicos como:
        // services.AddScoped<IProveedorEvaluationService, ProveedorEvaluationService>();
        // services.AddScoped<IProveedorPerformanceAnalyticsService, ProveedorPerformanceAnalyticsService>();
        
        // ✅ SERVICIOS DE GESTIÓN DE CONTACTOS
        // Servicios para comunicación y gestión de contactos
        // TODO: Agregar cuando se implementen servicios específicos como:
        // services.AddScoped<IContactoManagementService, ContactoManagementService>();
        // services.AddScoped<IProveedorCommunicationService, ProveedorCommunicationService>();
        
        // ✅ SERVICIOS DE CATEGORIZACIÓN
        // Servicios para clasificación y categorización automática
        // TODO: Agregar cuando se implementen servicios específicos como:
        // services.AddScoped<IProveedorCategorizationService, ProveedorCategorizationService>();
        // services.AddScoped<IProveedorMatchingService, ProveedorMatchingService>();
        
        // ✅ SERVICIOS DE INTEGRACIÓN
        // Servicios para integración con sistemas externos
        // TODO: Agregar cuando se implementen servicios específicos como:
        // services.AddScoped<IProveedorIntegrationService, ProveedorIntegrationService>();
        // services.AddScoped<IProveedorAPIService, ProveedorAPIService>();
        
        // ✅ SERVICIOS DE REPORTING
        // Servicios para reportes y análisis de proveedores
        // TODO: Agregar cuando se implementen servicios específicos como:
        // services.AddScoped<IProveedorReportingService, ProveedorReportingService>();
        // services.AddScoped<IProveedorContractService, ProveedorContractService>();
        
        return services;
    }

    /// <summary>
    /// 🔥 Registra todos los Domain Event Handlers para procesamiento automático de eventos
    /// </summary>
    private static void RegisterDomainEventHandlers(IServiceCollection services)
    {
        // ========================================================================================
        // 🍽️ OPERACIONES - COMANDAS EVENT HANDLERS
        // ========================================================================================
        
        // ComandaCreada Event Handlers
        services.AddScoped<Domain.Core.Base.Events.Handlers.IDomainEventHandler<Domain.Operaciones.Comandas.Events.Comanda.ComandaCreada>, 
            Operaciones.Comandas.EventHandlers.ComandaCreada.ComandaCreadaInventarioHandler>();
        
        // ComandaFinalizada Event Handlers
        services.AddScoped<Domain.Core.Base.Events.Handlers.IDomainEventHandler<Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada>, 
            Operaciones.Comandas.EventHandlers.ComandaFinalizada.ComandaFinalizadaFidelizacionHandler>();
        
        services.AddScoped<Domain.Core.Base.Events.Handlers.IDomainEventHandler<Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada>, 
            Operaciones.Comandas.EventHandlers.ComandaFinalizada.ComandaFinalizadaMesaHandler>();
        
        // ========================================================================================
        // 📅 OPERACIONES - RESERVACIONES EVENT HANDLERS
        // ========================================================================================
        
        // ReservacionCreada Event Handlers
        services.AddScoped<Domain.Core.Base.Events.Handlers.IDomainEventHandler<Domain.Operaciones.Reservaciones.Events.Reservacion.ReservacionCreada>, 
            Operaciones.Reservaciones.EventHandlers.ReservacionCreada.ReservacionCreadaNotificacionHandler>();
        
        services.AddScoped<Domain.Core.Base.Events.Handlers.IDomainEventHandler<Domain.Operaciones.Reservaciones.Events.Reservacion.ReservacionCreada>, 
            Operaciones.Reservaciones.EventHandlers.ReservacionCreada.ReservacionCreadaMesaHandler>();
        
        // ReservacionCancelada Event Handlers
        services.AddScoped<Domain.Core.Base.Events.Handlers.IDomainEventHandler<Domain.Operaciones.Reservaciones.Events.Reservacion.ReservacionCancelada>, 
            Operaciones.Reservaciones.EventHandlers.ReservacionCancelada.ReservacionCanceladaMesaHandler>();
        
        // ========================================================================================
        // 💰 COMERCIAL - FACTURACIÓN EVENT HANDLERS
        // ========================================================================================
        
        // FacturaCreada Event Handlers
        services.AddScoped<Domain.Core.Base.Events.Handlers.IDomainEventHandler<Domain.Comercial.Facturacion.Events.FacturaCreada>, 
            Comercial.Facturacion.EventHandlers.FacturaCreada.FacturaCreadaNotificacionHandler>();
        
        // ========================================================================================
        // 🎯 TODO: PRÓXIMOS EVENT HANDLERS A IMPLEMENTAR
        // ========================================================================================
        
        // ProductoAgregadoAComanda → Actualizar stock automáticamente
        // services.AddScoped<Domain.Core.Base.Events.Handlers.IDomainEventHandler<Domain.Operaciones.Comandas.Events.Comanda.ProductoAgregadoAComanda>, 
        //     Operaciones.Comandas.EventHandlers.ProductoAgregadoAComanda.ProductoAgregadoStockHandler>();
        
        // FacturaPagada → Confirmar pago + Liberar servicios
        // services.AddScoped<Domain.Core.Base.Events.Handlers.IDomainEventHandler<Domain.Comercial.Facturacion.Events.FacturaPagada>, 
        //     Comercial.Facturacion.EventHandlers.FacturaPagada.FacturaPagadaConfirmacionHandler>();
        
        // ReservacionConfirmada → Preparar mesa + Notificar personal
        // services.AddScoped<Domain.Core.Base.Events.Handlers.IDomainEventHandler<Domain.Operaciones.Reservaciones.Events.Reservacion.ReservacionConfirmada>, 
        //     Operaciones.Reservaciones.EventHandlers.ReservacionConfirmada.ReservacionConfirmadaPreparacionHandler>();
        
        // MesaAsignada → Notificar mesero + Actualizar disponibilidad
        // services.AddScoped<Domain.Core.Base.Events.Handlers.IDomainEventHandler<Domain.Operaciones.Mesas.Events.MesaAsignada>, 
        //     Operaciones.Mesas.EventHandlers.MesaAsignada.MesaAsignadaNotificacionHandler>();
    }
}

/// <summary>
/// Configuración para FluentValidation
/// </summary>
public class ValidationSettings
{
    public bool ValidateOnCreate { get; set; } = true;
    public bool ValidateOnUpdate { get; set; } = true;
    public bool ValidateComplexTypes { get; set; } = true;
} 