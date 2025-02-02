using FluentValidation;
using RestaurantePro.Core.DTOs.Comanda;

namespace RestaurantePro.Core.Validators
{
    public class ComandaDetalleValidator : AbstractValidator<ComandaDetalleDto>
    {
        public ComandaDetalleValidator()
        {
            RuleFor(x => x.PlatoId)
                .NotEmpty()
                .WithMessage("El ID del plato es requerido");

            RuleFor(x => x.Cantidad)
                .GreaterThan(0)
                .WithMessage("La cantidad debe ser mayor a 0");

            RuleFor(x => x.PrecioUnitario)
                .GreaterThan(0)
                .WithMessage("El precio unitario debe ser mayor a 0");
        }
    }
} 