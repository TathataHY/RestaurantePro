using MediatR;

namespace RestaurantePro.Application.Features.Reservaciones.Commands.MarcarNoShow
{
    public class MarcarNoShowCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string Observaciones { get; set; }
    }
} 