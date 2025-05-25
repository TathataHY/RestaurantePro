namespace RestaurantePro.Domain.Core.SharedKernel.Services.Notification
{
    /// <summary>
    /// Interfaz del servicio de notificaciones basadas en eventos de dominio
    /// </summary>
    public interface IEventBasedNotificationService
    {
        /// <summary>
        /// Suscribe un manejador de eventos para generar notificaciones cuando ocurra un evento específico
        /// </summary>
        /// <typeparam name="TEvent">Tipo de evento al que suscribirse</typeparam>
        /// <param name="nombreCanal">Nombre del canal de notificación (email, sms, push, etc.)</param>
        /// <param name="generadorNotificacion">Función que genera una notificación a partir del evento</param>
        /// <param name="selectorDestinatarios">Función que selecciona los destinatarios a partir del evento</param>
        void SuscribirseAEvento<TEvent>(
            string nombreCanal, 
            Func<TEvent, Task<Notificaciones.Entities.Notificacion>> generadorNotificacion, 
            Func<TEvent, Task<IEnumerable<Guid>>> selectorDestinatarios) 
            where TEvent : Base.Events.DomainEvent;
        
        /// <summary>
        /// Suscribe un manejador de eventos para generar notificaciones para usuarios con un rol específico
        /// </summary>
        /// <typeparam name="TEvent">Tipo de evento al que suscribirse</typeparam>
        /// <param name="nombreCanal">Nombre del canal de notificación (email, sms, push, etc.)</param>
        /// <param name="generadorNotificacion">Función que genera una notificación a partir del evento</param>
        /// <param name="rolNecesario">Rol que deben tener los usuarios para recibir la notificación</param>
        void SuscribirseAEventoPorRol<TEvent>(
            string nombreCanal, 
            Func<TEvent, Task<Notificaciones.Entities.Notificacion>> generadorNotificacion, 
            Usuarios.Enums.RolUsuario rolNecesario) 
            where TEvent : Base.Events.DomainEvent;
        
        /// <summary>
        /// Envía una notificación a un usuario específico
        /// </summary>
        /// <param name="usuarioId">ID del usuario destinatario</param>
        /// <param name="titulo">Título de la notificación</param>
        /// <param name="contenido">Contenido de la notificación</param>
        /// <param name="tipo">Tipo de notificación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se envió correctamente, False si no se encontró el usuario</returns>
        Task<bool> EnviarNotificacionAUsuarioAsync(
            Guid usuarioId, 
            string titulo, 
            string contenido, 
            Notificaciones.Enums.TipoNotificacion tipo, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Envía una notificación a todos los usuarios con un rol específico
        /// </summary>
        /// <param name="rol">Rol de los usuarios destinatarios</param>
        /// <param name="titulo">Título de la notificación</param>
        /// <param name="contenido">Contenido de la notificación</param>
        /// <param name="tipo">Tipo de notificación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task EnviarNotificacionAUsuariosConRolAsync(
            Usuarios.Enums.RolUsuario rol, 
            string titulo, 
            string contenido, 
            Notificaciones.Enums.TipoNotificacion tipo, 
            CancellationToken cancellationToken = default);
    }
} 