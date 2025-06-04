namespace RestaurantePro.Application.Comercial.Facturacion.Queries.ObtenerFacturaPorId;

public class ObtenerFacturaPorIdValidator : AbstractValidator<ObtenerFacturaPorIdQuery>
{
    private readonly IApplicationDbContext _context;
    private readonly string[] _formatosValidos = { "Basico", "Resumido", "Completo" };

    public ObtenerFacturaPorIdValidator(IApplicationDbContext context)
    {
        _context = context;

        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesFormato();
        ConfigurarValidacionesPermisos();
        ConfigurarValidacionesSeguridad();
        ConfigurarValidacionesConsistencia();
    }

    private void ConfigurarValidacionesBasicas()
    {
        RuleFor(v => v.FacturaId)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID de la factura es requerido.")
            .WithErrorCode("FACTURA_ID_REQUERIDO")
            .MustAsync(FacturaExiste)
            .WithMessage("La factura especificada no existe.");

        RuleFor(v => v.FormatoRespuesta)
            .NotEmpty()
            .WithMessage("El formato de respuesta es requerido.")
            .Must(formato => _formatosValidos.Contains(formato, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"El formato de respuesta debe ser uno de: {string.Join(", ", _formatosValidos)}.");

        RuleFor(v => v.MotivoConsulta)
            .MaximumLength(200)
            .WithMessage("El motivo de consulta no puede exceder 200 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.MotivoConsulta));
    }

    private void ConfigurarValidacionesFormato()
    {
        // Para formato básico, no debería incluir datos complejos
        RuleFor(v => v)
            .Must(query => !query.IncluirAuditoria && !query.IncluirMetricasRentabilidad && !query.IncluirDocumentosAdjuntos)
            .WithMessage("El formato básico no puede incluir auditoría, métricas de rentabilidad o documentos adjuntos.")
            .When(v => v.FormatoRespuesta.Equals("Basico", StringComparison.OrdinalIgnoreCase))
            .WithName("FormatoBasicoConsistente");

        // Para formato resumido, limitaciones en datos sensibles
        RuleFor(v => v)
            .Must(query => !query.IncluirMetricasRentabilidad || (query.ValidarPermisos && query.UsuarioConsultaId.HasValue))
            .WithMessage("Las métricas de rentabilidad en formato resumido requieren validación de permisos.")
            .When(v => v.FormatoRespuesta.Equals("Resumido", StringComparison.OrdinalIgnoreCase))
            .WithName("FormatoResumidoSeguro");

        // Para auditoría, es obligatorio el usuario
        RuleFor(v => v.UsuarioConsultaId)
            .NotNull()
            .WithMessage("La consulta de auditoría requiere especificar el usuario que consulta.")
            .When(v => v.IncluirAuditoria);

        RuleFor(v => v.MotivoConsulta)
            .NotEmpty()
            .WithMessage("La consulta de auditoría requiere especificar el motivo.")
            .When(v => v.IncluirAuditoria);
    }

    private void ConfigurarValidacionesPermisos()
    {
        RuleFor(v => v.UsuarioConsultaId)
            .NotNull()
            .WithMessage("Se requiere especificar el usuario para validar permisos.")
            .MustAsync(UsuarioExiste)
            .WithMessage("El usuario especificado no existe.")
            .When(v => v.ValidarPermisos);

        RuleFor(v => v)
            .MustAsync(UsuarioTienePermisosParaConsultar)
            .WithMessage("El usuario no tiene permisos para consultar facturas.")
            .When(v => v.ValidarPermisos && v.UsuarioConsultaId.HasValue)
            .WithName("PermisosConsultaBasica");

        RuleFor(v => v)
            .MustAsync(UsuarioTienePermisosParaMetricasRentabilidad)
            .WithMessage("El usuario no tiene permisos para consultar métricas de rentabilidad.")
            .When(v => v.IncluirMetricasRentabilidad && v.ValidarPermisos && v.UsuarioConsultaId.HasValue)
            .WithName("PermisosMetricasRentabilidad");

        RuleFor(v => v)
            .MustAsync(UsuarioTienePermisosParaAuditoria)
            .WithMessage("El usuario no tiene permisos para consultar información de auditoría.")
            .When(v => v.IncluirAuditoria && v.ValidarPermisos && v.UsuarioConsultaId.HasValue)
            .WithName("PermisosAuditoria");

        RuleFor(v => v)
            .MustAsync(UsuarioTienePermisosParaDocumentosAdjuntos)
            .WithMessage("El usuario no tiene permisos para consultar documentos adjuntos.")
            .When(v => v.IncluirDocumentosAdjuntos && v.ValidarPermisos && v.UsuarioConsultaId.HasValue)
            .WithName("PermisosDocumentosAdjuntos");
    }

    private void ConfigurarValidacionesSeguridad()
    {
        RuleFor(v => v)
            .MustAsync(FacturaEsAccesibleParaUsuario)
            .WithMessage("El usuario no tiene acceso a esta factura específica.")
            .When(v => v.ValidarPermisos && v.UsuarioConsultaId.HasValue)
            .WithName("AccesoFacturaEspecifica");

        RuleFor(v => v)
            .MustAsync(FacturaNoEstaRestringida)
            .WithMessage("Esta factura tiene restricciones de acceso.")
            .WithName("FacturaNoRestringida");

        // Validar límites de consultas por usuario
        RuleFor(v => v)
            .MustAsync(ValidarLimitesConsultaUsuario)
            .WithMessage("Ha excedido el límite de consultas permitidas por día.")
            .When(v => v.ValidarPermisos && v.UsuarioConsultaId.HasValue)
            .WithName("LimitesConsultaUsuario");
    }

    private void ConfigurarValidacionesConsistencia()
    {
        // Si incluye información tributaria, debe incluir detalles de productos
        RuleFor(v => v.IncluirDetallesProductos)
            .Equal(true)
            .WithMessage("La información tributaria requiere incluir detalles de productos.")
            .When(v => v.IncluirInformacionTributaria);

        // Si incluye métricas de rentabilidad, debe incluir detalles y descuentos
        RuleFor(v => v)
            .Must(query => query.IncluirDetallesProductos && query.IncluirDescuentos)
            .WithMessage("Las métricas de rentabilidad requieren incluir detalles de productos y descuentos.")
            .When(v => v.IncluirMetricasRentabilidad)
            .WithName("RequisitosMertricasRentabilidad");

        // Validar combinaciones que requieren recursos intensivos
        RuleFor(v => v)
            .Must(query => !(query.IncluirAuditoria && query.IncluirMetricasRentabilidad && query.IncluirDocumentosAdjuntos))
            .WithMessage("No se pueden incluir auditoría, métricas de rentabilidad y documentos adjuntos simultáneamente por limitaciones de rendimiento.")
            .WithName("LimitacionesRendimiento");
    }

    // Métodos de validación personalizados
    private async Task<bool> FacturaExiste(Guid facturaId, CancellationToken cancellationToken)
    {
        return await _context.Facturas
            .AnyAsync(f => f.Id == facturaId, cancellationToken);
    }

    private async Task<bool> UsuarioExiste(Guid? usuarioId, CancellationToken cancellationToken)
    {
        if (!usuarioId.HasValue) return false;

        return await _context.Usuarios
            .AnyAsync(u => u.Id == usuarioId.Value && u.Estado == EstadoUsuario.Activo, cancellationToken);
    }

    private async Task<bool> UsuarioTienePermisosParaConsultar(ObtenerFacturaPorIdQuery query, CancellationToken cancellationToken)
    {
        if (!query.UsuarioConsultaId.HasValue) return false;

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == query.UsuarioConsultaId.Value, cancellationToken);

        if (usuario == null) return false;

        // Usuario dominio tiene Roles (colección de RolUsuario), usando valores reales del enum
        return usuario.Roles.Any(r => r == RolUsuario.Cajero || 
                                     r == RolUsuario.Mesero || 
                                     r == RolUsuario.Gerente || 
                                     r == RolUsuario.Administrador) ||
               usuario.EsAdministrador; // Esta propiedad SÍ existe
    }

    private async Task<bool> UsuarioTienePermisosParaMetricasRentabilidad(ObtenerFacturaPorIdQuery query, CancellationToken cancellationToken)
    {
        if (!query.UsuarioConsultaId.HasValue) return false;

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == query.UsuarioConsultaId.Value, cancellationToken);

        if (usuario == null) return false;

        return usuario.Roles.Any(r => r == RolUsuario.Gerente || r == RolUsuario.Administrador) ||
               usuario.EsAdministrador;
    }

    private async Task<bool> UsuarioTienePermisosParaAuditoria(ObtenerFacturaPorIdQuery query, CancellationToken cancellationToken)
    {
        if (!query.UsuarioConsultaId.HasValue) return false;

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == query.UsuarioConsultaId.Value, cancellationToken);

        if (usuario == null) return false;

        return usuario.Roles.Any(r => r == RolUsuario.Gerente || r == RolUsuario.Administrador) ||
               usuario.EsAdministrador;
    }

    private async Task<bool> UsuarioTienePermisosParaDocumentosAdjuntos(ObtenerFacturaPorIdQuery query, CancellationToken cancellationToken)
    {
        if (!query.UsuarioConsultaId.HasValue) return false;

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == query.UsuarioConsultaId.Value, cancellationToken);

        if (usuario == null) return false;

        return usuario.Roles.Any(r => r == RolUsuario.Cajero || 
                                     r == RolUsuario.Gerente || 
                                     r == RolUsuario.Administrador) ||
               usuario.EsAdministrador;
    }

    private async Task<bool> FacturaEsAccesibleParaUsuario(ObtenerFacturaPorIdQuery query, CancellationToken cancellationToken)
    {
        if (!query.UsuarioConsultaId.HasValue) return true; // Sin validación de permisos

        var factura = await _context.Facturas
            .FirstOrDefaultAsync(f => f.Id == query.FacturaId, cancellationToken);

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == query.UsuarioConsultaId.Value, cancellationToken);

        if (factura == null || usuario == null) return false;

        // Administradores y gerentes tienen acceso completo
        if (usuario.Roles.Any(r => r == RolUsuario.Administrador || r == RolUsuario.Gerente) || usuario.EsAdministrador)
            return true;

        // Cajeros pueden ver facturas de su turno
        if (usuario.Roles.Contains(RolUsuario.Cajero))
        {
            var facturaEnTurno = factura.FechaEmision.Date == DateTime.UtcNow.Date;
            return facturaEnTurno;
        }

        // TODO: Descomentar cuando Factura tenga UsuarioCreaId para validar facturas propias
        // Empleados solo pueden ver facturas que ellos procesaron
        // if (usuario.Roles.Contains(RolUsuario.Mesero))
        // {
        //     return factura.UsuarioCreaId == usuario.Id;
        // }

        // Por defecto permitir acceso
        return true;
    }

    private async Task<bool> FacturaNoEstaRestringida(ObtenerFacturaPorIdQuery query, CancellationToken cancellationToken)
    {
        var factura = await _context.Facturas
            .FirstOrDefaultAsync(f => f.Id == query.FacturaId, cancellationToken);

        if (factura == null) return false;

        // TODO: Descomentar cuando Factura tenga EsConfidencial
        // Verificar si la factura está marcada como confidencial
        // if (factura.EsConfidencial && query.ValidarPermisos)
        // {
        //     if (!query.UsuarioConsultaId.HasValue) return false;
        //
        //     var usuario = await _context.Usuarios
        //         .FirstOrDefaultAsync(u => u.Id == query.UsuarioConsultaId.Value, cancellationToken);
        //
        //     if (usuario == null) return false;
        //
        //     return usuario.Roles.Any(r => r == RolUsuario.Gerente || r == RolUsuario.Administrador) || usuario.EsAdministrador;
        // }

        // TODO: Descomentar cuando PeriodosContables esté disponible en el contexto
        // Verificar si está en período de restricción contable
        // var periodoContable = await _context.PeriodosContables
        //     .FirstOrDefaultAsync(p => p.FechaInicio <= factura.FechaEmision &&
        //                             p.FechaFin >= factura.FechaEmision &&
        //                             p.Estado == "Cerrado", cancellationToken);
        //
        // if (periodoContable != null && query.IncluirAuditoria)
        // {
        //     if (!query.UsuarioConsultaId.HasValue) return false;
        //
        //     var usuario = await _context.Usuarios
        //         .FirstOrDefaultAsync(u => u.Id == query.UsuarioConsultaId.Value, cancellationToken);
        //
        //     // Como no existe Permisos en Usuario, usar roles
        //     return usuario?.Roles.Any(r => r == RolUsuario.Administrador || r == RolUsuario.Gerente) == true || usuario.EsAdministrador;
        // }

        return true;
    }

    private async Task<bool> ValidarLimitesConsultaUsuario(ObtenerFacturaPorIdQuery query, CancellationToken cancellationToken)
    {
        if (!query.UsuarioConsultaId.HasValue) return true;

        // TODO: Descomentar cuando EventosAuditoria esté disponible en el contexto
        // Contar consultas del usuario en las últimas 24 horas
        // var hace24Horas = DateTime.UtcNow.AddDays(-1);
        // var consultasRecientes = await _context.EventosAuditoria
        //     .Where(e => e.UsuarioId == query.UsuarioConsultaId.Value &&
        //                e.TipoEvento == "ConsultaFactura" &&
        //                e.FechaEvento >= hace24Horas)
        //     .CountAsync(cancellationToken);

        // Límites según el rol del usuario
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == query.UsuarioConsultaId.Value, cancellationToken);

        if (usuario == null) return false;

        // TODO: Descomentar cuando se implemente el conteo real de consultas
        // var limiteDiario = ObtenerLimitePorRol(usuario);
        // return consultasRecientes < limiteDiario;

        // Por ahora siempre permitir (temporal)
        return true;
    }

    // TODO: Descomentar cuando se implemente EventosAuditoria
    // private static int ObtenerLimitePorRol(Usuario usuario)
    // {
    //     if (usuario.Roles.Contains(RolUsuario.Administrador)) return 1000;
    //     if (usuario.Roles.Contains(RolUsuario.Gerente)) return 500;
    //     if (usuario.Roles.Contains(RolUsuario.Cajero)) return 200;
    //     if (usuario.Roles.Contains(RolUsuario.Mesero)) return 50;
    //     return 20;
    // }
} 