namespace RestaurantePro.Application.Comercial.Facturacion.Commands.AplicarDescuento;

public class AplicarDescuentoValidator : AbstractValidator<AplicarDescuentoCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly string[] _tiposDescuentoValidos = { "General", "MontoFijo", "Empleado", "Promocional", "Volumen", "ProductosEspecificos", "Categoria", "Cortesia" };
    private readonly string[] _categoriasValidas = { "Comidas", "Bebidas", "Postres", "Entradas", "Especialidades", "Promociones" };

    public AplicarDescuentoValidator(IApplicationDbContext context)
    {
        _context = context;

        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesFactura();
        ConfigurarValidacionesDescuento();
        ConfigurarValidacionesAutorizacion();
        ConfigurarValidacionesProductosYCategorias();
        ConfigurarValidacionesMontos();
        ConfigurarValidacionesFechas();
        ConfigurarValidacionesNegocio();
    }

    private void ConfigurarValidacionesBasicas()
    {
        RuleFor(v => v.FacturaId)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID de la factura es requerido.")
            .MustAsync(FacturaExiste)
            .WithMessage("La factura especificada no existe.");

        RuleFor(v => v.TipoDescuento)
            .NotEmpty()
            .WithMessage("El tipo de descuento es requerido.")
            .Must(tipo => _tiposDescuentoValidos.Contains(tipo, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"El tipo de descuento debe ser uno de: {string.Join(", ", _tiposDescuentoValidos)}.");

        RuleFor(v => v.Concepto)
            .NotEmpty()
            .WithMessage("El concepto del descuento es requerido.")
            .MinimumLength(5)
            .WithMessage("El concepto debe tener al menos 5 caracteres.")
            .MaximumLength(200)
            .WithMessage("El concepto no puede exceder 200 caracteres.");

        RuleFor(v => v.Motivo)
            .NotEmpty()
            .WithMessage("El motivo del descuento es requerido.")
            .MinimumLength(10)
            .WithMessage("El motivo debe tener al menos 10 caracteres.")
            .MaximumLength(500)
            .WithMessage("El motivo no puede exceder 500 caracteres.");
    }

    private void ConfigurarValidacionesFactura()
    {
        RuleFor(v => v.FacturaId)
            .MustAsync(FacturaEstaEnEstadoValido)
            .WithMessage("La factura debe estar en estado Borrador o Pendiente para aplicar descuentos.")
            .MustAsync(FacturaNoEstaAnulada)
            .WithMessage("No se pueden aplicar descuentos a facturas anuladas.");
    }

    private void ConfigurarValidacionesDescuento()
    {
        // Validar que tenga porcentaje O monto fijo, pero no ambos
        RuleFor(v => v)
            .Must(command => (command.Porcentaje > 0 && command.MontoFijo == 0) || 
                           (command.MontoFijo > 0 && command.Porcentaje == 0))
            .WithMessage("Debe especificar un porcentaje o un monto fijo, pero no ambos.")
            .WithName("TipoValorDescuento");

        // Validaciones para porcentaje
        RuleFor(v => v.Porcentaje)
            .GreaterThan(0)
            .WithMessage("El porcentaje de descuento debe ser mayor a 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("El porcentaje de descuento no puede exceder 100%.")
            .When(v => v.Porcentaje > 0);

        // Validaciones para monto fijo
        RuleFor(v => v.MontoFijo)
            .GreaterThan(0)
            .WithMessage("El monto fijo debe ser mayor a 0.")
            .LessThanOrEqualTo(100000)
            .WithMessage("El monto fijo no puede exceder $100,000.")
            .When(v => v.MontoFijo > 0);

        RuleFor(v => v.Prioridad)
            .GreaterThanOrEqualTo(1)
            .WithMessage("La prioridad mínima es 1.")
            .LessThanOrEqualTo(10)
            .WithMessage("La prioridad máxima es 10.");
    }

    private void ConfigurarValidacionesAutorizacion()
    {
        RuleFor(v => v.UsuarioAutorizaId)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del usuario que autoriza es requerido.")
            .MustAsync(UsuarioAutorizadorExiste)
            .WithMessage("El usuario autorizador especificado no existe.")
            .MustAsync(UsuarioTienePermisosParaDescuento)
            .WithMessage("El usuario no tiene permisos para autorizar este tipo de descuento.");

        // Para descuentos de cortesía es obligatorio el código de autorización
        RuleFor(v => v.CodigoAutorizacion)
            .NotEmpty()
            .WithMessage("El código de autorización es obligatorio para descuentos de cortesía.")
            .MinimumLength(6)
            .WithMessage("El código de autorización debe tener al menos 6 caracteres.")
            .When(v => v.TipoDescuento.Equals("Cortesia", StringComparison.OrdinalIgnoreCase));

        // Para descuentos promocionales validar código promocional
        RuleFor(v => v.CodigoAutorizacion)
            .MustAsync(CodigoPromocionalEsValido)
            .WithMessage("El código promocional no es válido o ha expirado.")
            .When(v => v.TipoDescuento.Equals("Promocional", StringComparison.OrdinalIgnoreCase) && 
                      !string.IsNullOrEmpty(v.CodigoAutorizacion));
    }

    private void ConfigurarValidacionesProductosYCategorias()
    {
        RuleFor(v => v.ProductosEspecificos)
            .Must(productos => productos.Count <= 50)
            .WithMessage("No se pueden especificar más de 50 productos.")
            .MustAsync(TodosLosProductosExisten)
            .WithMessage("Uno o más productos especificados no existen.")
            .When(v => v.ProductosEspecificos.Any());

        RuleFor(v => v.CategoriasAplicables)
            .Must(categorias => categorias.Count <= 10)
            .WithMessage("No se pueden especificar más de 10 categorías.")
            .Must(categorias => categorias.All(cat => _categoriasValidas.Contains(cat, StringComparer.OrdinalIgnoreCase)))
            .WithMessage($"Las categorías deben ser válidas: {string.Join(", ", _categoriasValidas)}.")
            .When(v => v.CategoriasAplicables.Any());

        // Si es descuento específico, debe tener productos o categorías
        RuleFor(v => v)
            .Must(command => command.ProductosEspecificos.Any() || command.CategoriasAplicables.Any())
            .WithMessage("Para descuentos específicos debe especificar productos o categorías.")
            .When(v => v.TipoDescuento.Equals("ProductosEspecificos", StringComparison.OrdinalIgnoreCase) ||
                      v.TipoDescuento.Equals("Categoria", StringComparison.OrdinalIgnoreCase))
            .WithName("ProductosOCategoriasRequeridos");
    }

    private void ConfigurarValidacionesMontos()
    {
        RuleFor(v => v.MontoMinimoFactura)
            .GreaterThan(0)
            .WithMessage("El monto mínimo debe ser mayor a 0.")
            .LessThanOrEqualTo(1000000)
            .WithMessage("El monto mínimo no puede exceder $1,000,000.")
            .When(v => v.MontoMinimoFactura.HasValue);

        RuleFor(v => v.MontoMaximoDescuento)
            .GreaterThan(0)
            .WithMessage("El monto máximo de descuento debe ser mayor a 0.")
            .LessThanOrEqualTo(500000)
            .WithMessage("El monto máximo de descuento no puede exceder $500,000.")
            .When(v => v.MontoMaximoDescuento.HasValue);

        // El monto fijo no puede exceder el máximo permitido
        RuleFor(v => v.MontoFijo)
            .LessThanOrEqualTo(v => v.MontoMaximoDescuento ?? decimal.MaxValue)
            .WithMessage("El monto fijo no puede exceder el monto máximo de descuento.")
            .When(v => v.MontoFijo > 0 && v.MontoMaximoDescuento.HasValue);
    }

    private void ConfigurarValidacionesFechas()
    {
        RuleFor(v => v.FechaExpiracion)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("La fecha de expiración debe ser futura.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(2))
            .WithMessage("La fecha de expiración no puede ser más de 2 años en el futuro.")
            .When(v => v.FechaExpiracion.HasValue);

        RuleFor(v => v.NotasAdicionales)
            .MaximumLength(1000)
            .WithMessage("Las notas adicionales no pueden exceder 1000 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.NotasAdicionales));
    }

    private void ConfigurarValidacionesNegocio()
    {
        // Validar que la factura cumpla con el monto mínimo
        RuleFor(v => v)
            .MustAsync(FacturaCumpleMontoMinimo)
            .WithMessage("La factura no cumple con el monto mínimo requerido para este descuento.")
            .When(v => v.MontoMinimoFactura.HasValue)
            .WithName("MontoMinimoFactura");

        // Validar límites por tipo de descuento
        RuleFor(v => v)
            .MustAsync(ValidarLimitesPorTipoDescuento)
            .WithMessage("El descuento excede los límites permitidos para este tipo.")
            .WithName("LimitesTipoDescuento");

        // Validar que no se excedan los límites de descuentos acumulados
        RuleFor(v => v)
            .MustAsync(ValidarDescuentosAcumulados)
            .WithMessage("El descuento haría que se excedan los límites de descuentos acumulados.")
            .WithName("DescuentosAcumulados");

        // Validar autorización según el monto del descuento
        RuleFor(v => v)
            .MustAsync(ValidarAutorizacionSegunMonto)
            .WithMessage("El monto del descuento requiere una autorización de nivel superior.")
            .WithName("AutorizacionSegunMonto");
    }

    // Métodos de validación personalizados
    private async Task<bool> FacturaExiste(Guid facturaId, CancellationToken cancellationToken)
    {
        return await _context.Facturas
            .AnyAsync(f => f.Id == facturaId, cancellationToken);
    }

    private async Task<bool> FacturaEstaEnEstadoValido(Guid facturaId, CancellationToken cancellationToken)
    {
        var factura = await _context.Facturas
            .FirstOrDefaultAsync(f => f.Id == facturaId, cancellationToken);

        if (factura == null) return false;

        var estadosValidos = new[] { "Borrador", "Pendiente" };
        return estadosValidos.Contains(factura.Estado.ToString(), StringComparer.OrdinalIgnoreCase);
    }

    private async Task<bool> FacturaNoEstaAnulada(Guid facturaId, CancellationToken cancellationToken)
    {
        var factura = await _context.Facturas
            .FirstOrDefaultAsync(f => f.Id == facturaId, cancellationToken);

        return factura?.Estado.ToString() != "Anulada";
    }

    private async Task<bool> UsuarioAutorizadorExiste(Guid usuarioId, CancellationToken cancellationToken)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Id == usuarioId && u.Activo, cancellationToken);
    }

    private async Task<bool> UsuarioTienePermisosParaDescuento(AplicarDescuentoCommand command, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioAutorizaId, cancellationToken);

        if (usuario == null) return false;

        // Determinar el nivel de autorización requerido según el tipo y monto
        var nivelRequerido = DeterminarNivelAutorizacionRequerido(command);
        
        return usuario.NivelAcceso >= nivelRequerido || 
               usuario.Permisos?.Contains("AprobarDescuentos") == true ||
               usuario.Rol == "Administrador" ||
               usuario.Rol == "Gerente";
    }

    private async Task<bool> CodigoPromocionalEsValido(AplicarDescuentoCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.CodigoAutorizacion)) return true;

        // Buscar el código promocional en la base de datos
        var promocion = await _context.Promociones
            .FirstOrDefaultAsync(p => p.Codigo == command.CodigoAutorizacion && 
                                    p.Activo && 
                                    p.FechaInicio <= DateTime.UtcNow && 
                                    p.FechaFin >= DateTime.UtcNow, cancellationToken);

        return promocion != null;
    }

    private async Task<bool> TodosLosProductosExisten(List<Guid> productosIds, CancellationToken cancellationToken)
    {
        if (!productosIds.Any()) return true;

        var productosExistentes = await _context.Productos
            .Where(p => productosIds.Contains(p.Id))
            .CountAsync(cancellationToken);

        return productosExistentes == productosIds.Count;
    }

    private async Task<bool> FacturaCumpleMontoMinimo(AplicarDescuentoCommand command, CancellationToken cancellationToken)
    {
        if (!command.MontoMinimoFactura.HasValue) return true;

        var factura = await _context.Facturas
            .FirstOrDefaultAsync(f => f.Id == command.FacturaId, cancellationToken);

        return factura?.Total >= command.MontoMinimoFactura.Value;
    }

    private async Task<bool> ValidarLimitesPorTipoDescuento(AplicarDescuentoCommand command, CancellationToken cancellationToken)
    {
        var limites = command.TipoDescuento.ToLower() switch
        {
            "empleado" => (porcentajeMax: 15m, montoMax: 500m),
            "promocional" => (porcentajeMax: 25m, montoMax: 1000m),
            "volumen" => (porcentajeMax: 20m, montoMax: 2000m),
            "cortesia" => (porcentajeMax: 100m, montoMax: 5000m),
            _ => (porcentajeMax: 30m, montoMax: 1500m)
        };

        if (command.Porcentaje > 0)
        {
            return command.Porcentaje <= limites.porcentajeMax;
        }

        return command.MontoFijo <= limites.montoMax;
    }

    private async Task<bool> ValidarDescuentosAcumulados(AplicarDescuentoCommand command, CancellationToken cancellationToken)
    {
        var factura = await _context.Facturas
            .Include(f => f.Descuentos)
            .FirstOrDefaultAsync(f => f.Id == command.FacturaId, cancellationToken);

        if (factura == null) return false;

        var descuentosActuales = factura.TotalDescuentos;
        var nuevoDescuento = command.MontoFijo > 0 ? command.MontoFijo : 
                           (factura.Subtotal * command.Porcentaje / 100);

        var totalDescuentos = descuentosActuales + nuevoDescuento;
        var porcentajeTotal = (totalDescuentos / factura.Subtotal) * 100;

        // No más del 50% de descuento total
        return porcentajeTotal <= 50m;
    }

    private async Task<bool> ValidarAutorizacionSegunMonto(AplicarDescuentoCommand command, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioAutorizaId, cancellationToken);

        if (usuario == null) return false;

        var montoDescuento = command.MontoFijo > 0 ? command.MontoFijo : 0;
        
        if (command.Porcentaje > 0)
        {
            var factura = await _context.Facturas
                .FirstOrDefaultAsync(f => f.Id == command.FacturaId, cancellationToken);
            
            if (factura != null)
            {
                montoDescuento = factura.Subtotal * (command.Porcentaje / 100);
            }
        }

        // Niveles de autorización según monto
        if (montoDescuento > 5000) return usuario.Rol == "Administrador";
        if (montoDescuento > 2000) return usuario.NivelAcceso >= 7;
        if (montoDescuento > 500) return usuario.NivelAcceso >= 5;
        
        return true;
    }

    private static int DeterminarNivelAutorizacionRequerido(AplicarDescuentoCommand command)
    {
        return command.TipoDescuento.ToLower() switch
        {
            "cortesia" => 7,
            "promocional" => 5,
            "volumen" => 4,
            "empleado" => 3,
            _ => 2
        };
    }
} 