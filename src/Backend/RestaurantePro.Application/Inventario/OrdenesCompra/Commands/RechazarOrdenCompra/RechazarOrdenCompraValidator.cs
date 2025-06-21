namespace RestaurantePro.Application.Inventario.OrdenesCompra.Commands.RechazarOrdenCompra;

/// <summary>
/// Validador para RechazarOrdenCompraCommand
/// </summary>
public class RechazarOrdenCompraValidator : AbstractValidator<RechazarOrdenCompraCommand>
{
    public RechazarOrdenCompraValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la orden de compra es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID de la orden de compra no puede ser un GUID vacío");

        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("El ID del usuario es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del usuario no puede ser un GUID vacío");

        RuleFor(x => x.MotivoRechazo)
            .NotEmpty().WithMessage("El motivo de rechazo es obligatorio")
            .MaximumLength(300).WithMessage("El motivo de rechazo no puede exceder 300 caracteres");
    }
} 