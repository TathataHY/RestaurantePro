namespace RestaurantePro.Domain.Comercial.Facturacion.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se crea una nueva factura
    /// </summary>
    public class FacturaCreada : DomainEvent
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
        /// Tipo de la factura
        /// </summary>
        public TipoFactura TipoFactura { get; }

        /// <summary>
        /// Fecha de emisión de la factura
        /// </summary>
        public DateTime FechaEmision { get; }

        /// <summary>
        /// Constructor para el evento FacturaCreada
        /// </summary>
        /// <param name="facturaId">Identificador de la factura</param>
        /// <param name="numeroFactura">Número de la factura</param>
        /// <param name="tipoFactura">Tipo de la factura</param>
        /// <param name="fechaEmision">Fecha de emisión</param>
        public FacturaCreada(Guid facturaId, string numeroFactura, TipoFactura tipoFactura, DateTime fechaEmision)
        {
            FacturaId = facturaId;
            NumeroFactura = numeroFactura;
            TipoFactura = tipoFactura;
            FechaEmision = fechaEmision;
        }
    }
} 