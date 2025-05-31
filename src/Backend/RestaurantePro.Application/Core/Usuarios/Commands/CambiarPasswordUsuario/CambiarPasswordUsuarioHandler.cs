using System.Security.Cryptography;
using System.Text;

namespace RestaurantePro.Application.Core.Usuarios.Commands.CambiarPasswordUsuario;

public class CambiarPasswordUsuarioHandler : IRequestHandler<CambiarPasswordUsuarioCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CambiarPasswordUsuarioHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    public CambiarPasswordUsuarioHandler(
        IApplicationDbContext context,
        ILogger<CambiarPasswordUsuarioHandler> logger,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _notificationService = notificationService;
    }

    public async Task<Result<bool>> Handle(CambiarPasswordUsuarioCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando cambio de contraseña: {UsuarioId}, Tipo: {TipoCambio}, Prioridad: {Prioridad}",
                request.UsuarioId, request.ObtenerTipoCambio(), request.Prioridad);

            // 1. Obtener usuario completo con validaciones
            var usuarioResult = await ObtenerUsuarioCompleto(request.UsuarioId, cancellationToken);
            if (!usuarioResult.Succeeded)
            {
                return Result.Failure<bool>(usuarioResult.Error);
            }

            var usuario = usuarioResult.Value;
            var datosOriginalesPassword = await CrearBackupPassword(usuario);

            // 2. Validar contraseña actual si es necesario
            if (!request.EsCambioAdministrativo() && !request.EsPrimerCambio)
            {
                var validacionPasswordResult = await ValidarPasswordActual(request, usuario);
                if (!validacionPasswordResult.Succeeded)
                {
                    await RegistrarIntentoFallido(request, usuario, "Password actual incorrecta");
                    return Result.Failure<bool>(validacionPasswordResult.Error);
                }
            }

            // 3. Verificar límites de seguridad y política
            var validacionLimitesResult = await ValidarLimitesSeguridad(request, usuario);
            if (!validacionLimitesResult.Succeeded)
            {
                return Result.Failure<bool>(validacionLimitesResult.Error);
            }

            // 4. Generar nueva información de seguridad
            var nuevaInfoSeguridad = await GenerarNuevaInformacionSeguridad(request);

            // 5. Actualizar contraseña del usuario
            await ActualizarPasswordUsuario(usuario, nuevaInfoSeguridad, request);

            // 6. Guardar contraseña anterior en historial
            await GuardarEnHistorialPasswords(usuario, datosOriginalesPassword, request);

            // 7. Invalidar sesiones activas si es necesario
            if (request.InvalidarSesionesActivas)
            {
                await InvalidarSesionesUsuario(usuario.Id, request);
            }

            // 8. Guardar cambios en base de datos
            await _context.SaveChangesAsync(cancellationToken);

            // 9. Registrar auditoría completa
            await RegistrarAuditoriaCompleta(request, usuario, nuevaInfoSeguridad);

            // 10. Procesar notificaciones
            await ProcesarNotificaciones(request, usuario);

            // 11. Programar recordatorios y tareas de seguimiento
            await ProgramarTareasDeSeguridad(request, usuario);

            _logger.LogInformation("Contraseña cambiada exitosamente: Usuario {Email}, Tipo: {TipoCambio}",
                usuario.Email, request.ObtenerTipoCambio());

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar contraseña para usuario {UsuarioId}: {Motivo}",
                request.UsuarioId, request.MotivosCambio);
            return Result.Failure<bool>("Error interno al cambiar la contraseña.");
        }
    }

    private async Task<Result<Usuario>> ObtenerUsuarioCompleto(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Supervisor)
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);

        if (usuario == null)
        {
            return Result.Failure<Usuario>("El usuario especificado no existe.");
        }

        if (!usuario.Activo)
        {
            return Result.Failure<Usuario>("No se puede cambiar la contraseña de un usuario inactivo.");
        }

        if (usuario.FechaEliminacion.HasValue)
        {
            return Result.Failure<Usuario>("No se puede cambiar la contraseña de un usuario eliminado.");
        }

        if (usuario.FechaBloqueado.HasValue)
        {
            return Result.Failure<Usuario>("No se puede cambiar la contraseña de un usuario bloqueado.");
        }

        return Result.Success(usuario);
    }

    private async Task<Dictionary<string, object>> CrearBackupPassword(Usuario usuario)
    {
        return new Dictionary<string, object>
        {
            { "UsuarioId", usuario.Id },
            { "Email", usuario.Email },
            { "PasswordHashAnterior", usuario.PasswordHash ?? "" },
            { "SaltAnterior", usuario.Salt ?? "" },
            { "FechaUltimocambio", usuario.UltimaActualizacionPassword },
            { "FechaExpiracionAnterior", usuario.FechaExpiracionPassword },
            { "DebeResetearAnterior", usuario.DebeResetearPassword },
            { "FechaBackup", DateTime.UtcNow }
        };
    }

    private async Task<Result<bool>> ValidarPasswordActual(CambiarPasswordUsuarioCommand request, Usuario usuario)
    {
        if (string.IsNullOrWhiteSpace(request.PasswordActual))
        {
            return Result.Failure<bool>("La contraseña actual es requerida.");
        }

        var hashPasswordActual = await HashPassword(request.PasswordActual, usuario.Salt ?? "");

        if (hashPasswordActual != usuario.PasswordHash)
        {
            return Result.Failure<bool>("La contraseña actual es incorrecta.");
        }

        return Result.Success(true);
    }

    private async Task<Result<bool>> ValidarLimitesSeguridad(CambiarPasswordUsuarioCommand request, Usuario usuario)
    {
        // Validar límites diarios
        var hoy = DateTime.Today;
        var cambiosHoy = await _context.EventosAuditoria
            .Where(e => e.EntidadId == request.UsuarioId &&
                       e.TipoEvento == "CambioPassword" &&
                       e.FechaEvento.Date == hoy)
            .CountAsync();

        if (cambiosHoy >= 3)
        {
            return Result.Failure<bool>("Se ha excedido el límite diario de cambios de contraseña (3 máximo).");
        }

        // Validar frecuencia por hora
        var ultimaHora = DateTime.UtcNow.AddHours(-1);
        var cambiosUltimaHora = await _context.EventosAuditoria
            .Where(e => e.EntidadId == request.UsuarioId &&
                       e.TipoEvento == "CambioPassword" &&
                       e.FechaEvento >= ultimaHora)
            .CountAsync();

        if (cambiosUltimaHora >= 2)
        {
            return Result.Failure<bool>("Se ha excedido el límite por hora de cambios de contraseña (2 máximo).");
        }

        // Validar que no esté en período de enfriamiento
        var ultimoCambio = await _context.EventosAuditoria
            .Where(e => e.EntidadId == request.UsuarioId &&
                       e.TipoEvento == "CambioPassword")
            .OrderByDescending(e => e.FechaEvento)
            .FirstOrDefaultAsync();

        if (ultimoCambio != null && !request.EsCambioCritico())
        {
            var tiempoDesdeUltimoCambio = DateTime.UtcNow - ultimoCambio.FechaEvento;
            if (tiempoDesdeUltimoCambio.TotalMinutes < 15)
            {
                return Result.Failure<bool>("Debe esperar al menos 15 minutos entre cambios de contraseña.");
            }
        }

        return Result.Success(true);
    }

    private async Task<InformacionSeguridadDto> GenerarNuevaInformacionSeguridad(CambiarPasswordUsuarioCommand request)
    {
        var salt = GenerarSalt();
        var passwordHash = await HashPassword(request.PasswordNueva, salt);
        var tokenSeguridad = GenerarTokenSeguridad();

        return new InformacionSeguridadDto
        {
            PasswordHash = passwordHash,
            Salt = salt,
            TokenSeguridad = tokenSeguridad,
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = request.FechaExpiracion ?? ObtenerFechaExpiracionDefecto(request),
            DebeResetear = request.EsPrimerCambio,
            TipoCambio = request.ObtenerTipoCambio(),
            NivelSeguridad = CalcularNivelSeguridad(request.PasswordNueva),
            AlgoritmoHash = "SHA256",
            IteracionesHash = 10000
        };
    }

    private async Task ActualizarPasswordUsuario(Usuario usuario, InformacionSeguridadDto nuevaInfo, CambiarPasswordUsuarioCommand request)
    {
        usuario.PasswordHash = nuevaInfo.PasswordHash;
        usuario.Salt = nuevaInfo.Salt;
        usuario.UltimaActualizacionPassword = nuevaInfo.FechaCreacion;
        usuario.FechaExpiracionPassword = nuevaInfo.FechaExpiracion;
        usuario.DebeResetearPassword = nuevaInfo.DebeResetear;
        usuario.TokenSeguridadActual = nuevaInfo.TokenSeguridad;
        usuario.CantidadCambiosPassword = (usuario.CantidadCambiosPassword ?? 0) + 1;
        usuario.UltimoTipoCambioPassword = nuevaInfo.TipoCambio;
        usuario.FechaUltimaActualizacion = DateTime.UtcNow;
        usuario.UsuarioUltimaActualizacion = request.UsuarioAutorizaId;

        // Actualizar estadísticas de seguridad
        usuario.NivelSeguridadPassword = nuevaInfo.NivelSeguridad;
        usuario.AlgoritmoHashPassword = nuevaInfo.AlgoritmoHash;

        // Si es primer cambio, marcar como activado
        if (request.EsPrimerCambio)
        {
            usuario.FechaActivacion = DateTime.UtcNow;
            usuario.EstadoActivacion = "Activado";
        }

        _logger.LogInformation("Password actualizada para usuario {Email}: Tipo {TipoCambio}, Nivel seguridad: {NivelSeguridad}",
            usuario.Email, nuevaInfo.TipoCambio, nuevaInfo.NivelSeguridad);
    }

    private async Task GuardarEnHistorialPasswords(Usuario usuario, Dictionary<string, object> datosOriginales, CambiarPasswordUsuarioCommand request)
    {
        var historialPassword = new HistorialPassword
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            PasswordHash = datosOriginales["PasswordHashAnterior"].ToString() ?? "",
            Salt = datosOriginales["SaltAnterior"].ToString() ?? "",
            FechaCreacion = DateTime.UtcNow,
            FechaCambio = DateTime.UtcNow,
            MotivoCambio = request.MotivosCambio,
            TipoCambio = request.ObtenerTipoCambio(),
            UsuarioAutoriza = request.UsuarioAutorizaId,
            DireccionIP = request.DireccionIP ?? "",
            UserAgent = request.UserAgent ?? "",
            EsCambioCritico = request.EsCambioCritico(),
            Prioridad = request.Prioridad,
            ObservacionesAdicionales = request.ObservacionesAdicionales ?? ""
        };

        await _context.HistorialPasswords.AddAsync(historialPassword);

        // Limpiar historial antiguo (mantener solo últimas 10)
        var historialAntiguo = await _context.HistorialPasswords
            .Where(h => h.UsuarioId == usuario.Id)
            .OrderByDescending(h => h.FechaCreacion)
            .Skip(10)
            .ToListAsync();

        if (historialAntiguo.Any())
        {
            _context.HistorialPasswords.RemoveRange(historialAntiguo);
            _logger.LogInformation("Limpiado historial de passwords antiguo para usuario {Email}: {Cantidad} registros eliminados",
                usuario.Email, historialAntiguo.Count);
        }
    }

    private async Task InvalidarSesionesUsuario(Guid usuarioId, CambiarPasswordUsuarioCommand request)
    {
        var sesionesActivas = await _context.SesionesUsuario
            .Where(s => s.UsuarioId == usuarioId && s.Activa)
            .ToListAsync();

        foreach (var sesion in sesionesActivas)
        {
            sesion.Activa = false;
            sesion.FechaInvalidacion = DateTime.UtcNow;
            sesion.MotivoInvalidacion = $"Cambio de contraseña: {request.ObtenerTipoCambio()}";
            sesion.UsuarioInvalida = request.UsuarioAutorizaId;
        }

        _logger.LogInformation("Invalidadas {CantidadSesiones} sesiones activas para usuario {UsuarioId} por cambio de contraseña",
            sesionesActivas.Count, usuarioId);
    }

    private async Task RegistrarAuditoriaCompleta(CambiarPasswordUsuarioCommand request, Usuario usuario, InformacionSeguridadDto nuevaInfo)
    {
        var eventoAuditoria = new EventoAuditoria
        {
            Id = Guid.NewGuid(),
            TipoEvento = "CambioPassword",
            EntidadId = usuario.Id,
            EntidadTipo = "Usuario",
            UsuarioId = request.UsuarioAutorizaId,
            Detalles = $"Cambio de contraseña para usuario {usuario.Email} - Tipo: {request.ObtenerTipoCambio()}",
            FechaEvento = DateTime.UtcNow,
            DatosAdicionales = new Dictionary<string, object>
            {
                { "UsuarioAfectado", usuario.Email },
                { "TipoCambio", request.ObtenerTipoCambio() },
                { "MotivosCambio", request.MotivosCambio },
                { "EsCambioCritico", request.EsCambioCritico() },
                { "EsCambioForzado", request.EsCambioForzado },
                { "EsPrimerCambio", request.EsPrimerCambio },
                { "Prioridad", request.Prioridad },
                { "InvalidarSesiones", request.InvalidarSesionesActivas },
                { "DireccionIP", request.DireccionIP ?? "N/A" },
                { "UserAgent", request.UserAgent ?? "N/A" },
                { "FechaExpiracion", nuevaInfo.FechaExpiracion },
                { "NivelSeguridad", nuevaInfo.NivelSeguridad },
                { "ObservacionesAdicionales", request.ObservacionesAdicionales ?? "N/A" },
                { "CantidadCambiosTotal", usuario.CantidadCambiosPassword },
                { "TokenSeguridadGenerado", nuevaInfo.TokenSeguridad },
                { "AlgoritmoHash", nuevaInfo.AlgoritmoHash }
            }
        };

        await _context.EventosAuditoria.AddAsync(eventoAuditoria);

        _logger.LogInformation("Auditoría registrada para cambio de contraseña: Usuario {Email}, Tipo {TipoCambio}",
            usuario.Email, request.ObtenerTipoCambio());
    }

    private async Task ProcesarNotificaciones(CambiarPasswordUsuarioCommand request, Usuario usuario)
    {
        try
        {
            // Notificar al usuario
            if (request.NotificarPorEmail && !string.IsNullOrEmpty(usuario.Email))
            {
                await NotificarUsuarioCambioPassword(request, usuario);
            }

            // Notificar al supervisor para cambios críticos
            if (request.EsCambioCritico() && usuario.SupervisorId.HasValue)
            {
                await NotificarSupervisorCambioPassword(request, usuario);
            }

            // Notificar a administración para cambios de emergencia
            if (request.Prioridad == 4 || request.ObtenerTipoCambio().Contains("Emergencia"))
            {
                await NotificarAdministracionCambioEmergencia(request, usuario);
            }

            // Notificar a seguridad para cambios administrativos
            if (request.EsCambioAdministrativo())
            {
                await NotificarSeguridadCambioAdministrativo(request, usuario);
            }

            _logger.LogInformation("Notificaciones enviadas para cambio de contraseña: Usuario {Email}", usuario.Email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al enviar notificaciones para cambio de contraseña: Usuario {Email}", usuario.Email);
        }
    }

    private async Task ProgramarTareasDeSeguridad(CambiarPasswordUsuarioCommand request, Usuario usuario)
    {
        try
        {
            // Programar recordatorio de próxima expiración
            if (request.FechaExpiracion.HasValue)
            {
                var fechaRecordatorio = request.FechaExpiracion.Value.AddDays(-7); // 7 días antes
                
                var recordatorio = new TareaProgramada
                {
                    Id = Guid.NewGuid(),
                    TipoTarea = "RecordatorioExpiracionPassword",
                    UsuarioId = usuario.Id,
                    FechaProgramada = fechaRecordatorio,
                    ConfiguracionTarea = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        UsuarioEmail = usuario.Email,
                        FechaExpiracion = request.FechaExpiracion.Value,
                        TipoCambio = request.ObtenerTipoCambio()
                    }),
                    Estado = "Programada",
                    FechaCreacion = DateTime.UtcNow
                };

                await _context.TareasProgramadas.AddAsync(recordatorio);
            }

            // Programar análisis de seguridad si es cambio crítico
            if (request.EsCambioCritico())
            {
                var analisisSeguridad = new TareaProgramada
                {
                    Id = Guid.NewGuid(),
                    TipoTarea = "AnalisisSeguridadPostCambio",
                    UsuarioId = usuario.Id,
                    FechaProgramada = DateTime.UtcNow.AddHours(24), // Análisis en 24 horas
                    ConfiguracionTarea = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        UsuarioEmail = usuario.Email,
                        TipoCambio = request.ObtenerTipoCambio(),
                        FechaCambio = DateTime.UtcNow
                    }),
                    Estado = "Programada",
                    FechaCreacion = DateTime.UtcNow
                };

                await _context.TareasProgramadas.AddAsync(analisisSeguridad);
            }

            _logger.LogInformation("Tareas de seguridad programadas para usuario {Email}", usuario.Email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al programar tareas de seguridad para usuario {Email}", usuario.Email);
        }
    }

    private async Task RegistrarIntentoFallido(CambiarPasswordUsuarioCommand request, Usuario usuario, string motivo)
    {
        var intentoFallido = new EventoAuditoria
        {
            Id = Guid.NewGuid(),
            TipoEvento = "IntentoPasswordIncorrecto",
            EntidadId = usuario.Id,
            EntidadTipo = "Usuario",
            UsuarioId = request.UsuarioAutorizaId,
            Detalles = $"Intento fallido de cambio de contraseña para {usuario.Email}: {motivo}",
            FechaEvento = DateTime.UtcNow,
            DatosAdicionales = new Dictionary<string, object>
            {
                { "UsuarioAfectado", usuario.Email },
                { "MotivoFallo", motivo },
                { "DireccionIP", request.DireccionIP ?? "N/A" },
                { "UserAgent", request.UserAgent ?? "N/A" },
                { "TipoIntentoFallido", "CambioPassword" }
            }
        };

        await _context.EventosAuditoria.AddAsync(intentoFallido);
        await _context.SaveChangesAsync();

        _logger.LogWarning("Intento fallido de cambio de contraseña registrado: Usuario {Email}, Motivo: {Motivo}",
            usuario.Email, motivo);
    }

    private async Task NotificarUsuarioCambioPassword(CambiarPasswordUsuarioCommand request, Usuario usuario)
    {
        var asunto = $"Confirmación de cambio de contraseña - {request.ObtenerTipoCambio()}";
        var mensaje = $@"
            Estimado/a {usuario.NombreCompleto},

            Su contraseña ha sido cambiada exitosamente.

            Detalles del cambio:
            - Tipo de cambio: {request.ObtenerTipoCambio()}
            - Fecha y hora: {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC
            - Motivo: {request.MotivosCambio}
            - IP de origen: {request.DireccionIP ?? "No disponible"}
            {(request.FechaExpiracion.HasValue ? $"- Nueva contraseña expira el: {request.FechaExpiracion.Value:dd/MM/yyyy}" : "")}

            {(request.InvalidarSesionesActivas ? "⚠️ IMPORTANTE: Todas sus sesiones activas han sido invalidadas por seguridad. Deberá iniciar sesión nuevamente." : "")}

            {(request.EsCambioCritico() ? "🔐 NOTA DE SEGURIDAD: Este fue un cambio crítico de contraseña. Si no lo autorizó, contacte inmediatamente al administrador del sistema." : "")}

            Consejos de seguridad:
            - Nunca comparta su contraseña con nadie
            - Use contraseñas únicas para cada sistema
            - Considere usar un gestor de contraseñas

            Si no autorizó este cambio, contacte inmediatamente al equipo de seguridad.

            RestaurantePro - Sistema de Seguridad
        ";

        await _emailService.SendEmailAsync(usuario.Email, asunto, mensaje);
    }

    private async Task NotificarSupervisorCambioPassword(CambiarPasswordUsuarioCommand request, Usuario usuario)
    {
        var supervisor = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuario.SupervisorId.Value);

        if (supervisor?.Email == null) return;

        var asunto = $"Cambio de contraseña crítico: {usuario.NombreCompleto}";
        var mensaje = $@"
            Estimado/a {supervisor.NombreCompleto},

            Se realizó un cambio de contraseña crítico para uno de sus supervisados:

            Usuario: {usuario.NombreCompleto} ({usuario.Email})
            Tipo de cambio: {request.ObtenerTipoCambio()}
            Motivo: {request.MotivosCambio}
            Fecha: {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC
            Prioridad: {request.Prioridad}/4

            {(request.EsCambioForzado ? "⚠️ ATENCIÓN: Este fue un cambio forzado por política de seguridad." : "")}

            RestaurantePro - Notificaciones de Supervisión
        ";

        await _emailService.SendEmailAsync(supervisor.Email, asunto, mensaje);
    }

    private async Task NotificarAdministracionCambioEmergencia(CambiarPasswordUsuarioCommand request, Usuario usuario)
    {
        var asunto = $"EMERGENCIA: Cambio de contraseña crítico - {usuario.NombreCompleto}";
        var mensaje = $@"
            🚨 ALERTA DE SEGURIDAD 🚨

            Usuario: {usuario.NombreCompleto} ({usuario.Email})
            Tipo: {request.ObtenerTipoCambio()}
            Prioridad: CRÍTICA ({request.Prioridad}/4)
            Motivo: {request.MotivosCambio}
            
            Detalles de seguridad:
            - IP origen: {request.DireccionIP ?? "No disponible"}
            - Timestamp: {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} UTC
            - Sesiones invalidadas: {(request.InvalidarSesionesActivas ? "SÍ" : "NO")}
            
            {(request.ObservacionesAdicionales != null ? $"Observaciones: {request.ObservacionesAdicionales}" : "")}

            Requiere revisión inmediata del equipo de seguridad.

            RestaurantePro - Alertas de Seguridad
        ";

        await _emailService.SendEmailAsync("seguridad@restaurantepro.com", asunto, mensaje);
    }

    private async Task NotificarSeguridadCambioAdministrativo(CambiarPasswordUsuarioCommand request, Usuario usuario)
    {
        var administrador = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == request.UsuarioAutorizaId);

        var asunto = $"Cambio administrativo de contraseña: {usuario.NombreCompleto}";
        var mensaje = $@"
            CAMBIO ADMINISTRATIVO DE CONTRASEÑA

            Usuario afectado: {usuario.NombreCompleto} ({usuario.Email})
            Administrador: {administrador?.NombreCompleto ?? "Desconocido"} ({administrador?.Email ?? "N/A"})
            
            Detalles:
            - Motivo: {request.MotivosCambio}
            - Fecha: {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC
            - IP administrador: {request.DireccionIP ?? "No disponible"}
            
            RestaurantePro - Auditoría de Seguridad
        ";

        await _emailService.SendEmailAsync("auditoria@restaurantepro.com", asunto, mensaje);
    }

    // Métodos auxiliares de seguridad
    private string GenerarSalt()
    {
        var bytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes);
    }

    private async Task<string> HashPassword(string password, string salt)
    {
        using var sha256 = SHA256.Create();
        var saltedPassword = password + salt;
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        return Convert.ToBase64String(hashedBytes);
    }

    private string GenerarTokenSeguridad()
    {
        return Guid.NewGuid().ToString("N")[..16].ToUpper() + DateTime.UtcNow.Ticks.ToString()[^8..];
    }

    private DateTime ObtenerFechaExpiracionDefecto(CambiarPasswordUsuarioCommand request)
    {
        return request.ObtenerTipoCambio() switch
        {
            "Primer Cambio" => DateTime.UtcNow.AddDays(90),
            "Cambio Forzado Crítico" => DateTime.UtcNow.AddDays(30),
            "Cambio Administrativo" => DateTime.UtcNow.AddDays(7),
            "Cambio Emergencia" => DateTime.UtcNow.AddDays(30),
            _ => DateTime.UtcNow.AddDays(120)
        };
    }

    private int CalcularNivelSeguridad(string password)
    {
        var nivel = 0;

        if (password.Length >= 8) nivel += 1;
        if (password.Length >= 12) nivel += 1;
        if (password.Any(char.IsLower)) nivel += 1;
        if (password.Any(char.IsUpper)) nivel += 1;
        if (password.Any(char.IsDigit)) nivel += 1;
        if (password.Any(c => !char.IsLetterOrDigit(c))) nivel += 1;
        if (password.Length >= 16) nivel += 1;

        return Math.Min(nivel, 7); // Máximo nivel 7
    }

    public class InformacionSeguridadDto
    {
        public string PasswordHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public string TokenSeguridad { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool DebeResetear { get; set; }
        public string TipoCambio { get; set; } = string.Empty;
        public int NivelSeguridad { get; set; }
        public string AlgoritmoHash { get; set; } = string.Empty;
        public int IteracionesHash { get; set; }
    }
} 