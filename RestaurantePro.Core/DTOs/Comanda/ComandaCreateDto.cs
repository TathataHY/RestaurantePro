namespace RestaurantePro.Core.DTOs.Comanda
{
    public class ComandaCreateDto
    {
        public int MesaId { get; set; }
        public string MeseroId { get; set; }
        public string Observaciones { get; set; }
    }
}