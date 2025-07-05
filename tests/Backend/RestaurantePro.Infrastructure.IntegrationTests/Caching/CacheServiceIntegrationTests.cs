using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Telemetry;
using RestaurantePro.Infrastructure.Caching.Configuration;
using RestaurantePro.Infrastructure.Caching.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Caching
{
    public class CacheServiceIntegrationTests : IDisposable
    {
        private ServiceProvider _serviceProvider;
        private ICacheService _cacheService;
        private ICacheTelemetry _cacheTelemetry;

        public CacheServiceIntegrationTests()
        {
            // Configurar servicios para pruebas
            var services = new ServiceCollection();
            
            // Agregar servicios de caché
            services.AddMemoryCache();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<ICacheTelemetry, InMemoryCacheTelemetry>();
            services.Configure<CacheConfiguration>(options => {
                options.DefaultExpirationMinutes = 60;
            });
            services.AddSingleton<RestaurantePro.Infrastructure.Caching.Services.MemoryCacheService>();
            services.AddSingleton<ICacheService>(sp => sp.GetRequiredService<RestaurantePro.Infrastructure.Caching.Services.MemoryCacheService>());
            
            _serviceProvider = services.BuildServiceProvider();
            
            // Obtener servicios
            _cacheService = _serviceProvider.GetRequiredService<ICacheService>();
            _cacheTelemetry = _serviceProvider.GetRequiredService<ICacheTelemetry>();
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }

        [Fact]
        public void Get_WhenKeyDoesNotExist_ShouldReturnDefault()
        {
            // Act
            var result = _cacheService.Get<string>("non_existent_key");
            
            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Set_And_Get_ShouldStoreAndRetrieveValue()
        {
            // Arrange
            string key = "test_key";
            string value = "test_value";
            
            // Act
            _cacheService.Set(key, value);
            var result = _cacheService.Get<string>(key);
            
            // Assert
            Assert.Equal(value, result);
        }

        [Fact]
        public void Exists_WhenKeyExists_ShouldReturnTrue()
        {
            // Arrange
            string key = "exists_key";
            string value = "exists_value";
            _cacheService.Set(key, value);
            
            // Act
            bool exists = _cacheService.Exists(key);
            
            // Assert
            Assert.True(exists);
        }

        [Fact]
        public void Exists_WhenKeyDoesNotExist_ShouldReturnFalse()
        {
            // Act
            bool exists = _cacheService.Exists("non_existent_key");
            
            // Assert
            Assert.False(exists);
        }

        [Fact]
        public void Remove_ShouldRemoveItemFromCache()
        {
            // Arrange
            string key = "remove_key";
            string value = "remove_value";
            _cacheService.Set(key, value);
            
            // Act
            _cacheService.Remove(key);
            bool exists = _cacheService.Exists(key);
            
            // Assert
            Assert.False(exists);
        }

        [Fact]
        public void GetOrCreate_WhenKeyDoesNotExist_ShouldCreateNewValue()
        {
            // Arrange
            string key = "get_or_create_key";
            string expectedValue = "created_value";
            
            // Act
            var result = _cacheService.GetOrCreate(key, () => expectedValue);
            
            // Assert
            Assert.Equal(expectedValue, result);
            Assert.True(_cacheService.Exists(key));
        }

        [Fact]
        public void GetOrCreate_WhenKeyExists_ShouldReturnExistingValue()
        {
            // Arrange
            string key = "existing_key";
            string existingValue = "existing_value";
            _cacheService.Set(key, existingValue);
            
            // Act
            var result = _cacheService.GetOrCreate(key, () => "new_value");
            
            // Assert
            Assert.Equal(existingValue, result);
        }

        [Fact]
        public async Task GetOrAddAsync_WhenKeyDoesNotExist_ShouldCreateNewValue()
        {
            // Arrange
            string key = "get_or_add_async_key";
            string expectedValue = "async_created_value";
            
            // Act
            var result = await _cacheService.GetOrAddAsync(key, ct => Task.FromResult(expectedValue));
            
            // Assert
            Assert.Equal(expectedValue, result);
            Assert.True(_cacheService.Exists(key));
        }
    }
} 