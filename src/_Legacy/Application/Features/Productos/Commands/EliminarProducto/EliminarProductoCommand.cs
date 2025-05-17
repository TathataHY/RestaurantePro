using MediatR;

namespace RestaurantePro.Application.Features.Productos.Commands.EliminarProducto
{
    public class EliminarProductoCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
} 