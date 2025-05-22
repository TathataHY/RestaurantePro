namespace RestaurantePro.Domain.Comercial.Facturacion.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando una factura se marca como vencida
    /// </summary>
    public class FacturaVencida : DomainEvent
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
        /// Fecha de vencimiento de la factura
        /// </summary>
        public DateTime FechaVencimiento { get; }

        /// <summary>
        /// Fecha en la que se marcó como vencida
        /// </summary>
        public DateTime FechaMarcacion { get; }

        /// <summary>
        /// Constructor para el evento FacturaVencida
        /// </summary>
        /// <param name="facturaId">Identificador de la factura</param>
        /// <param name="numeroFactura">Número de la factura</param>
        /// <param name="fechaVencimiento">Fecha de vencimiento de la factura</param>
        /// <param name="fechaMarcacion">Fecha en la que se marcó como vencida</param>
        public FacturaVencida(Guid facturaId, string numeroFactura, DateTime fechaVencimiento, DateTime fechaMarcacion)
        {
            FacturaId = facturaId;
            NumeroFactura = numeroFactura;
            FechaVencimiento = fechaVencimiento;
            FechaMarcacion = fechaMarcacion;
        }
    }
} 