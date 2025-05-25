namespace RestaurantePro.Domain.Core.Productos.Services
{
    /// <summary>
    /// Implementación del servicio de recetas con soporte de caché para mejorar el rendimiento
    /// </summary>
    public class RecetaServiceCached : IRecetaServiceCached
    {
        private readonly IRecetaService _recetaServiceOriginal;
        private readonly ICacheService _cacheService;
        private const string CacheKeyPrefix = "RecetaService_";
        private const int CacheDurationMinutes = 60; // Caché de 1 hora para datos de recetas
        
        /// <summary>
        /// Constructor
        /// </summary>
        public RecetaServiceCached(
            IRecetaService recetaService,
            ICacheService cacheService)
        {
            _recetaServiceOriginal = recetaService ?? throw new ArgumentNullException(nameof(recetaService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }
        
        /// <inheritdoc/>
        public async Task<Dictionary<Guid, decimal>> ObtenerIngredientesParaProductoAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{CacheKeyPrefix}ObtenerIngredientesParaProducto_{productoId}";
            
            return await _cacheService.GetOrAddAsync(
                cacheKey,
                async (ct) => await _recetaServiceOriginal.ObtenerIngredientesParaProductoAsync(productoId, ct),
                CacheDurationMinutes,
                cancellationToken);
        }
        
        /// <inheritdoc/>
        public async Task<bool> VerificarDisponibilidadIngredientesAsync(Guid productoId, int cantidad, CancellationToken cancellationToken = default)
        {
            // No cachear este método porque la disponibilidad puede cambiar frecuentemente
            // y necesitamos datos en tiempo real
            return await _recetaServiceOriginal.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, cancellationToken);
        }
        
        /// <inheritdoc/>
        public async Task<Dictionary<Guid, decimal>> ObtenerIngredientesFaltantesAsync(Guid productoId, int cantidad, CancellationToken cancellationToken = default)
        {
            // No cachear este método porque los faltantes pueden cambiar frecuentemente
            // y necesitamos datos en tiempo real
            return await _recetaServiceOriginal.ObtenerIngredientesFaltantesAsync(productoId, cantidad, cancellationToken);
        }
        
        /// <inheritdoc/>
        public async Task<decimal> CalcularCostoRecetaAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{CacheKeyPrefix}CalcularCostoReceta_{productoId}";
            
            return await _cacheService.GetOrAddAsync(
                cacheKey,
                async (ct) => await _recetaServiceOriginal.CalcularCostoRecetaAsync(productoId, ct),
                CacheDurationMinutes,
                cancellationToken);
        }
        
        /// <inheritdoc/>
        public async Task<ValueObjects.RentabilidadProducto> CalcularRentabilidadProductoAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{CacheKeyPrefix}CalcularRentabilidadProducto_{productoId}";
            
            return await _cacheService.GetOrAddAsync(
                cacheKey,
                async (ct) => await _recetaServiceOriginal.CalcularRentabilidadProductoAsync(productoId, ct),
                CacheDurationMinutes,
                cancellationToken);
        }
        
        /// <inheritdoc/>
        public void InvalidarCache()
        {
            _cacheService.InvalidatePattern(CacheKeyPrefix);
        }
        
        /// <inheritdoc/>
        public void InvalidarCacheProducto(Guid productoId)
        {
            // Invalidar todas las entradas de caché relacionadas con este producto
            _cacheService.InvalidatePattern($"{CacheKeyPrefix}ObtenerIngredientesParaProducto_{productoId}");
            _cacheService.InvalidatePattern($"{CacheKeyPrefix}CalcularCostoReceta_{productoId}");
            _cacheService.InvalidatePattern($"{CacheKeyPrefix}CalcularRentabilidadProducto_{productoId}");
        }
    }
} 