namespace RestaurantePro.Domain.Comercial.Facturacion.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando una factura se paga completamente
    /// </summary>
    public class FacturaPagada : DomainEvent
    {
        /// <summary>
        /// Identificador único de la factura
        /// </summary>
        public Guid FacturaId { get; }

        /// <summary>
        /// Número de la factura
        /// </summary>
        public string NumeroFactura { get; }

        /// <summary>
        /// Fecha de pago completo
        /// </summary>
        public DateTime FechaPago { get; }

        /// <summary>
        /// Constructor para el evento FacturaPagada
        /// </summary>
        /// <param name="facturaId">Identificador de la factura</param>
        /// <param name="numeroFactura">Número de la factura</param>
        /// <param name="fechaPago">Fecha de pago completo</param>
        public FacturaPagada(Guid facturaId, string numeroFactura, DateTime fechaPago)
        {
            FacturaId = facturaId;
            NumeroFactura = numeroFactura;
            FechaPago = fechaPago;
        }
    }
} 