using RestaurantePro.Domain.Core.Base.Interfaces;

namespace RestaurantePro.Domain.Inventario.Events
{
    /// <summary>
    /// Evento de dominio que se lanza cuando se desactiva un ingrediente
    /// </summary>
    public class IngredienteDesactivadoEvent : IDomainEvent
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
        /// Nombre del ingrediente desactivado
        /// </summary>
        public string Nombre { get; }
        
        public IngredienteDesactivadoEvent(Guid ingredienteId, string nombre)
        {
            OccurredOn = DateTime.UtcNow;
            IngredienteId = ingredienteId;
            Nombre = nombre;
        }
    }
} 