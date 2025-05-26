namespace RestaurantePro.Domain.Inventario.Services
{
    /// <summary>
    /// Implementación del VerificadorStock que utiliza caché para mejorar el rendimiento
    /// </summary>
    public class VerificadorStockCached : IVerificadorStockCached
    {
        private readonly IVerificadorStock _verificadorStockOriginal;
        private readonly ICacheService _cacheService;
        private const string CacheKeyPrefix = "VerificadorStock_";
        private const int CacheDurationMinutes = 30; // Caché de 30 minutos para evitar verificaciones frecuentes
        
        /// <summary>
        /// Constructor
        /// </summary>
        public VerificadorStockCached(
            IVerificadorStock verificadorStock,
            ICacheService cacheService)
        {
            _verificadorStockOriginal = verificadorStock ?? throw new ArgumentNullException(nameof(verificadorStock));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }
        
        /// <inheritdoc/>
        public async Task<Result<ResultadoVerificacionStock>> VerificarYGenerarOrdenesCompraAsync(CancellationToken cancellationToken = default)
        {
            // Clave única para esta operación
            var cacheKey = $"{CacheKeyPrefix}VerificarYGenerarOrdenesCompra";
            
            try
            {
                // Intentar obtener de caché o ejecutar la operación costosa
                return await _cacheService.GetOrAddAsync(
                    cacheKey, 
                    async (ct) => await _verificadorStockOriginal.VerificarYGenerarOrdenesCompraAsync(ct),
                    CacheDurationMinutes,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                // Si hay un error en la caché, ejecutamos directamente la operación
                return await _verificadorStockOriginal.VerificarYGenerarOrdenesCompraAsync(cancellationToken);
            }
        }
        
        /// <inheritdoc/>
        public void InvalidarCache()
        {
            _cacheService.InvalidatePattern(CacheKeyPrefix);
        }
    }
} 