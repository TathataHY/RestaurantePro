using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Core.SharedKernel.Services
{
    /// <summary>
    /// Implementación básica de ICacheService que utiliza un diccionario en memoria
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private class CacheEntry<T>
        {
            public T Value { get; set; }
            public DateTime ExpirationTime { get; set; }
            public bool IsExpired => DateTime.Now > ExpirationTime;
        }
        
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
        private readonly object _lock = new object();
        
        /// <inheritdoc />
        public T Get<T>(string key)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var entry) && entry is CacheEntry<T> typedEntry)
                {
                    if (!typedEntry.IsExpired)
                    {
                        return typedEntry.Value;
                    }
                    _cache.Remove(key);
                }
                return default;
            }
        }
        
        /// <inheritdoc />
        public bool Exists(string key)
        {
            lock (_lock)
            {
                return _cache.ContainsKey(key) && !((dynamic)_cache[key]).IsExpired;
            }
        }
        
        /// <inheritdoc />
        public void Set<T>(string key, T value, int expirationMinutes = 60)
        {
            lock (_lock)
            {
                var entry = new CacheEntry<T>
                {
                    Value = value,
                    ExpirationTime = DateTime.Now.AddMinutes(expirationMinutes)
                };
                _cache[key] = entry;
            }
        }
        
        /// <inheritdoc />
        public void Remove(string key)
        {
            lock (_lock)
            {
                _cache.Remove(key);
            }
        }
        
        /// <inheritdoc />
        public void InvalidatePattern(string pattern)
        {
            lock (_lock)
            {
                var keysToRemove = _cache.Keys
                    .Where(k => k.Contains(pattern))
                    .ToList();
                
                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                }
            }
        }
        
        /// <inheritdoc />
        public T GetOrCreate<T>(string key, Func<T> factory, int expirationMinutes = 60)
        {
            T result = Get<T>(key);
            
            if (EqualityComparer<T>.Default.Equals(result, default))
            {
                result = factory();
                Set(key, result, expirationMinutes);
            }
            
            return result;
        }
        
        /// <inheritdoc />
        public T GetOrAdd<T>(string key, Func<T> loadFunc, int timeToLiveMinutes = 10)
        {
            return GetOrCreate(key, loadFunc, timeToLiveMinutes);
        }
        
        /// <inheritdoc />
        public async Task<T> GetOrAddAsync<T>(string key, Func<CancellationToken, Task<T>> loadFunc, int timeToLiveMinutes = 10, CancellationToken cancellationToken = default)
        {
            T result = Get<T>(key);
            
            if (EqualityComparer<T>.Default.Equals(result, default))
            {
                result = await loadFunc(cancellationToken);
                Set(key, result, timeToLiveMinutes);
            }
            
            return result;
        }
    }
} 