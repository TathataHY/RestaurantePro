namespace RestaurantePro.Application.Proveedores.Proveedores.Commands.DesactivarProveedor;

public class DesactivarProveedorValidator : AbstractValidator<DesactivarProveedorCommand>
{
    private readonly IApplicationDbContext _context;

    public DesactivarProveedorValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID del proveedor es obligatorio")
            .MustAsync(ProveedorDebeExistir)
            .WithMessage("El proveedor especificado no fue encontrado")
            .MustAsync(ProveedorNoDebeEstarDesactivado)
            .WithMessage("El proveedor ya está desactivado")
            .MustAsync(ProveedorNoDebeTenerOrdenesActivas)
            .WithMessage("No se puede desactivar el proveedor porque tiene órdenes de compra activas")
            .MustAsync(ProveedorDebeTenerAntiguedadSuficiente)
            .WithMessage("No se puede desactivar un proveedor que fue creado hace menos de 24 horas");

        RuleFor(x => x.RazonDesactivacion)
            .NotEmpty()
            .WithMessage("motivo de desactivación es obligatorio")
            .MinimumLength(5)
            .WithMessage("debe tener al menos 5 caracteres")
            .MaximumLength(1000)
            .WithMessage("no puede exceder 1000 caracteres");
    }

    private async Task<bool> ProveedorDebeExistir(Guid proveedorId, CancellationToken cancellationToken)
    {
        var proveedor = await _context.Proveedores
            .FirstOrDefaultAsync(p => p.Id == proveedorId, cancellationToken);
        
        return proveedor != null;
    }

    private async Task<bool> ProveedorNoDebeEstarDesactivado(Guid proveedorId, CancellationToken cancellationToken)
    {
        var proveedor = await _context.Proveedores
            .FirstOrDefaultAsync(p => p.Id == proveedorId, cancellationToken);
        
        if (proveedor == null) return true; // Este error se maneja en otra validación
        
        return proveedor.Activo;
    }

    private async Task<bool> ProveedorNoDebeTenerOrdenesActivas(Guid proveedorId, CancellationToken cancellationToken)
    {
        var ordenesActivas = await _context.OrdenesCompra
            .Where(o => o.ProveedorId == proveedorId &&
                       (o.Estado == EstadoOrdenCompra.Pendiente ||
                        o.Estado == EstadoOrdenCompra.Confirmada ||
                        o.Estado == EstadoOrdenCompra.EnTransito))
            .AnyAsync(cancellationToken);
        
        return !ordenesActivas;
    }

    private async Task<bool> ProveedorDebeTenerAntiguedadSuficiente(Guid proveedorId, CancellationToken cancellationToken)
    {
        var proveedor = await _context.Proveedores
            .FirstOrDefaultAsync(p => p.Id == proveedorId, cancellationToken);
        
        if (proveedor == null) return true; // Este error se maneja en otra validación
        
        var tiempoTranscurrido = DateTime.Now - proveedor.FechaCreacion;
        return tiempoTranscurrido.TotalHours >= 24;
    }
} 