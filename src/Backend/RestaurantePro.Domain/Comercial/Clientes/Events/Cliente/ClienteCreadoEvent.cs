namespace RestaurantePro.Domain.Comercial.Clientes.Events.Cliente
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se crea un cliente
    /// </summary>
    public class ClienteCreadoEvent : IDomainEvent
    {
        /// <summary>
        /// Identificador del cliente creado
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Nombre completo del cliente
        /// </summary>
        public string NombreCompleto { get; }

        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        public ClienteCreadoEvent(Guid clienteId, string nombreCompleto)
        {
            ClienteId = clienteId;
            NombreCompleto = nombreCompleto;
        }
    }
} 