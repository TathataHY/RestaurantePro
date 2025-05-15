using MediatR;
using RestaurantePro.Application.Features.Mesas.Dtos;

namespace RestaurantePro.Application.Features.Mesas.Queries.ObtenerMesaPorId
{
    public class ObtenerMesaPorIdQuery : IRequest<MesaDto>
    {
        public int Id { get; set; }
    }
} 