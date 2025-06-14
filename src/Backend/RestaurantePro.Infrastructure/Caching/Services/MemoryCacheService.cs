using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Infrastructure.Caching.Configuration;
using RestaurantePro.Infrastructure.Caching.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Caching.Services
{
    /// <summary>
    /// Implementación del servicio de caché utilizando IMemoryCache
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly CacheConfiguration _cacheConfig;

        public MemoryCacheService(
            IMemoryCache memoryCache,
            ILogger<MemoryCacheService> logger,
            IOptions<CacheConfiguration> cacheConfig)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheConfig = cacheConfig?.Value ?? new CacheConfiguration();
        }

        /// <inheritdoc/>
        public T Get<T>(string key)
        {
            _logger.LogDebug("Obteniendo valor de caché para clave: {Key}", key);
            return _memoryCache.TryGetValue(key, out T value) ? value : default;
        }

        /// <inheritdoc/>
        public Task<T> GetAsync<T>(string key)
        {
            return Task.FromResult(Get<T>(key));
        }

        /// <inheritdoc/>
        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_cacheConfig.DefaultExpirationMinutes),
                Priority = CacheItemPriority.Normal
            };

            _logger.LogDebug("Estableciendo valor en caché para clave: {Key}", key);
            _memoryCache.Set(key, value, cacheEntryOptions);
        }

        /// <inheritdoc/>
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            Set(key, value, expiration);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            _logger.LogDebug("Eliminando valor de caché para clave: {Key}", key);
            _memoryCache.Remove(key);
        }

        /// <inheritdoc/>
        public Task RemoveAsync(string key)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public bool Exists(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return _memoryCache.TryGetValue(key, out _);
        }

        /// <inheritdoc/>
        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(Exists(key));
        }

        /// <inheritdoc/>
        public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (_memoryCache.TryGetValue(key, out T cachedValue))
            {
                _logger.LogDebug("Valor encontrado en caché para clave: {Key}", key);
                return cachedValue;
            }

            _logger.LogDebug("Valor no encontrado en caché para clave: {Key}, creando nuevo valor", key);
            T newValue = factory();
            Set(key, newValue, expiration);
            return newValue;
        }

        /// <inheritdoc/>
        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (_memoryCache.TryGetValue(key, out T cachedValue))
            {
                _logger.LogDebug("Valor encontrado en caché para clave: {Key}", key);
                return cachedValue;
            }

            _logger.LogDebug("Valor no encontrado en caché para clave: {Key}, creando nuevo valor", key);
            T newValue = await factory();
            await SetAsync(key, newValue, expiration);
            return newValue;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _logger.LogWarning("Limpiando toda la caché en memoria");
            
            // No hay un método directo para limpiar IMemoryCache
            // Necesitamos crear una nueva instancia o usar un enfoque alternativo
            // Esta es una limitación conocida de IMemoryCache
            
            // En una implementación real, podríamos usar un enfoque como:
            // 1. Mantener un registro de todas las claves
            // 2. Usar un campo de tipo MemoryCache que podamos reemplazar
            // 3. Usar un proveedor de caché diferente que soporte limpieza completa
            
            _logger.LogWarning("La limpieza completa de IMemoryCache no está soportada directamente");
        }

        /// <inheritdoc/>
        public Task ClearAsync()
        {
            Clear();
            return Task.CompletedTask;
        }
    }
} 