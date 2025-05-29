namespace RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPorCategoria;

public class ObtenerProductosPorCategoriaValidator : AbstractValidator<ObtenerProductosPorCategoriaQuery>
{
    public ObtenerProductosPorCategoriaValidator()
    {
        RuleFor(x => x.CategoriaId)
            .NotEmpty()
            .WithMessage("El ID de la categoría es obligatorio")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID de la categoría no puede ser vacío");
    }
} 