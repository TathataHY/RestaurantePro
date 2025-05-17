namespace RestaurantePro.Domain.Comercial.Clientes.Events.TarjetaFidelizacion
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se activa una tarjeta
    /// </summary>
    public class TarjetaFidelizacionActivada : DomainEvent
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
        /// Fecha de activación
        /// </summary>
        public DateTime FechaActivacion { get; }

        
        public TarjetaFidelizacionActivada(Guid tarjetaId, Guid clienteId, DateTime fechaActivacion)
        {
            TarjetaId = tarjetaId;
            ClienteId = clienteId;
            FechaActivacion = fechaActivacion;
        }
    }
} 

