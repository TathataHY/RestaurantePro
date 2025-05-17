namespace RestaurantePro.Domain.Inventario.Ingredientes.Events
{
    /// <summary>
    /// Evento emitido cuando se registra un nivel bajo de stock para un ingrediente
    /// </summary>
    public class StockBajoRegistrado : DomainEvent
    {
        /// <summary>
        /// ID del ingrediente con stock bajo
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string NombreIngrediente { get; }
        
        /// <summary>
        /// Nivel actual de stock
        /// </summary>
        public decimal StockActual { get; }
        
        /// <summary>
        /// Nivel mínimo requerido
        /// </summary>
        public decimal StockMinimo { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public StockBajoRegistrado(Guid ingredienteId, string nombreIngrediente, decimal stockActual, decimal stockMinimo)
        {
            IngredienteId = ingredienteId;
            NombreIngrediente = nombreIngrediente;
            StockActual = stockActual;
            StockMinimo = stockMinimo;
        }
    }
} 