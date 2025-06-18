namespace RestaurantePro.Application.Operaciones.Reportes.Commands.GenerarReporte;

/// <summary>
/// Validador para GenerarReporteCommand
/// Valida parámetros de generación de reportes, fechas, formatos y permisos
/// </summary>
public class GenerarReporteValidator : AbstractValidator<GenerarReporteCommand>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IComandaRepository _comandaRepository;
    private readonly IMovimientoInventarioRepository _movimientoInventarioRepository;
    private readonly IOrdenCompraRepository _ordenCompraRepository;

    private static readonly TipoReporte[] _tiposReporteValidos = 
    {
        TipoReporte.VentasDiarias,
        TipoReporte.VentasSemanales,
        TipoReporte.VentasMensuales,
        TipoReporte.Inventario,
        TipoReporte.Financiero,
        TipoReporte.ClientesFidelizacion,
        TipoReporte.RendimientoMeseros,
        TipoReporte.EficienciaMesas,
        TipoReporte.ProductosMasVendidos,
        TipoReporte.AnalisisCostos,
        TipoReporte.Personalizado
    };

    private static readonly FormatoReporte[] _formatosValidos = 
    {
        FormatoReporte.PDF,
        FormatoReporte.Excel,
        FormatoReporte.CSV,
        FormatoReporte.JSON,
        FormatoReporte.HTML
    };

    public GenerarReporteValidator(
        IUsuarioRepository usuarioRepository,
        IComandaRepository comandaRepository,
        IMovimientoInventarioRepository movimientoInventarioRepository,
        IOrdenCompraRepository ordenCompraRepository)
    {
        _usuarioRepository = usuarioRepository;
        _comandaRepository = comandaRepository;
        _movimientoInventarioRepository = movimientoInventarioRepository;
        _ordenCompraRepository = ordenCompraRepository;

        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesFechas();
        ConfigurarValidacionesFormato();
        ConfigurarValidacionesUsuario();
        ConfigurarValidacionesEmail();
        ConfigurarValidacionesNegocio();
        ConfigurarValidacionesPermisos();
    }

    /// <summary>
    /// Configura validaciones básicas del command
    /// </summary>
    private void ConfigurarValidacionesBasicas()
    {
        RuleFor(x => x.TipoReporte)
            .Must(tipo => _tiposReporteValidos.Contains(tipo))
            .WithMessage($"El tipo de reporte debe ser uno de: {string.Join(", ", _tiposReporteValidos)}.");

        RuleFor(x => x.Formato)
            .Must(formato => _formatosValidos.Contains(formato))
            .WithMessage($"El formato debe ser uno de: {string.Join(", ", _formatosValidos)}.");

        RuleFor(x => x.UsuarioSolicitanteId)
            .NotEqual(Guid.Empty)
            .WithMessage("El usuario solicitante es requerido.");

        RuleFor(x => x.UsuarioSolicitanteId)
            .MustAsync(UsuarioExiste)
            .WithMessage("El usuario especificado no existe.")
            .When(x => x.UsuarioSolicitanteId != Guid.Empty);

        RuleFor(x => x.NombrePersonalizado)
            .MaximumLength(200)
            .WithMessage("El nombre personalizado no puede exceder 200 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.NombrePersonalizado));
    }

    /// <summary>
    /// Configura validaciones de fechas
    /// </summary>
    private void ConfigurarValidacionesFechas()
    {
        RuleFor(x => x.FechaInicio)
            .NotEmpty()
            .WithMessage("La fecha de inicio es requerida.")
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("La fecha de inicio no puede ser futura.")
            .GreaterThanOrEqualTo(DateTime.Today.AddYears(-5))
            .WithMessage("La fecha de inicio no puede ser anterior a 5 años.");

        RuleFor(x => x.FechaFin)
            .NotEmpty()
            .WithMessage("La fecha de fin es requerida.")
            .GreaterThan(x => x.FechaInicio)
            .WithMessage("La fecha de fin debe ser posterior a la fecha de inicio.")
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("La fecha de fin no puede ser futura.");

        // Validar rango de fechas no mayor a 1 año
        RuleFor(x => x)
            .Must(command => (command.FechaFin - command.FechaInicio).TotalDays <= 365)
            .WithMessage("El rango de fechas no puede ser mayor a un año.")
            .WithName("FechaFin");
    }

    /// <summary>
    /// Configura validaciones de formato y contenido
    /// </summary>
    private void ConfigurarValidacionesFormato()
    {
        RuleFor(x => x.FiltrosEspecificos)
            .Must(filtros => filtros == null || filtros.Count <= 10)
            .WithMessage("No se pueden especificar más de 10 filtros específicos.");

        // Validar GUIDs válidos en FiltrosEspecificos por índice
        RuleForEach(x => x.FiltrosEspecificos)
            .Must(id => id != Guid.Empty)
            .WithMessage("Los filtros específicos no pueden estar vacíos.");

        RuleFor(x => x.ParametrosAdicionales)
            .Must(parametros => parametros == null || parametros.Count <= 20)
            .WithMessage("No se pueden especificar más de 20 parámetros adicionales.");

        // Validar que al menos una opción de contenido esté habilitada
        RuleFor(x => x)
            .Must(command => command.IncluirGraficos || command.IncluirDetalles || command.IncluirResumenEjecutivo)
            .WithMessage("Debe incluir al menos un tipo de contenido en el reporte.")
            .WithName("ContenidoIncluido");
    }

    /// <summary>
    /// Configura validaciones de usuario
    /// </summary>
    private void ConfigurarValidacionesUsuario()
    {
        RuleFor(x => x.UsuarioSolicitanteId)
            .MustAsync(UsuarioTienePermisosReportes)
            .WithMessage("El usuario no tiene permisos para generar reportes.")
            .When(x => x.UsuarioSolicitanteId != Guid.Empty);

        RuleFor(x => x)
            .MustAsync(UsuarioTienePermisosParaTipoReporte)
            .WithMessage("El usuario no tiene permisos para generar este tipo de reporte.")
            .When(x => x.UsuarioSolicitanteId != Guid.Empty)
            .WithName("PermisosEspecificos");
    }

    /// <summary>
    /// Configura validaciones de email
    /// </summary>
    private void ConfigurarValidacionesEmail()
    {
        RuleFor(x => x.EmailDestino)
            .NotEmpty()
            .WithMessage("El email de destino es requerido cuando se solicita envío por email.")
            .EmailAddress()
            .WithMessage("El formato del email de destino no es válido.")
            .MaximumLength(254)
            .WithMessage("El email no puede exceder 254 caracteres.")
            .When(x => x.EnviarPorEmail);

        RuleFor(x => x)
            .Must(command => !command.EnviarPorEmail || (command.FechaFin - command.FechaInicio).TotalDays <= 30)
            .WithMessage("El envío por email está limitado a reportes con rango máximo de 30 días.")
            .WithName("EnviarPorEmail");
    }

    /// <summary>
    /// Configura validaciones de negocio específicas
    /// </summary>
    private void ConfigurarValidacionesNegocio()
    {
        // Reportes financieros requieren prioridad alta
        RuleFor(x => x.Prioridad)
            .Equal(NivelPrioridad.Alta)
            .WithMessage("Los reportes financieros requieren prioridad alta.")
            .When(x => x.TipoReporte == TipoReporte.Financiero);

        // Reportes personalizados requieren nombre
        RuleFor(x => x.NombrePersonalizado)
            .NotEmpty()
            .WithMessage("El nombre personalizado es requerido para reportes personalizados.")
            .When(x => x.TipoReporte == TipoReporte.Personalizado);

        // Debe tener al menos un tipo de contenido incluido
        RuleFor(x => x)
            .Must(x => x.IncluirGraficos || x.IncluirDetalles || x.IncluirResumenEjecutivo)
            .WithMessage("Debe incluir al menos un tipo de contenido (gráficos, detalles o resumen).")
            .WithName("ContenidoIncluido");

        // Validar que existan datos para el período solicitado
        RuleFor(x => x)
            .MustAsync(PeriodoTieneDatos)
            .WithMessage("El período seleccionado no tiene datos suficientes para generar el reporte.")
            .WithName("DatosDisponibles");

        // Limitar reportes concurrentes por usuario
        RuleFor(x => x.UsuarioSolicitanteId)
            .MustAsync(UsuarioNoTieneMuchasGeneracionesPendientes)
            .WithMessage("El usuario tiene demasiadas generaciones de reportes pendientes.")
            .When(x => x.UsuarioSolicitanteId != Guid.Empty);
    }

    /// <summary>
    /// Configura validaciones de permisos específicos
    /// </summary>
    private void ConfigurarValidacionesPermisos()
    {
        // Reportes de inventario requieren permisos específicos
        RuleFor(x => x.UsuarioSolicitanteId)
            .MustAsync(UsuarioTienePermisosInventario)
            .WithMessage("El usuario no tiene permisos para generar reportes de inventario.")
            .When(x => x.TipoReporte == TipoReporte.Inventario);

        // Reportes financieros requieren permisos especiales
        RuleFor(x => x.UsuarioSolicitanteId)
            .MustAsync(UsuarioTienePermisosFinancieros)
            .WithMessage("El usuario no tiene permisos para generar reportes financieros.")
            .When(x => x.TipoReporte == TipoReporte.Financiero);
    }

    // Métodos de validación personalizados
    private async Task<bool> UsuarioExiste(Guid usuarioId, CancellationToken cancellationToken)
    {
        return await _usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken) != null;
    }

    private async Task<bool> UsuarioTienePermisosReportes(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
        if (usuario == null) return false;

        return usuario.Roles.Any(r => r == RolUsuario.Administrador || r == RolUsuario.Gerente);
    }
    
    private async Task<bool> UsuarioTienePermisosParaTipoReporte(GenerarReporteCommand command, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(command.UsuarioSolicitanteId, cancellationToken);
        if (usuario == null) return false;

        return command.TipoReporte switch
        {
            TipoReporte.Financiero => usuario.Roles.Any(r => r == RolUsuario.Administrador || r == RolUsuario.Gerente),
            TipoReporte.Inventario => usuario.Roles.Any(r => r == RolUsuario.Administrador || r == RolUsuario.Gerente || r == RolUsuario.EncargadoInventario),
            _ => true
        };
    }

    private async Task<bool> UsuarioTienePermisosInventario(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
        if (usuario == null) return false;
        
        return usuario.Roles.Any(r => r == RolUsuario.Administrador || r == RolUsuario.Gerente || r == RolUsuario.EncargadoInventario);
    }

    private async Task<bool> UsuarioTienePermisosFinancieros(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
        if (usuario == null) return false;
        
        return usuario.Roles.Any(r => r == RolUsuario.Administrador || r == RolUsuario.Gerente);
    }

    private async Task<bool> PeriodoTieneDatos(GenerarReporteCommand command, CancellationToken cancellationToken)
    {
        return command.TipoReporte switch
        {
            TipoReporte.VentasDiarias or TipoReporte.VentasSemanales or TipoReporte.VentasMensuales or TipoReporte.RendimientoMeseros or TipoReporte.EficienciaMesas or TipoReporte.ProductosMasVendidos =>
                (await _comandaRepository.ObtenerPorRangoFechasAsync(command.FechaInicio, command.FechaFin, false, cancellationToken)).Any(),

            TipoReporte.Inventario or TipoReporte.AnalisisCostos =>
                (await _movimientoInventarioRepository.ObtenerPorRangoFechasAsync(command.FechaInicio, command.FechaFin, cancellationToken)).Any() ||
                (await _ordenCompraRepository.ObtenerPorRangoFechasAsync(command.FechaInicio, command.FechaFin, cancellationToken)).Any(),
            
            _ => true
        };
    }

    private async Task<bool> UsuarioNoTieneMuchasGeneracionesPendientes(Guid usuarioId, CancellationToken cancellationToken)
    {
        // Lógica para verificar generaciones pendientes (ej. en un servicio de caché o BD)
        // Por ahora, retornamos true para no bloquear.
        await Task.Delay(10, cancellationToken); // Simular I/O
        return true;
    }
} 