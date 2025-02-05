namespace RestaurantePro.Core.DTOs.Estadisticas
{
    public class VentasDiariasDto
    {
        public int Hora { get; set; }
        public decimal TotalVentas { get; set; }
        public int CantidadComandas { get; set; }
    }
}