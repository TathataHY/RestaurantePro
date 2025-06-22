namespace RestaurantePro.Application.Core.Notificaciones.Commands.CrearNotificacion;

/// <summary>
/// Comando para crear una nueva notificación
/// </summary>
public class CrearNotificacionCommand : IRequest<Result<NotificacionDto>>
{
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
    public string Tipo { get; set; } = "Informativa";
    
    /// <summary>
    /// ID del destinatario de la notificación
    /// </summary>
    public Guid DestinatarioId { get; set; }
    
    /// <summary>
    /// ID de la entidad relacionada con la notificación (opcional)
    /// </summary>
    public Guid? EntidadRelacionadaId { get; set; }
} 