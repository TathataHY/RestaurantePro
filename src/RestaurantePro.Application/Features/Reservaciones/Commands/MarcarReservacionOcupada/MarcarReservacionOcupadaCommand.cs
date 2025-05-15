using MediatR;

namespace RestaurantePro.Application.Features.Reservaciones.Commands.MarcarReservacionOcupada
{
    public class MarcarReservacionOcupadaCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public int? NumeroPersonasReal { get; set; }
        public string Notas { get; set; }
    }
} 