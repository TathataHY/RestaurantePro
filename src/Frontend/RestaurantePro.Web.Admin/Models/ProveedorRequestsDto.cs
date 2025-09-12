using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// Request para actualizar un proveedor
/// </summary>
public class ActualizarProveedorRequest
{
    [Required(ErrorMessage = "El ID del proveedor es obligatorio")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Telefono { get; set; }

    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
    public string? Direccion { get; set; }

    [StringLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres")]
    public string? Ciudad { get; set; }

    [StringLength(50, ErrorMessage = "El estado no puede exceder 50 caracteres")]
    public string? Estado { get; set; }

    [StringLength(10, ErrorMessage = "El código postal no puede exceder 10 caracteres")]
    public string? CodigoPostal { get; set; }

    [StringLength(100, ErrorMessage = "El país no puede exceder 100 caracteres")]
    public string? Pais { get; set; }

    [StringLength(50, ErrorMessage = "El RFC no puede exceder 50 caracteres")]
    public string? RFC { get; set; }

    [StringLength(100, ErrorMessage = "El contacto principal no puede exceder 100 caracteres")]
    public string? ContactoPrincipal { get; set; }

    [StringLength(20, ErrorMessage = "El teléfono de contacto no puede exceder 20 caracteres")]
    public string? TelefonoContacto { get; set; }

    [StringLength(100, ErrorMessage = "El email de contacto no puede exceder 100 caracteres")]
    [EmailAddress(ErrorMessage = "El formato del email de contacto no es válido")]
    public string? EmailContacto { get; set; }

    [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
    public string? Notas { get; set; }

    public bool Activo { get; set; } = true;
}
