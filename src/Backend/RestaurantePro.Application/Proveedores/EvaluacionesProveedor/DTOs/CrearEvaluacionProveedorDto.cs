using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.DTOs;

/// <summary>
/// DTO para crear una nueva evaluación de proveedor
/// </summary>
public class CrearEvaluacionProveedorDto
{
    /// <summary>
    /// ID del proveedor a evaluar
    /// </summary>
    [Required(ErrorMessage = "El ID del proveedor es obligatorio")]
    public Guid ProveedorId { get; set; }

    /// <summary>
    /// Calificación general del proveedor (1-5)
    /// </summary>
    [Required(ErrorMessage = "La calificación general es obligatoria")]
    [Range(1, 5, ErrorMessage = "La calificación general debe estar entre 1 y 5")]
    public int CalificacionGeneral { get; set; }

    /// <summary>
    /// Calificación de calidad de productos (1-5)
    /// </summary>
    [Required(ErrorMessage = "La calificación de calidad es obligatoria")]
    [Range(1, 5, ErrorMessage = "La calificación de calidad debe estar entre 1 y 5")]
    public int CalificacionCalidad { get; set; }

    /// <summary>
    /// Calificación de puntualidad en entregas (1-5)
    /// </summary>
    [Required(ErrorMessage = "La calificación de puntualidad es obligatoria")]
    [Range(1, 5, ErrorMessage = "La calificación de puntualidad debe estar entre 1 y 5")]
    public int CalificacionPuntualidad { get; set; }

    /// <summary>
    /// Calificación de comunicación (1-5)
    /// </summary>
    [Required(ErrorMessage = "La calificación de comunicación es obligatoria")]
    [Range(1, 5, ErrorMessage = "La calificación de comunicación debe estar entre 1 y 5")]
    public int CalificacionComunicacion { get; set; }

    /// <summary>
    /// Calificación de precios (1-5)
    /// </summary>
    [Required(ErrorMessage = "La calificación de precios es obligatoria")]
    [Range(1, 5, ErrorMessage = "La calificación de precios debe estar entre 1 y 5")]
    public int CalificacionPrecios { get; set; }

    /// <summary>
    /// Comentarios de la evaluación
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Los comentarios no pueden exceder los 1000 caracteres")]
    public string Comentarios { get; set; } = string.Empty;
} 