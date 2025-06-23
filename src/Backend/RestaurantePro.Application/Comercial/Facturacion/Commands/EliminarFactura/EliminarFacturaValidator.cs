using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Comercial.Facturacion.Commands.EliminarFactura;

public class EliminarFacturaValidator : AbstractValidator<EliminarFacturaCommand>
{
    private readonly IApplicationDbContext _context;

    public EliminarFacturaValidator(IApplicationDbContext context)
    {
        _context = context;

        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesFactura();
        ConfigurarValidacionesAutorizacion();
        ConfigurarValidacionesNegocio();
    }

    private void ConfigurarValidacionesBasicas()
    {
        RuleFor(v => v.FacturaId)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID de la factura es requerido.")
            .MustAsync(FacturaExiste)
            .WithMessage("La factura especificada no existe.");

        RuleFor(v => v.Motivo)
            .NotEmpty()
            .WithMessage("El motivo de anulación es requerido.")
            .MinimumLength(10)
            .WithMessage("El motivo debe tener al menos 10 caracteres.")
            .MaximumLength(500)
            .WithMessage("El motivo no puede exceder 500 caracteres.");

        RuleFor(v => v.UsuarioAutorizaId)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del usuario autorizador es requerido.")
            .MustAsync(UsuarioExiste)
            .WithMessage("El usuario autorizador no existe.");
    }

    private void ConfigurarValidacionesFactura()
    {
        RuleFor(v => v.FacturaId)
            .MustAsync(FacturaNoEstaAnulada)
            .WithMessage("La factura ya se encuentra anulada.")
            .When(v => v.FacturaId != Guid.Empty);

        RuleFor(v => v.FacturaId)
            .MustAsync(FacturaNoEstaPagada)
            .WithMessage("No se puede anular una factura que ya ha sido pagada.")
            .When(v => v.FacturaId != Guid.Empty);
    }

    private void ConfigurarValidacionesAutorizacion()
    {
        RuleFor(v => v.UsuarioAutorizaId)
            .MustAsync(UsuarioTienePermisosAnulacion)
            .WithMessage("El usuario no tiene permisos para realizar anulaciones.")
            .When(v => v.UsuarioAutorizaId != Guid.Empty);
    }

    private void ConfigurarValidacionesNegocio()
    {
        RuleFor(v => v.FacturaId)
            .MustAsync(FacturaNoTieneMovimientosPosteriores)
            .WithMessage("No se puede anular una factura que tiene movimientos posteriores.")
            .When(v => v.FacturaId != Guid.Empty);

        RuleFor(v => v.FacturaId)
            .MustAsync(FacturaNoEstaEnPeriodoContableCerrado)
            .WithMessage("No se puede anular una factura en un período contable cerrado.")
            .When(v => v.FacturaId != Guid.Empty);

        RuleFor(v => v.FacturaId)
            .MustAsync(FacturaNoTieneDependencias)
            .WithMessage("No se puede anular una factura que tiene dependencias (pagos, notas de crédito, etc.).")
            .When(v => v.FacturaId != Guid.Empty);
    }

    private async Task<bool> FacturaExiste(Guid facturaId, CancellationToken cancellationToken)
    {
        try
        {
            return await _context.Facturas
                .AnyAsync(f => f.Id == facturaId, cancellationToken);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task<bool> UsuarioExiste(Guid usuarioId, CancellationToken cancellationToken)
    {
        try
        {
            return await _context.Usuarios
                .AnyAsync(u => u.Id == usuarioId, cancellationToken);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task<bool> FacturaNoEstaAnulada(Guid facturaId, CancellationToken cancellationToken)
    {
        try
        {
            var factura = await _context.Facturas
                .FirstOrDefaultAsync(f => f.Id == facturaId, cancellationToken);

            return factura?.Estado != Domain.Comercial.Facturacion.Enums.EstadoFactura.Anulada;
        }
        catch (Exception)
        {
            return true;
        }
    }

    private async Task<bool> FacturaNoEstaPagada(Guid facturaId, CancellationToken cancellationToken)
    {
        try
        {
            var factura = await _context.Facturas
                .FirstOrDefaultAsync(f => f.Id == facturaId, cancellationToken);

            return factura?.Estado != Domain.Comercial.Facturacion.Enums.EstadoFactura.Pagada;
        }
        catch (Exception)
        {
            return true;
        }
    }

    private async Task<bool> UsuarioTienePermisosAnulacion(Guid usuarioId, CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);

            if (usuario == null) return false;

            // TODO: Implementar cuando se agregue la propiedad Rol al Usuario
            // var rolesPermitidos = new[] { "Administrador", "Gerente", "Cajero" };
            // return rolesPermitidos.Contains(usuario.Rol);

            return true; // Por ahora permitir a todos los usuarios
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task<bool> FacturaNoTieneMovimientosPosteriores(Guid facturaId, CancellationToken cancellationToken)
    {
        try
        {
            var factura = await _context.Facturas
                .FirstOrDefaultAsync(f => f.Id == facturaId, cancellationToken);

            if (factura == null) return false;

            // TODO: Implementar cuando tengamos tabla de movimientos
            // var fechaLimite = factura.FechaEmision.AddDays(1);
            // var tieneMovimientosPosteriores = await _context.MovimientosFactura
            //     .AnyAsync(m => m.FacturaId == facturaId && m.FechaMovimiento > fechaLimite, cancellationToken);

            // return !tieneMovimientosPosteriores;
            return true; // Por ahora asumir que es válido
        }
        catch (Exception)
        {
            return true;
        }
    }

    private async Task<bool> FacturaNoEstaEnPeriodoContableCerrado(Guid facturaId, CancellationToken cancellationToken)
    {
        try
        {
            var factura = await _context.Facturas
                .FirstOrDefaultAsync(f => f.Id == facturaId, cancellationToken);

            if (factura == null) return false;

            // TODO: Implementar cuando tengamos tabla de períodos contables
            // var periodo = await _context.PeriodosContables
            //     .FirstOrDefaultAsync(p => 
            //         factura.FechaCreacion >= p.FechaInicio && 
            //         factura.FechaCreacion <= p.FechaFin, 
            //         cancellationToken);

            // return periodo?.Estado != "Cerrado";
            return true; // Por ahora asumir que es válido
        }
        catch (Exception)
        {
            return true;
        }
    }

    private async Task<bool> FacturaNoTieneDependencias(Guid facturaId, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Implementar cuando tengamos las tablas relacionadas
            // var tienePagos = await _context.PagosFactura
            //     .AnyAsync(p => p.FacturaId == facturaId, cancellationToken);

            // var tieneNotasCredito = await _context.NotasCredito
            //     .AnyAsync(n => n.FacturaId == facturaId, cancellationToken);

            // var tieneNotasDebito = await _context.NotasDebito
            //     .AnyAsync(n => n.FacturaId == facturaId, cancellationToken);

            // return !tienePagos && !tieneNotasCredito && !tieneNotasDebito;
            return true; // Por ahora asumir que es válido
        }
        catch (Exception)
        {
            return true;
        }
    }
} 