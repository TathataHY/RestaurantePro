using System;

namespace RestaurantePro.Application.Common.Models
{
    /// <summary>
    /// Modelo que representa una notificación del sistema
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// ID del usuario destinatario
        /// </summary>
        public Guid UserId { get; set; }
        
        /// <summary>
        /// Título de la notificación
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Mensaje de la notificación
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Tipo de notificación (Facturación, Inventario, Sistema, etc.)
        /// </summary>
        public string Type { get; set; } = "Info";
        
        /// <summary>
        /// Prioridad de la notificación (Alta, Media, Baja)
        /// </summary>
        public string Priority { get; set; } = "Media";
        
        /// <summary>
        /// Fecha de creación de la notificación
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Indica si la notificación ha sido leída
        /// </summary>
        public bool IsRead { get; set; } = false;
        
        /// <summary>
        /// ID de la entidad relacionada con esta notificación
        /// </summary>
        public Guid? RelatedEntityId { get; set; }
    }
} 