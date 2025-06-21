namespace RestaurantePro.Application.Inventario.OrdenesCompra.Commands.RecibirOrdenCompra;

/// <summary>
/// Validador para RecibirOrdenCompraCommand
/// </summary>
public class RecibirOrdenCompraValidator : AbstractValidator<RecibirOrdenCompraCommand>
{
    public RecibirOrdenCompraValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la orden de compra es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID de la orden de compra no puede ser un GUID vacío");

        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("El ID del usuario es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del usuario no puede ser un GUID vacío");

        RuleFor(x => x.NotasRecepcion)
            .MaximumLength(300).WithMessage("Las notas de recepción no pueden exceder 300 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.NotasRecepcion));
    }
} 