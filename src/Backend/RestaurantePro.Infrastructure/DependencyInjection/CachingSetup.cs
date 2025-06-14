using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Decorators;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Strategy;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Telemetry;
using RestaurantePro.Infrastructure.Caching.Configuration;
using RestaurantePro.Infrastructure.Caching.Services;
using StackExchange.Redis;
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
        public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuración
            services.Configure<CacheConfiguration>(configuration.GetSection("Cache"));
            var cacheConfig = configuration.GetSection("Cache").Get<CacheConfiguration>() ?? new CacheConfiguration();
            
            // Caché en memoria de Microsoft
            services.AddMemoryCache();
            
            // Configuración de Redis
            if (cacheConfig.Enabled && configuration.GetValue<bool>("UseRedisCache", false))
            {
                // Registrar conexión Redis
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    var redisConfig = cacheConfig.Redis;
                    var options = new ConfigurationOptions
                    {
                        EndPoints = { redisConfig.ConnectionString },
                        ClientName = redisConfig.InstanceName,
                        ConnectTimeout = redisConfig.ConnectTimeout,
                        SyncTimeout = redisConfig.SyncTimeout,
                        AbortOnConnectFail = false,
                        AllowAdmin = true,
                        Ssl = redisConfig.UseSsl
                    };
                    
                    return ConnectionMultiplexer.Connect(options);
                });
                
                // Registrar RedisCacheService
                services.AddSingleton<RedisCacheService>();
            }
            
            // Servicios de telemetría y TTL dinámico
            services.AddSingleton<ICacheTelemetry, InMemoryCacheTelemetry>();
            services.AddSingleton<IDynamicTtlStrategy, UsageBasedTtlStrategy>();
            
            // Registrar el servicio de caché en memoria
            services.AddSingleton<MemoryCacheService>();
            
            // Registrar la implementación final con decoradores
            services.AddSingleton<ICacheService>(sp => 
            {
                ICacheService baseCache;
                var telemetry = sp.GetRequiredService<ICacheTelemetry>();
                var ttlStrategy = sp.GetRequiredService<IDynamicTtlStrategy>();
                
                // Usar Redis si está configurado, de lo contrario usar MemoryCache
                if (cacheConfig.Enabled && configuration.GetValue<bool>("UseRedisCache", false) && 
                    sp.GetService<IConnectionMultiplexer>() != null)
                {
                    baseCache = sp.GetRequiredService<RedisCacheService>();
                }
                else
                {
                    baseCache = sp.GetRequiredService<MemoryCacheService>();
                }
                
                // Decorar con SmartCacheDecorator para añadir telemetría y TTL dinámico
                return new SmartCacheDecorator(baseCache, telemetry, ttlStrategy);
            });
            
            return services;
        }
    }
} 