namespace RestaurantePro.Core.DTOs.Comanda
{
    public class ComandaDetalleCreateDto
    {
        public int PlatoId { get; set; }
        public int Cantidad { get; set; }
        public string Observaciones { get; set; }
    }
}