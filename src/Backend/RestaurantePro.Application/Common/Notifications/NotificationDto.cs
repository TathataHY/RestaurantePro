namespace RestaurantePro.Application.Common.Notifications;

/// <summary>
/// DTO base para todas las notificaciones
/// </summary>
public abstract class NotificationDto
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Tipo { get; set; } = string.Empty;
}

/// <summary>
/// DTO para notificación de nueva comanda
/// </summary>
public class NuevaComandaNotificationDto : NotificationDto
{
    public Guid ComandaId { get; set; }
    public Guid MesaId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public List<ComandaItemNotificationDto> Items { get; set; } = new();
}

/// <summary>
/// DTO para item de comanda en notificaciones
/// </summary>
public class ComandaItemNotificationDto
{
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para notificación de actualización de comanda
/// </summary>
public class ComandaActualizadaNotificationDto : NotificationDto
{
    public Guid ComandaId { get; set; }
    public string NuevoEstado { get; set; } = string.Empty;
    public string? Comentario { get; set; }
}

/// <summary>
/// DTO para notificación de evento del sistema
/// </summary>
public class EventoSistemaNotificationDto : NotificationDto
{
    public string TipoEvento { get; set; } = string.Empty;
    public object Datos { get; set; } = new();
}

/// <summary>
/// DTO para notificación personal
/// </summary>
public class NotificacionPersonalDto : NotificationDto
{
    public string TipoNotificacion { get; set; } = string.Empty;
    public object Datos { get; set; } = new();
}

/// <summary>
/// DTO para notificación de grupo
/// </summary>
public class NotificacionGrupoDto : NotificationDto
{
    public string TipoNotificacion { get; set; } = string.Empty;
    public object Datos { get; set; } = new();
} 