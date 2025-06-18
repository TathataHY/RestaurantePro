using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Infrastructure.Services;

namespace RestaurantePro.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Configuración general de la infraestructura
    /// </summary>
    public static class InfrastructureSetup
    {
        /// <summary>
        /// Registra todos los servicios de infraestructura
        /// </summary>
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration,
            bool isTestEnvironment = false)
        {
            // Registrar implementaciones de servicios de la aplicación
            services.AddSingleton<IDelayProvider, DelayProvider>();
            services.AddSingleton<ITimeProvider, SystemTimeProvider>();
            
            // Registrar servicios de persistencia
            services.AddPersistenceServices(configuration, isTestEnvironment);
            
            // Registrar servicios de identidad
            IdentitySetup.AddIdentityServices(services, configuration);
            
            // Registrar servicios externos
            services.AddExternalServices(configuration);
            
            // Registrar servicios de caché
            services.AddCachingServices(configuration);
            
            // Registrar servicios de logging
            services.AddLoggingServices(configuration);
            
            return services;
        }
    }
} 