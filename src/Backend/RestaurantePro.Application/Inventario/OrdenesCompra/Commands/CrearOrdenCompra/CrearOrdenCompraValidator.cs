using FluentValidation;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Commands.CrearOrdenCompra;

/// <summary>
/// Validador para CrearOrdenCompraCommand
/// </summary>
public class CrearOrdenCompraValidator : AbstractValidator<CrearOrdenCompraCommand>
{
    public CrearOrdenCompraValidator()
    {
        RuleFor(x => x.ProveedorId)
            .NotEmpty().WithMessage("El proveedor es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El proveedor no puede ser un GUID vacío");

        RuleFor(x => x.FechaEntregaEsperada)
            .GreaterThan(DateTime.Today).WithMessage("La fecha de entrega esperada debe ser posterior a hoy");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500).WithMessage("Las observaciones no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Debe agregar al menos un item");

        RuleForEach(x => x.Items)
            .SetValidator(new OrdenCompraItemCommandValidator());
    }
}

/// <summary>
/// Validador para items de orden de compra
/// </summary>
public class OrdenCompraItemCommandValidator : AbstractValidator<OrdenCompraItemCommand>
{
    public OrdenCompraItemCommandValidator()
    {
        RuleFor(x => x.IngredienteId)
            .NotEmpty().WithMessage("El ID del ingrediente es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del ingrediente no puede ser un GUID vacío");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0");

        RuleFor(x => x.PrecioUnitario)
            .GreaterThanOrEqualTo(0).WithMessage("El precio unitario no puede ser negativo");

        RuleFor(x => x.Observaciones)
            .MaximumLength(200).WithMessage("Las observaciones del item no pueden exceder 200 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));
    }
} 