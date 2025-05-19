namespace RestaurantePro.Domain.Core.Notificaciones.Events
{
    /// <summary>
    /// Evento de dominio que se emite cuando se crea una nueva notificación
    /// </summary>
    public class NotificacionCreada : DomainEvent
    {
        /// <summary>
        /// ID de la notificación creada
        /// </summary>
        public Guid NotificacionId { get; }
        
        /// <summary>
        /// Título de la notificación
        /// </summary>
        public string Titulo { get; }
        
        /// <summary>
        /// Tipo de notificación
        /// </summary>
        public TipoNotificacion Tipo { get; }
        
        /// <summary>
        /// ID del destinatario de la notificación
        /// </summary>
        public Guid DestinatarioId { get; }

        /// <summary>
        /// Constructor del evento
        /// </summary>
        public NotificacionCreada(
            Guid notificacionId,
            string titulo,
            TipoNotificacion tipo,
            Guid destinatarioId) : base()
        {
            NotificacionId = notificacionId;
            Titulo = titulo;
            Tipo = tipo;
            DestinatarioId = destinatarioId;
        }
    }
} 