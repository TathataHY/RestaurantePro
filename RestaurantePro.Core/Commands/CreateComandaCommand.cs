using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.Specifications;
using MediatR;
using RestaurantePro.Core.Common;

namespace RestaurantePro.Core.Commands
{
    public class CreateComandaCommand : IRequest<Result<ComandaDto>>
    {
        public DateTime FechaHora { get; set; }
        public int MesaId { get; set; }
        public string MeseroId { get; set; }
        public string Observaciones { get; set; }
        public EstadoComanda Estado { get; set; }
        public List<ComandaDetalleDto> Detalles { get; set; }

        public Comanda ToEntity()
        {
            return new Comanda
            {
                FechaHora = DateTime.UtcNow,
                MesaId = MesaId,
                MeseroId = MeseroId,
                Observaciones = Observaciones,
                Estado = EstadoComanda.Pendiente,
                Detalles = Detalles?.Select(d => new ComandaDetalle
                {
                    PlatoId = d.PlatoId,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                }).ToList() ?? new List<ComandaDetalle>()
            };
        }
    }
}