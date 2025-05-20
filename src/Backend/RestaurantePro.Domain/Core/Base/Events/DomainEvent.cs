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
        /// Identificador de la entidad que generó el evento
        /// </summary>
        public virtual Guid EntityId { get; protected set; }
        
        /// <summary>
        /// Nombre del contexto delimitado al que pertenece este evento
        /// </summary>
        public virtual string? BoundedContext { get; protected set; }
        
        /// <summary>
        /// Constructor base para todos los eventos de dominio
        /// </summary>
        protected DomainEvent()
        {
            OccurredOn = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Constructor base para todos los eventos de dominio
        /// </summary>
        /// <param name="entityId">ID de la entidad que generó el evento</param>
        /// <param name="boundedContext">Contexto delimitado al que pertenece el evento</param>
        protected DomainEvent(Guid entityId, string? boundedContext = null)
        {
            OccurredOn = DateTime.UtcNow;
            EntityId = entityId;
            BoundedContext = boundedContext;
        }
    }
} 