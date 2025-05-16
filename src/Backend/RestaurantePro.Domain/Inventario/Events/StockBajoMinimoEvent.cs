using RestaurantePro.Domain.Core.Base.Interfaces;

namespace RestaurantePro.Domain.Inventario.Events
{
    /// <summary>
    /// Evento de dominio que se lanza cuando el stock de un ingrediente cae por debajo del mínimo
    /// </summary>
    public class StockBajoMinimoEvent : IDomainEvent
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
        /// Stock actual del ingrediente
        /// </summary>
        public decimal StockActual { get; }
        
        /// <summary>
        /// Stock mínimo del ingrediente
        /// </summary>
        public decimal StockMinimo { get; }
        
        public StockBajoMinimoEvent(Guid ingredienteId, string ingredienteNombre, decimal stockActual, decimal stockMinimo)
        {
            OccurredOn = DateTime.UtcNow;
            IngredienteId = ingredienteId;
            IngredienteNombre = ingredienteNombre;
            StockActual = stockActual;
            StockMinimo = stockMinimo;
        }
    }
} 