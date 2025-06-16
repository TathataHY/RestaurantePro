using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Strategy;

namespace RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Decorators
{
    /// <summary>
    /// Decorador para ICacheService que aplica TTL dinámico basado en patrones de uso
    /// </summary>
    public class DynamicTtlCacheDecorator : ICacheService
    {
        private readonly ICacheService _innerCache;
        private readonly IDynamicTtlStrategy _ttlStrategy;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="innerCache">Servicio de caché a decorar</param>
        /// <param name="ttlStrategy">Estrategia de TTL dinámico</param>
        public DynamicTtlCacheDecorator(ICacheService innerCache, IDynamicTtlStrategy ttlStrategy)
        {
            _innerCache = innerCache ?? throw new ArgumentNullException(nameof(innerCache));
            _ttlStrategy = ttlStrategy ?? throw new ArgumentNullException(nameof(ttlStrategy));
        }
        
        /// <inheritdoc />
        public T Get<T>(string key)
        {
            var result = _innerCache.Get<T>(key);
            bool hit = !EqualityComparer<T>.Default.Equals(result, default);
            _ttlStrategy.RegisterAccess(key, hit, "Get");
            return result;
        }
        
        /// <inheritdoc />
        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var result = await _innerCache.GetAsync<T>(key, cancellationToken);
            bool hit = !EqualityComparer<T>.Default.Equals(result, default);
            _ttlStrategy.RegisterAccess(key, hit, "GetAsync");
            return result;
        }
        
        /// <inheritdoc />
        public bool Exists(string key)
        {
            var result = _innerCache.Exists(key);
            _ttlStrategy.RegisterAccess(key, result, "Exists");
            return result;
        }
        
        /// <inheritdoc />
        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            var result = await _innerCache.ExistsAsync(key, cancellationToken);
            _ttlStrategy.RegisterAccess(key, result, "ExistsAsync");
            return result;
        }
        
        /// <inheritdoc />
        public void Set<T>(string key, T value, int expirationMinutes = 60)
        {
            // Calcular TTL dinámico
            int dynamicTtl = _ttlStrategy.CalculateTtl(key, expirationMinutes);
            
            // Usar el TTL calculado
            _innerCache.Set(key, value, dynamicTtl);
            
            // Registrar para análisis
            _ttlStrategy.RegisterAccess(key, true, "Set");
        }
        
        /// <inheritdoc />
        public async Task SetAsync<T>(string key, T value, int expirationMinutes = 60, CancellationToken cancellationToken = default)
        {
            // Calcular TTL dinámico
            int dynamicTtl = _ttlStrategy.CalculateTtl(key, expirationMinutes);
            
            // Usar el TTL calculado
            await _innerCache.SetAsync(key, value, dynamicTtl, cancellationToken);
            
            // Registrar para análisis
            _ttlStrategy.RegisterAccess(key, true, "SetAsync");
        }
        
        /// <inheritdoc />
        public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            // Convertir TimeSpan a minutos y aplicar estrategia dinámica
            int expirationMinutes = (int)expiration.TotalMinutes;
            int dynamicTtl = _ttlStrategy.CalculateTtl(key, expirationMinutes);
            
            // Usar el TTL calculado
            await _innerCache.SetAsync(key, value, dynamicTtl, cancellationToken);
            
            // Registrar para análisis
            _ttlStrategy.RegisterAccess(key, true, "SetAsync");
        }
        
        /// <inheritdoc />
        public void Remove(string key)
        {
            _innerCache.Remove(key);
            _ttlStrategy.RegisterAccess(key, true, "Remove");
        }
        
        /// <inheritdoc />
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _innerCache.RemoveAsync(key, cancellationToken);
            _ttlStrategy.RegisterAccess(key, true, "RemoveAsync");
        }
        
        /// <inheritdoc />
        public void InvalidatePattern(string pattern)
        {
            // Contabilizar cuántas claves podrían verse afectadas (estimación)
            int estimatedAffectedKeys = 5; // Un valor conservador
            
            _innerCache.InvalidatePattern(pattern);
            _ttlStrategy.RegisterInvalidation(pattern, estimatedAffectedKeys);
        }
        
        /// <inheritdoc />
        public async Task InvalidatePatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            // Contabilizar cuántas claves podrían verse afectadas (estimación)
            int estimatedAffectedKeys = 5; // Un valor conservador
            
            await _innerCache.InvalidatePatternAsync(pattern, cancellationToken);
            _ttlStrategy.RegisterInvalidation(pattern, estimatedAffectedKeys);
        }
        
        /// <inheritdoc />
        public T GetOrCreate<T>(string key, Func<T> factory, int expirationMinutes = 60)
        {
            if (_innerCache.Exists(key))
            {
                var value = _innerCache.Get<T>(key);
                _ttlStrategy.RegisterAccess(key, true, "GetOrCreate");
                return value;
            }
            
            var newValue = factory();
            
            // Usar TTL dinámico
            int dynamicTtl = _ttlStrategy.CalculateTtl(key, expirationMinutes);
            _innerCache.Set(key, newValue, dynamicTtl);
            
            _ttlStrategy.RegisterAccess(key, false, "GetOrCreate");
            return newValue;
        }
        
        /// <inheritdoc />
        public T GetOrAdd<T>(string key, Func<T> loadFunc, int timeToLiveMinutes = 10)
        {
            return GetOrCreate(key, loadFunc, timeToLiveMinutes);
        }
        
        /// <inheritdoc />
        public async Task<T> GetOrAddAsync<T>(string key, Func<CancellationToken, Task<T>> loadFunc, int timeToLiveMinutes = 10, CancellationToken cancellationToken = default)
        {
            if (await ExistsAsync(key, cancellationToken))
            {
                var value = await GetAsync<T>(key, cancellationToken);
                _ttlStrategy.RegisterAccess(key, true, "GetOrAddAsync");
                return value;
            }
            
            var newValue = await loadFunc(cancellationToken);
            
            // Usar TTL dinámico
            int dynamicTtl = _ttlStrategy.CalculateTtl(key, timeToLiveMinutes);
            await SetAsync(key, newValue, dynamicTtl, cancellationToken);
            
            _ttlStrategy.RegisterAccess(key, false, "GetOrAddAsync");
            return newValue;
        }
    }
} 