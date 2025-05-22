namespace RestaurantePro.Domain.Comercial.Facturacion.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se emite una factura
    /// </summary>
    public class FacturaEmitida : DomainEvent
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
        /// Fecha de emisión de la factura
        /// </summary>
        public DateTime FechaEmision { get; }

        /// <summary>
        /// Fecha de vencimiento para el pago (si aplica)
        /// </summary>
        public DateTime? FechaVencimiento { get; }

        /// <summary>
        /// Constructor para el evento FacturaEmitida
        /// </summary>
        /// <param name="facturaId">Identificador de la factura</param>
        /// <param name="numeroFactura">Número de la factura</param>
        /// <param name="fechaEmision">Fecha de emisión</param>
        /// <param name="fechaVencimiento">Fecha de vencimiento (opcional)</param>
        public FacturaEmitida(Guid facturaId, string numeroFactura, DateTime fechaEmision, DateTime? fechaVencimiento = null)
        {
            FacturaId = facturaId;
            NumeroFactura = numeroFactura;
            FechaEmision = fechaEmision;
            FechaVencimiento = fechaVencimiento;
        }
    }
} 