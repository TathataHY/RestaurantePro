using System;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Infrastructure.BackgroundTasks.Interfaces;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Core;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Comercial;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Inventario;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Operaciones;
using RestaurantePro.Infrastructure.BackgroundTasks.Schedulers;
using RestaurantePro.Infrastructure.BackgroundTasks.Workers;

namespace RestaurantePro.Infrastructure.DependencyInjection;

/// <summary>
/// Configuración de servicios para tareas en segundo plano
/// </summary>
public static class BackgroundTasksSetup
{
    /// <summary>
    /// Registra los servicios de tareas en segundo plano en la colección de servicios
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="configuration">Configuración de la aplicación</param>
    /// <param name="isTestEnvironment">Indica si el entorno es de prueba</param>
    /// <returns>La colección de servicios actualizada</returns>
    public static IServiceCollection AddBackgroundTasksServices(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isTestEnvironment = false)
    {
        // Configurar Hangfire para la gestión de tareas en segundo plano
        services.AddHangfire(config =>
        {
            if (!isTestEnvironment)
            {
                // Configurar provider de almacenamiento
                var connectionString = configuration.GetConnectionString("DefaultConnection");
            
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                      .UseSimpleAssemblyNameTypeSerializer()
                      .UseRecommendedSerializerSettings()
                      .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                      {
                          CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                          SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                          QueuePollInterval = TimeSpan.FromSeconds(15),
                          UseRecommendedIsolationLevel = true,
                          DisableGlobalLocks = true
                      });
            }
            else
            {
                // Usar almacenamiento en memoria para pruebas
                config.UseInMemoryStorage();
            }
        });

        // No agregar el servidor en entorno de prueba
        if (!isTestEnvironment)
        {
            services.AddHangfireServer(options =>
            {
                options.WorkerCount = Environment.ProcessorCount * 2;
                options.Queues = new[] { "default", "critical", "notifications", "reports", "emails" };
            });
        }
        
        // Registrar planificador de trabajos
        services.AddScoped<IJobScheduler, HangfireScheduler>();
        
        // Registrar opciones de configuración para los trabajos
        services.Configure<NotificationCleanupOptions>(
            configuration.GetSection("BackgroundTasks:NotificationCleanup"));
            
        services.Configure<UserInactivityOptions>(
            configuration.GetSection("BackgroundTasks:UserInactivity"));
            
        // services.Configure<LoyaltyPointsExpirationOptions>(
        //     configuration.GetSection("BackgroundTasks:LoyaltyPointsExpiration"));
            
        services.Configure<LowStockAlertOptions>(
            configuration.GetSection("BackgroundTasks:LowStockAlert"));
            
        // Registrar opciones de configuración para los workers
        services.Configure<NotificationWorkerOptions>(
            configuration.GetSection("BackgroundTasks:NotificationWorker"));
            
        services.Configure<EmailWorkerOptions>(
            configuration.GetSection("BackgroundTasks:EmailWorker"));
            
        services.Configure<ReportGenerationWorkerOptions>(
            configuration.GetSection("BackgroundTasks:ReportGenerationWorker"));
        
        // Registrar trabajos - Core
        services.AddTransient<NotificationCleanupJob>();
        services.AddTransient<UserInactivityJob>();
        
        // Registrar trabajos - Comercial
        // services.AddTransient<LoyaltyPointsExpirationJob>();
        services.AddTransient<InvoiceReminderJob>();
        
        // Registrar trabajos - Operaciones
        services.AddTransient<TableCleanupJob>();
        services.AddTransient<ReservationReminderJob>();
        
        // Registrar trabajos - Inventario
        services.AddTransient<LowStockAlertJob>();
        services.AddTransient<ExpirationCheckJob>();
        
        // Registrar workers
        services.AddHostedService<NotificationWorker>();
        services.AddHostedService<EmailWorker>();
        services.AddHostedService<ReportGenerationWorker>();
        
        return services;
    }
} 