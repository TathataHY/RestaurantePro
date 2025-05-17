namespace RestaurantePro.Domain.Comercial.Clientes.Events.Cliente
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se asocia una tarjeta de fidelización a un cliente
    /// </summary>
    public class TarjetaFidelizacionAsociada : DomainEvent
    {
        /// <summary>
        /// Identificador del cliente
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Identificador de la tarjeta de fidelización asociada
        /// </summary>
        public Guid TarjetaFidelizacionId { get; }

        public TarjetaFidelizacionAsociada(Guid clienteId, Guid tarjetaFidelizacionId)
        {
            ClienteId = clienteId;
            TarjetaFidelizacionId = tarjetaFidelizacionId;
        }
    }
} 