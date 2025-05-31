namespace RestaurantePro.Application.Core.Usuarios.DTOs;

/// <summary>
/// DTO principal para Usuario con información completa
/// Hereda de BaseDto para tener propiedades de auditoría
/// </summary>
public class UsuarioDto : BaseDto
{
    /// <summary>
    /// Nombre del usuario
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Apellido del usuario
    /// </summary>
    public string Apellido { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();

    /// <summary>
    /// Email del usuario (también usado como username)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono del usuario
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Número de identificación (DNI, pasaporte, etc.)
    /// </summary>
    public string? NumeroIdentificacion { get; set; }

    /// <summary>
    /// Fecha de nacimiento del usuario
    /// </summary>
    public DateTime? FechaNacimiento { get; set; }

    /// <summary>
    /// Dirección del usuario
    /// </summary>
    public string? Direccion { get; set; }

    /// <summary>
    /// Ciudad del usuario
    /// </summary>
    public string? Ciudad { get; set; }

    /// <summary>
    /// País del usuario
    /// </summary>
    public string? Pais { get; set; }

    /// <summary>
    /// Rol principal del usuario
    /// </summary>
    public string Rol { get; set; } = string.Empty;

    /// <summary>
    /// Departamento al que pertenece el usuario
    /// </summary>
    public string? Departamento { get; set; }

    /// <summary>
    /// Cargo del usuario
    /// </summary>
    public string? Cargo { get; set; }

    /// <summary>
    /// ID del supervisor directo (si aplica)
    /// </summary>
    public Guid? SupervisorId { get; set; }

    /// <summary>
    /// Nombre del supervisor directo
    /// </summary>
    public string? NombreSupervisor { get; set; }

    /// <summary>
    /// Fecha de contratación
    /// </summary>
    public DateTime? FechaContratacion { get; set; }

    /// <summary>
    /// Salario base del usuario
    /// </summary>
    public decimal? SalarioBase { get; set; }

    /// <summary>
    /// Indica si es usuario temporal
    /// </summary>
    public bool EsTemporal { get; set; }

    /// <summary>
    /// Fecha de última conexión
    /// </summary>
    public DateTime? FechaUltimaConexion { get; set; }

    /// <summary>
    /// Número de intentos de login fallidos
    /// </summary>
    public int IntentosFallidos { get; set; }

    /// <summary>
    /// Fecha hasta la cual está bloqueado (si aplica)
    /// </summary>
    public DateTime? FechaBloqueado { get; set; }

    /// <summary>
    /// Fecha de expiración de la contraseña
    /// </summary>
    public DateTime? FechaExpiracionPassword { get; set; }

    /// <summary>
    /// Indica si debe cambiar la contraseña en el próximo login
    /// </summary>
    public bool DebeResetearPassword { get; set; }

    /// <summary>
    /// Lista de permisos asignados al usuario
    /// </summary>
    public List<string> Permisos { get; set; } = new();

    /// <summary>
    /// Lista de sucursales a las que tiene acceso
    /// </summary>
    public List<Guid> SucursalesAcceso { get; set; } = new();

    /// <summary>
    /// Configuración de horarios de trabajo
    /// </summary>
    public List<HorarioTrabajoDto>? HorariosTrabajo { get; set; }

    /// <summary>
    /// Configuración de notificaciones del usuario
    /// </summary>
    public ConfiguracionNotificacionesDto? ConfiguracionNotificaciones { get; set; }

    /// <summary>
    /// Observaciones adicionales sobre el usuario
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Avatar o foto del usuario (URL o base64)
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// Preferencias del usuario (JSON serializado)
    /// </summary>
    public Dictionary<string, object>? Preferencias { get; set; }

    /// <summary>
    /// Indica si el usuario está verificado (email confirmado)
    /// </summary>
    public bool Verificado { get; set; }

    /// <summary>
    /// Zona horaria preferida del usuario
    /// </summary>
    public string ZonaHoraria { get; set; } = "America/Mexico_City";

    /// <summary>
    /// Idioma preferido del usuario
    /// </summary>
    public string Idioma { get; set; } = "es-MX";

    /// <summary>
    /// Nivel de acceso del usuario (1-5, siendo 5 el máximo)
    /// </summary>
    public int NivelAcceso { get; set; } = 1;

    /// <summary>
    /// Nombre de usuario (alias para compatibilidad)
    /// </summary>
    public string NombreUsuario => Email;

    /// <summary>
    /// Roles adicionales del usuario
    /// </summary>
    public List<string> RolesAdicionales { get; set; } = new();

    /// <summary>
    /// Puesto del usuario (alias para Cargo)
    /// </summary>
    public string? Puesto => Cargo;

    /// <summary>
    /// Fecha de ingreso (alias para FechaContratacion)
    /// </summary>
    public DateTime? FechaIngreso => FechaContratacion;

    /// <summary>
    /// Último acceso (alias para FechaUltimaConexion)
    /// </summary>
    public DateTime? UltimoAcceso => FechaUltimaConexion;

    /// <summary>
    /// Identificación (alias para NumeroIdentificacion)
    /// </summary>
    public string? Identificacion => NumeroIdentificacion;

    /// <summary>
    /// Posición (alias para Cargo)
    /// </summary>
    public string? Posicion => Cargo;

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime? FechaUltimaActualizacion { get; set; }

    /// <summary>
    /// Usuario que realizó la última actualización
    /// </summary>
    public string? UsuarioUltimaActualizacion { get; set; }

    /// <summary>
    /// Cantidad de subordinados
    /// </summary>
    public int CantidadSubordinados { get; set; }

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public UsuarioDto()
    {
        Verificado = false;
        DebeResetearPassword = true;
        IntentosFallidos = 0;
        EsTemporal = false;
    }

    /// <summary>
    /// Factory method para crear un usuario básico
    /// </summary>
    public static UsuarioDto CrearBasico(string nombre, string apellido, string email, string rol)
    {
        return new UsuarioDto
        {
            Nombre = nombre,
            Apellido = apellido,
            Email = email,
            Rol = rol,
            FechaContratacion = DateTime.UtcNow,
            Verificado = false,
            DebeResetearPassword = true
        };
    }

    /// <summary>
    /// Factory method para crear un administrador
    /// </summary>
    public static UsuarioDto CrearAdministrador(string nombre, string apellido, string email)
    {
        return new UsuarioDto
        {
            Nombre = nombre,
            Apellido = apellido,
            Email = email,
            Rol = "Administrador",
            NivelAcceso = 5,
            FechaContratacion = DateTime.UtcNow,
            Verificado = true,
            DebeResetearPassword = true,
            Permisos = new List<string> 
            { 
                "full_access", 
                "manage_users", 
                "manage_system", 
                "view_reports", 
                "manage_finances" 
            }
        };
    }

    /// <summary>
    /// Factory method para crear un mesero
    /// </summary>
    public static UsuarioDto CrearMesero(string nombre, string apellido, string email, Guid? supervisorId = null)
    {
        return new UsuarioDto
        {
            Nombre = nombre,
            Apellido = apellido,
            Email = email,
            Rol = "Mesero",
            Departamento = "Operaciones",
            Cargo = "Mesero",
            SupervisorId = supervisorId,
            NivelAcceso = 2,
            FechaContratacion = DateTime.UtcNow,
            Verificado = false,
            DebeResetearPassword = true,
            Permisos = new List<string> 
            { 
                "view_menu", 
                "create_orders", 
                "view_tables", 
                "process_payments" 
            }
        };
    }

    /// <summary>
    /// Determina si el usuario está bloqueado
    /// </summary>
    public bool EstaBloqueado => FechaBloqueado.HasValue && FechaBloqueado.Value > DateTime.UtcNow;

    /// <summary>
    /// Determina si la contraseña ha expirado
    /// </summary>
    public bool PasswordExpirada => FechaExpiracionPassword.HasValue && FechaExpiracionPassword.Value < DateTime.UtcNow;

    /// <summary>
    /// Determina si el usuario puede iniciar sesión
    /// </summary>
    public bool PuedeIniciarSesion => Activo && Verificado && !EstaBloqueado && !PasswordExpirada;

    /// <summary>
    /// Obtiene la antigüedad del usuario en días
    /// </summary>
    public int AntiguedadDias
    {
        get
        {
            if (!FechaContratacion.HasValue) return 0;
            return (DateTime.UtcNow - FechaContratacion.Value).Days;
        }
    }

    /// <summary>
    /// Obtiene el tiempo desde la última conexión en días
    /// </summary>
    public int DiasDesdeUltimaConexion
    {
        get
        {
            if (!FechaUltimaConexion.HasValue) return int.MaxValue;
            return (DateTime.UtcNow - FechaUltimaConexion.Value).Days;
        }
    }
} 