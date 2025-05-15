using RestaurantePro.Domain.Common;

namespace RestaurantePro.Domain.Entities
{
    public class DetalleOrdenCompra : BaseEntity
    {
        public int OrdenCompraId { get; set; }
        public virtual OrdenCompra OrdenCompra { get; set; }
        public int IngredienteId { get; set; }
        public virtual Ingrediente Ingrediente { get; set; }
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public decimal PorcentajeImpuesto { get; set; }
        public decimal Impuesto { get; set; }
        public decimal PorcentajeDescuento { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public decimal CantidadRecibida { get; set; }
        public string Observaciones { get; set; }
    }
} 