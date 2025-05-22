namespace RestaurantePro.Domain.Comercial.Facturacion.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se registra un pago en una factura
    /// </summary>
    public class PagoFacturaRegistrado : DomainEvent
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
        /// Identificador del pago asociado
        /// </summary>
        public Guid PagoId { get; }

        /// <summary>
        /// Monto del pago
        /// </summary>
        public decimal Monto { get; }

        /// <summary>
        /// Monto total pagado hasta el momento
        /// </summary>
        public decimal TotalPagado { get; }

        /// <summary>
        /// Fecha del pago
        /// </summary>
        public DateTime FechaPago { get; }

        /// <summary>
        /// Constructor para el evento PagoFacturaRegistrado
        /// </summary>
        /// <param name="facturaId">Identificador de la factura</param>
        /// <param name="numeroFactura">Número de la factura</param>
        /// <param name="pagoId">Identificador del pago</param>
        /// <param name="monto">Monto del pago</param>
        /// <param name="totalPagado">Total pagado hasta el momento</param>
        /// <param name="fechaPago">Fecha del pago</param>
        public PagoFacturaRegistrado(
            Guid facturaId,
            string numeroFactura,
            Guid pagoId,
            decimal monto,
            decimal totalPagado,
            DateTime fechaPago)
        {
            FacturaId = facturaId;
            NumeroFactura = numeroFactura;
            PagoId = pagoId;
            Monto = monto;
            TotalPagado = totalPagado;
            FechaPago = fechaPago;
        }
    }
} 