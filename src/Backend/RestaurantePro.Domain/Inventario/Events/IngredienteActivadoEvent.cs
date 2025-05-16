using RestaurantePro.Domain.Core.Base.Interfaces;

namespace RestaurantePro.Domain.Inventario.Events
{
    /// <summary>
    /// Evento de dominio que se lanza cuando se activa un ingrediente
    /// </summary>
    public class IngredienteActivadoEvent : IDomainEvent
    {
        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; }
        
        /// <summary>
        /// Id del ingrediente activado
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente activado
        /// </summary>
        public string Nombre { get; }
        
        public IngredienteActivadoEvent(Guid ingredienteId, string nombre)
        {
            OccurredOn = DateTime.UtcNow;
            IngredienteId = ingredienteId;
            Nombre = nombre;
        }
    }
} 