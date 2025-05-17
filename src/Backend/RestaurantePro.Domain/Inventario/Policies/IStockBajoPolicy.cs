namespace RestaurantePro.Domain.Inventario.Policies
{
    /// <summary>
    /// Resultado de la ejecución de la política de stock bajo
    /// </summary>
    public class ResultadoStockBajoPolicy
    {
        /// <summary>
        /// Lista de IDs de notificaciones generadas
        /// </summary>
        public List<Guid> Notificaciones { get; } = new List<Guid>();
        
        /// <summary>
        /// Lista de IDs de órdenes de compra generadas
        /// </summary>
        public List<Guid> OrdenesCompraGeneradas { get; } = new List<Guid>();
    }
    
    /// <summary>
    /// Interfaz para la política de stock bajo.
    /// Esta política se encarga de monitorear los niveles de inventario
    /// y ejecutar acciones automáticas cuando el stock está por debajo del mínimo.
    /// </summary>
    public interface IStockBajoPolicy
    {
        /// <summary>
        /// Ejecuta la política de stock bajo, verificando ingredientes con stock bajo
        /// y tomando acciones automáticamente (notificaciones, órdenes de compra)
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la ejecución de la política</returns>
        Task<ResultadoStockBajoPolicy> EjecutarPolicy(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Ejecuta la política para un ingrediente específico
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente a verificar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la ejecución de la política</returns>
        Task<ResultadoStockBajoPolicy> EjecutarPolicyParaIngrediente(Guid ingredienteId, CancellationToken cancellationToken = default);
    }
} 