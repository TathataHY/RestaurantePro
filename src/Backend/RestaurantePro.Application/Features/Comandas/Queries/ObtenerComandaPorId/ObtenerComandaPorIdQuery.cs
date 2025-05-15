using MediatR;
using RestaurantePro.Application.Features.Comandas.Dtos;

namespace RestaurantePro.Application.Features.Comandas.Queries.ObtenerComandaPorId
{
    public class ObtenerComandaPorIdQuery : IRequest<ComandaDto>
    {
        public int Id { get; set; }
    }
} 