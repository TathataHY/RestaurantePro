using RestaurantePro.Core.Enums;

namespace RestaurantePro.Core.DTOs.Comanda
{
    public class ComandaDto
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public int MesaId { get; set; }
        public string MesaNumero { get; set; }
        public EstadoComanda Estado { get; set; }
        public decimal Total { get; set; }

        public List<ComandaDetalleDto> Detalles { get; set; }
    }
}