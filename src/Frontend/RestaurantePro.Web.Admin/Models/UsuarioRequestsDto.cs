using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// Request para crear un nuevo usuario
/// </summary>
public class CrearUsuarioRequest
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder 50 caracteres")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio")]
    [StringLength(200, ErrorMessage = "El nombre completo no puede exceder 200 caracteres")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string Contrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmación de contraseña es obligatoria")]
    [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmarContrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio")]
    public string Rol { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Telefono { get; set; }

    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
    public string? Direccion { get; set; }

    public bool Activo { get; set; } = true;

    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
}

/// <summary>
/// Request para actualizar un usuario existente
/// </summary>
public class ActualizarUsuarioRequest
{
    [Required(ErrorMessage = "El ID del usuario es obligatorio")]
    public Guid Id { get; set; }

    [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder 50 caracteres")]
    public string? NombreUsuario { get; set; }

    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string? Email { get; set; }

    [StringLength(200, ErrorMessage = "El nombre completo no puede exceder 200 caracteres")]
    public string? NombreCompleto { get; set; }

    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string? Nombre { get; set; }

    [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
    public string? Apellido { get; set; }

    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string? Contrasena { get; set; }

    [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
    public string? ConfirmarContrasena { get; set; }

    public string? Rol { get; set; }

    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Telefono { get; set; }

    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
    public string? Direccion { get; set; }

    public bool? Activo { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
}

/// <summary>
/// Request para cambiar el estado de un usuario
/// </summary>
public class CambiarEstadoUsuarioRequest
{
    [Required(ErrorMessage = "El ID del usuario es obligatorio")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio")]
    public bool Activo { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
}

/// <summary>
/// Request para cambiar la contraseña de un usuario
/// </summary>
public class CambiarContrasenaRequest
{
    [Required(ErrorMessage = "El ID del usuario es obligatorio")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "La contraseña actual es obligatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string ContrasenaActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string NuevaContrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmación de contraseña es obligatoria")]
    [Compare("NuevaContrasena", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmarNuevaContrasena { get; set; } = string.Empty;
}

/// <summary>
/// Request para resetear la contraseña de un usuario
/// </summary>
public class ResetearContrasenaRequest
{
    [Required(ErrorMessage = "El ID del usuario es obligatorio")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string NuevaContrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmación de contraseña es obligatoria")]
    [Compare("NuevaContrasena", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmarNuevaContrasena { get; set; } = string.Empty;
}

/// <summary>
/// Request para asignar roles a un usuario
/// </summary>
public class AsignarRolesRequest
{
    [Required(ErrorMessage = "El ID del usuario es obligatorio")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Los roles son obligatorios")]
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// Request para filtros de usuarios
/// </summary>
public class FiltroUsuariosRequest
{
    public string? Busqueda { get; set; }
    public string? Rol { get; set; }
    public bool? Activo { get; set; }
    public DateTime? FechaCreacionDesde { get; set; }
    public DateTime? FechaCreacionHasta { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? OrdenarPor { get; set; }
    public bool OrdenDescendente { get; set; } = false;
}
