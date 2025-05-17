namespace RestaurantePro.Domain.Inventario.Ingredientes.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se crea un ingrediente
    /// </summary>
    public class IngredienteCreado : DomainEvent
    {
        /// <summary>
        /// ID del ingrediente creado
        /// </summary>
        public Guid IngredienteId { get; }

        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; }

        /// <summary>
        /// Constructor del evento IngredienteCreado
        /// </summary>
        public IngredienteCreado(Guid ingredienteId, string nombre)
        {
            IngredienteId = ingredienteId;
            Nombre = nombre;
        }
    }
}
