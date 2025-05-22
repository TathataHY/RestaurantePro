namespace RestaurantePro.Domain.Comercial.Facturacion.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se anula una factura
    /// </summary>
    public class FacturaAnulada : DomainEvent
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
        /// Motivo de la anulación
        /// </summary>
        public string Motivo { get; }

        /// <summary>
        /// Fecha de anulación
        /// </summary>
        public DateTime FechaAnulacion { get; }

        /// <summary>
        /// Constructor para el evento FacturaAnulada
        /// </summary>
        /// <param name="facturaId">Identificador de la factura</param>
        /// <param name="numeroFactura">Número de la factura</param>
        /// <param name="motivo">Motivo de la anulación</param>
        /// <param name="fechaAnulacion">Fecha de anulación</param>
        public FacturaAnulada(Guid facturaId, string numeroFactura, string motivo, DateTime fechaAnulacion)
        {
            FacturaId = facturaId;
            NumeroFactura = numeroFactura;
            Motivo = motivo;
            FechaAnulacion = fechaAnulacion;
        }
    }
} 