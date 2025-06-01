namespace RestaurantePro.Domain.Comercial.Facturacion.Events
{
    /// <summary>
    /// Evento de dominio que se emite cuando se aplica un descuento a una factura
    /// </summary>
    public class DescuentoAplicado : DomainEvent
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
        /// Tipo de descuento aplicado
        /// </summary>
        public string TipoDescuento { get; }

        /// <summary>
        /// Monto del descuento aplicado
        /// </summary>
        public decimal MontoDescuento { get; }

        /// <summary>
        /// Concepto del descuento
        /// </summary>
        public string Concepto { get; }

        /// <summary>
        /// Motivo del descuento
        /// </summary>
        public string Motivo { get; }

        /// <summary>
        /// Usuario que autoriza el descuento
        /// </summary>
        public Guid UsuarioAutorizaId { get; }

        /// <summary>
        /// Código de autorización (opcional)
        /// </summary>
        public string? CodigoAutorizacion { get; }

        /// <summary>
        /// Fecha de aplicación del descuento
        /// </summary>
        public DateTime FechaAplicacion { get; }

        /// <summary>
        /// Constructor para el evento DescuentoAplicado
        /// </summary>
        public DescuentoAplicado(
            Guid facturaId,
            string numeroFactura,
            string tipoDescuento,
            decimal montoDescuento,
            string concepto,
            string motivo,
            Guid usuarioAutorizaId,
            string? codigoAutorizacion,
            DateTime fechaAplicacion)
        {
            FacturaId = facturaId;
            NumeroFactura = numeroFactura;
            TipoDescuento = tipoDescuento;
            MontoDescuento = montoDescuento;
            Concepto = concepto;
            Motivo = motivo;
            UsuarioAutorizaId = usuarioAutorizaId;
            CodigoAutorizacion = codigoAutorizacion;
            FechaAplicacion = fechaAplicacion;
        }
    }
} 