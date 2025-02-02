namespace RestaurantePro.Core.DTOs.Comanda
{
    public class ComandaDetalleDto
    {
        public int Id { get; set; }
        public int PlatoId { get; set; }
        public string PlatoNombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }
        public string Observaciones { get; set; }
    }
}