using RestaurantePro.Core.Entities.Base;

namespace RestaurantePro.Core.Entities
{
    public class ComandaDetalle : BaseEntity
    {
        public int ComandaId { get; set; }
        public int PlatoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string Observaciones { get; set; }

        public virtual Comanda Comanda { get; set; }
        public virtual Plato Plato { get; set; }
    }
} 