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
        
        /// <summary>
        /// Ingredientes priorizados por rotación, temporada y criticidad
        /// </summary>
        public List<IngredientePriorizado> IngredientesPriorizados { get; } = new List<IngredientePriorizado>();
    }
    
    /// <summary>
    /// Clase que representa un ingrediente priorizado para reposición
    /// </summary>
    public class IngredientePriorizado
    {
        /// <summary>
        /// ID del ingrediente
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Prioridad calculada (mayor número = mayor prioridad)
        /// </summary>
        public int Prioridad { get; }
        
        /// <summary>
        /// Nivel de rotación
        /// </summary>
        public RotacionIngrediente Rotacion { get; }
        
        /// <summary>
        /// Temporada
        /// </summary>
        public TemporadaIngrediente Temporada { get; }
        
        /// <summary>
        /// Stock actual
        /// </summary>
        public decimal Stock { get; }
        
        /// <summary>
        /// Stock mínimo
        /// </summary>
        public decimal StockMinimo { get; }
        
        /// <summary>
        /// Porcentaje de stock (actual / mínimo)
        /// </summary>
        public decimal PorcentajeStock => Stock / (StockMinimo > 0 ? StockMinimo : 1) * 100;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public IngredientePriorizado(
            Guid ingredienteId, 
            string nombre, 
            int prioridad, 
            RotacionIngrediente rotacion, 
            TemporadaIngrediente temporada,
            decimal stock,
            decimal stockMinimo)
        {
            IngredienteId = ingredienteId;
            Nombre = nombre;
            Prioridad = prioridad;
            Rotacion = rotacion;
            Temporada = temporada;
            Stock = stock;
            StockMinimo = stockMinimo;
        }
    }
    
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
        Task<ResultadoStockBajoPolicy> EjecutarPolicy(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Ejecuta la política para un ingrediente específico
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente a verificar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la ejecución de la política</returns>
        Task<ResultadoStockBajoPolicy> EjecutarPolicyParaIngrediente(Guid ingredienteId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Prioriza los ingredientes que necesitan reposición según rotación y temporada
        /// </summary>
        /// <param name="considerarTemporadaActual">Si es true, prioriza ingredientes según la temporada actual</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la lista de ingredientes priorizados</returns>
        Task<ResultadoStockBajoPolicy> PriorizarIngredientesParaReposicion(bool considerarTemporadaActual = true, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Alias de EjecutarPolicy que ejecuta la política de stock bajo
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la ejecución de la política</returns>
        Task<ResultadoStockBajoPolicy> EjecutarAsync(CancellationToken cancellationToken = default);
    }
} 