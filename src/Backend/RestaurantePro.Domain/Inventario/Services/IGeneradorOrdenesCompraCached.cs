namespace RestaurantePro.Domain.Inventario.Services
{
    /// <summary>
    /// Interfaz extendida para el generador de órdenes de compra con soporte de caché
    /// </summary>
    public interface IGeneradorOrdenesCompraCached : IGeneradorOrdenesCompra
    {
        /// <summary>
        /// Invalida toda la caché del generador de órdenes de compra
        /// </summary>
        void InvalidarCache();
        
        /// <summary>
        /// Invalida la caché para un ingrediente específico
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        void InvalidarCachePorIngrediente(Guid ingredienteId);
    }
} 