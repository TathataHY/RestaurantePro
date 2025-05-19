namespace RestaurantePro.Domain.Core.Notificaciones.Services
{
    /// <summary>
    /// Interfaz para el servicio de notificaciones del sistema
    /// </summary>
    public interface IServicioNotificaciones
    {
        /// <summary>
        /// Envía una notificación al destinatario especificado
        /// </summary>
        Task<Notificacion> EnviarNotificacionAsync(
            string titulo,
            string mensaje,
            TipoNotificacion tipo,
            Guid destinatarioId,
            Guid? entidadRelacionadaId = null,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Envía una notificación a múltiples destinatarios
        /// </summary>
        Task<IEnumerable<Notificacion>> EnviarNotificacionMasivaAsync(
            string titulo,
            string mensaje,
            TipoNotificacion tipo,
            IEnumerable<Guid> destinatarioIds,
            Guid? entidadRelacionadaId = null,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        Task<bool> MarcarComoLeidaAsync(
            Guid notificacionId, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Obtiene las notificaciones de un destinatario
        /// </summary>
        Task<IEnumerable<Notificacion>> ObtenerNotificacionesAsync(
            Guid destinatarioId, 
            bool soloNoLeidas = false, 
            CancellationToken cancellationToken = default);
    }
} 