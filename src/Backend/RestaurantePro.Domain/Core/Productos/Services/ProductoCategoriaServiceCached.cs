namespace RestaurantePro.Domain.Core.Productos.Services
{
    /// <summary>
    /// Implementación del servicio de categorías de productos con soporte de caché para mejorar el rendimiento
    /// </summary>
    public class ProductoCategoriaServiceCached : IProductoCategoriaServiceCached
    {
        private readonly IProductoCategoriaService _servicioOriginal;
        private readonly ICacheService _cacheService;
        private const string CacheKeyPrefix = "ProductoCategoriaService_";
        private const int CacheDurationMinutes = 60; // Caché de 1 hora para catálogos
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ProductoCategoriaServiceCached(
            IProductoCategoriaService productoCategoriaSevice,
            ICacheService cacheService)
        {
            _servicioOriginal = productoCategoriaSevice ?? throw new ArgumentNullException(nameof(productoCategoriaSevice));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }
        
        /// <inheritdoc/>
        public async Task<List<Entities.Producto>> ObtenerProductosPorCategoriaAsync(
            Guid categoriaId, 
            bool soloActivos = true, 
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{CacheKeyPrefix}ObtenerProductosPorCategoria_{categoriaId}_{soloActivos}";
            
            return await _cacheService.GetOrAddAsync(
                cacheKey,
                async (ct) => await _servicioOriginal.ObtenerProductosPorCategoriaAsync(categoriaId, soloActivos, ct),
                CacheDurationMinutes,
                cancellationToken);
        }
        
        /// <inheritdoc/>
        public async Task<int> ActualizarCategoriaProductosAsync(
            List<Guid> productosIds, 
            Guid categoriaId, 
            CancellationToken cancellationToken = default)
        {
            // Esta operación modifica datos, no la cacheamos
            var resultado = await _servicioOriginal.ActualizarCategoriaProductosAsync(productosIds, categoriaId, cancellationToken);
            
            // Invalidar caché para la categoría afectada
            InvalidarCacheCategoria(categoriaId);
            
            // Invalidar caché para cada producto afectado
            foreach (var productoId in productosIds)
            {
                InvalidarCacheProducto(productoId);
            }
            
            return resultado;
        }
        
        /// <inheritdoc/>
        public async Task<int> ReorganizarCategoriasAsync(
            Dictionary<Guid, int> nuevosOrdenes, 
            CancellationToken cancellationToken = default)
        {
            // Esta operación modifica datos, no la cacheamos
            var resultado = await _servicioOriginal.ReorganizarCategoriasAsync(nuevosOrdenes, cancellationToken);
            
            // Invalidar caché para cada categoría afectada
            foreach (var categoriaId in nuevosOrdenes.Keys)
            {
                InvalidarCacheCategoria(categoriaId);
            }
            
            return resultado;
        }
        
        /// <inheritdoc/>
        public void InvalidarCache()
        {
            _cacheService.InvalidatePattern(CacheKeyPrefix);
        }
        
        /// <inheritdoc/>
        public void InvalidarCacheCategoria(Guid categoriaId)
        {
            _cacheService.InvalidatePattern($"{CacheKeyPrefix}ObtenerProductosPorCategoria_{categoriaId}_");
        }
        
        /// <inheritdoc/>
        public void InvalidarCacheProducto(Guid productoId)
        {
            // Como los productos se obtienen por categoría, no hay una caché específica por producto
            // Sin embargo, esta función puede ser útil para invalidar otras cachés relacionadas con productos en el futuro
        }
    }
} 