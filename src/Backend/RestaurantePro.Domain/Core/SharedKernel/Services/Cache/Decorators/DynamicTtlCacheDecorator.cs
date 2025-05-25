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
        public bool Exists(string key)
        {
            var result = _innerCache.Exists(key);
            _ttlStrategy.RegisterAccess(key, result, "Exists");
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
        public void Remove(string key)
        {
            _innerCache.Remove(key);
            _ttlStrategy.RegisterAccess(key, true, "Remove");
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
            if (_innerCache.Exists(key))
            {
                var value = _innerCache.Get<T>(key);
                _ttlStrategy.RegisterAccess(key, true, "GetOrAddAsync");
                return value;
            }
            
            var newValue = await loadFunc(cancellationToken);
            
            // Usar TTL dinámico
            int dynamicTtl = _ttlStrategy.CalculateTtl(key, timeToLiveMinutes);
            _innerCache.Set(key, newValue, dynamicTtl);
            
            _ttlStrategy.RegisterAccess(key, false, "GetOrAddAsync");
            return newValue;
        }
    }
} 