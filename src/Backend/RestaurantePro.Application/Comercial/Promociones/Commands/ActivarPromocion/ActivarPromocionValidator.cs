using FluentValidation;

namespace RestaurantePro.Application.Comercial.Promociones.Commands.ActivarPromocion;

public class ActivarPromocionValidator : AbstractValidator<ActivarPromocionCommand>
{
    public ActivarPromocionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El Id de la promoción es obligatorio");
    }
} 