namespace RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se crea una nueva reservación
    /// </summary>
    public class ReservacionCreada : DomainEvent
    {
        /// <summary>
        /// Identificador de la reservación creada
        /// </summary>
        public Guid ReservacionId { get; }

        /// <summary>
        /// Identificador del cliente que realizó la reservación
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Identificador de la mesa reservada
        /// </summary>
        public Guid MesaId { get; }

        /// <summary>
        /// Fecha de la reservación
        /// </summary>
        public DateTime Fecha { get; }

        /// <summary>
        /// Hora de la reservación
        /// </summary>
        public TimeSpan Hora { get; }

        /// <summary>
        /// Cantidad de personas
        /// </summary>
        public int CantidadPersonas { get; }
        
        /// <summary>
        /// Constructor para crear una nueva instancia del evento
        /// </summary>
        public ReservacionCreada(Guid reservacionId, Guid clienteId, Guid mesaId, DateTime fecha, TimeSpan hora, int cantidadPersonas)
        {
            ReservacionId = reservacionId;
            ClienteId = clienteId;
            MesaId = mesaId;
            Fecha = fecha;
            Hora = hora;
            CantidadPersonas = cantidadPersonas;
        }
    }
}


