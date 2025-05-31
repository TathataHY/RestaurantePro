using System.Security.Cryptography;
using System.Text;

namespace RestaurantePro.Application.Core.Usuarios.Commands.CrearUsuario;

public class CrearUsuarioHandler : IRequestHandler<CrearUsuarioCommand, Result<UsuarioDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CrearUsuarioHandler> _logger;
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUserService;

    public CrearUsuarioHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CrearUsuarioHandler> logger,
        IEmailService emailService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _emailService = emailService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UsuarioDto>> Handle(CrearUsuarioCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando creación de usuario: {NombreUsuario}, Rol: {Rol}, Departamento: {Departamento}",
                request.NombreUsuario, request.Rol, request.Departamento);

            // 1. Verificar permisos del usuario creador
            var verificacionPermisos = await VerificarPermisosCreacion(request, cancellationToken);
            if (!verificacionPermisos.Succeeded)
            {
                return Result.Failure<UsuarioDto>(verificacionPermisos.Error);
            }

            // 2. Generar información de seguridad
            var infoSeguridad = await GenerarInformacionSeguridad(request);

            // 3. Crear entidad Usuario
            var usuario = await CrearEntidadUsuario(request, infoSeguridad);

            // 4. Configurar roles y permisos
            await ConfigurarRolesYPermisos(usuario, request);

            // 5. Configurar horarios de trabajo
            await ConfigurarHorariosTrabajo(usuario, request.HorariosTrabajo);

            // 6. Guardar en base de datos
            await _context.Usuarios.AddAsync(usuario, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // 7. Asignar a sucursal si aplica
            if (request.SucursalId.HasValue)
            {
                await AsignarASucursal(usuario.Id, request.SucursalId.Value, cancellationToken);
            }

            // 8. Crear perfil de usuario extendido
            await CrearPerfilExtendido(usuario.Id, request, cancellationToken);

            // 9. Enviar notificaciones
            await EnviarNotificacionesCreacion(usuario, request.Password);

            // 10. Registrar evento de auditoría
            await RegistrarEventoAuditoria(usuario, request.UsuarioCreadorId);

            // 11. Mapear a DTO y devolver resultado
            var usuarioDto = await MapearUsuarioADto(usuario);

            _logger.LogInformation("Usuario {NombreUsuario} creado exitosamente con ID {UsuarioId}. Rol: {Rol}",
                usuario.NombreUsuario, usuario.Id, usuario.Roles.FirstOrDefault());

            return Result.Success(usuarioDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario: {NombreUsuario}", request.NombreUsuario);
            return Result.Failure<UsuarioDto>("Error interno al crear el usuario.");
        }
    }

    private async Task<Result<bool>> VerificarPermisosCreacion(CrearUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuarioCreador = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == request.UsuarioCreadorId, cancellationToken);

        if (usuarioCreador == null)
        {
            return Result.Failure<bool>("El usuario creador no existe.");
        }

        // Usuario dominio tiene Roles (colección), usar roles reales
        var puedeCrearUsuarios = usuarioCreador.EsAdministrador || // Esta propiedad SÍ existe
                                usuarioCreador.Roles.Any(r => r == RolUsuario.Administrador || r == RolUsuario.Gerente);

        if (!puedeCrearUsuarios)
        {
            return Result.Failure<bool>("No tiene permisos para crear usuarios.");
        }

        // TODO: Descomentar cuando Usuario tenga NivelAcceso
        // Verificar que no pueda crear usuarios con nivel superior al suyo
        // if (request.NivelAcceso >= usuarioCreador.NivelAcceso)
        // {
        //     return Result.Failure<bool>("No puede crear usuarios con nivel de acceso igual o superior al suyo.");
        // }

        // Verificar que no pueda asignar roles superiores usando roles reales
        if (!Enum.TryParse<RolUsuario>(request.Rol, true, out var rolAsignar))
        {
            return Result.Failure<bool>("Rol no válido.");
        }

        var rolCreadorMasAlto = usuarioCreador.Roles.Max(); // Enum se puede comparar directamente
        if (rolAsignar >= rolCreadorMasAlto)
        {
            return Result.Failure<bool>("No puede asignar un rol igual o superior al suyo.");
        }

        return Result.Success(true);
    }

    private async Task<InformacionSeguridadDto> GenerarInformacionSeguridad(CrearUsuarioCommand request)
    {
        return new InformacionSeguridadDto
        {
            PasswordHash = await HashPassword(request.Password),
            Salt = GenerarSalt(),
            TokenActivacion = GenerarTokenActivacion(),
            FechaCreacion = DateTime.UtcNow,
            DebeResetearPassword = request.DebeResetearPassword,
            UltimaActualizacionPassword = DateTime.UtcNow
        };
    }

    private async Task<Usuario> CrearEntidadUsuario(CrearUsuarioCommand request, InformacionSeguridadDto seguridad)
    {
        if (!Enum.TryParse<RolUsuario>(request.Rol, true, out var rolEnum))
        {
            throw new InvalidOperationException($"Rol no válido: {request.Rol}");
        }

        var usuario = Usuario.Crear(request.NombreUsuario, request.NombreCompleto, request.Email, rolEnum);

        _logger.LogInformation("Usuario {NombreUsuario} creado usando factory domain method", usuario.NombreUsuario);

        return usuario;
    }

    private async Task ConfigurarRolesYPermisos(Usuario usuario, CrearUsuarioCommand request)
    {
        var permisosBasicos = await ObtenerPermisosPorRol(request.Rol);
        
        var todosPermisos = permisosBasicos.Union(request.PermisosEspecificos).ToList();
        _logger.LogInformation("Configurados {CantidadPermisos} permisos para usuario {NombreUsuario} (pendiente implementación completa)",
            todosPermisos.Count, usuario.NombreUsuario);
    }

    private async Task ConfigurarHorariosTrabajo(Usuario usuario, List<HorarioTrabajoDto> horarios)
    {
        if (horarios.Any())
        {
            _logger.LogInformation("Configuración de {CantidadHorarios} horarios omitida - HorariosTrabajo no implementado en Usuario",
                horarios.Count);
        }
    }

    private async Task AsignarASucursal(Guid usuarioId, Guid sucursalId, CancellationToken cancellationToken)
    {
        // TODO: Descomentar cuando UsuarioSucursal esté disponible en el contexto
        // var asignacion = new UsuarioSucursal
        // {
        //     Id = Guid.NewGuid(),
        //     UsuarioId = usuarioId,
        //     SucursalId = sucursalId,
        //     FechaAsignacion = DateTime.UtcNow,
        //     EsAsignacionPrincipal = true,
        //     Activo = true
        // };
        //
        // await _context.UsuarioSucursales.AddAsync(asignacion, cancellationToken);
        
        _logger.LogInformation("Asignación a sucursal omitida - UsuarioSucursales no implementado");
    }

    private async Task CrearPerfilExtendido(Guid usuarioId, CrearUsuarioCommand request, CancellationToken cancellationToken)
    {
        // TODO: Descomentar cuando PerfilUsuario esté disponible en el contexto
        // var perfil = new PerfilUsuario
        // {
        //     Id = Guid.NewGuid(),
        //     UsuarioId = usuarioId,
        //     Configuraciones = request.ConfiguracionPersonal,
        //     PreferenciasNotificacion = new Dictionary<string, string>
        //     {
        //         { "Email", "true" },
        //         { "SMS", "false" },
        //         { "Push", "true" }
        //     },
        //     FechaCreacion = DateTime.UtcNow,
        //     UltimaActualizacion = DateTime.UtcNow
        // };
        //
        // await _context.PerfilesUsuario.AddAsync(perfil, cancellationToken);
        
        _logger.LogInformation("Perfil extendido omitido - PerfilesUsuario no implementado");
    }

    private async Task EnviarNotificacionesCreacion(Usuario usuario, string passwordTemporal)
    {
        try
        {
            var asuntoEmail = "Bienvenido a RestaurantePro - Credenciales de Acceso";
            var mensajeEmail = GenerarMensajeBienvenida(usuario, passwordTemporal);

            await _emailService.SendEmailAsync(usuario.Email, asuntoEmail, mensajeEmail);

            // TODO: Descomentar cuando Usuario tenga SupervisorId
            // Notificar al supervisor si existe
            // if (usuario.SupervisorId.HasValue)
            // {
            //     await NotificarASupervisor(usuario);
            // }

            _logger.LogInformation("Notificaciones de creación enviadas para usuario {NombreUsuario}", usuario.NombreUsuario);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al enviar notificaciones para usuario {NombreUsuario}", usuario.NombreUsuario);
        }
    }

    private async Task RegistrarEventoAuditoria(Usuario usuario, Guid usuarioCreadorId)
    {
        // TODO: Descomentar cuando EventoAuditoria esté disponible en el contexto
        // var evento = new EventoAuditoria
        // {
        //     Id = Guid.NewGuid(),
        //     TipoEvento = "UsuarioCreado",
        //     EntidadId = usuario.Id,
        //     EntidadTipo = "Usuario",
        //     UsuarioId = usuarioCreadorId,
        //     Detalles = $"Usuario {usuario.NombreUsuario} creado con rol {usuario.Roles.FirstOrDefault()}",
        //     FechaEvento = DateTime.UtcNow,
        //     DatosAdicionales = new Dictionary<string, object>
        //     {
        //         { "NombreUsuario", usuario.NombreUsuario },
        //         { "Rol", usuario.Roles.FirstOrDefault().ToString() },
        //         { "Departamento", "N/A" }, // TODO: cuando Usuario tenga Departamento
        //         { "NivelAcceso", 1 } // TODO: cuando Usuario tenga NivelAcceso
        //     }
        // };
        //
        // await _context.EventosAuditoria.AddAsync(evento);

        _logger.LogInformation("Auditoría de creación omitida - EventosAuditoria no implementado");
    }

    private async Task<List<string>> ObtenerPermisosPorRol(string rol)
    {
        return rol.ToLower() switch
        {
            "empleado" => new List<string> { "VerDashboard", "CrearComandas", "VerReportesBasicos" },
            "supervisor" => new List<string> { "VerDashboard", "CrearComandas", "VerReportes", "GestionarEmpleados", "AprobarDescuentos" },
            "gerente" => new List<string> { "VerDashboard", "CrearComandas", "VerReportes", "GestionarEmpleados", "AprobarDescuentos", "GestionarInventario" },
            "administrador" => new List<string> { "GestionarUsuarios", "GestionarRoles", "VerTodosReportes", "ConfigurarSistema", "GestionarSucursales" },
            "superadministrador" => new List<string> { "TodosPermisos" },
            _ => new List<string>()
        };
    }

    private async Task<string> HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + GenerarSalt()));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private string GenerarSalt()
    {
        var bytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes);
    }

    private string GenerarTokenActivacion()
    {
        return Guid.NewGuid().ToString("N")[..16].ToUpper();
    }

    private string GenerarMensajeBienvenida(Usuario usuario, string passwordTemporal)
    {
        return $@"
            Estimado/a {usuario.NombreCompleto},

            Bienvenido/a a RestaurantePro. Tu cuenta ha sido creada exitosamente.

            Tus credenciales de acceso son:
            - Usuario: {usuario.NombreUsuario}
            - Email: {usuario.Email}
            - Contraseña temporal: {passwordTemporal}
            - Rol: {usuario.Roles.FirstOrDefault()}

            Por seguridad, deberás cambiar tu contraseña en el primer inicio de sesión.

            Saludos,
            Equipo RestaurantePro
        ";
    }

    private async Task NotificarASupervisor(Usuario usuario)
    {
        try
        {
            // TODO: Descomentar cuando Usuario tenga SupervisorId
            // var supervisor = await _context.Usuarios
            //     .FirstOrDefaultAsync(u => u.Id == usuario.SupervisorId.Value);
            //
            // if (supervisor != null)
            // {
            //     var asunto = $"Nuevo usuario bajo tu supervisión: {usuario.NombreCompleto}";
            //     var mensaje = $@"
            //         Estimado/a {supervisor.NombreCompleto},
            //
            //         Se ha creado un nuevo usuario bajo tu supervisión:
            //
            //         - Nombre: {usuario.NombreCompleto}
            //         - Email: {usuario.Email}
            //         - Usuario: {usuario.NombreUsuario}
            //         - Rol: {usuario.Roles.FirstOrDefault()}
            //
            //         RestaurantePro - Notificaciones de Supervisión
            //     ";
            //
            //     await _emailService.SendEmailAsync(supervisor.Email, asunto, mensaje);
            // }

            _logger.LogInformation("Notificación a supervisor omitida - SupervisorId no implementado");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al notificar supervisor para usuario {NombreUsuario}", usuario.NombreUsuario);
        }
    }

    private async Task<UsuarioDto> MapearUsuarioADto(Usuario usuario)
    {
        return new UsuarioDto
        {
            Id = usuario.Id,
            // Propiedades que SÍ existen en Usuario dominio
            Nombre = usuario.NombreCompleto, // UsuarioDto espera Nombre, Usuario tiene NombreCompleto
            Email = usuario.Email,
            Activo = usuario.Estado == EstadoUsuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
            
            // Usuario dominio tiene Roles (colección), usar el primer rol como principal
            Rol = usuario.Roles.FirstOrDefault().ToString(),
            
            // Usar solo propiedades que existen en UsuarioDto y son settables
            // TODO: Verificar qué propiedades tiene realmente UsuarioDto
            
            // Configuraciones por defecto para propiedades requeridas en DTO
            Apellido = "", // UsuarioDto tiene Apellido separado, Usuario tiene NombreCompleto
            DebeResetearPassword = true, // TODO: usar valor real cuando exista
            Verificado = usuario.Estado == EstadoUsuario.Activo,
            Permisos = new List<string>(), // TODO: implementar cuando exista
            RolesAdicionales = new List<string>() // TODO: implementar cuando exista
        };
    }
}

/// <summary>
/// DTO interno para información de seguridad del usuario
/// </summary>
internal class InformacionSeguridadDto
{
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string TokenActivacion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public bool DebeResetearPassword { get; set; }
    public DateTime UltimaActualizacionPassword { get; set; }
} 