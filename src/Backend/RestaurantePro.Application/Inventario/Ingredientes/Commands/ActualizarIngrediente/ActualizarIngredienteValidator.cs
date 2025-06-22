using FluentValidation;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.ActualizarIngrediente;

public class ActualizarIngredienteValidator : AbstractValidator<ActualizarIngredienteCommand>
{
    private readonly IApplicationDbContext _context;

    public ActualizarIngredienteValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID del ingrediente es requerido")
            .MustAsync(IngredienteExisteAsync)
            .WithMessage("El ingrediente especificado no existe");

        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre del ingrediente es requerido")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder los 100 caracteres")
            .MustAsync(NombreUnicoAsync)
            .WithMessage("Ya existe un ingrediente con ese nombre");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500)
            .WithMessage("La descripción no puede exceder los 500 caracteres");

        RuleFor(x => x.StockMinimo)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El stock mínimo debe ser mayor o igual a 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("El stock mínimo no puede exceder 10,000");

        RuleFor(x => x.Rotacion)
            .IsInEnum()
            .WithMessage("El tipo de rotación especificado no es válido");

        RuleFor(x => x.Temporada)
            .IsInEnum()
            .WithMessage("La temporada especificada no es válida");

        RuleFor(x => x.CostoPromedio)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El costo promedio debe ser mayor o igual a 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("El costo promedio no puede exceder $10,000");

        RuleFor(x => x.FechaExpiracion)
            .GreaterThan(DateTime.Now)
            .When(x => x.FechaExpiracion.HasValue)
            .WithMessage("La fecha de expiración debe ser futura");

        RuleFor(x => x.ProveedorPrincipalId)
            .MustAsync(ProveedorExisteAsync)
            .When(x => x.ProveedorPrincipalId.HasValue)
            .WithMessage("El proveedor especificado no existe");
    }

    private async Task<bool> IngredienteExisteAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Ingredientes.FindAsync(new object[] { id }, cancellationToken) != null;
    }

    private async Task<bool> NombreUnicoAsync(string nombre, CancellationToken cancellationToken)
    {
        // Verificar que no exista otro ingrediente con el mismo nombre (excluyendo el actual)
        var ingredienteActual = await _context.Ingredientes.FindAsync(new object[] { Guid.Empty }, cancellationToken);
        if (ingredienteActual == null) return true;

        return !await _context.Ingredientes
            .AnyAsync(i => i.Nombre.ToLower() == nombre.ToLower() && i.Id != ingredienteActual.Id, cancellationToken);
    }

    private async Task<bool> ProveedorExisteAsync(Guid? proveedorId, CancellationToken cancellationToken)
    {
        if (!proveedorId.HasValue) return true;
        return await _context.Proveedores.FindAsync(new object[] { proveedorId.Value }, cancellationToken) != null;
    }
} 