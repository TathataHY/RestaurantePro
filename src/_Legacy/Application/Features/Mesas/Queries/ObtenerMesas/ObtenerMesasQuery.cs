using MediatR;
using RestaurantePro.Application.Features.Mesas.Dtos;
using RestaurantePro.Domain.Enums;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Mesas.Queries.ObtenerMesas
{
    public class ObtenerMesasQuery : IRequest<List<MesaDto>>
    {
        public EstadoMesa? Estado { get; set; }
        public int? CapacidadMinima { get; set; }
        public bool? SoloActivas { get; set; }
    }
} 