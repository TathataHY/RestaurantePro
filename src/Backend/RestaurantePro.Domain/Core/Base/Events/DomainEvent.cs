namespace RestaurantePro.Domain.Core.Base.Events
{
    /// <summary>
    /// Clase base abstracta para todos los eventos de dominio
    /// </summary>
    public abstract class DomainEvent
    {
        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; }
        
        /// <summary>
        /// Constructor base para todos los eventos de dominio
        /// </summary>
        protected DomainEvent()
        {
            OccurredOn = DateTime.UtcNow;
        }
    }
} 