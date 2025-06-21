using FluentValidation;

namespace RestaurantePro.Application.Core.Usuarios.Commands.ResetPasswordUsuario;

/// <summary>
/// Validador para el comando ResetPasswordUsuarioCommand
/// </summary>
public class ResetPasswordUsuarioValidator : AbstractValidator<ResetPasswordUsuarioCommand>
{
    public ResetPasswordUsuarioValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido");

        RuleFor(x => x.UsuarioReseteadorId)
            .NotEmpty()
            .WithMessage("El ID del usuario que realiza el reset es requerido");

        RuleFor(x => x.NuevaPasswordTemporal)
            .MinimumLength(8)
            .WithMessage("La contraseña temporal debe tener al menos 8 caracteres")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            .WithMessage("La contraseña temporal debe contener al menos una letra minúscula, una mayúscula, un número y un carácter especial")
            .When(x => !string.IsNullOrWhiteSpace(x.NuevaPasswordTemporal));

        RuleFor(x => x.Motivo)
            .MaximumLength(500)
            .WithMessage("El motivo no puede exceder los 500 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Motivo));
    }
} 