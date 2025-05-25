namespace RestaurantePro.Domain.Comercial.Services
{
    /// <summary>
    /// Implementación con caché del servicio de fidelización de clientes
    /// </summary>
    public class ServicioFidelizacionCached : IServicioFidelizacionCached
    {
        private readonly IServicioFidelizacion _servicioFidelizacionOriginal;
        private readonly ICacheService _cacheService;
        private const string CacheKeyPrefix = "ServicioFidelizacion_";
        private const int CacheDurationMinutes = 60; // Caché de 1 hora para datos de fidelización
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ServicioFidelizacionCached(
            IServicioFidelizacion servicioFidelizacion,
            ICacheService cacheService)
        {
            _servicioFidelizacionOriginal = servicioFidelizacion ?? throw new ArgumentNullException(nameof(servicioFidelizacion));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }
        
        /// <inheritdoc/>
        public async Task<ResultadoDescuento> CalcularDescuentoAsync(Guid clienteId, decimal montoTotal)
        {
            // Solo aplicamos caché para este método que es consultivo
            // La clave incluye el clienteId y el montoTotal para ser único por cada combinación
            var cacheKey = $"{CacheKeyPrefix}CalcularDescuento_{clienteId}_{montoTotal}";
            
            return await _cacheService.GetOrAddAsync(
                cacheKey,
                async (ct) => await _servicioFidelizacionOriginal.CalcularDescuentoAsync(clienteId, montoTotal),
                CacheDurationMinutes);
        }
        
        /// <inheritdoc/>
        public async Task AcumularPuntosAsync(Guid clienteId, Guid comandaId, decimal montoTotal)
        {
            // Este método modifica estado, no lo cachemos pero invalidamos la caché existente
            await _servicioFidelizacionOriginal.AcumularPuntosAsync(clienteId, comandaId, montoTotal);
            
            // Invalida cualquier caché relacionada con este cliente
            InvalidarCacheCliente(clienteId);
        }
        
        /// <inheritdoc/>
        public async Task CanjearPuntosAsync(Guid clienteId, int puntos, string concepto)
        {
            // Este método modifica estado, no lo cachemos pero invalidamos la caché existente
            await _servicioFidelizacionOriginal.CanjearPuntosAsync(clienteId, puntos, concepto);
            
            // Invalida cualquier caché relacionada con este cliente
            InvalidarCacheCliente(clienteId);
        }
        
        /// <inheritdoc/>
        public decimal CalcularDescuentoPorPuntos(int puntos, decimal montoTotal)
        {
            // Para este método simple, también podemos usar caché
            var cacheKey = $"{CacheKeyPrefix}CalcularDescuentoPorPuntos_{puntos}_{montoTotal}";
            
            return _cacheService.GetOrAdd(
                cacheKey,
                () => _servicioFidelizacionOriginal.CalcularDescuentoPorPuntos(puntos, montoTotal),
                CacheDurationMinutes);
        }
        
        /// <summary>
        /// Invalida toda la caché relacionada con un cliente específico
        /// </summary>
        private void InvalidarCacheCliente(Guid clienteId)
        {
            _cacheService.InvalidatePattern($"{CacheKeyPrefix}CalcularDescuento_{clienteId}_");
        }
        
        /// <inheritdoc/>
        public void InvalidarCache()
        {
            _cacheService.InvalidatePattern(CacheKeyPrefix);
        }
    }
} 