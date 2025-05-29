namespace RestaurantePro.Application.Comercial.Clientes.Commands.ActualizarCliente;

/// <summary>
/// Validador para ActualizarClienteCommand
/// Implementa validaciones condicionales para campos opcionales
/// </summary>
public class ActualizarClienteValidator : AbstractValidator<ActualizarClienteCommand>
{
    public ActualizarClienteValidator()
    {
        // ID siempre requerido
        RuleFor(x => x.ClienteId)
            .NotEmpty().WithMessage("El ID del cliente es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del cliente no puede ser un GUID vacío");

        // Al menos un campo debe ser actualizado
        RuleFor(x => x)
            .Must(HaveAtLeastOneFieldToUpdate)
            .WithMessage("Debe proporcionar al menos un campo para actualizar");

        // Nombre - validar solo si se proporciona
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre no puede estar vacío si se proporciona")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Nombre));

        // Email - validar solo si se proporciona
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email no puede estar vacío si se proporciona")
            .EmailAddress().WithMessage("El formato del email no es válido")
            .MaximumLength(320).WithMessage("El email no puede exceder 320 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        // Teléfono - validar solo si se proporciona
        RuleFor(x => x.Telefono)
            .NotEmpty().WithMessage("El teléfono no puede estar vacío si se proporciona")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("El formato del teléfono no es válido")
            .When(x => !string.IsNullOrWhiteSpace(x.Telefono));

        // Fecha de nacimiento - validar solo si se proporciona
        RuleFor(x => x.FechaNacimiento)
            .LessThan(DateTime.Now).WithMessage("La fecha de nacimiento debe ser anterior a hoy")
            .GreaterThan(DateTime.Now.AddYears(-120)).WithMessage("La fecha de nacimiento no puede ser hace más de 120 años")
            .Must(BeAtLeast18YearsOld).WithMessage("El cliente debe ser mayor de 18 años")
            .When(x => x.FechaNacimiento.HasValue);
    }

    /// <summary>
    /// Verifica que al menos un campo esté presente para actualizar
    /// </summary>
    private static bool HaveAtLeastOneFieldToUpdate(ActualizarClienteCommand command)
    {
        return !string.IsNullOrWhiteSpace(command.Nombre) ||
               !string.IsNullOrWhiteSpace(command.Email) ||
               !string.IsNullOrWhiteSpace(command.Telefono) ||
               command.FechaNacimiento.HasValue;
    }

    /// <summary>
    /// Verifica que el cliente sea mayor de 18 años
    /// </summary>
    private static bool BeAtLeast18YearsOld(DateTime? fechaNacimiento)
    {
        if (!fechaNacimiento.HasValue) return true;
        return fechaNacimiento.Value <= DateTime.Now.AddYears(-18);
    }
} 