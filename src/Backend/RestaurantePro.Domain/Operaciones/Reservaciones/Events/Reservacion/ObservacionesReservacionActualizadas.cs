namespace RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se actualizan las observaciones de una reservación
    /// </summary>
    public class ObservacionesReservacionActualizadas : DomainEvent
    {
        /// <summary>
        /// Identificador de la reservación
        /// </summary>
        public Guid ReservacionId { get; }

        /// <summary>
        /// Observaciones anteriores
        /// </summary>
        public string ObservacionesAnteriores { get; }

        /// <summary>
        /// Nuevas observaciones
        /// </summary>
        public string NuevasObservaciones { get; }

        /// <summary>
        /// Constructor para crear una nueva instancia del evento
        /// </summary>
        public ObservacionesReservacionActualizadas(Guid reservacionId, string observacionesAnteriores, string nuevasObservaciones)
        {
            ReservacionId = reservacionId;
            ObservacionesAnteriores = observacionesAnteriores;
            NuevasObservaciones = nuevasObservaciones;
        }
    }
} 