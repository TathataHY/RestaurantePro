namespace RestaurantePro.Application.Comercial.Clientes.Commands.DesactivarCliente;

/// <summary>
/// Validador para DesactivarClienteCommand
/// </summary>
public class DesactivarClienteValidator : AbstractValidator<DesactivarClienteCommand>
{
    public DesactivarClienteValidator()
    {
        RuleFor(x => x.ClienteId)
            .NotEmpty().WithMessage("El ID del cliente es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del cliente no puede ser un GUID vacío");

        RuleFor(x => x.MotivoDesactivacion)
            .MaximumLength(500).WithMessage("El motivo de desactivación no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.MotivoDesactivacion));
    }
} 