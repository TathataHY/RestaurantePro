using MediatR;
using RestaurantePro.Application.Features.Productos.Dtos;

namespace RestaurantePro.Application.Features.Productos.Queries.ObtenerProductoPorId
{
    public class ObtenerProductoPorIdQuery : IRequest<ProductoDto>
    {
        public int Id { get; set; }
    }
} 