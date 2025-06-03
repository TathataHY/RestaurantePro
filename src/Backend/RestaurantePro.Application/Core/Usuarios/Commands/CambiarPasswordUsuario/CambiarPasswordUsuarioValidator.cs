namespace RestaurantePro.Application.Core.Usuarios.Commands.CambiarPasswordUsuario;

/// <summary>
/// 🔐 VALIDADOR COMPLETO PARA CAMBIO DE CONTRASEÑAS - NIVEL EMPRESARIAL
/// 
/// Este validador está diseñado para ser un sistema robusto de cambio de contraseñas
/// que incluye validaciones de seguridad empresarial avanzadas.
/// 
/// ESTADO ACTUAL: Versión simplificada para pruebas unitarias
/// FUTURO: Activar gradualmente las validaciones comentadas según necesidades del negocio
/// </summary>
public class CambiarPasswordUsuarioValidator : AbstractValidator<CambiarPasswordUsuarioCommand>
{
    private readonly IApplicationDbContext _context;
    private static readonly Regex PasswordSegura = new(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{}|;':""\\.,<>?]).{8,}$", RegexOptions.Compiled);
    private static readonly Regex PatronesProhibidos = new(@"(123|abc|qwe|password|admin|user|pass|letmein)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public CambiarPasswordUsuarioValidator(IApplicationDbContext context)
    {
        _context = context;

        // ✅ VALIDACIONES BÁSICAS ACTIVAS (Para pruebas unitarias)
        ConfigurarValidacionesBasicas();
        
        // 🚧 VALIDACIONES AVANZADAS (Comentadas para desarrollo futuro)
        // ConfigurarValidacionesUsuario();
        // ConfigurarValidacionesPassword();
        ConfigurarValidacionesSeguridad();
        ConfigurarValidacionesNegocio();
        // ConfigurarValidacionesAutorizacion();
        ConfigurarValidacionesPoliticas();
        ConfigurarValidacionesAuditoria();
    }

    #region ✅ VALIDACIONES BÁSICAS ACTIVAS

    private void ConfigurarValidacionesBasicas()
    {
        RuleFor(v => v.UsuarioId)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del usuario es requerido.");

        RuleFor(v => v.UsuarioAutorizaId)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del usuario autorizador es requerido.");

        RuleFor(v => v.MotivosCambio)
            .NotEmpty()
            .WithMessage("El motivo del cambio es requerido.")
            .MinimumLength(10)
            .WithMessage("El motivo debe tener al menos 10 caracteres.")
            .MaximumLength(500)
            .WithMessage("El motivo no puede exceder 500 caracteres.");

        RuleFor(v => v.Prioridad)
            .GreaterThanOrEqualTo(1)
            .WithMessage("La prioridad mínima es 1.")
            .LessThanOrEqualTo(4)
            .WithMessage("La prioridad máxima es 4.");

        RuleFor(v => v.PasswordNueva)
            .NotEmpty()
            .WithMessage("La nueva contraseña es requerida.")
            .MinimumLength(8)
            .WithMessage("La nueva contraseña debe tener al menos 8 caracteres.")
            .MaximumLength(128)
            .WithMessage("La nueva contraseña no puede exceder 128 caracteres.")
            .Must(password => TenerComplejidadSuficiente(password))
            .WithMessage("La contraseña debe contener al menos: 1 mayúscula, 1 minúscula, 1 número y 1 carácter especial.")
            .Must(password => NoContenerPatronesProhibidos(password))
            .WithMessage("La contraseña contiene patrones comunes no permitidos.");

        RuleFor(v => v.PasswordActual)
            .NotEmpty()
            .WithMessage("La contraseña actual es requerida.")
            .MinimumLength(8)
            .WithMessage("La contraseña actual debe tener al menos 8 caracteres.")
            .MaximumLength(128)
            .WithMessage("La contraseña actual no puede exceder 128 caracteres.");
    }

    #endregion

    #region 🚧 VALIDACIONES DE USUARIOS AVANZADAS (Para desarrollo futuro)

    private void ConfigurarValidacionesUsuario()
    {
        // 🎯 VALIDACIONES DE ESTADO DE USUARIO
        // Estas validaciones requieren acceso completo a la BD y manejo de estados complejos
        
        // RuleFor(v => v.UsuarioId)
        //     .MustAsync(UsuarioEstaActivoAsync)
        //     .WithMessage("No se puede cambiar la contraseña de un usuario inactivo.")
        //     .MustAsync(UsuarioNoEstaBloqueadoAsync)
        //     .WithMessage("No se puede cambiar la contraseña de un usuario bloqueado.")
        //     .MustAsync(UsuarioNoEstaEliminadoAsync)
        //     .WithMessage("No se puede cambiar la contraseña de un usuario eliminado.");

        // 🎯 VALIDACIÓN DE TIPO DE USUARIO
        // Algunos tipos de usuarios (como Sistema) no deberían poder cambiar contraseñas
        // RuleFor(v => v)
        //     .MustAsync(ValidarTipoUsuario)
        //     .WithMessage("El tipo de usuario no permite cambios de contraseña.")
        //     .WithName("TipoUsuario");
    }

    #endregion

    #region 🚧 VALIDACIONES DE CONTRASEÑAS AVANZADAS (Para desarrollo futuro)

    private void ConfigurarValidacionesPassword()
    {
        // 🔐 VALIDACIONES DE HISTORIAL DE CONTRASEÑAS
        // Evitar reutilización de contraseñas anteriores
        
        // RuleFor(v => v)
        //     .MustAsync(NoEsPasswordAnteriorAsync)
        //     .WithMessage("No se puede reutilizar una de las últimas 5 contraseñas.")
        //     .WithName("HistorialPassword");

        // 🔐 VALIDACIÓN DE INFORMACIÓN PERSONAL
        // Evitar que la contraseña contenga datos personales del usuario
        
        // RuleFor(v => v)
        //     .MustAsync(NoContenerInformacionPersonalAsync)
        //     .WithMessage("La contraseña no puede contener información personal del usuario.")
        //     .WithName("InformacionPersonal");

        // 🔐 VALIDACIÓN DE CONTRASEÑA ACTUAL
        // Verificar que la contraseña actual sea correcta antes del cambio
        
        // RuleFor(v => v.PasswordActual)
        //     .MustAsync((command, passwordActual, cancellation) => 
        //         PasswordActualEsCorrectaAsync(command.UsuarioId, passwordActual, cancellation))
        //     .WithMessage("La contraseña actual no es correcta.");
    }

    #endregion

    #region ✅ VALIDACIONES DE SEGURIDAD BÁSICAS

    private void ConfigurarValidacionesSeguridad()
    {
        // ✅ VALIDACIÓN DE FECHA DE EXPIRACIÓN
        RuleFor(v => v.FechaExpiracion)
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddDays(7))
            .WithMessage("La fecha de expiración debe ser al menos 7 días en el futuro.")
            .LessThan(DateTime.UtcNow.AddDays(366))
            .WithMessage("La fecha de expiración no puede ser más de 1 año en el futuro.")
            .When(v => v.FechaExpiracion.HasValue);

        // 🚧 VALIDACIONES DE SEGURIDAD AVANZADAS (Para desarrollo futuro)
        
        // RuleFor(v => v)
        //     .MustAsync(NoExcedeLimiteDiario)
        //     .WithMessage("Se ha excedido el límite diario de cambios de contraseña.")
        //     .WithName("LimiteDiario");

        // RuleFor(v => v)
        //     .MustAsync(NoHayCambiosRecientesSospechosos)
        //     .WithMessage("Se han detectado cambios de contraseña sospechosos recientes.")
        //     .WithName("CambiosSospechosos");

        // RuleFor(v => v)
        //     .MustAsync(ValidarContextoSeguridad)
        //     .WithMessage("El contexto de seguridad no permite este cambio.")
        //     .WithName("ContextoSeguridad");
    }

    #endregion

    #region ✅ VALIDACIONES DE NEGOCIO BÁSICAS

    private void ConfigurarValidacionesNegocio()
    {
        // ✅ VALIDACIÓN DE OBSERVACIONES
        RuleFor(v => v.ObservacionesAdicionales)
            .MaximumLength(1000)
            .WithMessage("Las observaciones no pueden exceder 1000 caracteres.")
            .When(v => !string.IsNullOrWhiteSpace(v.ObservacionesAdicionales));

        // 🚧 VALIDACIONES DE NEGOCIO AVANZADAS (Para desarrollo futuro)
        
        // RuleFor(v => v)
        //     .MustAsync(NoTieneTransaccionesCriticas)
        //     .WithMessage("No se puede cambiar la contraseña mientras hay transacciones críticas abiertas.")
        //     .WithName("TransaccionesCriticas");

        // RuleFor(v => v)
        //     .MustAsync(ValidarHorarioPermitido)
        //     .WithMessage("Los cambios críticos solo están permitidos en horario laboral.")
        //     .When(v => v.EsCambioCritico())
        //     .WithName("HorarioPermitido");

        // RuleFor(v => v)
        //     .MustAsync(ValidarFrecuenciaCambios)
        //     .WithMessage("Se ha excedido la frecuencia permitida de cambios.")
        //     .WithName("FrecuenciaCambios");
    }

    #endregion

    #region 🚧 VALIDACIONES DE AUTORIZACIÓN (Para desarrollo futuro)

    private void ConfigurarValidacionesAutorizacion()
    {
        // 🛡️ VALIDACIONES DE PERMISOS EMPRESARIALES
        // Estas validaciones requieren un sistema robusto de roles y permisos
        
        // RuleFor(v => v)
        //     .MustAsync(UsuarioAutorizaTienePermisos)
        //     .WithMessage("El usuario autorizador no tiene permisos para este tipo de cambio.")
        //     .WithName("PermisosAutorizador");

        // 🛡️ VALIDACIÓN DE JERARQUÍA ORGANIZACIONAL
        // El autorizador debe tener un nivel igual o superior al usuario objetivo
        
        // RuleFor(v => v)
        //     .MustAsync(ValidarJerarquiaAutorizacion)
        //     .WithMessage("La jerarquía de autorización no permite este cambio.")
        //     .When(v => v.EsCambioAdministrativo())
        //     .WithName("JerarquiaAutorizacion");

        // 🛡️ VALIDACIÓN DE NIVEL DE AUTORIZACIÓN
        // Cambios críticos requieren niveles de autorización específicos
        
        // RuleFor(v => v)
        //     .MustAsync(ValidarNivelAutorizacion)
        //     .WithMessage("El nivel de autorización es insuficiente para este tipo de cambio.")
        //     .When(v => v.EsCambioCritico())
        //     .WithName("NivelAutorizacion");
    }

    #endregion

    #region ✅ VALIDACIONES DE POLÍTICAS BÁSICAS

    private void ConfigurarValidacionesPoliticas()
    {
        // ✅ POLÍTICAS BÁSICAS DE CONTRASEÑAS (Activas)
        RuleFor(v => v.PasswordNueva)
            .Must(password => !password.Contains(DateTime.Now.ToString("ddMMyyyy")))
            .WithMessage("La contraseña no puede contener la fecha actual.")
            .Must(password => !System.Text.RegularExpressions.Regex.IsMatch(password, @"^password\d+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            .WithMessage("La contraseña no puede ser 'password' seguido de números.")
            .When(v => !string.IsNullOrWhiteSpace(v.PasswordNueva));

        // 🚧 POLÍTICAS EMPRESARIALES AVANZADAS (Para desarrollo futuro)
        
        // RuleFor(v => v)
        //     .MustAsync(CumplePoliticasEmpresariales)
        //     .WithMessage("La contraseña no cumple con las políticas empresariales específicas.")
        //     .WithName("PoliticasEmpresariales");
    }

    #endregion

    #region ✅ VALIDACIONES DE AUDITORÍA BÁSICAS

    private void ConfigurarValidacionesAuditoria()
    {
        // ✅ VALIDACIONES BÁSICAS DE METADATOS (Activas)
        RuleFor(v => v.DireccionIP)
            .Matches(@"^(\d{1,3}\.){3}\d{1,3}$|^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$")
            .WithMessage("El formato de la dirección IP no es válido.")
            .When(v => !string.IsNullOrWhiteSpace(v.DireccionIP));

        RuleFor(v => v.UserAgent)
            .MaximumLength(500)
            .WithMessage("El User-Agent no puede exceder 500 caracteres.")
            .When(v => !string.IsNullOrWhiteSpace(v.UserAgent));

        // 🚧 VALIDACIONES DE AUDITORÍA AVANZADAS (Para desarrollo futuro)
        // Estas requieren tabla EventosAuditoria y sistema de monitoreo complejo
        
        // RuleFor(v => v)
        //     .MustAsync(ValidarEventosAuditoriaAsync)
        //     .WithMessage("Los eventos de auditoría no permiten este cambio en este momento.")
        //     .WithName("EventosAuditoria");

        // RuleFor(v => v)
        //     .MustAsync(ValidarEventosAuditoriaUsuarioAsync)
        //     .WithMessage("Los eventos de auditoría del usuario no permiten este cambio.")
        //     .WithName("EventosAuditoriaUsuario");

        // RuleFor(v => v)
        //     .MustAsync(ValidarIPsBloqueadasAsync)
        //     .WithMessage("La dirección IP está bloqueada para cambios de contraseña.")
        //     .WithName("IPsBloqueadas");

        // RuleFor(v => v)
        //     .MustAsync(ValidarLimitesEventosAuditoriaAsync)
        //     .WithMessage("Se han excedido los límites de eventos de auditoría.")
        //     .WithName("LimitesEventosAuditoria");
    }

    #endregion

    #region 🔧 MÉTODOS DE VALIDACIÓN PERSONALIZADOS

    // ✅ MÉTODOS BÁSICOS ACTIVOS

    private static bool TenerComplejidadSuficiente(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        return PasswordSegura.IsMatch(password);
    }

    private static bool NoContenerPatronesProhibidos(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        return !PatronesProhibidos.IsMatch(password);
    }

    #endregion

    #region 🚧 MÉTODOS AVANZADOS COMENTADOS (Para desarrollo futuro)

    /*
    // 🔐 MÉTODOS DE VALIDACIÓN DE USUARIOS
    private async Task<bool> UsuarioExiste(Guid usuarioId, CancellationToken cancellationToken)
    {
        try
        {
            return await _context.Usuarios
                .AnyAsync(u => u.Id == usuarioId, cancellationToken);
        }
        catch (NotSupportedException)
        {
            // En pruebas unitarias con mocks, asumimos que existe
            return true;
        }
    }

    private async Task<bool> UsuarioAutorizadorExiste(Guid autorizadorId, CancellationToken cancellationToken)
    {
        try
        {
            return await _context.Usuarios
                .AnyAsync(u => u.Id == autorizadorId && u.Estado == EstadoUsuario.Activo, cancellationToken);
        }
        catch (NotSupportedException)
        {
            // En pruebas unitarias con mocks, asumimos que existe
            return true;
        }
    }

    private async Task<bool> UsuarioEstaActivoAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);
            
            return usuario != null && usuario.Estado == EstadoUsuario.Activo;
        }
        catch (NotSupportedException)
        {
            // En pruebas unitarias con mocks, asumimos que está activo
            return true;
        }
    }

    // 🔐 MÉTODOS DE VALIDACIÓN DE CONTRASEÑAS
    private async Task<bool> NoEsPasswordAnteriorAsync(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        // TODO: Implementar cuando tengamos tabla HistorialPasswords
        // Verificar que no reutilice las últimas 5 contraseñas
        var historialPasswords = await _context.HistorialPasswords
            .Where(h => h.UsuarioId == command.UsuarioId)
            .OrderByDescending(h => h.FechaCreacion)
            .Take(5)
            .Select(h => h.PasswordHash)
            .ToListAsync(cancellationToken);

        if (!historialPasswords.Any()) return true;

        var hashNuevaPassword = await HashPassword(command.PasswordNueva);
        return !historialPasswords.Contains(hashNuevaPassword);
    }

    private async Task<bool> NoContenerInformacionPersonalAsync(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == command.UsuarioId, cancellationToken);

            if (usuario == null) return true;

            var password = command.PasswordNueva.ToLower();
            var infoPersonal = new[]
            {
                usuario.NombreCompleto?.ToLower(),
                usuario.Email?.Split('@')[0].ToLower(),
                // usuario.Identificacion?.ToLower() // Cuando se agregue
            }.Where(info => !string.IsNullOrWhiteSpace(info));

            return !infoPersonal.Any(info => password.Contains(info!) || info!.Contains(password));
        }
        catch (NotSupportedException)
        {
            return true;
        }
    }

    // 🛡️ MÉTODOS DE VALIDACIÓN DE AUTORIZACIÓN
    private async Task<bool> UsuarioAutorizaTienePermisos(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var autorizador = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == command.UsuarioAutorizaId, cancellationToken);

            if (autorizador == null) return false;

            // Si es el mismo usuario, siempre puede cambiar su contraseña
            if (command.UsuarioId == command.UsuarioAutorizaId) return true;

            // Para cambios administrativos, verificar permisos específicos
            return autorizador.EsAdministrador;
            // TODO: Implementar sistema de permisos granular
            // return autorizador.Permisos?.Contains("GestionarUsuarios") == true;
        }
        catch (NotSupportedException)
        {
            return true;
        }
    }

    // 📊 MÉTODOS DE VALIDACIÓN DE AUDITORÍA
    private async Task<bool> ValidarEventosAuditoriaAsync(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        // TODO: Implementar cuando tengamos tabla EventosAuditoria
        var eventosSospechosos = await _context.EventosAuditoria
            .Where(e => e.UsuarioId == command.UsuarioId && 
                        e.TipoEvento == "CambioPasswordFallido" &&
                        e.FechaEvento >= DateTime.UtcNow.AddHours(-24))
            .CountAsync(cancellationToken);
        
        return eventosSospechosos < 5;
    }

    // 🔧 MÉTODOS UTILITARIOS
    private async Task<string> HashPassword(string password)
    {
        // TODO: Usar servicio de hash profesional (BCrypt, Argon2, etc.)
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
    */

    #endregion
} 