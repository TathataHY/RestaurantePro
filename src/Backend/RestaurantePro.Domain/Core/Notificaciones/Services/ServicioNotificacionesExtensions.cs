namespace RestaurantePro.Domain.Core.Notificaciones.Services
{
    /// <summary>
    /// Extensiones para el servicio de notificaciones
    /// </summary>
    public static class ServicioNotificacionesExtensions
    {
        /// <summary>
        /// Envía una notificación previamente creada
        /// </summary>
        /// <param name="servicioNotificaciones">Servicio de notificaciones</param>
        /// <param name="notificacion">Notificación a enviar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>La notificación enviada</returns>
        public static async Task<Notificacion> EnviarNotificacionAsync(
            this IServicioNotificaciones servicioNotificaciones,
            Notificacion notificacion,
            CancellationToken cancellationToken = default)
        {
            if (servicioNotificaciones == null)
                throw new ArgumentNullException(nameof(servicioNotificaciones));
                
            if (notificacion == null)
                throw new ArgumentNullException(nameof(notificacion));
            
            // Obtenemos los valores necesarios de la notificación ya creada
            return await servicioNotificaciones.EnviarNotificacionAsync(
                notificacion.Titulo,
                notificacion.Mensaje,
                notificacion.Tipo,
                notificacion.DestinatarioId,
                notificacion.EntidadRelacionadaId,
                cancellationToken);
        }
    }
} 