namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.DTOs;

/// <summary>
/// DTO para representar el promedio de evaluaciones de un proveedor
/// </summary>
public class PromedioEvaluacionesDto
{
    /// <summary>
    /// ID del proveedor
    /// </summary>
    public Guid ProveedorId { get; set; }

    /// <summary>
    /// Nombre del proveedor
    /// </summary>
    public string NombreProveedor { get; set; } = string.Empty;

    /// <summary>
    /// Promedio general de calificaciones
    /// </summary>
    public decimal PromedioGeneral { get; set; }

    /// <summary>
    /// Promedio de calificación de calidad
    /// </summary>
    public decimal PromedioCalidad { get; set; }

    /// <summary>
    /// Promedio de calificación de puntualidad
    /// </summary>
    public decimal PromedioPuntualidad { get; set; }

    /// <summary>
    /// Promedio de calificación de comunicación
    /// </summary>
    public decimal PromedioComunicacion { get; set; }

    /// <summary>
    /// Promedio de calificación de precios
    /// </summary>
    public decimal PromedioPrecios { get; set; }

    /// <summary>
    /// Promedio ponderado general
    /// </summary>
    public decimal PromedioPonderado { get; set; }

    /// <summary>
    /// Total de evaluaciones consideradas
    /// </summary>
    public int TotalEvaluaciones { get; set; }

    /// <summary>
    /// Fecha de la última evaluación
    /// </summary>
    public DateTime? FechaUltimaEvaluacion { get; set; }

    /// <summary>
    /// Calificación más alta recibida
    /// </summary>
    public decimal CalificacionMaxima { get; set; }

    /// <summary>
    /// Calificación más baja recibida
    /// </summary>
    public decimal CalificacionMinima { get; set; }
} 