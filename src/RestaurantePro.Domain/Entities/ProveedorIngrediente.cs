using RestaurantePro.Domain.Common;

namespace RestaurantePro.Domain.Entities
{
    public class ProveedorIngrediente : BaseEntity
    {
        public int ProveedorId { get; set; }
        public virtual Proveedor Proveedor { get; set; }
        public int IngredienteId { get; set; }
        public virtual Ingrediente Ingrediente { get; set; }
        public string CodigoProveedor { get; set; }
        public bool EsProveedorPrincipal { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string UnidadCompra { get; set; }
        public decimal FactorConversion { get; set; }
        public decimal CantidadMinima { get; set; }
        public string Observaciones { get; set; }
    }
} 