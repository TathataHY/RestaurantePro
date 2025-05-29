namespace RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientesPaginados;

/// <summary>
/// Validador para ObtenerClientesPaginadosQuery
/// </summary>
public class ObtenerClientesPaginadosValidator : AbstractValidator<ObtenerClientesPaginadosQuery>
{
    public ObtenerClientesPaginadosValidator()
    {
        RuleFor(x => x.Filtros)
            .NotNull().WithMessage("Los filtros son obligatorios")
            .Must(f => f.IsValid).WithMessage("Los filtros no son válidos");

        RuleFor(x => x.PuntosMinimos)
            .GreaterThanOrEqualTo(0).When(x => x.PuntosMinimos.HasValue)
            .WithMessage("Los puntos mínimos deben ser mayores o iguales a 0");

        RuleFor(x => x.VisitasMinimas)
            .GreaterThanOrEqualTo(0).When(x => x.VisitasMinimas.HasValue)
            .WithMessage("Las visitas mínimas deben ser mayores o iguales a 0");

        RuleFor(x => x.EdadMinima)
            .InclusiveBetween(18, 120).When(x => x.EdadMinima.HasValue)
            .WithMessage("La edad mínima debe estar entre 18 y 120 años");

        RuleFor(x => x.EdadMaxima)
            .InclusiveBetween(18, 120).When(x => x.EdadMaxima.HasValue)
            .WithMessage("La edad máxima debe estar entre 18 y 120 años");

        RuleFor(x => x)
            .Must(x => !x.EdadMinima.HasValue || !x.EdadMaxima.HasValue || x.EdadMinima <= x.EdadMaxima)
            .WithMessage("La edad mínima debe ser menor o igual a la edad máxima");
    }
} 