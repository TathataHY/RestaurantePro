using FluentValidation;

namespace RestaurantePro.Application.Inventario.MovimientosInventario.Queries.ObtenerMovimientoPorId;

public class ObtenerMovimientoPorIdQueryValidator : AbstractValidator<ObtenerMovimientoPorIdQuery>
{
    public ObtenerMovimientoPorIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID del movimiento es requerido");
    }
} 