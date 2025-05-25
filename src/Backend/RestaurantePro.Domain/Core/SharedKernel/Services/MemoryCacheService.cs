namespace RestaurantePro.Domain.Core.SharedKernel.Services
{
    /// <summary>
    /// Implementación del servicio de caché que utiliza memoria
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly Dictionary<string, CacheEntry> _cache = new();
        private readonly ReaderWriterLockSlim _lock = new();
        private readonly IDateTimeService _dateTimeService;
        
        /// <summary>
        /// Constructor del servicio de caché
        /// </summary>
        /// <param name="dateTimeService">Servicio de fecha y hora</param>
        public MemoryCacheService(IDateTimeService dateTimeService)
        {
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }
        
        /// <inheritdoc/>
        public T GetOrAdd<T>(string key, Func<T> loadFunc, int timeToLiveMinutes = 10)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("La clave no puede estar vacía", nameof(key));
            
            if (loadFunc == null)
                throw new ArgumentNullException(nameof(loadFunc));
            
            // Intenta obtener el valor de la caché
            _lock.EnterReadLock();
            try
            {
                if (_cache.TryGetValue(key, out var entry) && IsValid(entry))
                {
                    return (T)entry.Value;
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
            
            // Si no está en caché o ha expirado, cargarlo
            _lock.EnterWriteLock();
            try
            {
                // Verificar nuevamente dentro del write lock (double-check)
                if (_cache.TryGetValue(key, out var entry) && IsValid(entry))
                {
                    return (T)entry.Value;
                }
                
                // Cargar el valor
                var value = loadFunc();
                
                // Calcular la fecha de expiración
                DateTime? expiration = null;
                if (timeToLiveMinutes > 0)
                {
                    expiration = _dateTimeService.Now.AddMinutes(timeToLiveMinutes);
                }
                
                // Guardar en caché
                _cache[key] = new CacheEntry(value, expiration);
                
                return value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        
        /// <inheritdoc/>
        public async Task<T> GetOrAddAsync<T>(string key, Func<CancellationToken, Task<T>> loadFunc, int timeToLiveMinutes = 10, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("La clave no puede estar vacía", nameof(key));
            
            if (loadFunc == null)
                throw new ArgumentNullException(nameof(loadFunc));
            
            // Intenta obtener el valor de la caché
            _lock.EnterReadLock();
            try
            {
                if (_cache.TryGetValue(key, out var entry) && IsValid(entry))
                {
                    return (T)entry.Value;
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
            
            // Si no está en caché o ha expirado, cargarlo
            // Nota: Liberamos el lock durante la carga asíncrona para no bloquear
            
            // Cargar el valor
            var value = await loadFunc(cancellationToken);
            
            _lock.EnterWriteLock();
            try
            {
                // Verificar nuevamente dentro del write lock
                if (_cache.TryGetValue(key, out var entry) && IsValid(entry))
                {
                    return (T)entry.Value;
                }
                
                // Calcular la fecha de expiración
                DateTime? expiration = null;
                if (timeToLiveMinutes > 0)
                {
                    expiration = _dateTimeService.Now.AddMinutes(timeToLiveMinutes);
                }
                
                // Guardar en caché
                _cache[key] = new CacheEntry(value, expiration);
                
                return value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        
        /// <inheritdoc/>
        public bool Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("La clave no puede estar vacía", nameof(key));
            
            _lock.EnterWriteLock();
            try
            {
                return _cache.Remove(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        
        /// <inheritdoc/>
        public int InvalidatePattern(string keyPattern)
        {
            if (string.IsNullOrWhiteSpace(keyPattern))
                throw new ArgumentException("El patrón no puede estar vacío", nameof(keyPattern));
            
            _lock.EnterWriteLock();
            try
            {
                var keysToRemove = _cache.Keys.Where(k => k.StartsWith(keyPattern)).ToList();
                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                }
                
                return keysToRemove.Count;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        
        /// <inheritdoc/>
        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _cache.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        
        /// <summary>
        /// Verifica si una entrada de caché sigue siendo válida
        /// </summary>
        private bool IsValid(CacheEntry entry)
        {
            if (entry == null)
                return false;
            
            if (!entry.Expiration.HasValue)
                return true; // Sin expiración
            
            return entry.Expiration.Value > _dateTimeService.Now;
        }
        
        /// <summary>
        /// Clase interna para representar una entrada en la caché
        /// </summary>
        private class CacheEntry
        {
            /// <summary>
            /// Valor almacenado
            /// </summary>
            public object Value { get; }
            
            /// <summary>
            /// Fecha y hora de expiración (null si no expira)
            /// </summary>
            public DateTime? Expiration { get; }
            
            /// <summary>
            /// Constructor
            /// </summary>
            public CacheEntry(object value, DateTime? expiration = null)
            {
                Value = value;
                Expiration = expiration;
            }
        }
    }
} 