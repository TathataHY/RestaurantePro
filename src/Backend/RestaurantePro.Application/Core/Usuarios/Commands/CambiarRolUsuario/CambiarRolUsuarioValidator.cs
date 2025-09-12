using FluentValidation;
using RestaurantePro.Domain.Core.Usuarios.Enums;

namespace RestaurantePro.Application.Core.Usuarios.Commands.CambiarRolUsuario;

/// <summary>
/// Validador para el comando CambiarRolUsuarioCommand
/// </summary>
public class CambiarRolUsuarioValidator : AbstractValidator<CambiarRolUsuarioCommand>
{
    private readonly string[] _rolesValidos = { "Administrador", "Gerente", "Cajero", "Mesero", "Cocinero", "EncargadoInventario" };

    public CambiarRolUsuarioValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido");

        RuleFor(x => x.NuevoRol)
            .NotEmpty()
            .WithMessage("El nuevo rol es requerido")
            .Must(BeRolValido)
            .WithMessage("El rol '{PropertyValue}' no es válido. Roles válidos: {RolesValidos}");

        RuleFor(x => x.UsuarioCambiadorId)
            .NotEmpty()
            .WithMessage("El ID del usuario que realiza el cambio es requerido")
            .NotEqual(x => x.Id)
            .WithMessage("Un usuario no puede cambiar su propio rol");

        RuleFor(x => x.Motivo)
            .MaximumLength(500)
            .WithMessage("El motivo no puede exceder los 500 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Motivo));
    }

    private bool BeRolValido(string rol)
    {
        return !string.IsNullOrWhiteSpace(rol) && _rolesValidos.Contains(rol);
    }

    private string RolesValidos => string.Join(", ", _rolesValidos);
} 