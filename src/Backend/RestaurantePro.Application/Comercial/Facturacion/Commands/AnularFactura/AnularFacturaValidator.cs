namespace RestaurantePro.Application.Comercial.Facturacion.Commands.AnularFactura;

public class AnularFacturaValidator : AbstractValidator<AnularFacturaCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly string[] _tiposAnulacionValidos = { "Normal", "Emergencia", "Administrativa", "Devolución", "SolicitudCliente", "Programada" };
    private readonly string[] _metodosDevolucionValidos = { "Efectivo", "Tarjeta", "Transferencia", "SaldoFavor" };

    public AnularFacturaValidator(IApplicationDbContext context)
    {
        _context = context;

        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesFactura();
        ConfigurarValidacionesAutorizacion();
        ConfigurarValidacionesDevolucion();
        ConfigurarValidacionesInventario();
        ConfigurarValidacionesFechas();
        ConfigurarValidacionesDocumentacion();
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

        RuleFor(v => v.DescripcionDetallada)
            .MaximumLength(2000)
            .WithMessage("La descripción detallada no puede exceder 2000 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.DescripcionDetallada));

        RuleFor(v => v.TipoAnulacion)
            .NotEmpty()
            .WithMessage("El tipo de anulación es requerido.")
            .Must(tipo => _tiposAnulacionValidos.Contains(tipo, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"El tipo de anulación debe ser uno de: {string.Join(", ", _tiposAnulacionValidos)}.");

        RuleFor(v => v.Prioridad)
            .GreaterThanOrEqualTo(1)
            .WithMessage("La prioridad mínima es 1.")
            .LessThanOrEqualTo(4)
            .WithMessage("La prioridad máxima es 4.");
    }

    private void ConfigurarValidacionesFactura()
    {
        RuleFor(v => v.FacturaId)
            .MustAsync(FacturaEstaEnEstadoAnulable)
            .WithMessage("La factura no está en un estado que permita anulación.")
            .MustAsync(FacturaNoEstaVencida)
            .WithMessage("No se puede anular una factura vencida sin autorización especial.")
            .MustAsync(FacturaNotieneMovimientosPosterior)
            .WithMessage("La factura tiene movimientos posteriores que impiden su anulación.");

        RuleFor(v => v)
            .MustAsync(ValidarPlazoAnulacion)
            .WithMessage("Ha excedido el plazo permitido para anular esta factura.")
            .WithName("PlazoAnulacion");
    }

    private void ConfigurarValidacionesAutorizacion()
    {
        RuleFor(v => v.UsuarioAutorizaId)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del usuario que autoriza es requerido.")
            .MustAsync(UsuarioAutorizadorExiste)
            .WithMessage("El usuario autorizador especificado no existe.")
            .MustAsync(UsuarioTienePermisosParaAnular)
            .WithMessage("El usuario no tiene permisos para anular facturas.");

        // Para anulaciones de emergencia es obligatorio el código
        RuleFor(v => v.CodigoAutorizacion)
            .NotEmpty()
            .WithMessage("El código de autorización es obligatorio para anulaciones de emergencia.")
            .MinimumLength(8)
            .WithMessage("El código de autorización debe tener al menos 8 caracteres.")
            .When(v => v.TipoAnulacion.Equals("Emergencia", StringComparison.OrdinalIgnoreCase));

        // Validar gerente aprobador cuando se requiere
        RuleFor(v => v.GerenteAprobadorId)
            .NotEqual(Guid.Empty)
            .WithMessage("Se requiere ID del gerente aprobador.")
            .MustAsync(GerenteEsValido)
            .WithMessage("El gerente especificado no es válido o no tiene permisos.")
            .When(v => v.RequiereAprobacionGerencia);

        RuleFor(v => v)
            .MustAsync(ValidarAutorizacionSegunMonto)
            .WithMessage("El monto de la factura requiere autorización de nivel superior.")
            .WithName("AutorizacionSegunMonto");
    }

    private void ConfigurarValidacionesDevolucion()
    {
        RuleFor(v => v.MetodoDevolucion)
            .NotEmpty()
            .WithMessage("El método de devolución es requerido cuando se procesa devolución.")
            .Must(metodo => _metodosDevolucionValidos.Contains(metodo, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"El método de devolución debe ser uno de: {string.Join(", ", _metodosDevolucionValidos)}.")
            .When(v => v.ProcesarDevolucionPago);

        RuleFor(v => v.FechaLimiteDevolucion)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("La fecha límite de devolución debe ser futura.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(90))
            .WithMessage("La fecha límite de devolución no puede ser más de 90 días.")
            .When(v => v.FechaLimiteDevolucion.HasValue);

        RuleFor(v => v.ReferenciaDevolucion)
            .MaximumLength(100)
            .WithMessage("La referencia de devolución no puede exceder 100 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.ReferenciaDevolucion));

        RuleFor(v => v)
            .MustAsync(ValidarCapacidadDevolucion)
            .WithMessage("No es posible procesar la devolución para esta factura.")
            .When(v => v.ProcesarDevolucionPago)
            .WithName("CapacidadDevolucion");
    }

    private void ConfigurarValidacionesInventario()
    {
        RuleFor(v => v)
            .MustAsync(ValidarRevertirInventarioPosible)
            .WithMessage("No es posible revertir el inventario debido a movimientos posteriores.")
            .When(v => v.RevertirInventario)
            .WithName("RevertirInventario");
    }

    private void ConfigurarValidacionesFechas()
    {
        RuleFor(v => v.FechaProgramadaAnulacion)
            .GreaterThan(DateTime.UtcNow.AddMinutes(5))
            .WithMessage("La fecha programada debe ser al menos 5 minutos en el futuro.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(30))
            .WithMessage("La fecha programada no puede ser más de 30 días en el futuro.")
            .When(v => v.FechaProgramadaAnulacion.HasValue);

        // Para anulaciones programadas es obligatoria la fecha
        RuleFor(v => v.FechaProgramadaAnulacion)
            .NotNull()
            .WithMessage("La fecha programada es obligatoria para anulaciones programadas.")
            .When(v => v.TipoAnulacion.Equals("Programada", StringComparison.OrdinalIgnoreCase));
    }

    private void ConfigurarValidacionesDocumentacion()
    {
        RuleFor(v => v.ObservacionesAdicionales)
            .MaximumLength(2000)
            .WithMessage("Las observaciones adicionales no pueden exceder 2000 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.ObservacionesAdicionales));

        RuleFor(v => v.DocumentosAdjuntos)
            .Must(docs => docs.Count <= 10)
            .WithMessage("No se pueden adjuntar más de 10 documentos.")
            .Must(docs => docs.All(doc => !string.IsNullOrWhiteSpace(doc)))
            .WithMessage("Todos los documentos adjuntos deben tener contenido válido.")
            .When(v => v.DocumentosAdjuntos.Any());

        // Para anulaciones administrativas se requieren observaciones
        RuleFor(v => v.ObservacionesAdicionales)
            .NotEmpty()
            .WithMessage("Las observaciones son obligatorias para anulaciones administrativas.")
            .When(v => v.TipoAnulacion.Equals("Administrativa", StringComparison.OrdinalIgnoreCase));
    }

    private void ConfigurarValidacionesNegocio()
    {
        RuleFor(v => v)
            .MustAsync(ValidarImpactoFidelizacion)
            .WithMessage("La anulación afectará puntos de fidelización que ya fueron utilizados.")
            .When(v => v.CancelarPuntosFidelizacion)
            .WithName("ImpactoFidelizacion");

        RuleFor(v => v)
            .MustAsync(ValidarImpactoContable)
            .WithMessage("La anulación tiene impactos contables que requieren aprobación adicional.")
            .WithName("ImpactoContable");

        RuleFor(v => v)
            .MustAsync(ValidarEstadoComandas)
            .WithMessage("Existen comandas asociadas que impiden la anulación.")
            .WithName("EstadoComandas");
    }

    // Métodos de validación personalizados
    private async Task<bool> FacturaExiste(Guid facturaId, CancellationToken cancellationToken)
    {
        return await _context.Facturas
            .AnyAsync(f => f.Id == facturaId, cancellationToken);
    }

    private async Task<bool> FacturaEstaEnEstadoAnulable(Guid facturaId, CancellationToken cancellationToken)
    {
        var factura = await _context.Facturas
            .FirstOrDefaultAsync(f => f.Id == facturaId, cancellationToken);

        if (factura == null) return false;

        var estadosAnulables = new[] { "Borrador", "Pendiente", "Emitida", "Pagada" };
        return estadosAnulables.Contains(factura.Estado.ToString(), StringComparer.OrdinalIgnoreCase) &&
               factura.Estado.ToString() != "Anulada";
    }

    private async Task<bool> FacturaNoEstaVencida(Guid facturaId, CancellationToken cancellationToken)
    {
        var factura = await _context.Facturas
            .FirstOrDefaultAsync(f => f.Id == facturaId, cancellationToken);

        if (factura?.FechaVencimiento == null) return true;

        // Permitir anular facturas vencidas solo si tienen menos de 30 días
        return factura.FechaVencimiento.Value.AddDays(30) > DateTime.UtcNow;
    }

    private async Task<bool> FacturaNotieneMovimientosPosterior(Guid facturaId, CancellationToken cancellationToken)
    {
        var factura = await _context.Facturas
            .FirstOrDefaultAsync(f => f.Id == facturaId, cancellationToken);

        if (factura == null) return false;

        // Verificar que no tenga pagos posteriores a la fecha de emisión + 1 día
        var fechaLimite = factura.FechaEmision.AddDays(1);
        
        var tieneMovimientosPosterior = await _context.PagosFactura
            .AnyAsync(p => p.FacturaId == facturaId && p.FechaPago > fechaLimite, cancellationToken);

        return !tieneMovimientosPosterior;
    }

    private async Task<bool> ValidarPlazoAnulacion(AnularFacturaCommand command, CancellationToken cancellationToken)
    {
        var factura = await _context.Facturas
            .FirstOrDefaultAsync(f => f.Id == command.FacturaId, cancellationToken);

        if (factura == null) return false;

        // Diferentes plazos según el tipo de anulación
        var diasPermitidos = command.TipoAnulacion.ToLower() switch
        {
            "emergencia" => 7,
            "administrativa" => 30,
            "solicitudcliente" => 3,
            _ => 1
        };

        return factura.FechaEmision.AddDays(diasPermitidos) >= DateTime.UtcNow;
    }

    private async Task<bool> UsuarioAutorizadorExiste(Guid usuarioId, CancellationToken cancellationToken)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Id == usuarioId && u.Activo, cancellationToken);
    }

    private async Task<bool> UsuarioTienePermisosParaAnular(AnularFacturaCommand command, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioAutorizaId, cancellationToken);

        if (usuario == null) return false;

        // Determinar nivel requerido según tipo de anulación
        var nivelRequerido = command.TipoAnulacion.ToLower() switch
        {
            "emergencia" => 8,
            "administrativa" => 7,
            "devolución" => 6,
            "solicitudcliente" => 5,
            _ => 4
        };

        return usuario.NivelAcceso >= nivelRequerido || 
               usuario.Permisos?.Contains("AnularFacturas") == true ||
               usuario.Rol == "Administrador" ||
               usuario.Rol == "Gerente";
    }

    private async Task<bool> GerenteEsValido(Guid? gerenteId, CancellationToken cancellationToken)
    {
        if (!gerenteId.HasValue) return false;

        var gerente = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == gerenteId.Value, cancellationToken);

        return gerente?.Rol == "Gerente" || gerente?.Rol == "Administrador";
    }

    private async Task<bool> ValidarAutorizacionSegunMonto(AnularFacturaCommand command, CancellationToken cancellationToken)
    {
        var factura = await _context.Facturas
            .FirstOrDefaultAsync(f => f.Id == command.FacturaId, cancellationToken);

        if (factura == null) return false;

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioAutorizaId, cancellationToken);

        if (usuario == null) return false;

        // Niveles de autorización según monto
        if (factura.Total > 10000) return usuario.Rol == "Administrador";
        if (factura.Total > 5000) return usuario.NivelAcceso >= 8;
        if (factura.Total > 2000) return usuario.NivelAcceso >= 6;
        
        return true;
    }

    private async Task<bool> ValidarCapacidadDevolucion(AnularFacturaCommand command, CancellationToken cancellationToken)
    {
        var factura = await _context.Facturas
            .Include(f => f.Pagos)
            .FirstOrDefaultAsync(f => f.Id == command.FacturaId, cancellationToken);

        if (factura == null) return false;

        // Verificar que tenga pagos para poder devolver
        return factura.TotalPagado > 0;
    }

    private async Task<bool> ValidarRevertirInventarioPosible(AnularFacturaCommand command, CancellationToken cancellationToken)
    {
        var factura = await _context.Facturas
            .Include(f => f.Detalles)
            .FirstOrDefaultAsync(f => f.Id == command.FacturaId, cancellationToken);

        if (factura == null) return false;

        // Verificar que no haya ventas posteriores de los mismos productos que agoten el stock
        foreach (var detalle in factura.Detalles)
        {
            var stockActual = await _context.Ingredientes
                .Where(i => i.ProductoId == detalle.ProductoId)
                .SumAsync(i => i.CantidadDisponible, cancellationToken);

            if (stockActual < detalle.Cantidad)
            {
                return false; // No hay suficiente stock para revertir
            }
        }

        return true;
    }

    private async Task<bool> ValidarImpactoFidelizacion(AnularFacturaCommand command, CancellationToken cancellationToken)
    {
        var factura = await _context.Facturas
            .FirstOrDefaultAsync(f => f.Id == command.FacturaId, cancellationToken);

        if (factura?.ClienteId == null) return true;

        // Verificar si el cliente ya usó puntos otorgados por esta factura
        var puntosOtorgados = await _context.MovimientosPuntos
            .Where(m => m.FacturaId == command.FacturaId && m.Tipo == "Credito")
            .SumAsync(m => m.Puntos, cancellationToken);

        var puntosUsados = await _context.MovimientosPuntos
            .Where(m => m.ClienteId == factura.ClienteId && 
                       m.FechaMovimiento > factura.FechaEmision &&
                       m.Tipo == "Debito")
            .SumAsync(m => m.Puntos, cancellationToken);

        return puntosUsados <= puntosOtorgados;
    }

    private async Task<bool> ValidarImpactoContable(AnularFacturaCommand command, CancellationToken cancellationToken)
    {
        var factura = await _context.Facturas
            .FirstOrDefaultAsync(f => f.Id == command.FacturaId, cancellationToken);

        if (factura == null) return false;

        // Verificar si la factura está en un período contable cerrado
        var periodoContable = await _context.PeriodosContables
            .FirstOrDefaultAsync(p => p.FechaInicio <= factura.FechaEmision && 
                                    p.FechaFin >= factura.FechaEmision &&
                                    p.Estado == "Cerrado", cancellationToken);

        return periodoContable == null;
    }

    private async Task<bool> ValidarEstadoComandas(AnularFacturaCommand command, CancellationToken cancellationToken)
    {
        var factura = await _context.Facturas
            .FirstOrDefaultAsync(f => f.Id == command.FacturaId, cancellationToken);

        if (factura == null) return false;

        // Verificar que las comandas asociadas estén en estado que permita anulación
        var comandasProblematicas = await _context.Comandas
            .Where(c => factura.ComandasIds.Contains(c.Id) && 
                       c.Estado != "Completada" && 
                       c.Estado != "Cancelada")
            .CountAsync(cancellationToken);

        return comandasProblematicas == 0;
    }
} 