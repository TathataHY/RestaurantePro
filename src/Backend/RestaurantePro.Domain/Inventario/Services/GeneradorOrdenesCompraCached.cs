namespace RestaurantePro.Domain.Inventario.Services
{
    /// <summary>
    /// Implementación del generador de órdenes de compra con soporte de caché para mejorar el rendimiento
    /// </summary>
    public class GeneradorOrdenesCompraCached : IGeneradorOrdenesCompraCached
    {
        private readonly IGeneradorOrdenesCompra _servicioOriginal;
        private readonly ICacheService _cacheService;
        private const string CacheKeyPrefix = "GeneradorOrdenesCompra_";
        private const int CacheDurationMinutes = 15; // Caché de 15 minutos para datos de órdenes de compra
        
        /// <summary>
        /// Constructor
        /// </summary>
        public GeneradorOrdenesCompraCached(
            IGeneradorOrdenesCompra generadorOrdenesCompra,
            ICacheService cacheService)
        {
            _servicioOriginal = generadorOrdenesCompra ?? throw new ArgumentNullException(nameof(generadorOrdenesCompra));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }
        
        /// <inheritdoc/>
        public async Task<IEnumerable<Guid>> GenerarOrdenesCompraAutomaticas()
        {
            // Esta operación es costosa pero sus resultados pueden cambiar frecuentemente
            // La cacheamos por un tiempo corto (15 minutos)
            var cacheKey = $"{CacheKeyPrefix}GenerarOrdenesCompraAutomaticas";
            
            return await _cacheService.GetOrAddAsync(
                cacheKey,
                async (ct) => await _servicioOriginal.GenerarOrdenesCompraAutomaticas(),
                CacheDurationMinutes);
        }
        
        /// <inheritdoc/>
        public async Task<Guid?> GenerarOrdenCompraParaIngrediente(Guid ingredienteId)
        {
            var cacheKey = $"{CacheKeyPrefix}GenerarOrdenCompraParaIngrediente_{ingredienteId}";
            
            return await _cacheService.GetOrAddAsync(
                cacheKey,
                async (ct) => await _servicioOriginal.GenerarOrdenCompraParaIngrediente(ingredienteId),
                CacheDurationMinutes);
        }
        
        /// <inheritdoc/>
        public void InvalidarCache()
        {
            _cacheService.InvalidatePattern(CacheKeyPrefix);
        }
        
        /// <inheritdoc/>
        public void InvalidarCachePorIngrediente(Guid ingredienteId)
        {
            _cacheService.InvalidatePattern($"{CacheKeyPrefix}GenerarOrdenCompraParaIngrediente_{ingredienteId}");
        }
    }
} 