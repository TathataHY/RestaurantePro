using MediatR;

namespace RestaurantePro.Application.Features.Reservaciones.Commands.CancelarReservacion
{
    public class CancelarReservacionCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string MotivoCancelacion { get; set; }
    }
} 