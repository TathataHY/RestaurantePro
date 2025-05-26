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
        public async Task<Result<IEnumerable<Guid>>> GenerarOrdenesCompraAutomaticas(CancellationToken cancellationToken = default)
        {
            // Esta operación es costosa pero sus resultados pueden cambiar frecuentemente
            // La cacheamos por un tiempo corto (15 minutos)
            var cacheKey = $"{CacheKeyPrefix}GenerarOrdenesCompraAutomaticas";
            
            try
            {
                return await _cacheService.GetOrAddAsync(
                    cacheKey,
                    async (ct) => await _servicioOriginal.GenerarOrdenesCompraAutomaticas(ct),
                    CacheDurationMinutes,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                // Si hay un error en la caché, ejecutamos directamente la operación
                return await _servicioOriginal.GenerarOrdenesCompraAutomaticas(cancellationToken);
            }
        }
        
        /// <inheritdoc/>
        public async Task<Result<Guid?>> GenerarOrdenCompraParaIngrediente(Guid ingredienteId, CancellationToken cancellationToken = default)
        {
            // Si el ID no es válido, falla temprano sin consultar caché
            if (ingredienteId == Guid.Empty)
                return Result.Failure<Guid?>("El ID del ingrediente no puede estar vacío");
                
            var cacheKey = $"{CacheKeyPrefix}GenerarOrdenCompraParaIngrediente_{ingredienteId}";
            
            try
            {
                return await _cacheService.GetOrAddAsync(
                    cacheKey,
                    async (ct) => await _servicioOriginal.GenerarOrdenCompraParaIngrediente(ingredienteId, ct),
                    CacheDurationMinutes,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                // Si hay un error en la caché, ejecutamos directamente la operación
                return await _servicioOriginal.GenerarOrdenCompraParaIngrediente(ingredienteId, cancellationToken);
            }
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