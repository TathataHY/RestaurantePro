using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Core.SharedKernel.Services
{
    /// <summary>
    /// Decorador inteligente para caché que combina telemetría y TTL dinámico
    /// </summary>
    public class SmartCacheDecorator : ICacheService
    {
        private readonly ICacheService _innerCache;
        private readonly ICacheTelemetry _telemetry;
        private readonly IDynamicTtlStrategy _ttlStrategy;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="innerCache">Servicio de caché subyacente</param>
        /// <param name="telemetry">Servicio de telemetría</param>
        /// <param name="ttlStrategy">Estrategia de TTL dinámico</param>
        public SmartCacheDecorator(
            ICacheService innerCache, 
            ICacheTelemetry telemetry,
            IDynamicTtlStrategy ttlStrategy)
        {
            _innerCache = innerCache ?? throw new ArgumentNullException(nameof(innerCache));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            _ttlStrategy = ttlStrategy ?? throw new ArgumentNullException(nameof(ttlStrategy));
        }
        
        /// <inheritdoc />
        public T Get<T>(string key)
        {
            var stopwatch = Stopwatch.StartNew();
            bool hit = false;
            
            try
            {
                var result = _innerCache.Get<T>(key);
                hit = !EqualityComparer<T>.Default.Equals(result, default);
                
                // Registrar para TTL dinámico
                _ttlStrategy.RegisterAccess(key, hit, "Get");
                
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
                result = _innerCache.Exists(key);
                
                // Registrar para TTL dinámico
                _ttlStrategy.RegisterAccess(key, result, "Exists");
                
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
                // Calcular TTL dinámico
                int dynamicTtl = _ttlStrategy.CalculateTtl(key, expirationMinutes);
                
                // Usar el TTL calculado
                _innerCache.Set(key, value, dynamicTtl);
                
                // Registrar para análisis de TTL
                _ttlStrategy.RegisterAccess(key, true, "Set");
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
                _innerCache.Remove(key);
                
                // Registrar para análisis de TTL
                _ttlStrategy.RegisterAccess(key, true, "Remove");
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
                // Estimación conservadora
                keysAffected = 5;
                
                _innerCache.InvalidatePattern(pattern);
                
                // Registrar para análisis de TTL
                _ttlStrategy.RegisterInvalidation(pattern, keysAffected);
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
                if (_innerCache.Exists(key))
                {
                    var value = _innerCache.Get<T>(key);
                    hit = true;
                    
                    // Registrar para análisis de TTL
                    _ttlStrategy.RegisterAccess(key, true, "GetOrCreate");
                    
                    return value;
                }
                
                var newValue = factory();
                
                // Calcular TTL dinámico
                int dynamicTtl = _ttlStrategy.CalculateTtl(key, expirationMinutes);
                _innerCache.Set(key, newValue, dynamicTtl);
                
                // Registrar para análisis de TTL
                _ttlStrategy.RegisterAccess(key, false, "GetOrCreate");
                
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
            return GetOrCreate(key, loadFunc, timeToLiveMinutes);
        }
        
        /// <inheritdoc />
        public async Task<T> GetOrAddAsync<T>(string key, Func<CancellationToken, Task<T>> loadFunc, int timeToLiveMinutes = 10, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            bool hit = false;
            
            try
            {
                if (_innerCache.Exists(key))
                {
                    var value = _innerCache.Get<T>(key);
                    hit = true;
                    
                    // Registrar para análisis de TTL
                    _ttlStrategy.RegisterAccess(key, true, "GetOrAddAsync");
                    
                    return value;
                }
                
                var newValue = await loadFunc(cancellationToken);
                
                // Calcular TTL dinámico
                int dynamicTtl = _ttlStrategy.CalculateTtl(key, timeToLiveMinutes);
                _innerCache.Set(key, newValue, dynamicTtl);
                
                // Registrar para análisis de TTL
                _ttlStrategy.RegisterAccess(key, false, "GetOrAddAsync");
                
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