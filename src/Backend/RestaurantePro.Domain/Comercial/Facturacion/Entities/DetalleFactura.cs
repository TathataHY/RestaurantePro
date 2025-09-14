using RestaurantePro.Domain.Core.Productos.Entities;

namespace RestaurantePro.Domain.Comercial.Facturacion.Entities
{
    /// <summary>
    /// Representa un detalle o línea de una factura
    /// </summary>
    public class DetalleFactura : EntityBase
    {
        /// <summary>
        /// Identificador de la factura a la que pertenece este detalle
        /// </summary>
        public Guid FacturaId { get; private set; }

        /// <summary>
        /// Identificador del producto o servicio
        /// </summary>
        public Guid ProductoId { get; private set; }

        /// <summary>
        /// Navegación al producto
        /// </summary>
        public Producto? Producto { get; private set; }

        /// <summary>
        /// Descripción del producto o servicio
        /// </summary>
        public string Descripcion { get; private set; }

        /// <summary>
        /// Cantidad del producto o servicio
        /// </summary>
        public decimal Cantidad { get; private set; }

        /// <summary>
        /// Precio unitario sin impuestos
        /// </summary>
        public decimal PrecioUnitario { get; private set; }

        /// <summary>
        /// Subtotal (Cantidad * PrecioUnitario)
        /// </summary>
        public decimal Subtotal { get; private set; }

        /// <summary>
        /// Porcentaje de impuesto aplicado
        /// </summary>
        public decimal PorcentajeImpuesto { get; private set; }

        /// <summary>
        /// Importe del impuesto calculado
        /// </summary>
        public decimal ImporteImpuesto { get; private set; }

        /// <summary>
        /// Porcentaje de descuento aplicado
        /// </summary>
        public decimal PorcentajeDescuento { get; private set; }

        /// <summary>
        /// Importe del descuento calculado
        /// </summary>
        public decimal ImporteDescuento { get; private set; }

        /// <summary>
        /// Total de la línea (Subtotal + ImporteImpuesto - ImporteDescuento)
        /// </summary>
        public decimal Total { get; private set; }

        // Constructor privado para EF Core
        private DetalleFactura() { }

        /// <summary>
        /// Constructor para crear un nuevo detalle de factura
        /// </summary>
        private DetalleFactura(
            Guid facturaId,
            Guid productoId,
            string descripcion,
            decimal cantidad,
            decimal precioUnitario,
            decimal porcentajeImpuesto,
            decimal porcentajeDescuento)
        {
            if (facturaId == Guid.Empty)
            {
                throw new ArgumentException("El ID de factura no puede estar vacío", nameof(facturaId));
            }

            if (productoId == Guid.Empty)
            {
                throw new ArgumentException("El ID de producto no puede estar vacío", nameof(productoId));
            }

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                throw new ArgumentException("La descripción no puede estar vacía", nameof(descripcion));
            }

            if (cantidad <= 0)
            {
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));
            }

            if (precioUnitario < 0)
            {
                throw new ArgumentException("El precio unitario no puede ser negativo", nameof(precioUnitario));
            }

            if (porcentajeImpuesto < 0)
            {
                throw new ArgumentException("El porcentaje de impuesto no puede ser negativo", nameof(porcentajeImpuesto));
            }

            if (porcentajeDescuento < 0 || porcentajeDescuento > 100)
            {
                throw new ArgumentException("El porcentaje de descuento debe estar entre 0 y 100", nameof(porcentajeDescuento));
            }

            Id = Guid.NewGuid();
            FacturaId = facturaId;
            ProductoId = productoId;
            Descripcion = descripcion;
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
            PorcentajeImpuesto = porcentajeImpuesto;
            PorcentajeDescuento = porcentajeDescuento;

            // Calcular importes
            CalcularImportes();
        }

        /// <summary>
        /// Factory method para crear un nuevo detalle de factura
        /// </summary>
        public static DetalleFactura Crear(
            Guid facturaId,
            Guid productoId,
            string descripcion,
            decimal cantidad,
            decimal precioUnitario,
            decimal porcentajeImpuesto,
            decimal porcentajeDescuento = 0)
        {
            return new DetalleFactura(
                facturaId,
                productoId,
                descripcion,
                cantidad,
                precioUnitario,
                porcentajeImpuesto,
                porcentajeDescuento);
        }

        /// <summary>
        /// Calcula los importes de subtotal, impuesto, descuento y total
        /// </summary>
        private void CalcularImportes()
        {
            // Calcular subtotal (cantidad * precio unitario)
            Subtotal = Cantidad * PrecioUnitario;

            // Calcular importe de impuesto
            ImporteImpuesto = Subtotal * (PorcentajeImpuesto / 100);

            // Calcular importe de descuento
            ImporteDescuento = Subtotal * (PorcentajeDescuento / 100);

            // Calcular total
            Total = Subtotal + ImporteImpuesto - ImporteDescuento;

            // Redondear importes a 2 decimales
            Subtotal = Math.Round(Subtotal, 2);
            ImporteImpuesto = Math.Round(ImporteImpuesto, 2);
            ImporteDescuento = Math.Round(ImporteDescuento, 2);
            Total = Math.Round(Total, 2);
        }
    }
} 