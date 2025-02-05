using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace RestaurantePro.Core.Extensions
{
    public static class DistributedCacheExtensions
    {
        public static async Task<T?> GetAsync<T>(this IDistributedCache cache, string key)

        {
            var bytes = await cache.GetAsync(key);
            if (bytes == null) return default;

            return JsonSerializer.Deserialize<T>(bytes);
        }

        public static async Task SetAsync<T>(
            this IDistributedCache cache,
            string key,
            T value,
            TimeSpan duration)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = duration
            };

            await cache.SetAsync(key, bytes, options);
        }
    }
}