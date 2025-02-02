using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.Specifications;
using MediatR;

namespace RestaurantePro.Core.Commands
{
    public class CreateComandaCommand : IRequest<ComandaDto>
    {
        public DateTime FechaHora { get; set; }
        public int MesaId { get; set; }
        public string MeseroId { get; set; }
        public string Observaciones { get; set; }
        public EstadoComanda Estado { get; set; }
        public List<ComandaDetalleDto> Detalles { get; set; }
    }
}