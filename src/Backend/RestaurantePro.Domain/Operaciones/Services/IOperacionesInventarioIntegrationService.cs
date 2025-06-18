namespace RestaurantePro.Domain.Operaciones.Services
{
    /// <summary>
    /// Interfaz que define los métodos para la integración entre los contextos de Operaciones e Inventario.
    /// </summary>
    public interface IOperacionesInventarioIntegrationService
    {
        /// <summary>
        /// Descuenta del inventario los ingredientes utilizados en una comanda.
        /// </summary>
        /// <param name="comandaId">Id de la comanda a procesar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result<bool>> DescontarInventarioPorComandaAsync(Guid comandaId, CancellationToken cancellationToken = default);
    }
} 