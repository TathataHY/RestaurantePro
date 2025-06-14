using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Infrastructure.Caching.Configuration;
using RestaurantePro.Infrastructure.Caching.Services;
using RestaurantePro.Infrastructure.Caching.Services.Interfaces;
using System;

namespace RestaurantePro.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Configuración de servicios de caché
    /// </summary>
    public static class CachingSetup
    {
        /// <summary>
        /// Agrega servicios de caché a la colección de servicios
        /// </summary>
        public static IServiceCollection AddCachingServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            // Configuración
            services.Configure<CacheConfiguration>(
                configuration.GetSection("CacheConfiguration"));

            // Caché en memoria
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();

            // Aquí se podrían agregar otros servicios de caché como Redis
            // services.AddStackExchangeRedisCache(options => { ... });
            // services.AddSingleton<IRedisCacheService, RedisCacheService>();

            return services;
        }
    }
} 