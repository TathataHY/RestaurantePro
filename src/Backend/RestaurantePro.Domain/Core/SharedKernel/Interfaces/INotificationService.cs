namespace RestaurantePro.Domain.Core.SharedKernel.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de notificaciones en tiempo real
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Envía una notificación a un usuario específico
        /// </summary>
        /// <param name="userId">ID del usuario destinatario</param>
        /// <param name="message">Mensaje de la notificación</param>
        /// <param name="type">Tipo de notificación (info, success, warning, error)</param>
        Task NotifyAsync(string userId, string message, string type = "info");
        
        /// <summary>
        /// Envía una notificación a múltiples usuarios
        /// </summary>
        /// <param name="userIds">Lista de IDs de usuarios destinatarios</param>
        /// <param name="message">Mensaje de la notificación</param>
        /// <param name="type">Tipo de notificación (info, success, warning, error)</param>
        Task NotifyManyAsync(IEnumerable<string> userIds, string message, string type = "info");
        
        /// <summary>
        /// Envía una notificación a todos los usuarios conectados
        /// </summary>
        /// <param name="message">Mensaje de la notificación</param>
        /// <param name="type">Tipo de notificación (info, success, warning, error)</param>
        Task NotifyAllAsync(string message, string type = "info");
        
        /// <summary>
        /// Envía una notificación a un grupo específico de usuarios
        /// </summary>
        /// <param name="groupName">Nombre del grupo</param>
        /// <param name="message">Mensaje de la notificación</param>
        /// <param name="type">Tipo de notificación (info, success, warning, error)</param>
        Task NotifyGroupAsync(string groupName, string message, string type = "info");
    }
} 