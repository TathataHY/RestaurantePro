namespace RestaurantePro.Application.Inventario.OrdenesCompra.Commands.AprobarOrdenCompra;

/// <summary>
/// Validador para AprobarOrdenCompraCommand
/// </summary>
public class AprobarOrdenCompraValidator : AbstractValidator<AprobarOrdenCompraCommand>
{
    public AprobarOrdenCompraValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la orden de compra es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID de la orden de compra no puede ser un GUID vacío");

        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("El ID del usuario es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del usuario no puede ser un GUID vacío");

        RuleFor(x => x.Observaciones)
            .MaximumLength(300).WithMessage("Las observaciones de aprobación no pueden exceder 300 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));
    }
} 