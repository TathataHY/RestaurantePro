using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Mobile.Services;

/// <summary>
/// Servicio de lazy loading para optimización de performance - V4
/// </summary>
public class LazyLoadingService
{
    private readonly ILogger<LazyLoadingService> _logger;
    private readonly ConcurrentDictionary<string, LazyItem> _lazyItems;
    private readonly SemaphoreSlim _semaphore;

    public LazyLoadingService(ILogger<LazyLoadingService> logger)
    {
        _logger = logger;
        _lazyItems = new ConcurrentDictionary<string, LazyItem>();
        _semaphore = new SemaphoreSlim(10, 10); // Máximo 10 operaciones concurrentes
    }

    #region Basic Lazy Loading

    /// <summary>
    /// Carga un elemento de forma lazy
    /// </summary>
    public async Task<T> LoadAsync<T>(string key, Func<Task<T>> loader, TimeSpan? expiration = null)
    {
        // Verificar si ya está cargado
        if (_lazyItems.TryGetValue(key, out var existingItem) && !existingItem.IsExpired)
        {
            _logger.LogDebug("Lazy loading hit: {Key}", key);
            return (T)existingItem.Value!;
        }

        // Usar semáforo para limitar operaciones concurrentes
        await _semaphore.WaitAsync();
        try
        {
            // Verificar nuevamente después de obtener el semáforo
            if (_lazyItems.TryGetValue(key, out var item) && !item.IsExpired)
            {
                _logger.LogDebug("Lazy loading hit (after semaphore): {Key}", key);
                return (T)item.Value!;
            }

            _logger.LogDebug("Lazy loading miss: {Key}", key);
            
            // Cargar el elemento
            var value = await loader();
            
            // Guardar en cache
            var lazyItem = new LazyItem
            {
                Value = value,
                LoadedAt = DateTime.UtcNow,
                ExpiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null
            };

            _lazyItems.AddOrUpdate(key, lazyItem, (k, v) => lazyItem);
            
            return value;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Precarga un elemento en background
    /// </summary>
    public void Preload<T>(string key, Func<Task<T>> loader, TimeSpan? expiration = null)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await LoadAsync(key, loader, expiration);
                _logger.LogDebug("Preloaded: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preloading: {Key}", key);
            }
        });
    }

    /// <summary>
    /// Verifica si un elemento está cargado
    /// </summary>
    public bool IsLoaded(string key)
    {
        return _lazyItems.TryGetValue(key, out var item) && !item.IsExpired;
    }

    /// <summary>
    /// Obtiene un elemento si está cargado, sin cargarlo
    /// </summary>
    public T? GetIfLoaded<T>(string key)
    {
        if (_lazyItems.TryGetValue(key, out var item) && !item.IsExpired)
        {
            return (T?)item.Value;
        }
        return default;
    }

    #endregion

    #region Batch Loading

    /// <summary>
    /// Carga múltiples elementos en paralelo
    /// </summary>
    public async Task<Dictionary<string, T>> LoadBatchAsync<T>(
        IEnumerable<string> keys, 
        Func<string, Task<T>> loader, 
        TimeSpan? expiration = null)
    {
        var tasks = keys.Select(async key =>
        {
            try
            {
                var value = await LoadAsync(key, () => loader(key), expiration);
                return new KeyValuePair<string, T>(key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading batch item: {Key}", key);
                return new KeyValuePair<string, T>(key, default(T)!);
            }
        });

        var results = await Task.WhenAll(tasks);
        return results.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// Precarga múltiples elementos en background
    /// </summary>
    public void PreloadBatch<T>(IEnumerable<string> keys, Func<string, Task<T>> loader, TimeSpan? expiration = null)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await LoadBatchAsync(keys, loader, expiration);
                _logger.LogDebug("Preloaded batch: {Count} items", keys.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preloading batch");
            }
        });
    }

    #endregion

    #region Progressive Loading

    /// <summary>
    /// Carga progresiva con callback de progreso
    /// </summary>
    public async Task<List<T>> LoadProgressiveAsync<T>(
        IEnumerable<string> keys, 
        Func<string, Task<T>> loader, 
        IProgress<int>? progress = null, 
        TimeSpan? expiration = null)
    {
        var keyList = keys.ToList();
        var results = new List<T>();
        var completed = 0;

        foreach (var key in keyList)
        {
            try
            {
                var value = await LoadAsync(key, () => loader(key), expiration);
                results.Add(value);
                completed++;
                progress?.Report((int)((double)completed / keyList.Count * 100));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in progressive loading: {Key}", key);
                completed++;
                progress?.Report((int)((double)completed / keyList.Count * 100));
            }
        }

        return results;
    }

    #endregion

    #region Smart Loading

    /// <summary>
    /// Carga inteligente basada en prioridad
    /// </summary>
    public async Task<Dictionary<string, T>> LoadWithPriorityAsync<T>(
        IEnumerable<(string Key, int Priority)> items, 
        Func<string, Task<T>> loader, 
        TimeSpan? expiration = null)
    {
        var sortedItems = items.OrderByDescending(x => x.Priority).ToList();
        var results = new Dictionary<string, T>();

        foreach (var (key, priority) in sortedItems)
        {
            try
            {
                var value = await LoadAsync(key, () => loader(key), expiration);
                results[key] = value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading priority item: {Key} (Priority: {Priority})", key, priority);
            }
        }

        return results;
    }

    /// <summary>
    /// Carga con dependencias
    /// </summary>
    public async Task<Dictionary<string, T>> LoadWithDependenciesAsync<T>(
        Dictionary<string, (Func<Task<T>> Loader, string[] Dependencies)> items, 
        TimeSpan? expiration = null)
    {
        var results = new Dictionary<string, T>();
        var loadedKeys = new HashSet<string>();

        while (results.Count < items.Count)
        {
            var canLoad = items
                .Where(item => !loadedKeys.Contains(item.Key))
                .Where(item => item.Value.Dependencies.All(dep => loadedKeys.Contains(dep)))
                .ToList();

            if (!canLoad.Any())
            {
                _logger.LogWarning("Circular dependency detected or missing dependencies");
                break;
            }

            var loadTasks = canLoad.Select(async item =>
            {
                try
                {
                    var value = await LoadAsync(item.Key, item.Value.Loader, expiration);
                    return new KeyValuePair<string, T>(item.Key, value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading item with dependencies: {Key}", item.Key);
                    return new KeyValuePair<string, T>(item.Key, default(T)!);
                }
            });

            var batchResults = await Task.WhenAll(loadTasks);
            
            foreach (var result in batchResults)
            {
                results[result.Key] = result.Value;
                loadedKeys.Add(result.Key);
            }
        }

        return results;
    }

    #endregion

    #region Cache Management

    /// <summary>
    /// Limpia elementos expirados
    /// </summary>
    public int CleanupExpiredItems()
    {
        var expiredKeys = _lazyItems.Keys
            .Where(key => _lazyItems.TryGetValue(key, out var item) && item.IsExpired)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _lazyItems.TryRemove(key, out _);
        }

        if (expiredKeys.Count > 0)
        {
            _logger.LogDebug("Cleaned up {Count} expired lazy items", expiredKeys.Count);
        }

        return expiredKeys.Count;
    }

    /// <summary>
    /// Obtiene estadísticas del lazy loading
    /// </summary>
    public LazyLoadingStatistics GetStatistics()
    {
        var totalItems = _lazyItems.Count;
        var expiredItems = _lazyItems.Values.Count(item => item.IsExpired);
        var validItems = totalItems - expiredItems;

        return new LazyLoadingStatistics
        {
            TotalItems = totalItems,
            ValidItems = validItems,
            ExpiredItems = expiredItems,
            AvailableSemaphoreCount = _semaphore.CurrentCount
        };
    }

    /// <summary>
    /// Limpia todo el cache
    /// </summary>
    public void Clear()
    {
        _lazyItems.Clear();
        _logger.LogInformation("Lazy loading cache cleared");
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        _semaphore?.Dispose();
    }

    #endregion
}

#region Data Models

/// <summary>
/// Elemento lazy
/// </summary>
public class LazyItem
{
    public object? Value { get; set; }
    public DateTime LoadedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
}

/// <summary>
/// Estadísticas del lazy loading
/// </summary>
public class LazyLoadingStatistics
{
    public int TotalItems { get; set; }
    public int ValidItems { get; set; }
    public int ExpiredItems { get; set; }
    public int AvailableSemaphoreCount { get; set; }
}

#endregion 