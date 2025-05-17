namespace RestaurantePro.Domain.Inventario.Ingredientes.Events
{
    /// <summary>
    /// Evento que se lanza cuando se activa un ingrediente
    /// </summary>
    public class IngredienteActivado : DomainEvent
    {
                
        /// <summary>
        /// Id del ingrediente activado
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; }
        
        public IngredienteActivado(Guid ingredienteId, string nombre)
        {
                        IngredienteId = ingredienteId;
            Nombre = nombre;
        }
    }
} 

