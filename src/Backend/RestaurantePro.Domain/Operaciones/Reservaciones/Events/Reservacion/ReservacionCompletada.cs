namespace RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion
{
    /// <summary>
    /// Evento de dominio que se dispara cuando una reservación se completa
    /// </summary>
    public class ReservacionCompletada : IDomainEvent
    {
        /// <summary>
        /// Identificador de la reservación
        /// </summary>
        public Guid ReservacionId { get; }

        /// <summary>
        /// Fecha en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Constructor para crear una nueva instancia del evento
        /// </summary>
        public ReservacionCompletada(Guid reservacionId)
        {
            ReservacionId = reservacionId;
            OccurredOn = DateTime.Now;
        }
    }
}
