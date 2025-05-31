using MediatR;
using RestaurantePro.Application.Core.Usuarios.DTOs;
using RestaurantePro.Domain.Common;

namespace RestaurantePro.Application.Core.Usuarios.Commands.CrearUsuario;

/// <summary>
/// Command para crear un nuevo usuario en el sistema
/// Incluye gestión de roles, permisos y configuración inicial
/// </summary>
public class CrearUsuarioCommand : IRequest<Result<UsuarioDto>>
{
    /// <summary>
    /// Nombre de usuario único para el login
    /// </summary>
    public string NombreUsuario { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Email del usuario (debe ser único)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña temporal inicial
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Confirmar contraseña
    /// </summary>
    public string ConfirmarPassword { get; set; } = string.Empty;

    /// <summary>
    /// Rol principal del usuario
    /// </summary>
    public string Rol { get; set; } = string.Empty;

    /// <summary>
    /// Roles adicionales del usuario
    /// </summary>
    public List<string> RolesAdicionales { get; set; } = new();

    /// <summary>
    /// Teléfono del usuario
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Departamento o área de trabajo
    /// </summary>
    public string? Departamento { get; set; }

    /// <summary>
    /// Puesto o cargo del usuario
    /// </summary>
    public string? Puesto { get; set; }

    /// <summary>
    /// Sucursal asignada al usuario
    /// </summary>
    public Guid? SucursalId { get; set; }

    /// <summary>
    /// ID del supervisor o jefe directo
    /// </summary>
    public Guid? SupervisorId { get; set; }

    /// <summary>
    /// Fecha de ingreso del usuario
    /// </summary>
    public DateTime? FechaIngreso { get; set; }

    /// <summary>
    /// Nivel de acceso del usuario (1-10, donde 10 es máximo)
    /// </summary>
    public int NivelAcceso { get; set; } = 1;

    /// <summary>
    /// Indica si el usuario debe cambiar la contraseña en el primer login
    /// </summary>
    public bool DebeResetearPassword { get; set; } = true;

    /// <summary>
    /// Indica si el usuario está activo desde su creación
    /// </summary>
    public bool ActivoDesdeCreacion { get; set; } = true;

    /// <summary>
    /// Horarios de trabajo del usuario
    /// </summary>
    public List<HorarioTrabajoDto> HorariosTrabajo { get; set; } = new();

    /// <summary>
    /// Permisos específicos del usuario (además de los del rol)
    /// </summary>
    public List<string> PermisosEspecificos { get; set; } = new();

    /// <summary>
    /// Configuraciones personalizadas del usuario
    /// </summary>
    public Dictionary<string, string> ConfiguracionPersonal { get; set; } = new();

    /// <summary>
    /// Notas administrativas sobre el usuario
    /// </summary>
    public string? NotasAdministrativas { get; set; }

    /// <summary>
    /// ID del usuario que crea este usuario
    /// </summary>
    public Guid UsuarioCreadorId { get; set; }

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public CrearUsuarioCommand()
    {
    }

    /// <summary>
    /// Constructor básico para usuario simple
    /// </summary>
    public CrearUsuarioCommand(string nombreUsuario, string nombreCompleto, string email, string password, string rol)
    {
        NombreUsuario = nombreUsuario;
        NombreCompleto = nombreCompleto;
        Email = email;
        Password = password;
        ConfirmarPassword = password;
        Rol = rol;
    }

    /// <summary>
    /// Factory method para crear empleado básico
    /// </summary>
    public static CrearUsuarioCommand CrearEmpleado(
        string nombreUsuario, 
        string nombreCompleto, 
        string email,
        string telefono,
        string departamento,
        Guid usuarioCreadorId)
    {
        return new CrearUsuarioCommand
        {
            NombreUsuario = nombreUsuario,
            NombreCompleto = nombreCompleto,
            Email = email,
            Password = GenerarPasswordTemporal(),
            ConfirmarPassword = GenerarPasswordTemporal(),
            Rol = "Empleado",
            Telefono = telefono,
            Departamento = departamento,
            NivelAcceso = 1,
            DebeResetearPassword = true,
            ActivoDesdeCreacion = true,
            FechaIngreso = DateTime.UtcNow,
            UsuarioCreadorId = usuarioCreadorId
        };
    }

    /// <summary>
    /// Factory method para crear supervisor
    /// </summary>
    public static CrearUsuarioCommand CrearSupervisor(
        string nombreUsuario,
        string nombreCompleto,
        string email,
        string telefono,
        string departamento,
        Guid sucursalId,
        Guid usuarioCreadorId)
    {
        return new CrearUsuarioCommand
        {
            NombreUsuario = nombreUsuario,
            NombreCompleto = nombreCompleto,
            Email = email,
            Password = GenerarPasswordTemporal(),
            ConfirmarPassword = GenerarPasswordTemporal(),
            Rol = "Supervisor",
            RolesAdicionales = new List<string> { "Empleado" },
            Telefono = telefono,
            Departamento = departamento,
            Puesto = "Supervisor",
            SucursalId = sucursalId,
            NivelAcceso = 5,
            DebeResetearPassword = true,
            ActivoDesdeCreacion = true,
            FechaIngreso = DateTime.UtcNow,
            UsuarioCreadorId = usuarioCreadorId,
            PermisosEspecificos = new List<string>
            {
                "GestionarEmpleados",
                "VerReportes",
                "AprobarDescuentos"
            }
        };
    }

    /// <summary>
    /// Factory method para crear administrador
    /// </summary>
    public static CrearUsuarioCommand CrearAdministrador(
        string nombreUsuario,
        string nombreCompleto,
        string email,
        string telefono,
        Guid usuarioCreadorId)
    {
        return new CrearUsuarioCommand
        {
            NombreUsuario = nombreUsuario,
            NombreCompleto = nombreCompleto,
            Email = email,
            Password = GenerarPasswordTemporal(),
            ConfirmarPassword = GenerarPasswordTemporal(),
            Rol = "Administrador",
            RolesAdicionales = new List<string> { "Supervisor", "Empleado" },
            Telefono = telefono,
            Departamento = "Administración",
            Puesto = "Administrador",
            NivelAcceso = 9,
            DebeResetearPassword = true,
            ActivoDesdeCreacion = true,
            FechaIngreso = DateTime.UtcNow,
            UsuarioCreadorId = usuarioCreadorId,
            PermisosEspecificos = new List<string>
            {
                "GestionarUsuarios",
                "GestionarRoles",
                "VerTodosReportes",
                "ConfigurarSistema",
                "GestionarSucursales"
            }
        };
    }

    /// <summary>
    /// Factory method para crear usuario con horarios específicos
    /// </summary>
    public static CrearUsuarioCommand CrearConHorarios(
        string nombreUsuario,
        string nombreCompleto,
        string email,
        string rol,
        List<HorarioTrabajoDto> horarios,
        Guid usuarioCreadorId)
    {
        return new CrearUsuarioCommand
        {
            NombreUsuario = nombreUsuario,
            NombreCompleto = nombreCompleto,
            Email = email,
            Password = GenerarPasswordTemporal(),
            ConfirmarPassword = GenerarPasswordTemporal(),
            Rol = rol,
            HorariosTrabajo = horarios,
            DebeResetearPassword = true,
            ActivoDesdeCreacion = true,
            FechaIngreso = DateTime.UtcNow,
            UsuarioCreadorId = usuarioCreadorId
        };
    }

    /// <summary>
    /// Genera una contraseña temporal segura
    /// </summary>
    private static string GenerarPasswordTemporal()
    {
        var random = new Random();
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@#$%&*";
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

/// <summary>
/// DTO para horarios de trabajo del usuario
/// </summary>
public class HorarioTrabajoDto
{
    public string DiaSemana { get; set; } = string.Empty;
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFin { get; set; }
    public bool EsDiaLibre { get; set; } = false;
    public string? NotasEspeciales { get; set; }
} 