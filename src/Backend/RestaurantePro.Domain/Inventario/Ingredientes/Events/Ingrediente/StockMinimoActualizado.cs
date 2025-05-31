namespace RestaurantePro.Domain.Inventario.Ingredientes.Events.Ingrediente
{
    /// <summary>
    /// Evento que se genera cuando se actualiza el stock mínimo de un ingrediente.
    /// </summary>
    public class StockMinimoActualizado : DomainEvent
    {
        /// <summary>
        /// ID del ingrediente cuyo stock mínimo ha sido actualizado
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Stock mínimo anterior
        /// </summary>
        public decimal StockMinimoAnterior { get; }
        
        /// <summary>
        /// Nuevo stock mínimo
        /// </summary>
        public decimal StockMinimoNuevo { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="stockMinimoAnterior">Stock mínimo anterior</param>
        /// <param name="stockMinimoNuevo">Nuevo stock mínimo</param>
        public StockMinimoActualizado(Guid ingredienteId, string nombre, decimal stockMinimoAnterior, decimal stockMinimoNuevo) : base()
        {
            IngredienteId = ingredienteId;
            Nombre = nombre;
            StockMinimoAnterior = stockMinimoAnterior;
            StockMinimoNuevo = stockMinimoNuevo;
        }
    }
} 