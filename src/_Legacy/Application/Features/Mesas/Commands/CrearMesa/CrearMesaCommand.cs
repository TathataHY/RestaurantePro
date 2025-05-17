using MediatR;
using RestaurantePro.Domain.Enums;

namespace RestaurantePro.Application.Features.Mesas.Commands.CrearMesa
{
    public class CrearMesaCommand : IRequest<int>
    {
        public int Numero { get; set; }
        public int Capacidad { get; set; }
        public string Ubicacion { get; set; }
        public bool Activa { get; set; } = true;
        public EstadoMesa Estado { get; set; } = EstadoMesa.Libre;
    }
} 