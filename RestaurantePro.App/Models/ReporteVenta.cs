namespace RestaurantePro.App.Models
{
    public class ReporteVenta
    {
        public DateTime Fecha { get; set; }
        public decimal TotalVentas { get; set; }
        public int TotalPlatosVendidos { get; set; }
    }
}