using FluentValidation;
using RestaurantePro.Domain.Interfaces.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Usuarios.Commands.RegistrarUsuario
{
    public class RegistrarUsuarioCommandValidator : AbstractValidator<RegistrarUsuarioCommand>
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public RegistrarUsuarioCommandValidator(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;

            RuleFor(v => v.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(50).WithMessage("El nombre no puede exceder los 50 caracteres");

            RuleFor(v => v.Apellido)
                .NotEmpty().WithMessage("El apellido es requerido")
                .MaximumLength(50).WithMessage("El apellido no puede exceder los 50 caracteres");

            RuleFor(v => v.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El email no tiene un formato válido")
                .MustAsync(EmailUnico).WithMessage("El email ya está registrado");

            RuleFor(v => v.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres")
                .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula")
                .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula")
                .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número");

            RuleFor(v => v.ConfirmPassword)
                .Equal(v => v.Password).WithMessage("Las contraseñas no coinciden");

            RuleFor(v => v.Rol)
                .NotEmpty().WithMessage("El rol es requerido")
                .Must(RolValido).WithMessage("El rol no es válido");
        }

        private async Task<bool> EmailUnico(string email, CancellationToken cancellationToken)
        {
            return !await _usuarioRepository.EmailExistsAsync(email);
        }

        private bool RolValido(string rol)
        {
            string[] rolesValidos = { "Administrador", "Gerente", "Mesero", "Cocinero" };
            return string.IsNullOrEmpty(rol) || System.Array.Exists(rolesValidos, r => r == rol);
        }
    }
} 