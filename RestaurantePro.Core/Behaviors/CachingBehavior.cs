using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using RestaurantePro.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace RestaurantePro.Core.Behaviors
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICacheableRequest
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

        public CachingBehavior(
            IDistributedCache cache,
            ILogger<CachingBehavior<TRequest, TResponse>> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var cacheKey = request.CacheKey;
            var cachedBytes = await _cache.GetAsync(cacheKey, cancellationToken);

            if (cachedBytes != null)
            {
                _logger.LogInformation($"Returning cached response for {cacheKey}");
                return JsonSerializer.Deserialize<TResponse>(cachedBytes);
            }

            var response = await next();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = request.CacheDuration
            };

            var serializedResponse = JsonSerializer.SerializeToUtf8Bytes(response);
            await _cache.SetAsync(cacheKey, serializedResponse, options, cancellationToken);

            return response;
        }
    }
}