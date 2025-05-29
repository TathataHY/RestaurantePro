namespace RestaurantePro.Application.Core.Productos.Commands.EliminarProducto;

public class EliminarProductoValidator : AbstractValidator<EliminarProductoCommand>
{
    public EliminarProductoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID del producto es obligatorio")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del producto no puede ser vacío");
    }
} 