using FluentValidation;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.AsociarProveedor;

public class AsociarProveedorValidator : AbstractValidator<AsociarProveedorCommand>
{
    private readonly IApplicationDbContext _context;

    public AsociarProveedorValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.IngredienteId)
            .NotEmpty()
            .WithMessage("El ID del ingrediente es requerido")
            .MustAsync(IngredienteExisteAsync)
            .WithMessage("El ingrediente especificado no existe");

        RuleFor(x => x.ProveedorId)
            .NotEmpty()
            .WithMessage("El ID del proveedor es requerido")
            .MustAsync(ProveedorExisteAsync)
            .WithMessage("El proveedor especificado no existe");

        RuleFor(x => x.PrecioUnitario)
            .GreaterThan(0)
            .When(x => x.PrecioUnitario.HasValue)
            .WithMessage("El precio unitario debe ser mayor a 0")
            .LessThanOrEqualTo(10000)
            .When(x => x.PrecioUnitario.HasValue)
            .WithMessage("El precio unitario no puede exceder $10,000");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Observaciones))
            .WithMessage("Las observaciones no pueden exceder los 500 caracteres");
    }

    private async Task<bool> IngredienteExisteAsync(Guid ingredienteId, CancellationToken cancellationToken)
    {
        return await _context.Ingredientes.FindAsync(new object[] { ingredienteId }, cancellationToken) != null;
    }

    private async Task<bool> ProveedorExisteAsync(Guid proveedorId, CancellationToken cancellationToken)
    {
        return await _context.Proveedores.FindAsync(new object[] { proveedorId }, cancellationToken) != null;
    }
} 