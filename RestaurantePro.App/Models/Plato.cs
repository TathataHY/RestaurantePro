using SQLite;

namespace RestaurantePro.App.Models
{
    public enum CategoriaPlato
    {
        Entrada,
        Principal,
        Postre,
        Bebida
    }

    public class Plato
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public bool Disponible { get; set; }
        public CategoriaPlato Categoria { get; set; } // Nueva propiedad para la categoría
        public int Stock { get; set; } // Nueva propiedad para el stock disponible
    }
}