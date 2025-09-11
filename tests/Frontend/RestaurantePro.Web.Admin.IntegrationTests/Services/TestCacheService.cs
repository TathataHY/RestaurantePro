using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;

namespace RestaurantePro.Web.Admin.IntegrationTests.Services;

/// <summary>
/// Implementación de prueba para ICacheService que no hace nada real
/// </summary>
public class TestCacheService : ICacheService
{
    private readonly Dictionary<string, object> _cache = new();

    public T Get<T>(string key)
    {
        _cache.TryGetValue(key, out var value);
        return value is T t ? t : default(T);
    }

    public Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        _cache.TryGetValue(key, out var value);
        return Task.FromResult(value is T t ? t : default(T));
    }

    public bool Exists(string key)
    {
        return _cache.ContainsKey(key);
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cache.ContainsKey(key));
    }

    public void Set<T>(string key, T value, int expirationMinutes = 60)
    {
        _cache[key] = value;
    }

    public Task SetAsync<T>(string key, T value, int expirationMinutes = 60, CancellationToken cancellationToken = default)
    {
        _cache[key] = value;
        return Task.CompletedTask;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        _cache[key] = value;
        return Task.CompletedTask;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public void InvalidatePattern(string pattern)
    {
        // Implementación simple para pruebas - remover todas las claves que contengan el patrón
        var keysToRemove = _cache.Keys.Where(k => k.Contains(pattern)).ToList();
        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
        }
    }

    public Task InvalidatePatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        InvalidatePattern(pattern);
        return Task.CompletedTask;
    }

    public T GetOrCreate<T>(string key, Func<T> factory, int expirationMinutes = 60)
    {
        if (_cache.TryGetValue(key, out var value) && value is T t)
        {
            return t;
        }

        var newValue = factory();
        _cache[key] = newValue;
        return newValue;
    }

    public T GetOrAdd<T>(string key, Func<T> loadFunc, int timeToLiveMinutes = 10)
    {
        return GetOrCreate(key, loadFunc, timeToLiveMinutes);
    }

    public async Task<T> GetOrAddAsync<T>(string key, Func<CancellationToken, Task<T>> loadFunc, int timeToLiveMinutes = 10, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var value) && value is T t)
        {
            return t;
        }

        var newValue = await loadFunc(cancellationToken);
        _cache[key] = newValue;
        return newValue;
    }
}
