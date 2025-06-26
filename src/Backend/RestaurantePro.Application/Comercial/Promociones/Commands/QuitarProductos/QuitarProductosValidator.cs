using FluentValidation;

namespace RestaurantePro.Application.Comercial.Promociones.Commands.QuitarProductos;

/// <summary>
/// Validador para quitar productos de una promoción
/// </summary>
public class QuitarProductosValidator : AbstractValidator<QuitarProductosCommand>
{
    public QuitarProductosValidator()
    {
        RuleFor(x => x.PromocionId)
            .NotEmpty()
            .WithMessage("El ID de la promoción es requerido");

        RuleFor(x => x.ProductosIds)
            .NotEmpty()
            .WithMessage("Debe especificar al menos un producto para quitar");

        RuleForEach(x => x.ProductosIds)
            .NotEmpty()
            .WithMessage("Cada ID de producto debe ser válido");
    }
} 