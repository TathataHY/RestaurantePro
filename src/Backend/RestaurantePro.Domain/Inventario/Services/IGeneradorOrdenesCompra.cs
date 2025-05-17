namespace RestaurantePro.Domain.Inventario.Services{

    /// <summary>
    /// Interfaz para el generador de órdenes de compra automáticas
    /// basadas en los niveles actuales de inventario
    /// </summary>
    public interface IGeneradorOrdenesCompra
    {
        /// <summary>
        /// Analiza el inventario actual y genera órdenes de compra automáticas
        /// para los ingredientes que están por debajo del stock mínimo
        /// </summary>
        /// <returns>Lista de IDs de las órdenes de compra generadas</returns>
        Task<IEnumerable<Guid>> GenerarOrdenesCompraAutomaticas();
        
        /// <summary>
        /// Analiza un ingrediente específico y genera una orden de compra
        /// si está por debajo del stock mínimo
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente a analizar</param>
        /// <returns>ID de la orden de compra generada o null si no fue necesario</returns>
        Task<Guid?> GenerarOrdenCompraParaIngrediente(Guid ingredienteId);
    }
} 