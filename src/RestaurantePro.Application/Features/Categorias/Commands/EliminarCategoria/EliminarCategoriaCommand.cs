using MediatR;

namespace RestaurantePro.Application.Features.Categorias.Commands.EliminarCategoria
{
    public class EliminarCategoriaCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
} 