using FluentValidation;

namespace RestaurantePro.Application.Comercial.Promociones.Commands.EliminarPromocion;

public class EliminarPromocionValidator : AbstractValidator<EliminarPromocionCommand>
{
    public EliminarPromocionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El Id de la promoción es obligatorio");
    }
} 