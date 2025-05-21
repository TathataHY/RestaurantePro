namespace RestaurantePro.Domain.Comercial.Clientes.Events.Cliente
{
    /// <summary>
    /// Evento que se dispara cuando se crea una tarjeta de fidelización para un cliente
    /// </summary>
    public class TarjetaFidelizacionCreada : DomainEvent
    {
        /// <summary>
        /// ID del cliente al que se le creó la tarjeta
        /// </summary>
        public Guid ClienteId { get; }
        
        /// <summary>
        /// ID de la tarjeta creada
        /// </summary>
        public Guid TarjetaId { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="tarjetaId">ID de la tarjeta</param>
        public TarjetaFidelizacionCreada(Guid clienteId, Guid tarjetaId)
        {
            ClienteId = clienteId;
            TarjetaId = tarjetaId;
        }
    }
} 