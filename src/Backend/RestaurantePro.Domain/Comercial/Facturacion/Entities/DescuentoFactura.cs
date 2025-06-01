namespace RestaurantePro.Domain.Comercial.Facturacion.Entities
{
    /// <summary>
    /// Representa un descuento aplicado a una factura
    /// </summary>
    public class DescuentoFactura : EntityBase
    {
        /// <summary>
        /// Identificador de la factura a la que se aplica el descuento
        /// </summary>
        public Guid FacturaId { get; set; }

        /// <summary>
        /// Identificador de la promoción asociada (opcional)
        /// </summary>
        public Guid? PromocionId { get; set; }

        /// <summary>
        /// Tipo de descuento aplicado
        /// </summary>
        public string TipoDescuento { get; set; } = string.Empty;

        /// <summary>
        /// Monto del descuento aplicado
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Porcentaje del descuento (si aplica)
        /// </summary>
        public decimal Porcentaje { get; set; }

        /// <summary>
        /// Motivo del descuento
        /// </summary>
        public string Motivo { get; set; } = string.Empty;

        /// <summary>
        /// Fecha en que se aplicó el descuento
        /// </summary>
        public DateTime FechaAplicacion { get; set; }

        /// <summary>
        /// Usuario que autorizó el descuento
        /// </summary>
        public Guid UsuarioAutorizaId { get; set; }

        /// <summary>
        /// Código de autorización (opcional)
        /// </summary>
        public string? CodigoAutorizacion { get; set; }

        /// <summary>
        /// Concepto del descuento
        /// </summary>
        public string Concepto { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el descuento se aplicó antes de impuestos
        /// </summary>
        public bool AplicadoAntesDeImpuestos { get; set; }

        /// <summary>
        /// Constructor por defecto para EF Core
        /// </summary>
        public DescuentoFactura()
        {
        }

        /// <summary>
        /// Constructor para crear un nuevo descuento
        /// </summary>
        public DescuentoFactura(
            Guid facturaId,
            string tipoDescuento,
            decimal monto,
            string motivo,
            Guid usuarioAutorizaId,
            string concepto)
        {
            Id = Guid.NewGuid();
            FacturaId = facturaId;
            TipoDescuento = tipoDescuento;
            Monto = monto;
            Motivo = motivo;
            UsuarioAutorizaId = usuarioAutorizaId;
            Concepto = concepto;
            FechaAplicacion = DateTime.UtcNow;
        }

        /// <summary>
        /// Factory method para crear un descuento
        /// </summary>
        public static DescuentoFactura Crear(
            Guid facturaId,
            string tipoDescuento,
            decimal monto,
            string motivo,
            Guid usuarioAutorizaId,
            string concepto)
        {
            return new DescuentoFactura(facturaId, tipoDescuento, monto, motivo, usuarioAutorizaId, concepto);
        }
    }
} 