using MediatR;
using RestaurantePro.Application.Features.Reservaciones.Dtos;

namespace RestaurantePro.Application.Features.Reservaciones.Queries.ObtenerReservacionPorId
{
    public class ObtenerReservacionPorIdQuery : IRequest<ReservacionDto>
    {
        public int Id { get; set; }
    }
} 