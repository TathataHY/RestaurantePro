namespace RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion
{
    /// <summary>
    /// Evento de dominio que se dispara cuando una reservación se marca como no asistida (no-show)
    /// </summary>
    public class ReservacionNoAsistio : DomainEvent
    {
        /// <summary>
        /// Identificador de la reservación
        /// </summary>
        public Guid ReservacionId { get; }

        /// <summary>
        /// Identificador del cliente que no asistió
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Identificador de la mesa que quedó libre
        /// </summary>
        public Guid MesaId { get; }

        /// <summary>
        /// Fecha de la reservación no asistida
        /// </summary>
        public DateTime Fecha { get; }

        /// <summary>
        /// Cantidad de personas que estaban en la reservación
        /// </summary>
        public int CantidadPersonas { get; }

        /// <summary>
        /// Constructor para crear una nueva instancia del evento
        /// </summary>
        public ReservacionNoAsistio(Guid reservacionId, Guid clienteId, Guid mesaId, DateTime fecha, int cantidadPersonas)
        {
            ReservacionId = reservacionId;
            ClienteId = clienteId;
            MesaId = mesaId;
            Fecha = fecha;
            CantidadPersonas = cantidadPersonas;
        }
    }
} 