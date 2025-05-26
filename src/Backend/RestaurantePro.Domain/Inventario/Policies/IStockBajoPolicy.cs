namespace RestaurantePro.Domain.Inventario.Policies
{
    /// <summary>
    /// Interfaz para la política de stock bajo.
    /// Esta política se encarga de monitorear los niveles de inventario
    /// y ejecutar acciones automáticamente cuando el stock está por debajo del mínimo.
    /// </summary>
    public interface IStockBajoPolicy
    {
        /// <summary>
        /// Ejecuta la política de stock bajo, verificando ingredientes con stock bajo
        /// y tomando acciones automáticamente (notificaciones, órdenes de compra)
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la ejecución de la política</returns>
        Task<Result<StockBajoPolicyData>> EjecutarPolicy(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Ejecuta la política para un ingrediente específico
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente a verificar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la ejecución de la política</returns>
        Task<Result<StockBajoPolicyData>> EjecutarPolicyParaIngrediente(Guid ingredienteId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Prioriza los ingredientes que necesitan reposición según rotación y temporada
        /// </summary>
        /// <param name="considerarTemporadaActual">Si es true, prioriza ingredientes según la temporada actual</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la lista de ingredientes priorizados</returns>
        Task<Result<StockBajoPolicyData>> PriorizarIngredientesParaReposicion(bool considerarTemporadaActual = true, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Alias de EjecutarPolicy que ejecuta la política de stock bajo
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la ejecución de la política</returns>
        Task<Result<StockBajoPolicyData>> EjecutarAsync(CancellationToken cancellationToken = default);
    }
} 