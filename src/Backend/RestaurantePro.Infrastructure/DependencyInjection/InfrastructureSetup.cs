using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base.Services;

namespace RestaurantePro.Infrastructure.DependencyInjection
{
    public static class InfrastructureSetup
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Registrar servicio de fecha y hora
            services.AddSingleton<IDateTimeService, DateTimeService>();
            
            // Configuración de base de datos
            services.AddPersistenceServices(configuration);
            
            // Servicios de identidad
            IdentitySetup.AddIdentityServices(services, configuration);
            
            // Servicios externos
            ExternalServicesSetup.AddExternalServices(services, configuration);
            
            // Sistema de caché
            services.AddCachingServices(configuration);
            
            // Logging
            // TODO: services.AddLoggingServices(configuration);
            
            // Background tasks
            // TODO: services.AddBackgroundTasksServices(configuration);
            
            // Monitoring
            // TODO: services.AddMonitoringServices(configuration);
            
            return services;
        }
    }
} 