using SQLite;

namespace RestaurantePro.App.Models
{
    public enum EstadoMesa
    {
        Disponible,
        Ocupada,
        Reservada
    }

    public class Mesa
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Numero { get; set; }
        public EstadoMesa Estado { get; set; } // Disponible, Ocupada, Reservada
    }
}