using MediatR;
using RestaurantePro.Domain.Enums;

namespace RestaurantePro.Application.Features.Mesas.Commands.ActualizarMesa
{
    public class ActualizarMesaCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public int Capacidad { get; set; }
        public string Ubicacion { get; set; }
        public bool Activa { get; set; }
        public EstadoMesa Estado { get; set; }
    }
} 