using MediatR;
using System;

namespace RestaurantePro.Application.Features.Clientes.Commands.RegistrarCliente
{
    public class RegistrarClienteCommand : IRequest<int>
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Observaciones { get; set; }
        public bool CrearTarjeta { get; set; } = true;
    }
} 