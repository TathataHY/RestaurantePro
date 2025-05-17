using MediatR;
using RestaurantePro.Domain.Enums;

namespace RestaurantePro.Application.Features.Pagos.Commands.RegistrarPago
{
    public class RegistrarPagoCommand : IRequest<int>
    {
        public int ComandaId { get; set; }
        public decimal Monto { get; set; }
        public MetodoPago MetodoPago { get; set; }
        public string Referencia { get; set; }
        public string Notas { get; set; }
    }
} 