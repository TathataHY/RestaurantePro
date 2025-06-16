using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Core.SharedKernel.Services.Cache
{
    /// <summary>
    /// Implementación simple de ICacheService para pruebas
    /// </summary>
    public class SimpleCacheService : ICacheService
    {
        private class CacheItem
        {
            public object Value { get; set; }
            public DateTime? ExpirationTime { get; set; }
            
            public bool IsExpired => ExpirationTime.HasValue && DateTime.UtcNow > ExpirationTime.Value;
        }
        
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();

        public T Get<T>(string key)
        {
            if (_cache.TryGetValue(key, out var item) && !item.IsExpired && item.Value is T typedValue)
            {
                return typedValue;
            }
            
            // Si está expirado, eliminarlo
            if (item?.IsExpired == true)
            {
                _cache.TryRemove(key, out _);
            }
            
            return default;
        }

        public Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Get<T>(key));
        }

        public void Set<T>(string key, T value, int expirationMinutes = 60)
        {
            // Si el TTL es 0, no almacenar en caché
            if (expirationMinutes == 0)
            {
                // Si existe, eliminarlo
                if (_cache.ContainsKey(key))
                {
                    _cache.TryRemove(key, out _);
                }
                return;
            }

            var expirationTime = expirationMinutes > 0 
                ? DateTime.UtcNow.AddMinutes(expirationMinutes) 
                : (DateTime?)null;
                
            _cache[key] = new CacheItem 
            { 
                Value = value, 
                ExpirationTime = expirationTime 
            };
        }

        public Task SetAsync<T>(string key, T value, int expirationMinutes = 60, CancellationToken cancellationToken = default)
        {
            Set(key, value, expirationMinutes);
            return Task.CompletedTask;
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            // Convertir TimeSpan a minutos
            int expirationMinutes = (int)expiration.TotalMinutes;
            Set(key, value, expirationMinutes);
            return Task.CompletedTask;
        }

        public bool Exists(string key)
        {
            if (_cache.TryGetValue(key, out var item) && !item.IsExpired)
            {
                return true;
            }
            
            // Si está expirado, eliminarlo
            if (item?.IsExpired == true)
            {
                _cache.TryRemove(key, out _);
            }
            
            return false;
        }

        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Exists(key));
        }

        public void Remove(string key)
        {
            _cache.TryRemove(key, out _);
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        public void InvalidatePattern(string pattern)
        {
            var keysToRemove = _cache.Keys.Where(k => k.Contains(pattern)).ToList();
            foreach (var key in keysToRemove)
            {
                _cache.TryRemove(key, out _);
            }
        }

        public Task InvalidatePatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            InvalidatePattern(pattern);
            return Task.CompletedTask;
        }

        public T GetOrAdd<T>(string key, Func<T> factory, int expirationMinutes = 60)
        {
            // Si el TTL es 0, siempre ejecutar la función factory y no almacenar en caché
            if (expirationMinutes == 0)
            {
                // Eliminar la clave si existe
                if (_cache.ContainsKey(key))
                {
                    _cache.TryRemove(key, out _);
                }
                
                return factory();
            }

            // Si la clave existe y no está expirada, devolver el valor
            if (_cache.TryGetValue(key, out var item) && !item.IsExpired && item.Value is T typedValue)
            {
                return typedValue;
            }

            // Si la clave existe pero está expirada, eliminarla
            if (item?.IsExpired == true)
            {
                _cache.TryRemove(key, out _);
            }

            // Crear un nuevo valor
            var newValue = factory();
            
            // Guardar en caché con el tiempo de expiración especificado
            Set(key, newValue, expirationMinutes);
            
            return newValue;
        }

        public async Task<T> GetOrAddAsync<T>(string key, Func<CancellationToken, Task<T>> factory, int expirationMinutes = 60, CancellationToken cancellationToken = default)
        {
            // Si el TTL es 0, siempre ejecutar la función factory y no almacenar en caché
            if (expirationMinutes == 0)
            {
                // Eliminar la clave si existe
                if (_cache.ContainsKey(key))
                {
                    _cache.TryRemove(key, out _);
                }
                
                return await factory(cancellationToken);
            }
            
            // Si la clave existe y no está expirada, devolver el valor
            if (_cache.TryGetValue(key, out var item) && !item.IsExpired && item.Value is T typedValue)
            {
                return typedValue;
            }
            
            // Si la clave existe pero está expirada, eliminarla
            if (item?.IsExpired == true)
            {
                _cache.TryRemove(key, out _);
            }
            
            // Crear un nuevo valor
            var newValue = await factory(cancellationToken);
            
            // Guardar en caché con el tiempo de expiración especificado
            await SetAsync(key, newValue, expirationMinutes, cancellationToken);
            
            return newValue;
        }

        public T GetOrCreate<T>(string key, Func<T> factory, int expirationMinutes = 60)
        {
            // Si el TTL es 0, siempre ejecutar la función factory y no almacenar en caché
            if (expirationMinutes == 0)
            {
                // Eliminar la clave si existe
                if (_cache.ContainsKey(key))
                {
                    _cache.TryRemove(key, out _);
                }
                
                return factory();
            }

            // Si la clave existe y no está expirada, devolver el valor
            if (_cache.TryGetValue(key, out var item) && !item.IsExpired && item.Value is T typedValue)
            {
                return typedValue;
            }

            // Si la clave existe pero está expirada, eliminarla
            if (item?.IsExpired == true)
            {
                _cache.TryRemove(key, out _);
            }

            // Crear un nuevo valor
            var newValue = factory();
            
            // Guardar en caché con el tiempo de expiración especificado
            Set(key, newValue, expirationMinutes);
            
            return newValue;
        }
    }
} 