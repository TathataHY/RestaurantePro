using System.Collections.Concurrent;
using System.Threading.Tasks;
using RestaurantePro.Mobile.Core.Services.Platform;

namespace RestaurantePro.Mobile.IntegrationTests.TestBase;

/// <summary>
/// Implementación fake de ISecureStorageService para pruebas de integración
/// </summary>
public class FakeSecureStorageService : ISecureStorageService
{
    // Cambiar de estático a instancia para evitar problemas de estado compartido
    private readonly ConcurrentDictionary<string, string> _store = new();

    public Task SetAsync(string key, string value)
    {
        _store[key] = value;
        return Task.CompletedTask;
    }

    public Task<string?> GetAsync(string key)
    {
        _store.TryGetValue(key, out var value);
        return Task.FromResult(value);
    }

    public Task RemoveAsync(string key)
    {
        _store.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        _store.Clear();
        return Task.CompletedTask;
    }

    public Task<bool> ContainsKeyAsync(string key)
    {
        return Task.FromResult(_store.ContainsKey(key));
    }
} 