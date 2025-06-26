using FluentValidation;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Comercial.Promociones.Enums;

namespace RestaurantePro.Application.Comercial.Promociones.Commands.AplicarPromocion;

/// <summary>
/// Validador para el comando AplicarPromocion
/// Valida reglas de negocio complejas para aplicación de promociones comerciales
/// </summary>
public class AplicarPromocionValidator : AbstractValidator<AplicarPromocionCommand>
{
    private readonly IApplicationDbContext _context;

    public AplicarPromocionValidator(IApplicationDbContext context)
    {
        _context = context;
        ConfigurarValidaciones();
    }

    private void ConfigurarValidaciones()
    {
        RuleFor(x => x)
            .Must(x => x.PromocionId != Guid.Empty || !string.IsNullOrEmpty(x.CodigoPromocion))
            .WithMessage("Debe proporcionar el ID o el código de la promoción");
        RuleFor(x => x)
            .Must(x => x.FacturaId.HasValue || x.ComandaId.HasValue)
            .WithMessage("Debe especificar una factura o comanda para aplicar la promoción");
        RuleFor(x => x)
            .Must(x => !(x.FacturaId.HasValue && x.ComandaId.HasValue))
            .WithMessage("No se puede aplicar la promoción a una factura y comanda simultáneamente");
        RuleFor(x => x.TipoAplicacion)
            .IsInEnum().WithMessage("El tipo de aplicación de promoción no es válido");
        RuleFor(x => x.ProductosIds)
            .NotEmpty().WithMessage("Debe especificar al menos un producto para este tipo de aplicación")
            .When(x => x.TipoAplicacion == TipoAplicacionPromocion.ProductosEspecificos);
        RuleFor(x => x.ProductosIds)
            .Must(productos => productos == null || (productos.Count > 0 && productos.Count <= 50))
            .WithMessage("Puede especificar entre 1 y 50 productos")
            .When(x => x.ProductosIds != null);
        RuleFor(x => x.NotasAplicacion)
            .MaximumLength(500).WithMessage("Las notas de aplicación no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.NotasAplicacion));
        RuleFor(x => x.FacturaId)
            .MustAsync(FacturaExiste).WithMessage("La factura especificada no existe").When(x => x.FacturaId.HasValue);
        RuleFor(x => x.ComandaId)
            .MustAsync(ComandaExiste).WithMessage("La comanda especificada no existe").When(x => x.ComandaId.HasValue);
        RuleFor(x => x.ClienteId)
            .MustAsync(ClienteExiste).WithMessage("El cliente especificado no existe").When(x => x.ClienteId.HasValue);
        RuleFor(x => x.ProductosIds)
            .MustAsync(TodosLosProductosExisten).WithMessage("Uno o más productos especificados no existen")
            .When(x => x.ProductosIds != null && x.ProductosIds.Any());
    }

    private async Task<bool> FacturaExiste(Guid? facturaId, CancellationToken cancellationToken)
    {
        if (!facturaId.HasValue) return true;
        if (_context?.Facturas == null) return true;
        try { return await _context.Facturas.AnyAsync(f => f.Id == facturaId.Value, cancellationToken); }
        catch { return true; }
    }
    private async Task<bool> ComandaExiste(Guid? comandaId, CancellationToken cancellationToken)
    {
        if (!comandaId.HasValue) return true;
        if (_context?.Comandas == null) return true;
        try { return await _context.Comandas.AnyAsync(c => c.Id == comandaId.Value, cancellationToken); }
        catch { return true; }
    }
    private async Task<bool> ClienteExiste(Guid? clienteId, CancellationToken cancellationToken)
    {
        if (!clienteId.HasValue) return true;
        if (_context?.Clientes == null) return true;
        try { return await _context.Clientes.AnyAsync(c => c.Id == clienteId.Value, cancellationToken); }
        catch { return true; }
    }
    private async Task<bool> TodosLosProductosExisten(List<Guid>? productosIds, CancellationToken cancellationToken)
    {
        if (productosIds == null || !productosIds.Any()) return true;
        if (_context?.Productos == null) return true;
        try {
            var productosEncontrados = await _context.Productos.Where(p => productosIds.Contains(p.Id)).CountAsync(cancellationToken);
            return productosEncontrados == productosIds.Count;
        } catch { return true; }
    }
} 