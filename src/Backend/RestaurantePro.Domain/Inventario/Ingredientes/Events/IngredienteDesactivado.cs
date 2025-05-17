    namespace RestaurantePro.Domain.Inventario.Ingredientes.Events
{
    /// <summary>
    /// Evento que se lanza cuando se desactiva un ingrediente
    /// </summary>
    public class IngredienteDesactivado : DomainEvent
    {
                
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
                        IngredienteId = ingredienteId;
            Nombre = nombre;
        }
    }
} 

