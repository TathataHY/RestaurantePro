namespace RestaurantePro.Domain.Comercial.Clientes.Events.TarjetaFidelizacion
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se canjean puntos de una tarjeta
    /// </summary>
    public class PuntosCanjeadosEvent : IDomainEvent
    {
        /// <summary>
        /// Identificador de la tarjeta
        /// </summary>
        public Guid TarjetaId { get; }

        /// <summary>
        /// Puntos canjeados
        /// </summary>
        public int PuntosCanjeados { get; }

        /// <summary>
        /// Concepto del canje
        /// </summary>
        public string Concepto { get; }

        /// <summary>
        /// Puntos disponibles después del canje
        /// </summary>
        public int PuntosDisponibles { get; }

        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        public PuntosCanjeadosEvent(Guid tarjetaId, int puntosCanjeados, string concepto, int puntosDisponibles)
        {
            TarjetaId = tarjetaId;
            PuntosCanjeados = puntosCanjeados;
            Concepto = concepto;
            PuntosDisponibles = puntosDisponibles;
        }
    }
} 