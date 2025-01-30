using SQLite;

namespace RestaurantePro.App.Models
{
    public class ComandaDetalle
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ComandaId { get; set; }
        public int PlatoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string Observaciones { get; set; }

        [Ignore]
        public string PlatoNombre { get; set; } // Nueva propiedad para el nombre del plato
    }
}