using MediatR;

namespace RestaurantePro.Application.Features.Productos.Commands.CambiarDisponibilidadProducto
{
    public class CambiarDisponibilidadProductoCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public bool Disponible { get; set; }
    }
} 