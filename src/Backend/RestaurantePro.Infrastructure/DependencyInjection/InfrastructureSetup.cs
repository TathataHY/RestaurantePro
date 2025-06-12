using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantePro.Infrastructure.DependencyInjection
{
    public static class InfrastructureSetup
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuración de persistencia
            services.AddPersistenceServices(configuration);

            // TODO: Implementar más configuraciones específicas

            return services;
        }
    }
} 