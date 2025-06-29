namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.DTOs;

/// <summary>
/// DTO para representar una evaluación de proveedor
/// </summary>
public class EvaluacionProveedorDto
{
    /// <summary>
    /// Identificador único de la evaluación
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del proveedor evaluado
    /// </summary>
    public Guid ProveedorId { get; set; }

    /// <summary>
    /// Nombre del proveedor evaluado
    /// </summary>
    public string NombreProveedor { get; set; } = string.Empty;

    /// <summary>
    /// ID del usuario que realizó la evaluación
    /// </summary>
    public Guid EvaluadorId { get; set; }

    /// <summary>
    /// Nombre del evaluador
    /// </summary>
    public string NombreEvaluador { get; set; } = string.Empty;

    /// <summary>
    /// Calificación general del proveedor (1-5)
    /// </summary>
    public int CalificacionGeneral { get; set; }

    /// <summary>
    /// Calificación de calidad de productos (1-5)
    /// </summary>
    public int CalificacionCalidad { get; set; }

    /// <summary>
    /// Calificación de puntualidad en entregas (1-5)
    /// </summary>
    public int CalificacionPuntualidad { get; set; }

    /// <summary>
    /// Calificación de comunicación (1-5)
    /// </summary>
    public int CalificacionComunicacion { get; set; }

    /// <summary>
    /// Calificación de precios (1-5)
    /// </summary>
    public int CalificacionPrecios { get; set; }

    /// <summary>
    /// Promedio ponderado de todas las calificaciones
    /// </summary>
    public decimal PromedioPonderado { get; set; }

    /// <summary>
    /// Comentarios de la evaluación
    /// </summary>
    public string Comentarios { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de la evaluación
    /// </summary>
    public DateTime FechaEvaluacion { get; set; }

    /// <summary>
    /// Fecha de la última actualización
    /// </summary>
    public DateTime FechaActualizacion { get; set; }

    /// <summary>
    /// Indica si la evaluación está activa
    /// </summary>
    public bool Activa { get; set; }

    /// <summary>
    /// Usuario que creó la evaluación
    /// </summary>
    public string CreadoPor { get; set; } = string.Empty;

    /// <summary>
    /// Usuario que actualizó por última vez la evaluación
    /// </summary>
    public string ActualizadoPor { get; set; } = string.Empty;
} 