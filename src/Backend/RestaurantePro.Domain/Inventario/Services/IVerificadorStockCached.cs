namespace RestaurantePro.Domain.Inventario.Services
{
    /// <summary>
    /// Interfaz extendida para el verificador de stock que utiliza caché
    /// </summary>
    public interface IVerificadorStockCached : IVerificadorStock
    {
        /// <summary>
        /// Invalida la caché del verificador de stock
        /// </summary>
        void InvalidarCache();
    }
} 