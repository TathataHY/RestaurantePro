namespace RestaurantePro.Application.Core.Usuarios.DTOs;

/// <summary>
/// DTO principal para Usuario con información completa
/// Hereda de BaseDto para tener propiedades de auditoría
/// </summary>
public class UsuarioDto : BaseDto
{
    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de usuario para login
    /// </summary>
    public string NombreUsuario { get; set; } = string.Empty;

    /// <summary>
    /// Email del usuario (también usado como username)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Estado actual del usuario
    /// </summary>
    public EstadoUsuario Estado { get; set; }

    /// <summary>
    /// Tipo de usuario
    /// </summary>
    public TipoUsuario TipoUsuario { get; set; }

    /// <summary>
    /// Rol principal del usuario
    /// </summary>
    public string Rol { get; set; } = string.Empty;

    /// <summary>
    /// Nivel de acceso del usuario (1-10, donde 10 es máximo)
    /// </summary>
    public int NivelAcceso { get; set; }

    /// <summary>
    /// Lista de permisos específicos del usuario
    /// </summary>
    public List<string> Permisos { get; set; } = new();

    /// <summary>
    /// ID del supervisor directo (si aplica)
    /// </summary>
    public Guid? SupervisorId { get; set; }

    /// <summary>
    /// Departamento al que pertenece el usuario
    /// </summary>
    public string? Departamento { get; set; }

    /// <summary>
    /// Posición del usuario (alias para Cargo)
    /// </summary>
    public string? Posicion { get; set; }

    /// <summary>
    /// Identificación (alias para compatibilidad)
    /// </summary>
    public string? Identificacion { get; set; }

    /// <summary>
    /// Fecha de último acceso al sistema
    /// </summary>
    public DateTime? UltimoAcceso { get; set; }

    /// <summary>
    /// Motivo de bloqueo si está bloqueado
    /// </summary>
    public string? MotivoBloqueo { get; set; }

    /// <summary>
    /// Indica si el usuario es administrador
    /// </summary>
    public bool EsAdministrador { get; set; }

    /// <summary>
    /// Lista de roles del usuario
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Teléfono del usuario
    /// </summary>
    public string? Telefono { get; set; }
} 