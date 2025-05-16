namespace RestaurantePro.Domain.Comercial.Clientes.Events.TarjetaFidelizacion
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se crea una tarjeta de fidelización
    /// </summary>
    public class TarjetaFidelizacionCreadaEvent : IDomainEvent
    {
        /// <summary>
        /// Identificador de la tarjeta creada
        /// </summary>
        public Guid TarjetaId { get; }

        /// <summary>
        /// Identificador del cliente
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Código de la tarjeta
        /// </summary>
        public string Codigo { get; }

        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        public TarjetaFidelizacionCreadaEvent(Guid tarjetaId, Guid clienteId, string codigo)
        {
            TarjetaId = tarjetaId;
            ClienteId = clienteId;
            Codigo = codigo;
        }
    }
} 