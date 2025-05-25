namespace RestaurantePro.Domain.Core.Productos.Services
{
    /// <summary>
    /// Interfaz extendida para el servicio de categorías de productos con soporte de caché
    /// </summary>
    public interface IProductoCategoriaServiceCached : IProductoCategoriaService
    {
        /// <summary>
        /// Invalida toda la caché del servicio de categorías de productos
        /// </summary>
        void InvalidarCache();
        
        /// <summary>
        /// Invalida la caché para una categoría específica
        /// </summary>
        /// <param name="categoriaId">ID de la categoría</param>
        void InvalidarCacheCategoria(Guid categoriaId);
        
        /// <summary>
        /// Invalida la caché para un producto específico
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        void InvalidarCacheProducto(Guid productoId);
    }
} 