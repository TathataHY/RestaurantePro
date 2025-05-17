using FluentValidation;
using RestaurantePro.Domain.Enums;

namespace RestaurantePro.Application.Features.Inventario.Commands.AjustarInventario
{
    public class AjustarInventarioCommandValidator : AbstractValidator<AjustarInventarioCommand>
    {
        public AjustarInventarioCommandValidator()
        {
            RuleFor(x => x.InventarioId)
                .GreaterThan(0)
                .When(x => !x.IngredienteId.HasValue)
                .WithMessage("Debe especificar el ID del inventario o el ID del ingrediente");

            RuleFor(x => x.IngredienteId)
                .NotNull()
                .When(x => x.InventarioId <= 0)
                .WithMessage("Debe especificar el ID del inventario o el ID del ingrediente");

            RuleFor(x => x.TipoMovimiento)
                .IsInEnum()
                .WithMessage("El tipo de movimiento no es válido");

            RuleFor(x => x.Cantidad)
                .GreaterThan(0)
                .When(x => x.TipoMovimiento != TipoMovimiento.Ajuste)
                .WithMessage("La cantidad debe ser mayor que cero para entradas, salidas o mermas");

            RuleFor(x => x.Cantidad)
                .GreaterThanOrEqualTo(0)
                .When(x => x.TipoMovimiento == TipoMovimiento.Ajuste)
                .WithMessage("La cantidad no puede ser negativa para ajustes directos");

            RuleFor(x => x.Motivo)
                .NotEmpty()
                .WithMessage("El motivo del ajuste es obligatorio");

            RuleFor(x => x.CostoUnitario)
                .GreaterThan(0)
                .When(x => x.CostoUnitario.HasValue && x.TipoMovimiento == TipoMovimiento.Entrada)
                .WithMessage("El costo unitario debe ser mayor que cero para entradas de inventario");
        }
    }
} 