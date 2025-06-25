using FluentValidation;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Queries.ObtenerEstadisticasTarjeta;

/// <summary>
/// Validador para la query de obtener estadísticas de tarjeta
/// </summary>
public class ObtenerEstadisticasTarjetaValidator : AbstractValidator<ObtenerEstadisticasTarjetaQuery>
{
    public ObtenerEstadisticasTarjetaValidator()
    {
        RuleFor(x => x.TarjetaFidelizacionId)
            .NotEmpty()
            .WithMessage("El ID de la tarjeta de fidelización es requerido");
    }
} 