namespace RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion
{
    /// <summary>
    /// Evento de dominio que se dispara cuando una reservación es cancelada
    /// </summary>
    public class ReservacionCancelada : DomainEvent
    {
        /// <summary>
        /// Identificador de la reservación
        /// </summary>
        public Guid ReservacionId { get; }

        /// <summary>
        /// Motivo de la cancelación
        /// </summary>
        public string Motivo { get; }

        /// <summary>
        /// Fecha en que ocurrió el evento
        /// </summary>
        
        /// <summary>
        /// Constructor para crear una nueva instancia del evento
        /// </summary>
        public ReservacionCancelada(Guid reservacionId, string motivo)
        {
            ReservacionId = reservacionId;
            Motivo = motivo;
                    }
    }
}


