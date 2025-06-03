namespace RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesasDisponibles;

/// <summary>
/// Validator para ObtenerMesasDisponiblesQuery
/// </summary>
public class ObtenerMesasDisponiblesValidator : AbstractValidator<ObtenerMesasDisponiblesQuery>
{
    public ObtenerMesasDisponiblesValidator()
    {
        RuleFor(x => x.Pagina)
            .GreaterThan(0)
            .WithMessage("La página debe ser mayor a 0");

        RuleFor(x => x.TamanoPagina)
            .GreaterThan(0)
            .WithMessage("El tamaño de página debe ser mayor a 0")
            .LessThanOrEqualTo(100)
            .WithMessage("El tamaño de página no puede exceder 100");

        RuleFor(x => x.CapacidadMinima)
            .GreaterThan(0)
            .WithMessage("La capacidad mínima debe ser mayor a 0")
            .LessThanOrEqualTo(50)
            .WithMessage("La capacidad mínima no puede exceder 50 personas")
            .When(x => x.CapacidadMinima.HasValue);

        RuleFor(x => x.Zona)
            .MaximumLength(100)
            .WithMessage("La zona no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Zona));
    }
} 