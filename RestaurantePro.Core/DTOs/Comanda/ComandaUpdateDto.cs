using RestaurantePro.Core.Enums;

namespace RestaurantePro.Core.DTOs.Comanda
{
    public class ComandaUpdateDto
    {
        public EstadoComanda Estado { get; set; }
        public string Observaciones { get; set; }
        public int? MesaId { get; set; }
    }
}