namespace RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPaginados;

public class ObtenerProductosPaginadosValidator : AbstractValidator<ObtenerProductosPaginadosQuery>
{
    private readonly string[] _allowedOrderFields = { "Nombre", "Precio", "FechaCreacion", "Popularidad" };
    private readonly string[] _allowedDirections = { "asc", "desc" };

    public ObtenerProductosPaginadosValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("El número de página debe ser mayor a 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("El tamaño de página debe ser mayor a 0")
            .LessThanOrEqualTo(100)
            .WithMessage("El tamaño de página no puede exceder 100 elementos");

        RuleFor(x => x.Filtro)
            .MaximumLength(100)
            .WithMessage("El filtro no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Filtro));

        RuleFor(x => x.OrderBy)
            .Must(BeValidOrderField)
            .WithMessage($"El campo de ordenamiento debe ser uno de: {string.Join(", ", _allowedOrderFields)}");

        RuleFor(x => x.OrderDirection)
            .Must(BeValidDirection)
            .WithMessage($"La dirección de ordenamiento debe ser: {string.Join(" o ", _allowedDirections)}");
    }

    private bool BeValidOrderField(string orderBy)
    {
        return _allowedOrderFields.Contains(orderBy, StringComparer.OrdinalIgnoreCase);
    }

    private bool BeValidDirection(string direction)
    {
        return _allowedDirections.Contains(direction, StringComparer.OrdinalIgnoreCase);
    }
} 