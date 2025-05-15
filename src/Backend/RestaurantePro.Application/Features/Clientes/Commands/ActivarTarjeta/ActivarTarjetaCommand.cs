using MediatR;

namespace RestaurantePro.Application.Features.Clientes.Commands.ActivarTarjeta
{
    public class ActivarTarjetaCommand : IRequest<bool>
    {
        public int ClienteId { get; set; }
    }
} 