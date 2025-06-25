using FluentValidation;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Queries.ObtenerHistorialPuntos;

/// <summary>
/// Validador para la query de obtener historial de puntos
/// </summary>
public class ObtenerHistorialPuntosValidator : AbstractValidator<ObtenerHistorialPuntosQuery>
{
    public ObtenerHistorialPuntosValidator()
    {
        RuleFor(x => x.TarjetaFidelizacionId)
            .NotEmpty()
            .WithMessage("El ID de la tarjeta de fidelización es requerido");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("El número de página debe ser mayor a cero");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("El tamaño de página debe estar entre 1 y 100");
    }
}