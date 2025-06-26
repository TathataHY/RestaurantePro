using FluentValidation;

namespace RestaurantePro.Application.Comercial.Promociones.Commands.AsignarProductos;

public class AsignarProductosValidator : AbstractValidator<AsignarProductosCommand>
{
    public AsignarProductosValidator()
    {
        RuleFor(x => x.PromocionId)
            .NotEmpty().WithMessage("El Id de la promoción es obligatorio");
        RuleFor(x => x.ProductosIds)
            .NotNull().WithMessage("Debe especificar los productos a asignar")
            .Must(x => x != null && x.Any()).WithMessage("Debe especificar al menos un producto");
    }
} 