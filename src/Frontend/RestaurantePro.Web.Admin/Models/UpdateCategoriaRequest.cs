using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

public class UpdateCategoriaRequest
{
    [Required(ErrorMessage = "El ID es requerido")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }

    [StringLength(50, ErrorMessage = "El tipo no puede exceder 50 caracteres")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El color es requerido")]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "El color debe ser un código hexadecimal válido (ej: #FF5733)")]
    public string Color { get; set; } = "#2196F3";

    [StringLength(10, ErrorMessage = "El icono no puede exceder 10 caracteres")]
    public string Icono { get; set; } = "🍽️";

    [Range(0, 1000, ErrorMessage = "El orden debe estar entre 0 y 1000")]
    public int Orden { get; set; } = 0;

    public bool Activa { get; set; } = true;
}
