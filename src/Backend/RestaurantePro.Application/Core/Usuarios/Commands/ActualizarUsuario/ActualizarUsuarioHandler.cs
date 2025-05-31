namespace RestaurantePro.Application.Core.Usuarios.Commands.ActualizarUsuario;

public class ActualizarUsuarioHandler : IRequestHandler<ActualizarUsuarioCommand, Result<UsuarioDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ActualizarUsuarioHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    public ActualizarUsuarioHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ActualizarUsuarioHandler> logger,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        INotificationService notificationService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _notificationService = notificationService;
    }

    public async Task<Result<UsuarioDto>> Handle(ActualizarUsuarioCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando actualización de usuario: {UsuarioId}, Campos: {Campos}, Crítico: {EsCritico}",
                request.UsuarioId, string.Join(", ", request.ObtenerCamposAModificar()), request.TieneCambiosCriticos());

            // 1. Obtener usuario actual completo
            var usuarioResult = await ObtenerUsuarioCompleto(request.UsuarioId, cancellationToken);
            if (!usuarioResult.Succeeded)
            {
                return Result.Failure<UsuarioDto>(usuarioResult.Error);
            }

            var usuario = usuarioResult.Value;
            var datosOriginales = await CrearBackupDatos(usuario);

            // 2. Procesar actualización programada si aplica
            if (request.FechaEfectivacambios.HasValue)
            {
                return await ProcesarActualizacionProgramada(request, usuario);
            }

            // 3. Validar cambios críticos y aprobaciones
            var validacionCriticaResult = await ValidarCambiosCriticos(request, usuario);
            if (!validacionCriticaResult.Succeeded)
            {
                return Result.Failure<UsuarioDto>(validacionCriticaResult.Error);
            }

            // 4. Crear backup si es necesario
            if (request.CrearBackup || request.TieneCambiosCriticos())
            {
                await CrearBackupCompleto(usuario, request);
            }

            // 5. Aplicar cambios al usuario
            var aplicacionResult = await AplicarCambiosAlUsuario(request, usuario, cancellationToken);
            if (!aplicacionResult.Succeeded)
            {
                return Result.Failure<UsuarioDto>(aplicacionResult.Error);
            }

            // 6. Procesar cambios en jerarquía organizacional
            if (request.SupervisorId.HasValue || !string.IsNullOrWhiteSpace(request.Departamento))
            {
                await ProcesarCambiosJerarquicos(request, usuario);
            }

            // 7. Invalidar sesiones activas si es necesario
            if (request.InvalidarSesionesActivas || request.TieneCambiosCriticos())
            {
                await InvalidarSesionesUsuario(usuario.Id);
            }

            // 8. Guardar cambios en base de datos
            await _context.SaveChangesAsync(cancellationToken);

            // 9. Registrar auditoría completa
            await RegistrarAuditoriaActualizacion(request, usuario, datosOriginales);

            // 10. Procesar notificaciones
            await ProcesarNotificaciones(request, usuario, datosOriginales);

            // 11. Mapear resultado a DTO
            var usuarioDto = await MapearUsuarioADto(usuario);

            _logger.LogInformation("Usuario actualizado exitosamente: {Email}, Campos modificados: {CamposModificados}",
                usuario.Email, string.Join(", ", request.ObtenerCamposAModificar()));

            return Result.Success(usuarioDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario {UsuarioId}: {Motivo}",
                request.UsuarioId, request.MotivoActualizacion);
            return Result.Failure<UsuarioDto>("Error interno al actualizar el usuario.");
        }
    }

    private async Task<Result<Usuario>> ObtenerUsuarioCompleto(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Supervisor)
            .Include(u => u.UsuariosASupervisa)
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);

        if (usuario == null)
        {
            return Result.Failure<Usuario>("El usuario especificado no existe.");
        }

        if (usuario.FechaEliminacion.HasValue)
        {
            return Result.Failure<Usuario>("No se puede actualizar un usuario eliminado.");
        }

        return Result.Success(usuario);
    }

    private async Task<Dictionary<string, object>> CrearBackupDatos(Usuario usuario)
    {
        return new Dictionary<string, object>
        {
            { "Id", usuario.Id },
            { "Nombre", usuario.Nombre },
            { "Email", usuario.Email },
            { "Telefono", usuario.Telefono ?? "" },
            { "Identificacion", usuario.Identificacion ?? "" },
            { "Direccion", usuario.Direccion ?? "" },
            { "Rol", usuario.Rol },
            { "NivelAcceso", usuario.NivelAcceso },
            { "Activo", usuario.Activo },
            { "SupervisorId", usuario.SupervisorId },
            { "Departamento", usuario.Departamento ?? "" },
            { "Posicion", usuario.Posicion ?? "" },
            { "FechaIngreso", usuario.FechaIngreso },
            { "SalarioBase", usuario.SalarioBase },
            { "Permisos", usuario.Permisos?.ToList() ?? new List<string>() },
            { "Preferencias", usuario.Preferencias ?? new Dictionary<string, object>() },
            { "FechaUltimaActualizacion", usuario.FechaUltimaActualizacion }
        };
    }

    private async Task<Result<UsuarioDto>> ProcesarActualizacionProgramada(ActualizarUsuarioCommand request, Usuario usuario)
    {
        var actualizacionProgramada = new ActualizacionUsuarioProgramada
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId,
            FechaProgramada = request.FechaEfectivacambios!.Value,
            ConfiguracionCambios = JsonSerializer.Serialize(request),
            UsuarioAutoriza = request.UsuarioAutorizaId,
            Estado = "Programada",
            FechaCreacion = DateTime.UtcNow,
            Motivo = request.MotivoActualizacion,
            Prioridad = request.Prioridad
        };

        await _context.ActualizacionesUsuariosProgramadas.AddAsync(actualizacionProgramada);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Actualización programada creada para usuario {Email} el {FechaProgramada}",
            usuario.Email, request.FechaEfectivacambios);

        // Programar job de actualización (esto sería con Hangfire o similar)
        // await _backgroundJobClient.Schedule(() => EjecutarActualizacionProgramada(actualizacionProgramada.Id), request.FechaEfectivacambios.Value);

        var usuarioDto = await MapearUsuarioADto(usuario);
        return Result.Success(usuarioDto);
    }

    private async Task<Result<bool>> ValidarCambiosCriticos(ActualizarUsuarioCommand request, Usuario usuario)
    {
        if (!request.TieneCambiosCriticos())
        {
            return Result.Success(true);
        }

        // Validar que cambios críticos tengan aprobación
        if (request.RequiereAprobacion)
        {
            var aprobaciones = await _context.AprobacionesCambiosUsuario
                .Where(a => a.UsuarioId == request.UsuarioId &&
                           a.EstadoAprobacion == "Aprobada" &&
                           a.FechaAprobacion >= DateTime.UtcNow.AddDays(-1))
                .ToListAsync();

            if (!aprobaciones.Any())
            {
                return Result.Failure<bool>("Los cambios críticos requieren aprobación previa.");
            }
        }

        // Validar límites específicos para cambios críticos
        if (!string.IsNullOrWhiteSpace(request.Rol))
        {
            var cambiosRolRecientes = await _context.EventosAuditoria
                .Where(e => e.EntidadId == request.UsuarioId &&
                           e.TipoEvento == "CambioRol" &&
                           e.FechaEvento >= DateTime.UtcNow.AddDays(-30))
                .CountAsync();

            if (cambiosRolRecientes >= 2)
            {
                return Result.Failure<bool>("No se puede cambiar el rol más de 2 veces en 30 días.");
            }
        }

        return Result.Success(true);
    }

    private async Task CrearBackupCompleto(Usuario usuario, ActualizarUsuarioCommand request)
    {
        var backup = new BackupUsuario
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            DatosOriginales = JsonSerializer.Serialize(await CrearBackupDatos(usuario)),
            MotivoBackup = $"Backup antes de actualización: {request.MotivoActualizacion}",
            UsuarioSolicita = request.UsuarioAutorizaId,
            FechaBackup = DateTime.UtcNow,
            TipoOperacion = "ActualizacionUsuario",
            EsCritico = request.TieneCambiosCriticos()
        };

        await _context.BackupsUsuarios.AddAsync(backup);
        
        _logger.LogInformation("Backup creado para usuario {Email} antes de actualización crítica", usuario.Email);
    }

    private async Task<Result<bool>> AplicarCambiosAlUsuario(
        ActualizarUsuarioCommand request, 
        Usuario usuario, 
        CancellationToken cancellationToken)
    {
        // Aplicar cambios básicos
        if (!string.IsNullOrWhiteSpace(request.Nombre))
            usuario.Nombre = request.Nombre;

        if (!string.IsNullOrWhiteSpace(request.Email))
            usuario.Email = request.Email;

        if (!string.IsNullOrWhiteSpace(request.Telefono))
            usuario.Telefono = request.Telefono;

        if (!string.IsNullOrWhiteSpace(request.Identificacion))
            usuario.Identificacion = request.Identificacion;

        if (!string.IsNullOrWhiteSpace(request.Direccion))
            usuario.Direccion = request.Direccion;

        // Aplicar cambios de rol y permisos
        if (!string.IsNullOrWhiteSpace(request.Rol))
        {
            var rolAnterior = usuario.Rol;
            usuario.Rol = request.Rol;
            usuario.FechaCambioRol = DateTime.UtcNow;
            
            _logger.LogInformation("Cambio de rol: {Email} de {RolAnterior} a {RolNuevo}",
                usuario.Email, rolAnterior, request.Rol);
        }

        if (request.NivelAcceso.HasValue)
            usuario.NivelAcceso = request.NivelAcceso.Value;

        if (request.Activo.HasValue)
        {
            var estadoAnterior = usuario.Activo;
            usuario.Activo = request.Activo.Value;
            
            if (estadoAnterior != request.Activo.Value)
            {
                usuario.FechaCambioEstado = DateTime.UtcNow;
                _logger.LogInformation("Cambio de estado: {Email} {EstadoAnterior} -> {EstadoNuevo}",
                    usuario.Email, estadoAnterior ? "Activo" : "Inactivo", request.Activo.Value ? "Activo" : "Inactivo");
            }
        }

        // Aplicar permisos específicos
        if (request.PermisosEspecificos.Any())
        {
            usuario.Permisos = request.PermisosEspecificos;
            usuario.FechaActualizacionPermisos = DateTime.UtcNow;
        }

        // Aplicar cambios jerárquicos
        if (request.SupervisorId.HasValue)
            usuario.SupervisorId = request.SupervisorId.Value;

        if (!string.IsNullOrWhiteSpace(request.Departamento))
            usuario.Departamento = request.Departamento;

        if (!string.IsNullOrWhiteSpace(request.Posicion))
            usuario.Posicion = request.Posicion;

        // Aplicar cambios laborales
        if (request.FechaIngreso.HasValue)
            usuario.FechaIngreso = request.FechaIngreso.Value;

        if (request.SalarioBase.HasValue)
        {
            var salarioAnterior = usuario.SalarioBase;
            usuario.SalarioBase = request.SalarioBase.Value;
            usuario.FechaActualizacionSalario = DateTime.UtcNow;
            
            _logger.LogInformation("Cambio de salario: {Email} de {SalarioAnterior:C} a {SalarioNuevo:C}",
                usuario.Email, salarioAnterior, request.SalarioBase.Value);
        }

        // Aplicar configuraciones
        if (request.ConfiguracionNotificaciones != null)
        {
            usuario.ConfiguracionNotificaciones = JsonSerializer.Serialize(request.ConfiguracionNotificaciones);
        }

        if (request.Preferencias.Any())
        {
            usuario.Preferencias = request.Preferencias;
        }

        // Actualizar metadatos de auditoría
        usuario.FechaUltimaActualizacion = DateTime.UtcNow;
        usuario.UsuarioUltimaActualizacion = request.UsuarioAutorizaId;
        usuario.MotivoUltimaActualizacion = request.MotivoActualizacion;

        return Result.Success(true);
    }

    private async Task ProcesarCambiosJerarquicos(ActualizarUsuarioCommand request, Usuario usuario)
    {
        // Si cambió el supervisor, actualizar relaciones
        if (request.SupervisorId.HasValue && request.SupervisorId != usuario.SupervisorId)
        {
            // Notificar al supervisor anterior
            if (usuario.SupervisorId.HasValue)
            {
                var supervisorAnterior = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == usuario.SupervisorId.Value);

                if (supervisorAnterior != null)
                {
                    await NotificarCambioJerarquico(supervisorAnterior, usuario, "SupervisorAnterior");
                }
            }

            // Notificar al nuevo supervisor
            var nuevoSupervisor = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.SupervisorId.Value);

            if (nuevoSupervisor != null)
            {
                await NotificarCambioJerarquico(nuevoSupervisor, usuario, "NuevoSupervisor");
            }
        }

        // Actualizar subordinados si cambió de departamento
        if (!string.IsNullOrWhiteSpace(request.Departamento) && request.Departamento != usuario.Departamento)
        {
            var subordinados = await _context.Usuarios
                .Where(u => u.SupervisorId == usuario.Id)
                .ToListAsync();

            foreach (var subordinado in subordinados)
            {
                await NotificarCambioDepartamentoSupervisor(subordinado, usuario, request.Departamento);
            }
        }
    }

    private async Task InvalidarSesionesUsuario(Guid usuarioId)
    {
        // Marcar todas las sesiones activas del usuario como invalidadas
        var sesionesActivas = await _context.SesionesUsuario
            .Where(s => s.UsuarioId == usuarioId && s.Activa)
            .ToListAsync();

        foreach (var sesion in sesionesActivas)
        {
            sesion.Activa = false;
            sesion.FechaInvalidacion = DateTime.UtcNow;
            sesion.MotivoInvalidacion = "Actualización de datos críticos del usuario";
        }

        _logger.LogInformation("Invalidadas {CantidadSesiones} sesiones activas para usuario {UsuarioId}",
            sesionesActivas.Count, usuarioId);
    }

    private async Task RegistrarAuditoriaActualizacion(
        ActualizarUsuarioCommand request, 
        Usuario usuario, 
        Dictionary<string, object> datosOriginales)
    {
        var camposModificados = request.ObtenerCamposAModificar();
        var cambiosDetallados = new Dictionary<string, object>();

        // Comparar valores originales vs nuevos
        foreach (var campo in camposModificados)
        {
            var valorOriginal = datosOriginales.ContainsKey(campo) ? datosOriginales[campo] : null;
            var valorNuevo = ObtenerValorCampoActual(usuario, campo);

            cambiosDetallados.Add($"{campo}_Original", valorOriginal ?? "N/A");
            cambiosDetallados.Add($"{campo}_Nuevo", valorNuevo ?? "N/A");
        }

        var eventoAuditoria = new EventoAuditoria
        {
            Id = Guid.NewGuid(),
            TipoEvento = "UsuarioActualizado",
            EntidadId = usuario.Id,
            EntidadTipo = "Usuario",
            UsuarioId = request.UsuarioAutorizaId,
            Detalles = $"Usuario {usuario.Email} actualizado - Campos: {string.Join(", ", camposModificados)} - Motivo: {request.MotivoActualizacion}",
            FechaEvento = DateTime.UtcNow,
            DatosAdicionales = new Dictionary<string, object>
            {
                { "CamposModificados", camposModificados },
                { "CambiosDetallados", cambiosDetallados },
                { "MotivoActualizacion", request.MotivoActualizacion },
                { "EsCritico", request.TieneCambiosCriticos() },
                { "RequiereAprobacion", request.RequiereAprobacion },
                { "Prioridad", request.Prioridad },
                { "DocumentosAdjuntos", request.DocumentosAdjuntos },
                { "ObservacionesAdicionales", request.ObservacionesAdicionales ?? "N/A" },
                { "UsuarioAfectado", usuario.Email },
                { "RolAnterior", datosOriginales["Rol"] },
                { "RolNuevo", usuario.Rol }
            }
        };

        await _context.EventosAuditoria.AddAsync(eventoAuditoria);
        
        _logger.LogInformation("Auditoría registrada para actualización de usuario {Email}: {CamposModificados}",
            usuario.Email, string.Join(", ", camposModificados));
    }

    private object? ObtenerValorCampoActual(Usuario usuario, string campo)
    {
        return campo switch
        {
            // Propiedades que SÍ existen en Usuario dominio
            "Nombre" => usuario.NombreCompleto,
            "Email" => usuario.Email,
            "Rol" => usuario.Roles.FirstOrDefault().ToString(),
            "Activo" => usuario.Estado == EstadoUsuario.Activo,
            
            // TODO: Descomentar cuando Usuario tenga estas propiedades
            // "Telefono" => usuario.Telefono,
            // "Identificacion" => usuario.Identificacion,
            // "Direccion" => usuario.Direccion,
            // "NivelAcceso" => usuario.NivelAcceso,
            // "SupervisorId" => usuario.SupervisorId,
            // "Departamento" => usuario.Departamento,
            // "Posicion" => usuario.Posicion,
            // "FechaIngreso" => usuario.FechaIngreso,
            // "SalarioBase" => usuario.SalarioBase,
            // "PermisosEspecificos" => usuario.Permisos,
            
            _ => null
        };
    }

    private async Task ProcesarNotificaciones(
        ActualizarUsuarioCommand request, 
        Usuario usuario, 
        Dictionary<string, object> datosOriginales)
    {
        try
        {
            // Notificar al usuario si está configurado
            if (request.NotificarUsuario && !string.IsNullOrEmpty(usuario.Email))
            {
                await NotificarUsuarioActualizado(request, usuario, datosOriginales);
            }

            // Notificar al supervisor si está configurado
            if (request.NotificarSupervisor && usuario.SupervisorId.HasValue)
            {
                await NotificarSupervisorActualizacion(request, usuario, datosOriginales);
            }

            // Notificar a administración para cambios críticos
            if (request.TieneCambiosCriticos())
            {
                await NotificarAdministracionCambiosCriticos(request, usuario, datosOriginales);
            }

            // Notificar a recursos humanos para cambios salariales
            if (request.SalarioBase.HasValue)
            {
                await NotificarRecursosHumanosCambioSalarial(request, usuario, datosOriginales);
            }

            _logger.LogInformation("Notificaciones enviadas para actualización de usuario {Email}", usuario.Email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al enviar notificaciones para actualización de usuario {Email}", usuario.Email);
        }
    }

    private async Task NotificarUsuarioActualizado(
        ActualizarUsuarioCommand request, 
        Usuario usuario, 
        Dictionary<string, object> datosOriginales)
    {
        var camposModificados = request.ObtenerCamposAModificar();
        var asunto = "Actualización de tu perfil de usuario";
        
        var mensaje = $@"
            Estimado/a {usuario.NombreCompleto},

            Tu perfil de usuario ha sido actualizado con los siguientes cambios:

            Campos modificados: {string.Join(", ", camposModificados)}
            Motivo: {request.MotivoActualizacion}
            Fecha: {DateTime.UtcNow:dd/MM/yyyy HH:mm}
            Autorizado por: Usuario ID {request.UsuarioAutorizaId}

            {(request.InvalidarSesionesActivas ? "IMPORTANTE: Tus sesiones activas han sido invalidadas por seguridad. Deberás iniciar sesión nuevamente." : "")}

            Si tienes alguna pregunta sobre estos cambios, contacta a tu supervisor o al departamento de administración.

            RestaurantePro - Sistema de Gestión de Personal
        ";

        await _emailService.SendEmailAsync(usuario.Email, asunto, mensaje);
    }

    private async Task NotificarSupervisorActualizacion(
        ActualizarUsuarioCommand request, 
        Usuario usuario, 
        Dictionary<string, object> datosOriginales)
    {
        // TODO: Descomentar cuando Usuario tenga SupervisorId
        // var supervisor = await _context.Usuarios
        //     .FirstOrDefaultAsync(u => u.Id == usuario.SupervisorId.Value);
        //
        // if (supervisor?.Email == null) return;
        //
        // var asunto = $"Actualización de usuario supervisado: {usuario.NombreCompleto}";
        // var mensaje = $@"
        //     Estimado/a {supervisor.NombreCompleto},
        //
        //     Se ha actualizado la información de uno de tus usuarios supervisados:
        //
        //     Usuario: {usuario.NombreCompleto} ({usuario.Email})
        //     Campos modificados: {string.Join(", ", request.ObtenerCamposAModificar())}
        //     Motivo: {request.MotivoActualizacion}
        //     Autorizado por: Usuario ID {request.UsuarioAutorizaId}
        //     Fecha: {DateTime.UtcNow:dd/MM/yyyy HH:mm}
        //
        //     {(request.TieneCambiosCriticos() ? "⚠️ ATENCIÓN: Este cambio incluye modificaciones críticas que requieren tu conocimiento." : "")}
        //
        //     RestaurantePro - Notificaciones de Supervisión
        // ";
        //
        // await _emailService.SendEmailAsync(supervisor.Email, asunto, mensaje);
        
        _logger.LogInformation("Notificación a supervisor omitida - SupervisorId no implementado en Usuario");
    }

    private async Task NotificarAdministracionCambiosCriticos(
        ActualizarUsuarioCommand request, 
        Usuario usuario, 
        Dictionary<string, object> datosOriginales)
    {
        var asunto = $"Cambios Críticos en Usuario: {usuario.NombreCompleto}";
        var mensaje = $@"
            NOTIFICACIÓN DE CAMBIOS CRÍTICOS

            Usuario: {usuario.NombreCompleto} ({usuario.Email})
            Tipo de cambios: {(request.TieneCambiosCriticos() ? "CRÍTICOS" : "Normales")}
            Campos modificados: {string.Join(", ", request.ObtenerCamposAModificar())}
            Motivo: {request.MotivoActualizacion}
            Autorizado por: Usuario ID {request.UsuarioAutorizaId}
            Prioridad: {request.Prioridad}
            Requiere aprobación: {(request.RequiereAprobacion ? "Sí" : "No")}

            Cambios específicos:
            {(datosOriginales.ContainsKey("Rol") && !string.IsNullOrWhiteSpace(request.Rol) ? $"- Rol: {datosOriginales["Rol"]} → {request.Rol}" : "")}
            {(request.NivelAcceso.HasValue ? $"- Nivel de acceso: {datosOriginales["NivelAcceso"]} → {request.NivelAcceso}" : "")}
            {(request.Activo.HasValue ? $"- Estado: {datosOriginales["Activo"]} → {request.Activo}" : "")}

            Fecha: {DateTime.UtcNow:dd/MM/yyyy HH:mm}

            RestaurantePro - Alertas Administrativas
        ";

        await _emailService.SendEmailAsync("admin@restaurantepro.com", asunto, mensaje);
    }

    private async Task NotificarRecursosHumanosCambioSalarial(
        ActualizarUsuarioCommand request, 
        Usuario usuario, 
        Dictionary<string, object> datosOriginales)
    {
        var asunto = $"Cambio Salarial: {usuario.NombreCompleto}";
        var mensaje = $@"
            NOTIFICACIÓN DE CAMBIO SALARIAL

            Usuario: {usuario.NombreCompleto} ({usuario.Email})
            // TODO: Descomentar cuando Usuario tenga estas propiedades
            // Departamento: {usuario.Departamento}
            // Posición: {usuario.Posicion}

            Cambio salarial:
            - Salario anterior: {(datosOriginales.ContainsKey("SalarioBase") ? Convert.ToDecimal(datosOriginales["SalarioBase"]).ToString("C") : "No registrado")}
            - Nuevo salario: {request.SalarioBase?.ToString("C")}

            Motivo: {request.MotivoActualizacion}
            Autorizado por: Usuario ID {request.UsuarioAutorizaId}
            Fecha efectiva: {DateTime.UtcNow:dd/MM/yyyy}

            RestaurantePro - Notificaciones RRHH
        ";

        await _emailService.SendEmailAsync("rrhh@restaurantepro.com", asunto, mensaje);
    }

    private async Task NotificarCambioJerarquico(Usuario supervisor, Usuario usuario, string tipoNotificacion)
    {
        if (string.IsNullOrEmpty(supervisor.Email)) return;

        var asunto = tipoNotificacion == "NuevoSupervisor" 
            ? $"Nuevo usuario bajo tu supervisión: {usuario.NombreCompleto}"
            : $"Usuario ya no está bajo tu supervisión: {usuario.NombreCompleto}";

        var mensaje = tipoNotificacion == "NuevoSupervisor"
            ? $@"
                Estimado/a {supervisor.NombreCompleto},

                Se te ha asignado un nuevo usuario para supervisar:

                Usuario: {usuario.NombreCompleto} ({usuario.Email})
                // TODO: Descomentar cuando Usuario tenga estas propiedades
                // Departamento: {usuario.Departamento}
                // Posición: {usuario.Posicion}
                Fecha de asignación: {DateTime.UtcNow:dd/MM/yyyy}

                RestaurantePro - Gestión de Supervisión
            "
            : $@"
                Estimado/a {supervisor.NombreCompleto},

                El usuario {usuario.NombreCompleto} ({usuario.Email}) ya no está bajo tu supervisión.

                RestaurantePro - Gestión de Supervisión
            ";

        await _emailService.SendEmailAsync(supervisor.Email, asunto, mensaje);
    }

    private async Task NotificarCambioDepartamentoSupervisor(Usuario subordinado, Usuario supervisor, string nuevoDepartamento)
    {
        if (string.IsNullOrEmpty(subordinado.Email)) return;

        var asunto = $"Tu supervisor ha cambiado de departamento";
        var mensaje = $@"
            Estimado/a {subordinado.Nombre},

            Te informamos que tu supervisor {supervisor.Nombre} ha sido trasladado al departamento de {nuevoDepartamento}.

            Este cambio puede afectar algunos procesos operativos. Si tienes dudas, contacta al departamento de administración.

            RestaurantePro - Notificaciones Organizacionales
        ";

        await _emailService.SendEmailAsync(subordinado.Email, asunto, mensaje);
    }

    private async Task<UsuarioDto> MapearUsuarioADto(Usuario usuario)
    {
        return new UsuarioDto
        {
            Id = usuario.Id,
            Nombre = usuario.NombreCompleto,
            Apellido = "",
            Email = usuario.Email,
            Rol = usuario.Roles.FirstOrDefault().ToString(),
            Activo = usuario.Estado == EstadoUsuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
            FechaModificacion = usuario.FechaModificacion,
            UltimoAcceso = usuario.UltimoAcceso,
            DebeResetearPassword = false,
            Verificado = usuario.Estado == EstadoUsuario.Activo,
            EsAdministrador = usuario.EsAdministrador
        };
    }
} 