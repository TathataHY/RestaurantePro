using FluentValidation;

namespace RestaurantePro.Application.Inventario.MovimientosInventario.Queries.ObtenerMovimientosInventario;

public class ObtenerMovimientosInventarioQueryValidator : AbstractValidator<ObtenerMovimientosInventarioQuery>
{
    public ObtenerMovimientosInventarioQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("El número de página debe ser mayor a 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("El tamaño de página debe estar entre 1 y 100");

        RuleFor(x => x.FechaInicio)
            .LessThanOrEqualTo(x => x.FechaFin)
            .When(x => x.FechaInicio.HasValue && x.FechaFin.HasValue)
            .WithMessage("La fecha de inicio debe ser menor o igual a la fecha de fin");

        RuleFor(x => x.FechaFin)
            .GreaterThanOrEqualTo(x => x.FechaInicio)
            .When(x => x.FechaInicio.HasValue && x.FechaFin.HasValue)
            .WithMessage("La fecha de fin debe ser mayor o igual a la fecha de inicio");

        RuleFor(x => x.OrdenarPor)
            .Must(BeValidSortField)
            .WithMessage("Campo de ordenamiento no válido. Valores permitidos: fecha, cantidad, tipo, ingrediente");

        RuleFor(x => x.DireccionOrdenamiento)
            .Must(BeValidSortDirection)
            .WithMessage("Dirección de ordenamiento no válida. Valores permitidos: asc, desc");
    }

    private static bool BeValidSortField(string sortField)
    {
        if (string.IsNullOrWhiteSpace(sortField))
            return true;

        var validFields = new[] { "fecha", "cantidad", "tipo", "ingrediente" };
        return validFields.Contains(sortField.ToLower());
    }

    private static bool BeValidSortDirection(string direction)
    {
        if (string.IsNullOrWhiteSpace(direction))
            return true;

        var validDirections = new[] { "asc", "desc" };
        return validDirections.Contains(direction.ToLower());
    }
} 