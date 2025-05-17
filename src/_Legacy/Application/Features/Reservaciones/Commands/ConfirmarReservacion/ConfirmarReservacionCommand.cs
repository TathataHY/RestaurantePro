using MediatR;

namespace RestaurantePro.Application.Features.Reservaciones.Commands.ConfirmarReservacion
{
    public class ConfirmarReservacionCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string NotasConfirmacion { get; set; }
    }
} 