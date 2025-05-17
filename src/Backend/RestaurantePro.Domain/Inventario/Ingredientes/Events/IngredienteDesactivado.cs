    namespace RestaurantePro.Domain.Inventario.Ingredientes.Events
{
    /// <summary>
    /// Evento que se lanza cuando se desactiva un ingrediente
    /// </summary>
    public class IngredienteDesactivado : IDomainEvent
    {
        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; }
        
        /// <summary>
        /// Id del ingrediente desactivado
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; }
        
        public IngredienteDesactivado(Guid ingredienteId, string nombre)
        {
            OccurredOn = DateTime.UtcNow;
            IngredienteId = ingredienteId;
            Nombre = nombre;
        }
    }
} 
