namespace RestaurantePro.Application.Common.DTOs;

/// <summary>
/// DTO base que contiene propiedades comunes de auditoría
/// Todos los DTOs de entidades principales deben heredar de esta clase
/// </summary>
public abstract class BaseDto
{
    /// <summary>
    /// Identificador único de la entidad
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Fecha y hora de creación de la entidad
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Fecha y hora de la última modificación
    /// </summary>
    public DateTime? FechaModificacion { get; set; }

    /// <summary>
    /// Usuario que creó la entidad
    /// </summary>
    public string CreadoPor { get; set; } = string.Empty;

    /// <summary>
    /// Usuario que realizó la última modificación
    /// </summary>
    public string? ModificadoPor { get; set; }

    /// <summary>
    /// Indica si la entidad está activa
    /// </summary>
    public bool Activo { get; set; } = true;
} 