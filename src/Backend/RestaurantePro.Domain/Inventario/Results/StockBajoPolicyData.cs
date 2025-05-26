namespace RestaurantePro.Domain.Inventario.Results
{
    /// <summary>
    /// Datos de resultado para las operaciones de la política de stock bajo
    /// Reemplaza a ResultadoStockBajoPolicy para adaptarse al patrón Result
    /// </summary>
    public class StockBajoPolicyData
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
        
        /// <summary>
        /// Indica si la operación generó algún resultado (notificaciones u órdenes)
        /// </summary>
        public bool TieneResultados => 
            Notificaciones.Any() || 
            OrdenesCompraGeneradas.Any() || 
            IngredientesPriorizados.Any();
    }
} 