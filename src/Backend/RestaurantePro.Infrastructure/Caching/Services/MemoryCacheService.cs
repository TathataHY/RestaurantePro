using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;
using RestaurantePro.Infrastructure.Caching.Configuration;
using System;
using System.Threading;
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
        public bool Exists(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return _memoryCache.TryGetValue(key, out _);
        }

        /// <inheritdoc/>
        public void Set<T>(string key, T value, int expirationMinutes = 60)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes),
                Priority = CacheItemPriority.Normal
            };

            _logger.LogDebug("Estableciendo valor en caché para clave: {Key}", key);
            _memoryCache.Set(key, value, cacheEntryOptions);
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
        public void InvalidatePattern(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentNullException(nameof(pattern));

            _logger.LogWarning("Invalidación por patrón no está completamente soportada en IMemoryCache: {Pattern}", pattern);
            
            // IMemoryCache no tiene una forma directa de buscar por patrón
            // Esta es una limitación de la implementación
            // En una implementación real con Redis, esto sería más eficiente
        }

        /// <inheritdoc/>
        public T GetOrCreate<T>(string key, Func<T> factory, int expirationMinutes = 60)
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
            Set(key, newValue, expirationMinutes);
            return newValue;
        }

        /// <inheritdoc/>
        public T GetOrAdd<T>(string key, Func<T> loadFunc, int timeToLiveMinutes = 10)
        {
            return GetOrCreate(key, loadFunc, timeToLiveMinutes);
        }

        /// <inheritdoc/>
        public async Task<T> GetOrAddAsync<T>(string key, Func<CancellationToken, Task<T>> loadFunc, int timeToLiveMinutes = 10, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (loadFunc == null)
                throw new ArgumentNullException(nameof(loadFunc));

            if (_memoryCache.TryGetValue(key, out T cachedValue))
            {
                _logger.LogDebug("Valor encontrado en caché para clave: {Key}", key);
                return cachedValue;
            }

            _logger.LogDebug("Valor no encontrado en caché para clave: {Key}, creando nuevo valor", key);
            T newValue = await loadFunc(cancellationToken);
            Set(key, newValue, timeToLiveMinutes);
            return newValue;
        }
    }
} 