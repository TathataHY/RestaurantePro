using MediatR;

namespace RestaurantePro.Application.Features.Reservaciones.Commands.CompletarReservacion
{
    public class CompletarReservacionCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string Comentarios { get; set; }
    }
} 