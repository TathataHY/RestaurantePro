namespace RestaurantePro.Application.Core.Productos.Commands.CrearProducto;

/// <summary>
/// Validador para el comando CrearProducto
/// Parte del Vertical Slice: CrearProducto
/// </summary>
public class CrearProductoValidator : AbstractValidator<CrearProductoCommand>
{
    public CrearProductoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre del producto es obligatorio")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder 100 caracteres")
            .MinimumLength(3)
            .WithMessage("El nombre debe tener al menos 3 caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500)
            .WithMessage("La descripción no puede exceder 500 caracteres");

        RuleFor(x => x.Precio)
            .GreaterThan(0)
            .WithMessage("El precio debe ser mayor a 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("El precio no puede exceder $10,000");

        RuleFor(x => x.CategoriaId)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID de categoría es obligatorio");
    }
} 