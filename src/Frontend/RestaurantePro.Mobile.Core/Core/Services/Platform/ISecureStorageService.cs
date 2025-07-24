using System.Threading.Tasks;

namespace RestaurantePro.Mobile.Core.Services.Platform;

public interface ISecureStorageService
{
    Task SetAsync(string key, string value);
    Task<string?> GetAsync(string key);
    Task RemoveAsync(string key);
    Task ClearAsync();
} 