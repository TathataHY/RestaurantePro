using MediatR;
using RestaurantePro.Application.Features.Usuarios.Dtos;

namespace RestaurantePro.Application.Features.Usuarios.Commands.LoginUsuario
{
    public class LoginUsuarioCommand : IRequest<UsuarioAutenticadoDto>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
} 