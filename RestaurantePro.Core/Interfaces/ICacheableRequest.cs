namespace RestaurantePro.Core.Interfaces
{
    public interface ICacheableRequest
    {
        string CacheKey { get; }
        TimeSpan CacheDuration { get; }
    }
}