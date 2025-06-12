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
            services.AddInfrastructureServices(configuration);

            // Configurar identidad
            services.AddIdentityServices(configuration);

            // Configurar servicios externos
            services.AddExternalServices(configuration);

            return services;
        }
    }
} 