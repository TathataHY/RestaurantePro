using FluentValidation;

namespace RestaurantePro.Application.Core.Usuarios.Commands.EliminarUsuario;

/// <summary>
/// Validador para el comando EliminarUsuarioCommand
/// </summary>
public class EliminarUsuarioValidator : AbstractValidator<EliminarUsuarioCommand>
{
    public EliminarUsuarioValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido");

        RuleFor(x => x.UsuarioEliminadorId)
            .NotEmpty()
            .WithMessage("El ID del usuario que realiza la eliminación es requerido")
            .NotEqual(x => x.Id)
            .WithMessage("Un usuario no puede eliminarse a sí mismo");

        RuleFor(x => x.Motivo)
            .MaximumLength(500)
            .WithMessage("El motivo no puede exceder los 500 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Motivo));
    }
} 