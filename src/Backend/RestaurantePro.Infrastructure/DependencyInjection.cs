using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Interfaces.Repositories;
using RestaurantePro.Domain.Interfaces.Services;
using RestaurantePro.Infrastructure.Persistence;
using RestaurantePro.Infrastructure.Repositories;
using RestaurantePro.Infrastructure.Services;
using RestaurantePro.Infrastructure.Services.BackgroundServices;
using RestaurantePro.Infrastructure.DependencyInjection;

namespace RestaurantePro.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurar servicios de infraestructura
            services.AddInfrastructureServices(configuration);

            // Configurar identidad
            services.AddIdentityServices(configuration);

            // Configurar servicios externos
            // TODO: Implementar ExternalServicesSetup.cs

            return services;
        }
    }
} 