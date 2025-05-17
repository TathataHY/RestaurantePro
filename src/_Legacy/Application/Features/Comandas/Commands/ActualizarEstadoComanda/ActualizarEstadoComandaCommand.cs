using MediatR;
using RestaurantePro.Domain.Enums;

namespace RestaurantePro.Application.Features.Comandas.Commands.ActualizarEstadoComanda
{
    public class ActualizarEstadoComandaCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public EstadoComanda NuevoEstado { get; set; }
    }
} 