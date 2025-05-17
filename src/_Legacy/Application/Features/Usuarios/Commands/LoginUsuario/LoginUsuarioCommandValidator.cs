using FluentValidation;

namespace RestaurantePro.Application.Features.Usuarios.Commands.LoginUsuario
{
    public class LoginUsuarioCommandValidator : AbstractValidator<LoginUsuarioCommand>
    {
        public LoginUsuarioCommandValidator()
        {
            RuleFor(v => v.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El email no tiene un formato válido");

            RuleFor(v => v.Password)
                .NotEmpty().WithMessage("La contraseña es requerida");
        }
    }
} 