using FluentValidation;

namespace RestaurantePro.Application.Comercial.Promociones.Commands.PausarPromocion;

public class PausarPromocionValidator : AbstractValidator<PausarPromocionCommand>
{
    public PausarPromocionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El Id de la promoción es obligatorio");
    }
} 