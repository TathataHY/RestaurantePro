namespace RestaurantePro.Domain.Comercial.Clientes.Events.TarjetaFidelizacion
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se suspende una tarjeta
    /// </summary>
    public class TarjetaFidelizacionSuspendida : DomainEvent
    {
        /// <summary>
        /// Identificador de la tarjeta
        /// </summary>
        public Guid TarjetaId { get; }

        /// <summary>
        /// Identificador del cliente
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Motivo de la suspensión
        /// </summary>
        public string Motivo { get; }

        
        public TarjetaFidelizacionSuspendida(Guid tarjetaId, Guid clienteId, string motivo)
        {
            TarjetaId = tarjetaId;
            ClienteId = clienteId;
            Motivo = motivo;
        }
    }
} 

