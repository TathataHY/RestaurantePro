using Microsoft.Maui.Storage;

namespace RestaurantePro.Mobile.Core.Services.Platform;

public class SecureStorageService : ISecureStorageService
{
    public async Task SetAsync(string key, string value)
    {
        await SecureStorage.Default.SetAsync(key, value);
    }

    public async Task<string?> GetAsync(string key)
    {
        return await SecureStorage.Default.GetAsync(key);
    }

    public async Task RemoveAsync(string key)
    {
        SecureStorage.Default.Remove(key);
        await Task.CompletedTask;
    }

    public async Task ClearAsync()
    {
        SecureStorage.Default.RemoveAll();
        await Task.CompletedTask;
    }
} 