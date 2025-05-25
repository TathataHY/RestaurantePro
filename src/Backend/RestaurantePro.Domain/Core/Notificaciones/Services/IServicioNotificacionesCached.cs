namespace RestaurantePro.Domain.Core.Notificaciones.Services
{
    /// <summary>
    /// Interfaz extendida para el servicio de notificaciones con soporte de caché
    /// </summary>
    public interface IServicioNotificacionesCached : IServicioNotificaciones
    {
        /// <summary>
        /// Invalida toda la caché del servicio de notificaciones
        /// </summary>
        void InvalidarCache();
        
        /// <summary>
        /// Invalida la caché para un tipo específico de notificación
        /// </summary>
        /// <param name="tipoNotificacion">Tipo de notificación</param>
        void InvalidarCachePorTipo(TipoNotificacion tipoNotificacion);
        
        /// <summary>
        /// Invalida la caché para un destinatario específico
        /// </summary>
        /// <param name="destinatarioId">ID del destinatario</param>
        void InvalidarCachePorDestinatario(Guid destinatarioId);
    }
} 