using FluentValidation;
using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Commands.ActualizarOrdenCompra;

/// <summary>
/// Validador para ActualizarOrdenCompraCommand
/// </summary>
public class ActualizarOrdenCompraValidator : AbstractValidator<ActualizarOrdenCompraCommand>
{
    public ActualizarOrdenCompraValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la orden de compra es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID de la orden de compra no puede ser un GUID vacío");

        RuleFor(x => x.FechaEntregaEsperada)
            .GreaterThan(DateTime.Today).WithMessage("La fecha de entrega esperada debe ser posterior a hoy")
            .When(x => x.FechaEntregaEsperada.HasValue);

        RuleFor(x => x.Observaciones)
            .MaximumLength(500).WithMessage("Las observaciones no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));

        RuleFor(x => x.Items)
            .Must(items => items == null || items.Any()).WithMessage("La orden debe tener al menos un item")
            .When(x => x.Items != null);

        RuleForEach(x => x.Items)
            .SetValidator(new OrdenCompraItemCommandValidator())
            .When(x => x.Items != null);
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