namespace RestaurantePro.Domain.Core.Notificaciones.Events
{
    /// <summary>
    /// Evento de dominio que se emite cuando se marca una notificación como leída
    /// </summary>
    public class NotificacionLeida : DomainEvent
    {
        /// <summary>
        /// ID de la notificación que fue leída
        /// </summary>
        public Guid NotificacionId { get; }
        
        /// <summary>
        /// ID del destinatario de la notificación
        /// </summary>
        public Guid DestinatarioId { get; }

        /// <summary>
        /// Constructor del evento
        /// </summary>
        public NotificacionLeida(
            Guid notificacionId,
            Guid destinatarioId) : base()
        {
            NotificacionId = notificacionId;
            DestinatarioId = destinatarioId;
        }
    }
} 