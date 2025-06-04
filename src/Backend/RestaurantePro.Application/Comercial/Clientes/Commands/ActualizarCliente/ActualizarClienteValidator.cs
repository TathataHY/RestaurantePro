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
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID del cliente es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del cliente no puede ser un GUID vacío");

        // Al menos un campo debe ser actualizado
        RuleFor(x => x)
            .Must(HaveAtLeastOneFieldToUpdate)
            .WithMessage("Debe proporcionar al menos un campo para actualizar");

        // Nombre - validar solo si se proporciona (no es null)
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre no puede estar vacío si se proporciona")
            .When(x => x.Nombre != null);
            
        RuleFor(x => x.Nombre)
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
            .When(x => x.Nombre != null);
            
        RuleFor(x => x.Nombre)
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres")
            .When(x => x.Nombre != null);

        // Email - validar solo si se proporciona (no es null)
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email no puede estar vacío si se proporciona")
            .EmailAddress().WithMessage("El formato del email no es válido")
            .MaximumLength(320).WithMessage("El email no puede exceder 320 caracteres")
            .When(x => x.Email != null);

        // Teléfono - validar solo si se proporciona (no es null)
        RuleFor(x => x.Telefono)
            .NotEmpty().WithMessage("El teléfono no puede estar vacío si se proporciona")
            .Matches(@"^(\+?[0-9]{1,3}[\s\-\(\)]?)?[\d\s\-\(\)]{7,14}$").WithMessage("El formato del teléfono no es válido")
            .When(x => x.Telefono != null);

        // Fecha de nacimiento - validar solo si se proporciona
        RuleFor(x => x.FechaNacimiento)
            .LessThanOrEqualTo(DateTime.Today.AddYears(-18)).WithMessage("El cliente debe ser mayor de 18 años")
            .GreaterThan(DateTime.Today.AddYears(-120)).WithMessage("La fecha de nacimiento no puede ser mayor a 120 años")
            .When(x => x.FechaNacimiento.HasValue && x.FechaNacimiento > DateTime.MinValue);
    }

    /// <summary>
    /// Valida que al menos un campo esté presente para actualizar
    /// </summary>
    private bool HaveAtLeastOneFieldToUpdate(ActualizarClienteCommand command)
    {
        return !string.IsNullOrWhiteSpace(command.Nombre) ||
               !string.IsNullOrWhiteSpace(command.Email) ||
               !string.IsNullOrWhiteSpace(command.Telefono) ||
               command.FechaNacimiento.HasValue ||
               command.EstaActivo.HasValue;
    }
} 