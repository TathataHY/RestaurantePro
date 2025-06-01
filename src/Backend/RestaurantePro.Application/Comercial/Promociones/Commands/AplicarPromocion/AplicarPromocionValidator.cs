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
        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesCondicionales();
        ConfigurarValidacionesNegocio();
    }

    private void ConfigurarValidacionesBasicas()
    {
        // Validación de identificación de promoción
        RuleFor(x => x.PromocionId)
            .NotEmpty()
            .WithErrorCode("APLICAR_PROMOCION_ID_REQUERIDO")
            .WithMessage("El ID de la promoción es obligatorio")
            .When(x => string.IsNullOrEmpty(x.CodigoPromocion));

        RuleFor(x => x.CodigoPromocion)
            .NotEmpty()
            .WithErrorCode("APLICAR_PROMOCION_CODIGO_REQUERIDO")
            .WithMessage("El código de promoción es obligatorio")
            .When(x => x.PromocionId == Guid.Empty);

        // Al menos uno debe estar presente
        RuleFor(x => x)
            .Must(x => x.PromocionId != Guid.Empty || !string.IsNullOrEmpty(x.CodigoPromocion))
            .WithErrorCode("APLICAR_PROMOCION_IDENTIFICACION_REQUERIDA")
            .WithMessage("Debe proporcionar el ID o el código de la promoción");

        // Validación de destino (factura o comanda)
        RuleFor(x => x)
            .Must(x => x.FacturaId.HasValue || x.ComandaId.HasValue)
            .WithErrorCode("APLICAR_PROMOCION_DESTINO_REQUERIDO")
            .WithMessage("Debe especificar una factura o comanda para aplicar la promoción");

        // No pueden estar ambos presentes
        RuleFor(x => x)
            .Must(x => !(x.FacturaId.HasValue && x.ComandaId.HasValue))
            .WithErrorCode("APLICAR_PROMOCION_DESTINO_MULTIPLE")
            .WithMessage("No se puede aplicar la promoción a una factura y comanda simultáneamente");

        // Validación de tipo de aplicación
        RuleFor(x => x.TipoAplicacion)
            .IsInEnum()
            .WithErrorCode("APLICAR_PROMOCION_TIPO_INVALIDO")
            .WithMessage("El tipo de aplicación de promoción no es válido");

        // Validación condicional de productos
        RuleFor(x => x.ProductosIds)
            .NotEmpty()
            .WithErrorCode("APLICAR_PROMOCION_PRODUCTOS_REQUERIDOS")
            .WithMessage("Debe especificar al menos un producto para este tipo de aplicación")
            .When(x => x.TipoAplicacion == TipoAplicacionPromocion.ProductosEspecificos);
    }

    private void ConfigurarValidacionesCondicionales()
    {
        // Validación de productos específicos
        RuleFor(x => x.ProductosIds)
            .Must(productos => productos != null && productos.Count > 0 && productos.Count <= 50)
            .WithErrorCode("APLICAR_PROMOCION_PRODUCTOS_LIMITE")
            .WithMessage("Puede especificar entre 1 y 50 productos")
            .When(x => x.ProductosIds != null);

        // Validación de notas
        RuleFor(x => x.NotasAplicacion)
            .MaximumLength(500)
            .WithErrorCode("APLICAR_PROMOCION_NOTAS_LONGITUD")
            .WithMessage("Las notas de aplicación no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.NotasAplicacion));

        // Validación de datos adicionales
        RuleFor(x => x.DatosAdicionales)
            .Must(datos => datos == null || datos.Count <= 10)
            .WithErrorCode("APLICAR_PROMOCION_DATOS_LIMITE")
            .WithMessage("No puede especificar más de 10 elementos en datos adicionales")
            .When(x => x.DatosAdicionales != null);
    }

    private void ConfigurarValidacionesNegocio()
    {
        // TODO: Validación de existencia de promoción cuando la entidad esté disponible
        /*
        RuleFor(x => x.PromocionId)
            .MustAsync(PromocionExiste)
            .WithErrorCode("APLICAR_PROMOCION_NO_EXISTE")
            .WithMessage("La promoción especificada no existe")
            .When(x => x.PromocionId != Guid.Empty);

        RuleFor(x => x.CodigoPromocion)
            .MustAsync(CodigoPromocionExiste)
            .WithErrorCode("APLICAR_PROMOCION_CODIGO_NO_EXISTE")
            .WithMessage("El código de promoción especificado no existe")
            .When(x => !string.IsNullOrEmpty(x.CodigoPromocion));

        RuleFor(x => x.PromocionId)
            .MustAsync(PromocionEstaActiva)
            .WithErrorCode("APLICAR_PROMOCION_INACTIVA")
            .WithMessage("La promoción no está activa")
            .When(x => x.PromocionId != Guid.Empty);

        RuleFor(x => x.PromocionId)
            .MustAsync(PromocionNoHaExcedidoLimites)
            .WithErrorCode("APLICAR_PROMOCION_LIMITE_EXCEDIDO")
            .WithMessage("La promoción ha alcanzado su límite de usos")
            .When(x => x.PromocionId != Guid.Empty);
        */

        // Validaciones de entidades que sí existen
        RuleFor(x => x.FacturaId)
            .MustAsync(FacturaExiste)
            .WithErrorCode("APLICAR_PROMOCION_FACTURA_NO_EXISTE")
            .WithMessage("La factura especificada no existe")
            .When(x => x.FacturaId.HasValue);

        RuleFor(x => x.ComandaId)
            .MustAsync(ComandaExiste)
            .WithErrorCode("APLICAR_PROMOCION_COMANDA_NO_EXISTE")
            .WithMessage("La comanda especificada no existe")
            .When(x => x.ComandaId.HasValue);

        RuleFor(x => x.ClienteId)
            .MustAsync(ClienteExiste)
            .WithErrorCode("APLICAR_PROMOCION_CLIENTE_NO_EXISTE")
            .WithMessage("El cliente especificado no existe")
            .When(x => x.ClienteId.HasValue);

        RuleFor(x => x.ProductosIds)
            .MustAsync(TodosLosProductosExisten)
            .WithErrorCode("APLICAR_PROMOCION_PRODUCTOS_NO_EXISTEN")
            .WithMessage("Uno o más productos especificados no existen")
            .When(x => x.ProductosIds != null && x.ProductosIds.Any());

        // TODO: Validación de autorización cuando la entidad Usuario tenga las propiedades correctas
        /*
        RuleFor(x => x.AutorizadoPor)
            .MustAsync(UsuarioEstaAutorizado)
            .WithErrorCode("APLICAR_PROMOCION_USUARIO_NO_AUTORIZADO")
            .WithMessage("El usuario no está autorizado para aplicar promociones")
            .When(x => x.AutorizadoPor.HasValue);
        */
    }

    #region Métodos de validación privados

    // TODO: Implementar cuando las entidades estén disponibles
    /*
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

        if (promocion == null)
            return false;

        var ahora = DateTime.UtcNow;
        return promocion.Activa && ahora >= promocion.FechaInicio && ahora <= promocion.FechaFin;
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
    */

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

    // TODO: Implementar cuando Usuario tenga las propiedades correctas
    /*
    private async Task<bool> UsuarioEstaAutorizado(Guid? usuarioId, CancellationToken cancellationToken)
    {
        if (!usuarioId.HasValue)
            return true;

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId.Value, cancellationToken);

        if (usuario == null)
            return false;

        // TODO: Cambiar por la propiedad correcta cuando esté disponible
        // return usuario.EstaActivo && (usuario.Rol == "Administrador" || usuario.Rol == "Gerente");
        return true; // Temporal
    }
    */

    #endregion
} 