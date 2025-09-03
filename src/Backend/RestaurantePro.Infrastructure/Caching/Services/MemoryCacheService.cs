using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;
using RestaurantePro.Infrastructure.Caching.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ConcurrentDictionary<string, HashSet<string>> _cacheKeyPatterns;

        public MemoryCacheService(
            IMemoryCache memoryCache,
            ILogger<MemoryCacheService> logger,
            IOptions<CacheConfiguration> cacheConfig)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheConfig = cacheConfig?.Value ?? new CacheConfiguration();
            _cacheKeyPatterns = new ConcurrentDictionary<string, HashSet<string>>();
        }

        /// <inheritdoc/>
        public T Get<T>(string key)
        {
            _logger.LogDebug("Obteniendo valor de caché para clave: {Key}", key);
            return _memoryCache.TryGetValue(key, out T value) ? value : default;
        }

        /// <inheritdoc/>
        public Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Get<T>(key));
        }

        /// <inheritdoc/>
        public bool Exists(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return _memoryCache.TryGetValue(key, out _);
        }

        /// <inheritdoc/>
        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Exists(key));
        }

        /// <inheritdoc/>
        public void Set<T>(string key, T value, int expirationMinutes = 60)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var options = new MemoryCacheEntryOptions();
            
            if (expirationMinutes > 0)
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes);
                options.SlidingExpiration = TimeSpan.FromMinutes(Math.Min(expirationMinutes / 2, 10));
            }
            
            // Registrar la clave para invalidación por patrón
            RegisterCacheKey(key);
            
            _logger.LogDebug("Estableciendo valor en caché para clave: {Key} con expiración de {ExpirationMinutes} minutos", 
                key, expirationMinutes);
                
            _memoryCache.Set(key, value, options);
        }

        /// <inheritdoc/>
        public Task SetAsync<T>(string key, T value, int expirationMinutes = 60, CancellationToken cancellationToken = default)
        {
            Set(key, value, expirationMinutes);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var options = new MemoryCacheEntryOptions();
            
            if (expiration != TimeSpan.Zero)
            {
                options.AbsoluteExpirationRelativeToNow = expiration;
                options.SlidingExpiration = TimeSpan.FromMinutes(Math.Min((int)expiration.TotalMinutes / 2, 10));
            }
            
            // Registrar la clave para invalidación por patrón
            RegisterCacheKey(key);
            
            _logger.LogDebug("Estableciendo valor en caché para clave: {Key} con expiración de {Expiration}", 
                key, expiration);
                
            _memoryCache.Set(key, value, options);
            
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
        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void InvalidatePattern(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return;

            _logger.LogDebug("Invalidando caché por patrón: {Pattern}", pattern);
            
            var keysToRemove = new List<string>();
            
            // Buscar todas las claves que coincidan con el patrón
            foreach (var kvp in _cacheKeyPatterns)
            {
                var patternKey = kvp.Key;
                var keys = kvp.Value;
                
                // Si el patrón de la clave coincide con el patrón solicitado
                if (patternKey.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    keysToRemove.AddRange(keys);
                }
                
                // También buscar claves individuales que coincidan
                foreach (var key in keys)
                {
                    if (key.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        keysToRemove.Add(key);
                    }
                }
            }
            
            // Eliminar las claves encontradas
            foreach (var key in keysToRemove.Distinct())
            {
                _memoryCache.Remove(key);
                _logger.LogDebug("Clave de caché eliminada: {Key}", key);
            }
            
            // Limpiar el registro de patrones
            var patternsToRemove = _cacheKeyPatterns.Keys
                .Where(k => k.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                .ToList();
                
            foreach (var patternToRemove in patternsToRemove)
            {
                _cacheKeyPatterns.TryRemove(patternToRemove, out _);
            }
            
            _logger.LogInformation("Invalidación de caché completada para patrón: {Pattern}. Claves eliminadas: {Count}", 
                pattern, keysToRemove.Count);
        }

        /// <inheritdoc/>
        public Task InvalidatePatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            InvalidatePattern(pattern);
            return Task.CompletedTask;
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
            await SetAsync(key, newValue, timeToLiveMinutes, cancellationToken);
            return newValue;
        }

        /// <summary>
        /// Registra una clave de caché para permitir invalidación por patrón
        /// </summary>
        /// <param name="key">Clave de caché a registrar</param>
        private void RegisterCacheKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            // Extraer el patrón de la clave (parte antes del hash)
            var pattern = ExtractPatternFromKey(key);
            
            _cacheKeyPatterns.AddOrUpdate(
                pattern,
                new HashSet<string> { key },
                (existingPattern, existingKeys) =>
                {
                    existingKeys.Add(key);
                    return existingKeys;
                });
        }

        /// <summary>
        /// Extrae el patrón de una clave de caché
        /// </summary>
        /// <param name="key">Clave de caché</param>
        /// <returns>Patrón extraído</returns>
        private static string ExtractPatternFromKey(string key)
        {
            // Para claves como "ObtenerComandasPaginadasQuery_12345", 
            // extraer "ObtenerComandasPaginadasQuery_"
            var lastUnderscoreIndex = key.LastIndexOf('_');
            if (lastUnderscoreIndex > 0)
            {
                return key.Substring(0, lastUnderscoreIndex + 1);
            }
            
            return key;
        }
    }
} 