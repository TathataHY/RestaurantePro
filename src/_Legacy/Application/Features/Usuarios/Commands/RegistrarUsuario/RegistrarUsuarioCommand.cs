using MediatR;
using RestaurantePro.Application.DTOs;

namespace RestaurantePro.Application.Features.Usuarios.Commands.RegistrarUsuario
{
    public class RegistrarUsuarioCommand : IRequest<int>
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Rol { get; set; }

        public RegistrarUsuarioCommand()
        {
        }

        public RegistrarUsuarioCommand(string nombre, string apellido, string email, string password, string confirmPassword, string rol = "Mesero")
        {
            Nombre = nombre;
            Apellido = apellido;
            Email = email;
            Password = password;
            ConfirmPassword = confirmPassword;
            Rol = rol;
        }
    }
} 