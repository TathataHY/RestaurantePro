using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;

namespace RestaurantePro.Application.Proveedores.Proveedores.Commands.DesactivarProveedor;

public class DesactivarProveedorValidator : AbstractValidator<DesactivarProveedorCommand>
{
    private readonly IApplicationDbContext _context;

    public DesactivarProveedorValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID del proveedor es obligatorio")
            .MustAsync(ProveedorDebeExistir)
            .WithMessage("El proveedor especificado no existe")
            .MustAsync(ProveedorNoDebeEstarDesactivado)
            .WithMessage("El proveedor ya está desactivado")
            .MustAsync(ProveedorNoDebeTenerOrdenesActivas)
            .WithMessage("No se puede desactivar el proveedor porque tiene órdenes de compra activas")
            .MustAsync(ProveedorDebeTenerAntiguedadSuficiente)
            .WithMessage("No se puede desactivar un proveedor que fue creado hace menos de 24 horas");

        RuleFor(x => x.RazonDesactivacion)
            .NotEmpty()
            .WithMessage("El motivo de desactivación es obligatorio")
            .MinimumLength(5)
            .WithMessage("La razón de desactivación debe tener al menos 5 caracteres")
            .MaximumLength(1000)
            .WithMessage("La razón de desactivación no puede exceder 1000 caracteres");
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
        
        return proveedor.EstaActivo;
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