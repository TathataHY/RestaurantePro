namespace RestaurantePro.Domain.Inventario.Ingredientes.Events
{
    /// <summary>
    /// Evento que se lanza cuando se crea un nuevo ingrediente
    /// </summary>
    public class IngredienteCreado : IDomainEvent
    {
        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; }
        
        /// <summary>
        /// Id del ingrediente creado
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; }
        
        public IngredienteCreado(Guid ingredienteId, string nombre)
        {
            OccurredOn = DateTime.UtcNow;
            IngredienteId = ingredienteId;
            Nombre = nombre;
        }
    }
}
