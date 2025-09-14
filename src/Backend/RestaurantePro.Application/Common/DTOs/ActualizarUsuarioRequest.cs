using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Application.Common.DTOs;

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

    // Campos adicionales requeridos por el backend
    public int? NivelAcceso { get; set; }

    [Required(ErrorMessage = "El ID del usuario que autoriza es obligatorio")]
    public Guid UsuarioAutorizaId { get; set; }

    [Required(ErrorMessage = "El motivo de actualización es obligatorio")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "El motivo debe tener entre 10 y 500 caracteres")]
    public string MotivoActualizacion { get; set; } = string.Empty;

    public string? ObservacionesAdicionales { get; set; }

    public bool NotificarUsuario { get; set; } = true;

    public bool NotificarSupervisor { get; set; } = false;

    public bool RequiereAprobacion { get; set; } = false;

    public bool CrearBackup { get; set; } = true;

    public int Prioridad { get; set; } = 2;

    public List<string> DocumentosAdjuntos { get; set; } = new();

    public bool InvalidarSesionesActivas { get; set; } = false;

    public DateTime? FechaEfectivacambios { get; set; }
}
