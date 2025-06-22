namespace RestaurantePro.Application.Core.Notificaciones.DTOs;

/// <summary>
/// DTO para representar una notificación
/// </summary>
public class NotificacionDto
{
    /// <summary>
    /// ID de la notificación
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Título de la notificación
    /// </summary>
    public string Titulo { get; set; } = string.Empty;
    
    /// <summary>
    /// Mensaje de la notificación
    /// </summary>
    public string Mensaje { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo de notificación
    /// </summary>
    public string Tipo { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha de creación de la notificación
    /// </summary>
    public DateTime FechaCreacion { get; set; }
    
    /// <summary>
    /// Fecha en que se leyó la notificación
    /// </summary>
    public DateTime? FechaLectura { get; set; }
    
    /// <summary>
    /// Indica si la notificación ha sido leída
    /// </summary>
    public bool EstaLeida { get; set; }
    
    /// <summary>
    /// ID de la entidad relacionada con la notificación
    /// </summary>
    public Guid? EntidadRelacionadaId { get; set; }
} 