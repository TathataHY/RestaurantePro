namespace RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion
{
    /// <summary>
    /// Evento de dominio que se dispara cuando una reservación es confirmada
    /// </summary>
    public class ReservacionConfirmada : DomainEvent
    {
        /// <summary>
        /// Identificador de la reservación
        /// </summary>
        public Guid ReservacionId { get; }

        /// <summary>
        /// Constructor para crear una nueva instancia del evento
        /// </summary>
        public ReservacionConfirmada(Guid reservacionId)
        {
            ReservacionId = reservacionId;
        }
    }
}


