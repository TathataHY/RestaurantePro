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
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatrServices();
        services.AddAutoMapperServices();
        services.AddFluentValidationServices();
        services.AddCachingServices();
        services.AddContextServices();

        return services;
    }

    /// <summary>
    /// Configura MediatR para Vertical Slices Architecture
    /// </summary>
    private static IServiceCollection AddMediatrServices(this IServiceCollection services)
    {
        // Registrar MediatR desde el assembly actual
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Registrar behaviors del pipeline (en orden de ejecución)
        // 1. Logging - Para trackear inicio/fin de operaciones
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        
        // 2. Performance - Para monitorear operaciones lentas
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        
        // 3. Caching - Para optimizar consultas repetidas (solo queries)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        
        // 4. Validation - Para validar datos antes de procesar (último antes del handler)
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

        return services;
    }

    /// <summary>
    /// Registra servicios específicos del contexto Core
    /// </summary>
    private static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // TODO: Registrar servicios de aplicación específicos del contexto Core
        // Por ejemplo: servicios de negocio, factories, etc.
        
        return services;
    }

    /// <summary>
    /// Registra servicios específicos del contexto Comercial
    /// </summary>
    private static IServiceCollection AddComercialServices(this IServiceCollection services)
    {
        // TODO: Registrar servicios de aplicación específicos del contexto Comercial
        // Por ejemplo: servicios de fidelización, cálculo de descuentos, etc.
        
        return services;
    }

    /// <summary>
    /// Registra servicios específicos del contexto Operaciones
    /// </summary>
    private static IServiceCollection AddOperacionesServices(this IServiceCollection services)
    {
        // TODO: Registrar servicios de aplicación específicos del contexto Operaciones
        // Por ejemplo: servicios de gestión de comandas, reservaciones, etc.
        
        return services;
    }

    /// <summary>
    /// Registra servicios específicos del contexto Inventario
    /// </summary>
    private static IServiceCollection AddInventarioServices(this IServiceCollection services)
    {
        // TODO: Registrar servicios de aplicación específicos del contexto Inventario
        // Por ejemplo: servicios de cálculo de stock, alertas, etc.
        
        return services;
    }

    /// <summary>
    /// Registra servicios específicos del contexto Proveedores
    /// </summary>
    private static IServiceCollection AddProveedoresServices(this IServiceCollection services)
    {
        // TODO: Registrar servicios de aplicación específicos del contexto Proveedores
        // Por ejemplo: servicios de evaluación de proveedores, gestión de contactos, etc.
        
        return services;
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