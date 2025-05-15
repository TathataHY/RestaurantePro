using RestaurantePro.Domain.Common;

namespace RestaurantePro.Domain.Entities
{
    public class ProveedorCategoria : BaseEntity
    {
        public int ProveedorId { get; set; }
        public virtual Proveedor Proveedor { get; set; }
        public int CategoriaId { get; set; }
        public virtual Categoria Categoria { get; set; }
        public bool EsProveedorPrincipal { get; set; }
        public decimal PorcentajeDescuento { get; set; }
        public string Observaciones { get; set; }
    }
} 