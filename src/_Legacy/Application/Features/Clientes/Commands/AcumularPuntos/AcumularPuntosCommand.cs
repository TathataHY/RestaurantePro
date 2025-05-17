using MediatR;

namespace RestaurantePro.Application.Features.Clientes.Commands.AcumularPuntos
{
    public class AcumularPuntosCommand : IRequest<bool>
    {
        public int ClienteId { get; set; }
        public int Puntos { get; set; }
        public string Descripcion { get; set; }
        public int? ComandaId { get; set; }
    }
} 