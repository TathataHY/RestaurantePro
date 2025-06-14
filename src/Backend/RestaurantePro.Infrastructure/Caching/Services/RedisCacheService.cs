using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;
using RestaurantePro.Infrastructure.Caching.Configuration;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Caching.Services
{
    /// <summary>
    /// Implementación del servicio de caché utilizando Redis
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly StackExchange.Redis.IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly CacheConfiguration _cacheConfig;
        private readonly string _keyPrefix;

        public RedisCacheService(
            IConnectionMultiplexer redis,
            ILogger<RedisCacheService> logger,
            IOptions<CacheConfiguration> cacheConfig)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheConfig = cacheConfig?.Value ?? new CacheConfiguration();
            _database = _redis.GetDatabase(_cacheConfig.Redis.Database);
            _keyPrefix = _cacheConfig.KeyPrefix;
        }

        /// <inheritdoc/>
        public T Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var fullKey = GetFullKey(key);
            _logger.LogDebug("Obteniendo valor de Redis para clave: {Key}", fullKey);

            var value = _database.StringGet(fullKey);
            if (!value.HasValue)
            {
                _logger.LogDebug("Valor no encontrado en Redis para clave: {Key}", fullKey);
                return default;
            }

            _logger.LogDebug("Valor encontrado en Redis para clave: {Key}", fullKey);
            return JsonSerializer.Deserialize<T>(value);
        }

        /// <inheritdoc/>
        public bool Exists(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var fullKey = GetFullKey(key);
            return _database.KeyExists(fullKey);
        }

        /// <inheritdoc/>
        public void Set<T>(string key, T value, int expirationMinutes = 60)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var fullKey = GetFullKey(key);
            var serializedValue = JsonSerializer.Serialize(value);

            _logger.LogDebug("Estableciendo valor en Redis para clave: {Key}", fullKey);
            var expiry = expirationMinutes > 0 ? TimeSpan.FromMinutes(expirationMinutes) : (TimeSpan?)null;
            _database.StringSet(fullKey, serializedValue, expiry);
        }

        /// <inheritdoc/>
        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var fullKey = GetFullKey(key);
            _logger.LogDebug("Eliminando valor de Redis para clave: {Key}", fullKey);
            _database.KeyDelete(fullKey);
        }

        /// <inheritdoc/>
        public void InvalidatePattern(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentNullException(nameof(pattern));

            var fullPattern = GetFullKey(pattern);
            _logger.LogDebug("Invalidando valores de Redis para patrón: {Pattern}", fullPattern);

            foreach (var endpoint in _redis.GetEndPoints())
            {
                var server = _redis.GetServer(endpoint);
                var keys = server.Keys(pattern: $"{fullPattern}*");

                foreach (var key in keys)
                {
                    _database.KeyDelete(key);
                }
            }
        }

        /// <inheritdoc/>
        public T GetOrCreate<T>(string key, Func<T> factory, int expirationMinutes = 60)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (Exists(key))
            {
                return Get<T>(key);
            }

            _logger.LogDebug("Creando nuevo valor para clave: {Key}", key);
            var newValue = factory();
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

            if (Exists(key))
            {
                return Get<T>(key);
            }

            _logger.LogDebug("Creando nuevo valor asíncrono para clave: {Key}", key);
            var newValue = await loadFunc(cancellationToken);
            Set(key, newValue, timeToLiveMinutes);
            return newValue;
        }

        /// <summary>
        /// Obtiene la clave completa con el prefijo configurado
        /// </summary>
        private string GetFullKey(string key)
        {
            return $"{_keyPrefix}{key}";
        }
    }
} 