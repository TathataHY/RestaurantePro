namespace RestaurantePro.Domain.Comercial.Clientes.Events.TarjetaFidelizacion
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se crea una tarjeta de fidelización
    /// </summary>
    public class TarjetaFidelizacionCreada : DomainEvent
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

        
        public TarjetaFidelizacionCreada(Guid tarjetaId, Guid clienteId, string codigo)
        {
            TarjetaId = tarjetaId;
            ClienteId = clienteId;
            Codigo = codigo;
        }
    }
} 

