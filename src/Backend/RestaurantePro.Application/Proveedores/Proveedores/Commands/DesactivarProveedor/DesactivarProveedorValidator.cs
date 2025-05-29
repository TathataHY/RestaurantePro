namespace RestaurantePro.Application.Proveedores.Proveedores.Commands.DesactivarProveedor;

public class DesactivarProveedorValidator : AbstractValidator<DesactivarProveedorCommand>
{
    public DesactivarProveedorValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID del proveedor es obligatorio");

        RuleFor(x => x.RazonDesactivacion)
            .MaximumLength(500)
            .WithMessage("La razón de desactivación no puede exceder 500 caracteres");
    }
} 