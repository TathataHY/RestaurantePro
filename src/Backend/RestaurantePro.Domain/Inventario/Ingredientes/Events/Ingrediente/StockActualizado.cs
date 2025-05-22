namespace RestaurantePro.Domain.Inventario.Ingredientes.Events.Ingrediente
{
    /// <summary>
    /// Evento que se lanza cuando se actualiza el stock de un ingrediente
    /// </summary>
    public class StockActualizado : DomainEvent
    {
                
        /// <summary>
        /// Id del ingrediente
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Cantidad actual de stock
        /// </summary>
        public decimal Stock { get; }
        
        public StockActualizado(Guid ingredienteId, string nombre, decimal stock)
        {
                        IngredienteId = ingredienteId;
            Nombre = nombre;
            Stock = stock;
        }
    }
} 

