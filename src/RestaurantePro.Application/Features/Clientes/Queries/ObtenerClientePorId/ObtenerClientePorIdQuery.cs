using MediatR;
using RestaurantePro.Application.Features.Clientes.Dtos;

namespace RestaurantePro.Application.Features.Clientes.Queries.ObtenerClientePorId
{
    public class ObtenerClientePorIdQuery : IRequest<ClienteDto>
    {
        public int Id { get; set; }
    }
} 