namespace RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se cambia la mesa asignada a una reservación
    /// </summary>
    public class MesaReservacionCambiada : DomainEvent
    {
        /// <summary>
        /// Identificador de la reservación
        /// </summary>
        public Guid ReservacionId { get; }

        /// <summary>
        /// Identificador de la mesa anterior
        /// </summary>
        public Guid MesaAnteriorId { get; }

        /// <summary>
        /// Identificador de la nueva mesa
        /// </summary>
        public Guid NuevaMesaId { get; }

        /// <summary>
        /// Constructor para crear una nueva instancia del evento
        /// </summary>
        public MesaReservacionCambiada(Guid reservacionId, Guid mesaAnteriorId, Guid nuevaMesaId)
        {
            ReservacionId = reservacionId;
            MesaAnteriorId = mesaAnteriorId;
            NuevaMesaId = nuevaMesaId;
        }
    }
} 