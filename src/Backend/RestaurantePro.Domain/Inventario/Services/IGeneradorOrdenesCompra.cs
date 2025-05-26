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
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con lista de IDs de las órdenes de compra generadas</returns>
        Task<Result<IEnumerable<Guid>>> GenerarOrdenesCompraAutomaticas(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Analiza un ingrediente específico y genera una orden de compra
        /// si está por debajo del stock mínimo
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente a analizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con ID de la orden de compra generada o null si no fue necesario</returns>
        Task<Result<Guid?>> GenerarOrdenCompraParaIngrediente(Guid ingredienteId, CancellationToken cancellationToken = default);
    }
} 