using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Application.Core.Productos.DTOs;

/// <summary>
/// DTO para crear una nueva categoría de productos
/// </summary>
public class CrearCategoriaDto
{
    /// <summary>
    /// Nombre de la categoría
    /// </summary>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la categoría
    /// </summary>
    [StringLength(500, ErrorMessage = "La descripción no debe superar los 500 caracteres")]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Color asociado a la categoría para UI (formato hexadecimal)
    /// </summary>
    [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "El color debe ser un código hexadecimal válido (ej: #FF0000)")]
    public string? Color { get; set; }

    /// <summary>
    /// Icono asociado a la categoría
    /// </summary>
    [StringLength(50, ErrorMessage = "El icono no debe superar los 50 caracteres")]
    public string? Icono { get; set; }

    /// <summary>
    /// Orden de visualización
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "El orden debe ser un número positivo")]
    public int Orden { get; set; } = 0;

    /// <summary>
    /// Indica si la categoría está activa
    /// </summary>
    public bool Activa { get; set; } = true;
}
