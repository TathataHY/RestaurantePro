namespace RestaurantePro.Domain.Inventario.Notificaciones.Events
{

    /// <summary>
    /// Evento de dominio emitido cuando una notificación es marcada como leída
    /// </summary>
    public class NotificacionLeida : DomainEvent
    {
        /// <summary>
        /// ID de la notificación leída
        /// </summary>
        public Guid NotificacionId { get; }
        
        /// <summary>
        /// ID del destinatario que leyó la notificación
        /// </summary>
        public Guid DestinatarioId { get; }
        
        /// <summary>
        /// Constructor del evento NotificacionLeida
        /// </summary>
        public NotificacionLeida(Guid notificacionId, Guid destinatarioId)
        {
            NotificacionId = notificacionId;
            DestinatarioId = destinatarioId;
        }
    }
} 