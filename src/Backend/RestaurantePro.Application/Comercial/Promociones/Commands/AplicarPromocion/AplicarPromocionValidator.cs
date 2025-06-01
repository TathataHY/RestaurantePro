namespace RestaurantePro.Application.Comercial.Promociones.Commands.AplicarPromocion;

/// <summary>
/// Validador para AplicarPromocionCommand
/// Valida reglas de negocio para la aplicación de promociones
/// </summary>
public class AplicarPromocionValidator : AbstractValidator<AplicarPromocionCommand>
{
    private readonly IApplicationDbContext _context;

    public AplicarPromocionValidator(IApplicationDbContext context)
    {
        _context = context;

        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesPromocion();
        ConfigurarValidacionesAplicacion();
        ConfigurarValidacionesNegocio();
    }

    private void ConfigurarValidacionesBasicas()
    {
        // Validación de identificación de promoción
        RuleFor(v => v.PromocionId)
            .NotEmpty()
            .WithMessage("El ID de la promoción es requerido.")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID de la promoción no puede ser un GUID vacío.")
            .MustAsync(PromocionExiste)
            .WithMessage("La promoción especificada no existe.")
            .When(v => v.PromocionId != Guid.Empty);

        // Validación alternativa por código
        RuleFor(v => v.CodigoPromocion)
            .NotEmpty()
            .WithMessage("El código de promoción es requerido cuando no se proporciona ID.")
            .Length(3, 20)
            .WithMessage("El código de promoción debe tener entre 3 y 20 caracteres.")
            .Matches(@"^[A-Z0-9\-_]+$")
            .WithMessage("El código de promoción solo puede contener letras mayúsculas, números, guiones y guiones bajos.")
            .MustAsync(CodigoPromocionExiste)
            .WithMessage("El código de promoción especificado no existe.")
            .When(v => v.PromocionId == Guid.Empty);

        // Validación de que se proporcione al menos un identificador
        RuleFor(v => v)
            .Must(v => v.PromocionId != Guid.Empty || !string.IsNullOrEmpty(v.CodigoPromocion))
            .WithMessage("Debe proporcionar el ID de promoción o el código de promoción.")
            .WithName("Identificacion");

        // Validación del tipo de aplicación
        RuleFor(v => v.TipoAplicacion)
            .IsInEnum()
            .WithMessage("El tipo de aplicación especificado no es válido.");
    }

    private void ConfigurarValidacionesPromocion()
    {
        // Validación de estado de promoción
        RuleFor(v => v.PromocionId)
            .MustAsync(PromocionEstaActiva)
            .WithMessage("La promoción no está activa o ha expirado.")
            .When(v => v.PromocionId != Guid.Empty);

        // Validación de vigencia de promoción
        RuleFor(v => v.PromocionId)
            .MustAsync(PromocionEstaVigente)
            .WithMessage("La promoción no está dentro del período de vigencia.")
            .When(v => v.PromocionId != Guid.Empty);

        // Validación de límites de uso
        RuleFor(v => v.PromocionId)
            .MustAsync(PromocionNoHaExcedidoLimites)
            .WithMessage("La promoción ha excedido su límite de uso.")
            .When(v => v.PromocionId != Guid.Empty && v.ValidarLimitesUso);
    }

    private void ConfigurarValidacionesAplicacion()
    {
        // Validación de que se especifique factura o comanda
        RuleFor(v => v)
            .Must(v => v.FacturaId.HasValue || v.ComandaId.HasValue)
            .WithMessage("Debe especificar una factura o comanda donde aplicar la promoción.")
            .WithName("DestinoAplicacion");

        // Validación de factura
        RuleFor(v => v.FacturaId)
            .MustAsync(FacturaExiste)
            .WithMessage("La factura especificada no existe.")
            .When(v => v.FacturaId.HasValue);

        // Validación de comanda
        RuleFor(v => v.ComandaId)
            .MustAsync(ComandaExiste)
            .WithMessage("La comanda especificada no existe.")
            .When(v => v.ComandaId.HasValue);

        // Validación de productos específicos
        RuleFor(v => v.ProductosIds)
            .NotEmpty()
            .WithMessage("Debe especificar al menos un producto para aplicaciones específicas.")
            .Must(productos => productos!.Count <= 50)
            .WithMessage("No se pueden especificar más de 50 productos.")
            .MustAsync(TodosLosProductosExisten)
            .WithMessage("Uno o más productos especificados no existen.")
            .When(v => v.TipoAplicacion == TipoAplicacionPromocion.ProductosEspecificos);

        // Validación de cliente
        RuleFor(v => v.ClienteId)
            .MustAsync(ClienteExiste)
            .WithMessage("El cliente especificado no existe.")
            .When(v => v.ClienteId.HasValue);
    }

    private void ConfigurarValidacionesNegocio()
    {
        // Validación de restricciones de cliente
        RuleFor(v => v)
            .MustAsync(ClienteCumpleRestricciones)
            .WithMessage("El cliente no cumple con las restricciones de la promoción.")
            .WithName("RestriccionesCliente")
            .When(v => v.ClienteId.HasValue && v.ValidarRestriccionesCliente);

        // Validación de compatibilidad de promoción
        RuleFor(v => v)
            .MustAsync(PromocionEsCompatible)
            .WithMessage("La promoción no es compatible con el destino especificado.")
            .WithName("CompatibilidadPromocion");

        // Validaciones opcionales
        RuleFor(v => v.NotasAplicacion)
            .MaximumLength(500)
            .WithMessage("Las notas de aplicación no pueden exceder 500 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.NotasAplicacion));

        // Validación de autorización
        RuleFor(v => v.AutorizadoPor)
            .NotEmpty()
            .WithMessage("Se requiere autorización para aplicaciones especiales.")
            .MustAsync(UsuarioEsValido)
            .WithMessage("El usuario autorizador no es válido.")
            .When(v => v.AutorizadoPor.HasValue);

        // Validación de datos adicionales
        RuleFor(v => v.DatosAdicionales)
            .Must(DatosAdicionalesValidos)
            .WithMessage("Los datos adicionales contienen información inválida.")
            .When(v => v.DatosAdicionales != null && v.DatosAdicionales.Any());
    }

    #region Métodos de validación privados

    private async Task<bool> PromocionExiste(Guid promocionId, CancellationToken cancellationToken)
    {
        return await _context.Promociones
            .AnyAsync(p => p.Id == promocionId, cancellationToken);
    }

    private async Task<bool> CodigoPromocionExiste(string codigoPromocion, CancellationToken cancellationToken)
    {
        return await _context.Promociones
            .AnyAsync(p => p.Codigo == codigoPromocion, cancellationToken);
    }

    private async Task<bool> PromocionEstaActiva(Guid promocionId, CancellationToken cancellationToken)
    {
        var promocion = await _context.Promociones
            .FirstOrDefaultAsync(p => p.Id == promocionId, cancellationToken);

        return promocion?.Activa == true;
    }

    private async Task<bool> PromocionEstaVigente(Guid promocionId, CancellationToken cancellationToken)
    {
        var promocion = await _context.Promociones
            .FirstOrDefaultAsync(p => p.Id == promocionId, cancellationToken);

        if (promocion == null)
            return false;

        var ahora = DateTime.UtcNow;
        return ahora >= promocion.FechaInicio && ahora <= promocion.FechaFin;
    }

    private async Task<bool> PromocionNoHaExcedidoLimites(Guid promocionId, CancellationToken cancellationToken)
    {
        var promocion = await _context.Promociones
            .FirstOrDefaultAsync(p => p.Id == promocionId, cancellationToken);

        if (promocion == null || !promocion.LimiteUso.HasValue)
            return true;

        var usosActuales = await _context.AplicacionesPromocion
            .CountAsync(ap => ap.PromocionId == promocionId, cancellationToken);

        return usosActuales < promocion.LimiteUso.Value;
    }

    private async Task<bool> FacturaExiste(Guid? facturaId, CancellationToken cancellationToken)
    {
        if (!facturaId.HasValue)
            return true;

        return await _context.Facturas
            .AnyAsync(f => f.Id == facturaId.Value, cancellationToken);
    }

    private async Task<bool> ComandaExiste(Guid? comandaId, CancellationToken cancellationToken)
    {
        if (!comandaId.HasValue)
            return true;

        return await _context.Comandas
            .AnyAsync(c => c.Id == comandaId.Value, cancellationToken);
    }

    private async Task<bool> ClienteExiste(Guid? clienteId, CancellationToken cancellationToken)
    {
        if (!clienteId.HasValue)
            return true;

        return await _context.Clientes
            .AnyAsync(c => c.Id == clienteId.Value, cancellationToken);
    }

    private async Task<bool> TodosLosProductosExisten(List<Guid>? productosIds, CancellationToken cancellationToken)
    {
        if (productosIds == null || !productosIds.Any())
            return true;

        var productosEncontrados = await _context.Productos
            .Where(p => productosIds.Contains(p.Id))
            .CountAsync(cancellationToken);

        return productosEncontrados == productosIds.Count;
    }

    private async Task<bool> ClienteCumpleRestricciones(AplicarPromocionCommand command, CancellationToken cancellationToken)
    {
        if (!command.ClienteId.HasValue)
            return true;

        var promocion = await _context.Promociones
            .FirstOrDefaultAsync(p => p.Id == command.PromocionId, cancellationToken);

        if (promocion == null)
            return false;

        // Aquí se implementarían las validaciones específicas de restricciones de cliente
        // Por ejemplo: cliente VIP, primera compra, etc.
        return true; // Temporalmente permitir
    }

    private async Task<bool> PromocionEsCompatible(AplicarPromocionCommand command, CancellationToken cancellationToken)
    {
        var promocion = await _context.Promociones
            .FirstOrDefaultAsync(p => p.Id == command.PromocionId, cancellationToken);

        if (promocion == null)
            return false;

        // Validar compatibilidad según el tipo de aplicación
        return command.TipoAplicacion switch
        {
            TipoAplicacionPromocion.ProductosEspecificos => command.ProductosIds?.Any() == true,
            TipoAplicacionPromocion.FacturaCompleta => command.FacturaId.HasValue || command.ComandaId.HasValue,
            _ => true
        };
    }

    private async Task<bool> UsuarioEsValido(Guid? usuarioId, CancellationToken cancellationToken)
    {
        if (!usuarioId.HasValue)
            return false;

        return await _context.Usuarios
            .AnyAsync(u => u.Id == usuarioId.Value && u.Activo, cancellationToken);
    }

    private static bool DatosAdicionalesValidos(Dictionary<string, object> datosAdicionales)
    {
        // Validar que no haya demasiados datos adicionales
        if (datosAdicionales.Count > 10)
            return false;

        // Validar que las claves no sean muy largas
        return datosAdicionales.All(kvp => 
            !string.IsNullOrEmpty(kvp.Key) && 
            kvp.Key.Length <= 50);
    }

    #endregion
} 