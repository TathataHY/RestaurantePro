using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Telemetry;

namespace RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Decorators
{
    /// <summary>
    /// Decorador para ICacheService que añade telemetría y métricas
    /// </summary>
    public class TelemetryCacheDecorator : ICacheService
    {
        private readonly ICacheService _innerCache;
        private readonly ICacheTelemetry _telemetry;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="innerCache">Servicio de caché a decorar</param>
        /// <param name="telemetry">Servicio de telemetría</param>
        public TelemetryCacheDecorator(ICacheService innerCache, ICacheTelemetry telemetry)
        {
            _innerCache = innerCache ?? throw new ArgumentNullException(nameof(innerCache));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        }
        
        /// <inheritdoc />
        public T Get<T>(string key)
        {
            var stopwatch = Stopwatch.StartNew();
            bool hit = false;
            
            try
            {
                var result = _innerCache.Get<T>(key);
                hit = result != null;
                return result;
            }
            catch (Exception ex)
            {
                _telemetry.TrackCacheError(key, "Get", ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetry.TrackCacheAccess(key, hit, "Get", stopwatch.ElapsedMilliseconds);
            }
        }
        
        /// <inheritdoc />
        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            bool hit = false;
            
            try
            {
                var result = await _innerCache.GetAsync<T>(key, cancellationToken);
                hit = result != null;
                return result;
            }
            catch (Exception ex)
            {
                _telemetry.TrackCacheError(key, "GetAsync", ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetry.TrackCacheAccess(key, hit, "GetAsync", stopwatch.ElapsedMilliseconds);
            }
        }
        
        /// <inheritdoc />
        public bool Exists(string key)
        {
            var stopwatch = Stopwatch.StartNew();
            bool result = false;
            
            try
            {
                result = _innerCache.Exists(key);
                return result;
            }
            catch (Exception ex)
            {
                _telemetry.TrackCacheError(key, "Exists", ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetry.TrackCacheAccess(key, result, "Exists", stopwatch.ElapsedMilliseconds);
            }
        }
        
        /// <inheritdoc />
        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            bool result = false;
            
            try
            {
                result = await _innerCache.ExistsAsync(key, cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                _telemetry.TrackCacheError(key, "ExistsAsync", ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetry.TrackCacheAccess(key, result, "ExistsAsync", stopwatch.ElapsedMilliseconds);
            }
        }
        
        /// <inheritdoc />
        public void Set<T>(string key, T value, int expirationMinutes = 60)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _innerCache.Set(key, value, expirationMinutes);
            }
            catch (Exception ex)
            {
                _telemetry.TrackCacheError(key, "Set", ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetry.TrackCacheAccess(key, true, "Set", stopwatch.ElapsedMilliseconds);
            }
        }
        
        /// <inheritdoc />
        public async Task SetAsync<T>(string key, T value, int expirationMinutes = 60, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                await _innerCache.SetAsync(key, value, expirationMinutes, cancellationToken);
            }
            catch (Exception ex)
            {
                _telemetry.TrackCacheError(key, "SetAsync", ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetry.TrackCacheAccess(key, true, "SetAsync", stopwatch.ElapsedMilliseconds);
            }
        }
        
        /// <inheritdoc />
        public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                await _innerCache.SetAsync(key, value, expiration, cancellationToken);
            }
            catch (Exception ex)
            {
                _telemetry.TrackCacheError(key, "SetAsync", ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetry.TrackCacheAccess(key, true, "SetAsync", stopwatch.ElapsedMilliseconds);
            }
        }
        
        /// <inheritdoc />
        public void Remove(string key)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _innerCache.Remove(key);
            }
            catch (Exception ex)
            {
                _telemetry.TrackCacheError(key, "Remove", ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetry.TrackCacheAccess(key, true, "Remove", stopwatch.ElapsedMilliseconds);
            }
        }
        
        /// <inheritdoc />
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                await _innerCache.RemoveAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                _telemetry.TrackCacheError(key, "RemoveAsync", ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetry.TrackCacheAccess(key, true, "RemoveAsync", stopwatch.ElapsedMilliseconds);
            }
        }
        
        /// <inheritdoc />
        public void InvalidatePattern(string pattern)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _innerCache.InvalidatePattern(pattern);
            }
            catch (Exception ex)
            {
                _telemetry.TrackCacheError(pattern, "InvalidatePattern", ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetry.TrackCacheInvalidation(pattern, 0, stopwatch.ElapsedMilliseconds);
            }
        }
        
        /// <inheritdoc />
        public async Task InvalidatePatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                await _innerCache.InvalidatePatternAsync(pattern, cancellationToken);
            }
            catch (Exception ex)
            {
                _telemetry.TrackCacheError(pattern, "InvalidatePatternAsync", ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetry.TrackCacheInvalidation(pattern, 0, stopwatch.ElapsedMilliseconds);
            }
        }
        
        /// <inheritdoc />
        public T GetOrCreate<T>(string key, Func<T> factory, int expirationMinutes = 60)
        {
            var stopwatch = Stopwatch.StartNew();
            bool hit = false;
            
            try
            {
                var result = _innerCache.GetOrCreate(key, factory, expirationMinutes);
                hit = true; // Siempre será hit porque GetOrCreate garantiza un valor
                return result;
            }
            catch (Exception ex)
            {
                _telemetry.TrackCacheError(key, "GetOrCreate", ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetry.TrackCacheAccess(key, hit, "GetOrCreate", stopwatch.ElapsedMilliseconds);
            }
        }
        
        /// <inheritdoc />
        public T GetOrAdd<T>(string key, Func<T> loadFunc, int timeToLiveMinutes = 10)
        {
            return GetOrCreate(key, loadFunc, timeToLiveMinutes);
        }
        
        /// <inheritdoc />
        public async Task<T> GetOrAddAsync<T>(string key, Func<CancellationToken, Task<T>> loadFunc, int timeToLiveMinutes = 10, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            bool hit = false;
            
            try
            {
                var result = await _innerCache.GetOrAddAsync(key, loadFunc, timeToLiveMinutes, cancellationToken);
                hit = true; // Siempre será hit porque GetOrAddAsync garantiza un valor
                return result;
            }
            catch (Exception ex)
            {
                _telemetry.TrackCacheError(key, "GetOrAddAsync", ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetry.TrackCacheAccess(key, hit, "GetOrAddAsync", stopwatch.ElapsedMilliseconds);
            }
        }
    }
} 