using FluentValidation;

namespace RestaurantePro.Application.Core.Usuarios.Queries.ObtenerUsuariosPaginados;

public class ObtenerUsuariosPaginadosValidator : AbstractValidator<ObtenerUsuariosPaginadosQuery>
{
    public ObtenerUsuariosPaginadosValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("El número de página debe ser mayor a 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("El tamaño de página debe estar entre 1 y 100");

        RuleFor(x => x.Filtro)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Filtro))
            .WithMessage("El filtro no puede exceder 200 caracteres");

        RuleFor(x => x.OrderBy)
            .Must(BeValidOrderBy)
            .WithMessage("Campo de ordenamiento inválido. Valores válidos: NombreCompleto, Email, FechaCreacion, Rol, Activo");

        RuleFor(x => x.OrderDirection)
            .Must(BeValidOrderDirection)
            .WithMessage("Dirección de ordenamiento inválida. Valores válidos: asc, desc");
    }

    private static bool BeValidOrderBy(string orderBy)
    {
        if (string.IsNullOrEmpty(orderBy)) return true;
        
        var validOrderByFields = new[] { "nombrecompleto", "email", "fechacreacion", "rol", "activo" };
        return validOrderByFields.Contains(orderBy.ToLowerInvariant());
    }

    private static bool BeValidOrderDirection(string direction)
    {
        if (string.IsNullOrEmpty(direction)) return true;
        
        var validDirections = new[] { "asc", "desc" };
        return validDirections.Contains(direction.ToLowerInvariant());
    }
}
