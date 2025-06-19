using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Decorators;
using RestaurantePro.Infrastructure.Caching.Services;
using RestaurantePro.Infrastructure.DependencyInjection;
using System.Collections.Generic;
using Xunit;
using NSubstitute;
using StackExchange.Redis;

namespace RestaurantePro.Infrastructure.IntegrationTests.DependencyInjection
{
    public class CachingSetupTests
    {
        [Fact]
        public void AddCachingServices_WithMemoryCache_ShouldRegisterMemoryCacheService()
        {
            // Arrange
            var services = new ServiceCollection();
            var inMemorySettings = new Dictionary<string, string>
            {
                { "Cache:Enabled", "true" },
                { "UseRedisCache", "false" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            
            services.AddSingleton(configuration);
            services.AddLogging();

            // Act
            services.AddCachingServices(configuration);
            var serviceProvider = services.BuildServiceProvider();
            var cacheService = serviceProvider.GetService<ICacheService>();

            // Assert
            Assert.NotNull(cacheService);
            Assert.IsType<SmartCacheDecorator>(cacheService);

            // Verificar el tipo subyacente
            var decoratedCache = ((SmartCacheDecorator)cacheService).GetBaseCacheService();
            Assert.IsType<MemoryCacheService>(decoratedCache);
        }

        [Fact]
        public void AddCachingServices_WithRedisCache_ShouldRegisterRedisCacheService()
        {
            // Arrange
            var services = new ServiceCollection();
            var inMemorySettings = new Dictionary<string, string>
            {
                { "Cache:Enabled", "true" },
                { "UseRedisCache", "true" },
                { "Cache:Redis:ConnectionString", "localhost:6379" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            services.AddSingleton(configuration);
            services.AddLogging();
            
            // Mock IConnectionMultiplexer para no requerir una instancia de Redis
            var mockMultiplexer = Substitute.For<IConnectionMultiplexer>();
            services.AddSingleton(mockMultiplexer);

            // Act
            services.AddCachingServices(configuration);
            var serviceProvider = services.BuildServiceProvider();
            var cacheService = serviceProvider.GetService<ICacheService>();

            // Assert
            Assert.NotNull(cacheService);
            Assert.IsType<SmartCacheDecorator>(cacheService);

            var decoratedCache = ((SmartCacheDecorator)cacheService).GetBaseCacheService();
            Assert.IsType<RedisCacheService>(decoratedCache);
        }
    }
} 