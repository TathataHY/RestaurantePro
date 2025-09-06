using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

using RestaurantePro.Mobile.Core.Services.Caching;

namespace RestaurantePro.Mobile.Services;

/// <summary>
/// Servicio de cache para optimización de performance - V4
/// </summary>
public class CacheService : ICacheService
{
    private readonly ILogger<CacheService> _logger;
    private readonly ConcurrentDictionary<string, CacheItem> _cache;
    private readonly Timer _cleanupTimer;

    public CacheService(ILogger<CacheService> logger)
    {
        _logger = logger;
        _cache = new ConcurrentDictionary<string, CacheItem>();
        
        // Limpiar cache expirado cada 5 minutos
        _cleanupTimer = new Timer(CleanupExpiredItems, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    #region Basic Cache Operations

    /// <summary>
    /// Obtiene un elemento del cache
    /// </summary>
    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out var item) && !item.IsExpired)
        {
            _logger.LogDebug("Cache hit: {Key}", key);
            return (T?)item.Value;
        }

        _logger.LogDebug("Cache miss: {Key}", key);
        return default;
    }

    /// <summary>
    /// Guarda un elemento en el cache
    /// </summary>
    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var cacheItem = new CacheItem
        {
            Value = value,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null
        };

        _cache.AddOrUpdate(key, cacheItem, (k, v) => cacheItem);
        
        _logger.LogDebug("Cache set: {Key} (expires: {ExpiresAt})", key, cacheItem.ExpiresAt);
    }

    /// <summary>
    /// Verifica si existe una clave en el cache
    /// </summary>
    public bool Contains(string key)
    {
        return _cache.TryGetValue(key, out var item) && !item.IsExpired;
    }

    /// <summary>
    /// Elimina un elemento del cache
    /// </summary>
    public bool Remove(string key)
    {
        var removed = _cache.TryRemove(key, out _);
        if (removed)
        {
            _logger.LogDebug("Cache removed: {Key}", key);
        }
        return removed;
    }

    /// <summary>
    /// Limpia todo el cache
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
        _logger.LogInformation("Cache cleared");
    }

    #endregion

    #region Advanced Cache Operations

    /// <summary>
    /// Obtiene o crea un elemento del cache
    /// </summary>
    public T GetOrSet<T>(string key, Func<T> factory, TimeSpan? expiration = null)
    {
        if (Contains(key))
        {
            return Get<T>(key)!;
        }

        var value = factory();
        Set(key, value, expiration);
        return value;
    }

    /// <summary>
    /// Obtiene o crea un elemento del cache de forma asíncrona
    /// </summary>
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (Contains(key))
        {
            return Get<T>(key)!;
        }

        var value = await factory();
        Set(key, value, expiration);
        return value;
    }

    /// <summary>
    /// Actualiza un elemento existente en el cache
    /// </summary>
    public bool Update<T>(string key, Func<T, T> updateFunction, TimeSpan? newExpiration = null)
    {
        if (_cache.TryGetValue(key, out var item) && !item.IsExpired)
        {
            var currentValue = (T)item.Value!;
            var newValue = updateFunction(currentValue);
            
            var updatedItem = new CacheItem
            {
                Value = newValue,
                CreatedAt = item.CreatedAt,
                ExpiresAt = newExpiration.HasValue ? DateTime.UtcNow.Add(newExpiration.Value) : item.ExpiresAt
            };

            _cache.TryUpdate(key, updatedItem, item);
            _logger.LogDebug("Cache updated: {Key}", key);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Obtiene múltiples elementos del cache
    /// </summary>
    public Dictionary<string, T> GetMultiple<T>(IEnumerable<string> keys)
    {
        var result = new Dictionary<string, T>();
        
        foreach (var key in keys)
        {
            if (Contains(key))
            {
                result[key] = Get<T>(key)!;
            }
        }

        return result;
    }

    /// <summary>
    /// Guarda múltiples elementos en el cache
    /// </summary>
    public void SetMultiple<T>(Dictionary<string, T> items, TimeSpan? expiration = null)
    {
        foreach (var item in items)
        {
            Set(item.Key, item.Value, expiration);
        }
    }

    #endregion

    #region Cache Statistics

    /// <summary>
    /// Obtiene estadísticas del cache
    /// </summary>
    public CacheStatistics GetStatistics()
    {
        var totalItems = _cache.Count;
        var expiredItems = _cache.Values.Count(item => item.IsExpired);
        var validItems = totalItems - expiredItems;

        return new CacheStatistics
        {
            TotalItems = totalItems,
            ValidItems = validItems,
            ExpiredItems = expiredItems,
            MemoryUsage = EstimateMemoryUsage()
        };
    }

    /// <summary>
    /// Estima el uso de memoria del cache
    /// </summary>
    private long EstimateMemoryUsage()
    {
        // Estimación básica: cada entrada del cache usa aproximadamente 100 bytes
        return _cache.Count * 100;
    }

    #endregion

    #region Cache Patterns

    /// <summary>
    /// Implementa el patrón de cache-aside
    /// </summary>
    public async Task<T> CacheAsideAsync<T>(string key, Func<Task<T>> dataLoader, TimeSpan? expiration = null)
    {
        // Intentar obtener del cache
        var cachedValue = Get<T>(key);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        // Si no está en cache, cargar datos
        var freshValue = await dataLoader();
        
        // Guardar en cache
        Set(key, freshValue, expiration);
        
        return freshValue;
    }

    /// <summary>
    /// Implementa el patrón de cache-through
    /// </summary>
    public async Task<T> CacheThroughAsync<T>(string key, Func<Task<T>> dataLoader, TimeSpan? expiration = null)
    {
        // Siempre cargar datos frescos
        var freshValue = await dataLoader();
        
        // Actualizar cache
        Set(key, freshValue, expiration);
        
        return freshValue;
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Limpia elementos expirados del cache
    /// </summary>
    private void CleanupExpiredItems(object? state)
    {
        var expiredKeys = _cache.Keys
            .Where(key => _cache.TryGetValue(key, out var item) && item.IsExpired)
            .ToList();

        foreach (var key in expiredKeys)
        {
            Remove(key);
        }

        if (expiredKeys.Count > 0)
        {
            _logger.LogDebug("Cleaned up {Count} expired cache items", expiredKeys.Count);
        }
    }

    /// <summary>
    /// Limpia elementos expirados manualmente
    /// </summary>
    public int CleanupExpiredItems()
    {
        var expiredKeys = _cache.Keys
            .Where(key => _cache.TryGetValue(key, out var item) && item.IsExpired)
            .ToList();

        foreach (var key in expiredKeys)
        {
            Remove(key);
        }

        return expiredKeys.Count;
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }

    #endregion
}

#region Data Models

/// <summary>
/// Elemento del cache
/// </summary>
public class CacheItem
{
    public object? Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
}

/// <summary>
/// Estadísticas del cache
/// </summary>
public class CacheStatistics
{
    public int TotalItems { get; set; }
    public int ValidItems { get; set; }
    public int ExpiredItems { get; set; }
    public long MemoryUsage { get; set; }
}

#endregion 