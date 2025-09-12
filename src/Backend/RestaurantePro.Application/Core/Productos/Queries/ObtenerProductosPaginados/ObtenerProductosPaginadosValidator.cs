using FluentValidation;

namespace RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPaginados;

public class ObtenerProductosPaginadosValidator : AbstractValidator<ObtenerProductosPaginadosQuery>
{
    public ObtenerProductosPaginadosValidator()
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

        // OrderBy y OrderDirection se validan en el handler para permitir valores por defecto
        // No validamos aquí para permitir que el handler maneje valores inválidos con defaults
    }

} 