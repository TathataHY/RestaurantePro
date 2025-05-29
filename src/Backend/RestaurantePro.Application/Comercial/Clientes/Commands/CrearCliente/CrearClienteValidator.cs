namespace RestaurantePro.Application.Comercial.Clientes.Commands.CrearCliente;

/// <summary>
/// Validador para CrearClienteCommand usando FluentValidation
/// </summary>
public class CrearClienteValidator : AbstractValidator<CrearClienteCommand>
{
    public CrearClienteValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del cliente es obligatorio")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio")
            .EmailAddress().WithMessage("El formato del email no es válido")
            .MaximumLength(320).WithMessage("El email no puede exceder 320 caracteres");

        RuleFor(x => x.Telefono)
            .NotEmpty().WithMessage("El teléfono es obligatorio")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("El formato del teléfono no es válido");

        RuleFor(x => x.FechaNacimiento)
            .NotEmpty().WithMessage("La fecha de nacimiento es obligatoria")
            .LessThan(DateTime.Now).WithMessage("La fecha de nacimiento debe ser anterior a hoy")
            .GreaterThan(DateTime.Now.AddYears(-120)).WithMessage("La fecha de nacimiento no puede ser hace más de 120 años")
            .Must(BeAtLeast18YearsOld).WithMessage("El cliente debe ser mayor de 18 años");
    }

    private static bool BeAtLeast18YearsOld(DateTime fechaNacimiento)
    {
        return fechaNacimiento <= DateTime.Now.AddYears(-18);
    }
} 