using MediatR;
using RestaurantePro.Application.Features.Clientes.Dtos;

namespace RestaurantePro.Application.Features.Clientes.Commands.ObtenerTarjetaCliente
{
    public class ObtenerTarjetaClienteQuery : IRequest<TarjetaFidelizacionDto>
    {
        public int ClienteId { get; set; }
    }
} 