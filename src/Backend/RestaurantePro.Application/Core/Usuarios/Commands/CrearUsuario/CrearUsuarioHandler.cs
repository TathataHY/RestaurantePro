using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Core.Usuarios.DTOs;
using RestaurantePro.Domain.Common;
using RestaurantePro.Domain.Core.Usuarios;
using RestaurantePro.Domain.Common.Services;
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
            if (!verificacionPermisos.IsSuccess)
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
                usuario.NombreUsuario, usuario.Id, usuario.Rol);

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

        // Verificar que el creador tenga permisos suficientes
        var puedeCrearUsuarios = usuarioCreador.Permisos?.Contains("GestionarUsuarios") == true ||
                                usuarioCreador.Rol == "Administrador" ||
                                usuarioCreador.Rol == "SuperAdministrador";

        if (!puedeCrearUsuarios)
        {
            return Result.Failure<bool>("No tiene permisos para crear usuarios.");
        }

        // Verificar que no pueda crear usuarios con nivel superior al suyo
        if (request.NivelAcceso >= usuarioCreador.NivelAcceso)
        {
            return Result.Failure<bool>("No puede crear usuarios con nivel de acceso igual o superior al suyo.");
        }

        // Verificar que no pueda asignar roles superiores
        if (await EsRolSuperior(request.Rol, usuarioCreador.Rol))
        {
            return Result.Failure<bool>("No puede asignar un rol superior al suyo.");
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
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            NombreUsuario = request.NombreUsuario,
            NombreCompleto = request.NombreCompleto,
            Email = request.Email,
            PasswordHash = seguridad.PasswordHash,
            Salt = seguridad.Salt,
            Rol = request.Rol,
            Telefono = request.Telefono,
            Departamento = request.Departamento,
            Puesto = request.Puesto,
            SupervisorId = request.SupervisorId,
            NivelAcceso = request.NivelAcceso,
            Activo = request.ActivoDesdeCreacion,
            FechaCreacion = seguridad.FechaCreacion,
            FechaIngreso = request.FechaIngreso ?? DateTime.UtcNow,
            DebeResetearPassword = seguridad.DebeResetearPassword,
            UltimaActualizacionPassword = seguridad.UltimaActualizacionPassword,
            TokenActivacion = seguridad.TokenActivacion,
            UsuarioCreadorId = request.UsuarioCreadorId,
            NotasAdministrativas = request.NotasAdministrativas
        };

        return usuario;
    }

    private async Task ConfigurarRolesYPermisos(Usuario usuario, CrearUsuarioCommand request)
    {
        // Asignar roles adicionales
        usuario.RolesAdicionales = request.RolesAdicionales;

        // Generar permisos basados en roles
        var permisosBasicos = await ObtenerPermisosPorRol(request.Rol);
        
        // Agregar permisos específicos
        var todosPermisos = permisosBasicos.Union(request.PermisosEspecificos).ToList();
        usuario.Permisos = todosPermisos;

        _logger.LogInformation("Configurados {CantidadPermisos} permisos para usuario {NombreUsuario}",
            todosPermisos.Count, usuario.NombreUsuario);
    }

    private async Task ConfigurarHorariosTrabajo(Usuario usuario, List<HorarioTrabajoDto> horarios)
    {
        if (horarios.Any())
        {
            usuario.HorariosTrabajo = horarios.Select(h => new HorarioTrabajo
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuario.Id,
                DiaSemana = h.DiaSemana,
                HoraInicio = h.HoraInicio,
                HoraFin = h.HoraFin,
                EsDiaLibre = h.EsDiaLibre,
                NotasEspeciales = h.NotasEspeciales
            }).ToList();

            _logger.LogInformation("Configurados {CantidadHorarios} horarios de trabajo para usuario {NombreUsuario}",
                horarios.Count, usuario.NombreUsuario);
        }
    }

    private async Task AsignarASucursal(Guid usuarioId, Guid sucursalId, CancellationToken cancellationToken)
    {
        var asignacion = new UsuarioSucursal
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            SucursalId = sucursalId,
            FechaAsignacion = DateTime.UtcNow,
            EsAsignacionPrincipal = true,
            Activo = true
        };

        await _context.UsuarioSucursales.AddAsync(asignacion, cancellationToken);
        
        _logger.LogInformation("Usuario {UsuarioId} asignado a sucursal {SucursalId}", usuarioId, sucursalId);
    }

    private async Task CrearPerfilExtendido(Guid usuarioId, CrearUsuarioCommand request, CancellationToken cancellationToken)
    {
        var perfil = new PerfilUsuario
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Configuraciones = request.ConfiguracionPersonal,
            PreferenciasNotificacion = new Dictionary<string, string>
            {
                { "Email", "true" },
                { "SMS", "false" },
                { "Push", "true" }
            },
            FechaCreacion = DateTime.UtcNow,
            UltimaActualizacion = DateTime.UtcNow
        };

        await _context.PerfilesUsuario.AddAsync(perfil, cancellationToken);
        
        _logger.LogInformation("Perfil extendido creado para usuario {UsuarioId}", usuarioId);
    }

    private async Task EnviarNotificacionesCreacion(Usuario usuario, string passwordTemporal)
    {
        try
        {
            // Enviar email de bienvenida
            var asuntoEmail = "Bienvenido a RestaurantePro - Credenciales de Acceso";
            var mensajeEmail = GenerarMensajeBienvenida(usuario, passwordTemporal);

            await _emailService.SendEmailAsync(usuario.Email, asuntoEmail, mensajeEmail);

            // Notificar al supervisor si existe
            if (usuario.SupervisorId.HasValue)
            {
                await NotificarASupervisor(usuario);
            }

            _logger.LogInformation("Notificaciones de creación enviadas para usuario {NombreUsuario}", usuario.NombreUsuario);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al enviar notificaciones para usuario {NombreUsuario}", usuario.NombreUsuario);
        }
    }

    private async Task RegistrarEventoAuditoria(Usuario usuario, Guid usuarioCreadorId)
    {
        var evento = new EventoAuditoria
        {
            Id = Guid.NewGuid(),
            TipoEvento = "UsuarioCreado",
            EntidadId = usuario.Id,
            EntidadTipo = "Usuario",
            UsuarioId = usuarioCreadorId,
            Detalles = $"Usuario {usuario.NombreUsuario} creado con rol {usuario.Rol}",
            FechaEvento = DateTime.UtcNow,
            DatosAdicionales = new Dictionary<string, object>
            {
                { "NombreUsuario", usuario.NombreUsuario },
                { "Rol", usuario.Rol },
                { "Departamento", usuario.Departamento ?? "N/A" },
                { "NivelAcceso", usuario.NivelAcceso }
            }
        };

        await _context.EventosAuditoria.AddAsync(evento);
    }

    // Métodos auxiliares
    private async Task<bool> EsRolSuperior(string rolAsignar, string rolCreador)
    {
        var jerarquiaRoles = new Dictionary<string, int>
        {
            { "Empleado", 1 },
            { "Supervisor", 2 },
            { "Gerente", 3 },
            { "Administrador", 4 },
            { "SuperAdministrador", 5 }
        };

        var nivelAsignar = jerarquiaRoles.GetValueOrDefault(rolAsignar, 0);
        var nivelCreador = jerarquiaRoles.GetValueOrDefault(rolCreador, 0);

        return nivelAsignar >= nivelCreador;
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

            Bienvenido/a a RestaurantePro. Su cuenta ha sido creada exitosamente.

            Credenciales de acceso:
            - Usuario: {usuario.NombreUsuario}
            - Contraseña temporal: {passwordTemporal}
            - Rol asignado: {usuario.Rol}
            - Departamento: {usuario.Departamento ?? "N/A"}

            Por seguridad, deberá cambiar su contraseña en el primer acceso.

            Acceda al sistema en: [URL del sistema]

            Si tiene dudas, contacte a su supervisor o al área de sistemas.

            Saludos cordiales,
            Equipo RestaurantePro
        ";
    }

    private async Task NotificarASupervisor(Usuario usuario)
    {
        var supervisor = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuario.SupervisorId.Value);

        if (supervisor != null && !string.IsNullOrEmpty(supervisor.Email))
        {
            var asunto = $"Nuevo empleado asignado - {usuario.NombreCompleto}";
            var mensaje = $@"
                Estimado/a {supervisor.NombreCompleto},

                Se le ha asignado un nuevo empleado:

                - Nombre: {usuario.NombreCompleto}
                - Usuario: {usuario.NombreUsuario}
                - Rol: {usuario.Rol}
                - Departamento: {usuario.Departamento ?? "N/A"}
                - Fecha de ingreso: {usuario.FechaIngreso:dd/MM/yyyy}

                Por favor, coordine su integración al equipo.

                Saludos cordiales,
                Sistema RestaurantePro
            ";

            await _emailService.SendEmailAsync(supervisor.Email, asunto, mensaje);
        }
    }

    private async Task<UsuarioDto> MapearUsuarioADto(Usuario usuario)
    {
        return new UsuarioDto
        {
            Id = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            NombreCompleto = usuario.NombreCompleto,
            Email = usuario.Email,
            Rol = usuario.Rol,
            RolesAdicionales = usuario.RolesAdicionales ?? new List<string>(),
            Telefono = usuario.Telefono,
            Departamento = usuario.Departamento,
            Puesto = usuario.Puesto,
            NivelAcceso = usuario.NivelAcceso,
            Activo = usuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
            FechaIngreso = usuario.FechaIngreso,
            UltimoAcceso = usuario.UltimoAcceso,
            Permisos = usuario.Permisos ?? new List<string>(),
            HorariosTrabajo = usuario.HorariosTrabajo?.Select(h => new HorarioTrabajoDto
            {
                DiaSemana = h.DiaSemana,
                HoraInicio = h.HoraInicio,
                HoraFin = h.HoraFin,
                EsDiaLibre = h.EsDiaLibre,
                NotasEspeciales = h.NotasEspeciales
            }).ToList() ?? new List<HorarioTrabajoDto>()
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