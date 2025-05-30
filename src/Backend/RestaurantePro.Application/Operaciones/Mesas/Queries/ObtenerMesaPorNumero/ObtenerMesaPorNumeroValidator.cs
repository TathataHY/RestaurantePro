using FluentValidation;

namespace RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesaPorNumero;

/// <summary>
/// Validator para ObtenerMesaPorNumeroQuery
/// </summary>
public class ObtenerMesaPorNumeroValidator : AbstractValidator<ObtenerMesaPorNumeroQuery>
{
    public ObtenerMesaPorNumeroValidator()
    {
        RuleFor(x => x.Numero)
            .GreaterThan(0)
            .WithMessage("El número de mesa debe ser mayor a 0")
            .LessThanOrEqualTo(999)
            .WithMessage("El número de mesa no puede exceder 999");

        RuleFor(x => x.Zona)
            .MaximumLength(50)
            .WithMessage("La zona no puede exceder 50 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Zona));
    }
} 