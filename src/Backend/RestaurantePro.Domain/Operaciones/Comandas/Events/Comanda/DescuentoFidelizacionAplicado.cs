namespace RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda
{
    /// <summary>
    /// Evento que se produce cuando se aplica un descuento de fidelización a una comanda
    /// </summary>
    public class DescuentoFidelizacionAplicado : DomainEvent
    {
        /// <summary>
        /// ID de la comanda a la que se aplicó el descuento
        /// </summary>
        public Guid ComandaId { get; }
        
        /// <summary>
        /// ID del cliente al que se aplicó el descuento
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Monto del descuento aplicado
        /// </summary>
        public decimal MontoDescuento { get; }

        /// <summary>
        /// Constructor para el evento de descuento aplicado
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="montoDescuento">Monto del descuento</param>
        public DescuentoFidelizacionAplicado(Guid comandaId, Guid clienteId, decimal montoDescuento)
        {
            ComandaId = comandaId;
            ClienteId = clienteId;
            MontoDescuento = montoDescuento;
        }
    }
} 