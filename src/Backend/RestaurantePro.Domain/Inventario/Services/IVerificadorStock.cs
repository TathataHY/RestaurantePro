namespace RestaurantePro.Domain.Inventario.Services
{
    /// <summary>
    /// Interfaz para el servicio que verifica el stock de ingredientes y genera órdenes de compra automáticas
    /// </summary>
    public interface IVerificadorStock
    {
        /// <summary>
        /// Verifica los ingredientes con stock bajo y genera órdenes de compra automáticas
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la verificación</returns>
        Task<ResultadoVerificacionStock> VerificarYGenerarOrdenesCompraAsync(CancellationToken cancellationToken = default);
    }
} 