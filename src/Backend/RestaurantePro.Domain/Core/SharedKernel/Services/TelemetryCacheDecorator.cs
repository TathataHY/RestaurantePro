using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Core.SharedKernel.Services
{
    /// <summary>
    /// Decorador para ICacheService que añade telemetría
    /// </summary>
    public class TelemetryCacheDecorator : ICacheService
    {
        private readonly ICacheService _innerCacheService;
        private readonly ICacheTelemetry _telemetry;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="innerCacheService">Servicio de caché decorado</param>
        /// <param name="telemetry">Servicio de telemetría</param>
        public TelemetryCacheDecorator(ICacheService innerCacheService, ICacheTelemetry telemetry)
        {
            _innerCacheService = innerCacheService ?? throw new ArgumentNullException(nameof(innerCacheService));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        }
        
        /// <inheritdoc />
        public T Get<T>(string key)
        {
            var stopwatch = Stopwatch.StartNew();
            bool hit = false;
            
            try
            {
                var result = _innerCacheService.Get<T>(key);
                hit = !EqualityComparer<T>.Default.Equals(result, default);
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
        public bool Exists(string key)
        {
            var stopwatch = Stopwatch.StartNew();
            bool result = false;
            
            try
            {
                result = _innerCacheService.Exists(key);
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
        public void Set<T>(string key, T value, int expirationMinutes = 60)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _innerCacheService.Set(key, value, expirationMinutes);
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
        public void Remove(string key)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _innerCacheService.Remove(key);
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
        public void InvalidatePattern(string pattern)
        {
            var stopwatch = Stopwatch.StartNew();
            int keysAffected = 0;
            
            try
            {
                // Asumimos que afecta a al menos 1 clave
                keysAffected = 1;
                _innerCacheService.InvalidatePattern(pattern);
            }
            catch (Exception ex)
            {
                _telemetry.TrackCacheError(pattern, "InvalidatePattern", ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetry.TrackCacheInvalidation(pattern, keysAffected, stopwatch.ElapsedMilliseconds);
            }
        }
        
        /// <inheritdoc />
        public T GetOrCreate<T>(string key, Func<T> factory, int expirationMinutes = 60)
        {
            var stopwatch = Stopwatch.StartNew();
            bool hit = false;
            
            try
            {
                // Primero verificamos si existe en caché
                var existingValue = _innerCacheService.Get<T>(key);
                hit = !EqualityComparer<T>.Default.Equals(existingValue, default);
                
                if (hit)
                {
                    return existingValue;
                }
                
                // Si no existe, lo creamos
                var newValue = factory();
                _innerCacheService.Set(key, newValue, expirationMinutes);
                return newValue;
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
            var stopwatch = Stopwatch.StartNew();
            bool hit = false;
            
            try
            {
                // Primero verificamos si existe en caché
                var existingValue = _innerCacheService.Get<T>(key);
                hit = !EqualityComparer<T>.Default.Equals(existingValue, default);
                
                if (hit)
                {
                    return existingValue;
                }
                
                // Si no existe, lo creamos
                var newValue = loadFunc();
                _innerCacheService.Set(key, newValue, timeToLiveMinutes);
                return newValue;
            }
            catch (Exception ex)
            {
                _telemetry.TrackCacheError(key, "GetOrAdd", ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetry.TrackCacheAccess(key, hit, "GetOrAdd", stopwatch.ElapsedMilliseconds);
            }
        }
        
        /// <inheritdoc />
        public async Task<T> GetOrAddAsync<T>(string key, Func<CancellationToken, Task<T>> loadFunc, int timeToLiveMinutes = 10, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            bool hit = false;
            
            try
            {
                // Primero verificamos si existe en caché
                var existingValue = _innerCacheService.Get<T>(key);
                hit = !EqualityComparer<T>.Default.Equals(existingValue, default);
                
                if (hit)
                {
                    return existingValue;
                }
                
                // Si no existe, lo creamos de forma asíncrona
                var newValue = await loadFunc(cancellationToken);
                _innerCacheService.Set(key, newValue, timeToLiveMinutes);
                return newValue;
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