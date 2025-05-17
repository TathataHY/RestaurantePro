using MediatR;

namespace RestaurantePro.Application.Features.Pagos.Commands.CancelarPago
{
    public class CancelarPagoCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string MotivoCancelacion { get; set; }
    }
} 