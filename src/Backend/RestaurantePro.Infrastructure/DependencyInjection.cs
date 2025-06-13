using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.DependencyInjection;

namespace RestaurantePro.Infrastructure
{
    public static class InfrastructureSetup
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurar servicios de infraestructura
            InfrastructureServiceCollectionExtensions.AddInfrastructureServices(services, configuration);

            // Configurar identidad
            IdentitySetup.AddIdentityServices(services, configuration);

            // Configurar servicios externos
            InfrastructureServiceCollectionExtensions.AddExternalServices(services, configuration);

            return services;
        }
    }
} 