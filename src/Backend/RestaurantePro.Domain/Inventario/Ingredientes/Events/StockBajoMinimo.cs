namespace RestaurantePro.Domain.Inventario.Ingredientes.Events
{
    /// <summary>
    /// Evento que se lanza cuando el stock de un ingrediente está por debajo del mínimo
    /// </summary>
    public class StockBajoMinimo : IDomainEvent
    {
        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; }
        
        /// <summary>
        /// Id del ingrediente
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Stock actual
        /// </summary>
        public decimal StockActual { get; }
        
        /// <summary>
        /// Stock mínimo configurado
        /// </summary>
        public decimal StockMinimo { get; }
        
        public StockBajoMinimo(Guid ingredienteId, string nombre, decimal stockActual, decimal stockMinimo)
        {
            OccurredOn = DateTime.UtcNow;
            IngredienteId = ingredienteId;
            Nombre = nombre;
            StockActual = stockActual;
            StockMinimo = stockMinimo;
        }
    }
} 
