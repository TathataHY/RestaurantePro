using RestaurantePro.Domain.Core.Base.Interfaces;

namespace RestaurantePro.Domain.Inventario.Events
{
    /// <summary>
    /// Evento de dominio que se lanza cuando se actualiza el stock de un ingrediente
    /// </summary>
    public class StockActualizadoEvent : IDomainEvent
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
        public string IngredienteNombre { get; }
        
        /// <summary>
        /// Nuevo stock del ingrediente
        /// </summary>
        public decimal NuevoStock { get; }
        
        public StockActualizadoEvent(Guid ingredienteId, string ingredienteNombre, decimal nuevoStock)
        {
            OccurredOn = DateTime.UtcNow;
            IngredienteId = ingredienteId;
            IngredienteNombre = ingredienteNombre;
            NuevoStock = nuevoStock;
        }
    }
} 