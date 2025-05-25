namespace RestaurantePro.Domain.Core.Productos.Services
{
    /// <summary>
    /// Interfaz extendida para el servicio de recetas con soporte de caché
    /// </summary>
    public interface IRecetaServiceCached : IRecetaService
    {
        /// <summary>
        /// Invalida toda la caché del servicio de recetas
        /// </summary>
        void InvalidarCache();
        
        /// <summary>
        /// Invalida la caché para un producto específico
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        void InvalidarCacheProducto(Guid productoId);
    }
} 