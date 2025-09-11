namespace RestaurantePro.Application.Core.Productos.Commands.ActualizarProducto;

public class ActualizarProductoValidator : AbstractValidator<ActualizarProductoCommand>
{
    public ActualizarProductoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID del producto es obligatorio");

        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre del producto es obligatorio")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder 100 caracteres")
            .MinimumLength(1)
            .WithMessage("El nombre debe tener al menos 1 carácter");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500)
            .WithMessage("La descripción no puede exceder 500 caracteres");

        RuleFor(x => x.Precio)
            .GreaterThan(0)
            .WithMessage("El precio debe ser mayor a 0")
            .LessThanOrEqualTo(1000000)
            .WithMessage("El precio no puede exceder $1,000,000");

        RuleFor(x => x.CategoriaId)
            .NotEmpty()
            .WithMessage("La categoría es obligatoria");
    }
} 