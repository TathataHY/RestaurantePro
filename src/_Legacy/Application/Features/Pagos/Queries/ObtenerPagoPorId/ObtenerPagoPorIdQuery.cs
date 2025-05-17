using MediatR;
using RestaurantePro.Application.Features.Pagos.Dtos;

namespace RestaurantePro.Application.Features.Pagos.Queries.ObtenerPagoPorId
{
    public class ObtenerPagoPorIdQuery : IRequest<PagoDto>
    {
        public int Id { get; set; }
    }
} 