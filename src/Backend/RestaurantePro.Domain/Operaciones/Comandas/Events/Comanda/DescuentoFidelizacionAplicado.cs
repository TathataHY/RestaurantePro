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
        /// Monto del descuento aplicado
        /// </summary>
        public decimal MontoDescuento { get; }

        /// <summary>
        /// Porcentaje del descuento aplicado (entre 0 y 1)
        /// </summary>
        public decimal PorcentajeDescuento { get; }

        /// <summary>
        /// Constructor para el evento de descuento aplicado
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="montoDescuento">Monto del descuento</param>
        /// <param name="porcentajeDescuento">Porcentaje aplicado (entre 0 y 1)</param>
        public DescuentoFidelizacionAplicado(Guid comandaId, decimal montoDescuento, decimal porcentajeDescuento)
        {
            ComandaId = comandaId;
            MontoDescuento = montoDescuento;
            PorcentajeDescuento = porcentajeDescuento;
        }
    }
} 