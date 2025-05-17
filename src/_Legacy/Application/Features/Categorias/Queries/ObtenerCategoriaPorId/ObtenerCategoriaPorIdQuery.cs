using MediatR;
using RestaurantePro.Application.Features.Categorias.Dtos;

namespace RestaurantePro.Application.Features.Categorias.Queries.ObtenerCategoriaPorId
{
    public class ObtenerCategoriaPorIdQuery : IRequest<CategoriaDto>
    {
        public int Id { get; set; }
    }
} 