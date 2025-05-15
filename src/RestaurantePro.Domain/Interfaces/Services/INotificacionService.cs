using System.Threading.Tasks;

namespace RestaurantePro.Domain.Interfaces.Services
{
    /// <summary>
    /// Servicio para envío de notificaciones
    /// </summary>
    public interface INotificacionService
    {
        /// <summary>
        /// Envía una notificación a un usuario específico
        /// </summary>
        /// <param name="usuarioId">ID del usuario destinatario</param>
        /// <param name="titulo">Título de la notificación</param>
        /// <param name="mensaje">Contenido de la notificación</param>
        /// <param name="datos">Datos adicionales (opcional)</param>
        /// <returns>True si se envió correctamente, false en caso contrario</returns>
        Task<bool> EnviarNotificacionAsync(int usuarioId, string titulo, string mensaje, object datos = null);

        /// <summary>
        /// Envía una notificación a un grupo de usuarios
        /// </summary>
        /// <param name="grupo">Nombre del grupo de usuarios</param>
        /// <param name="titulo">Título de la notificación</param>
        /// <param name="mensaje">Contenido de la notificación</param>
        /// <param name="datos">Datos adicionales (opcional)</param>
        /// <returns>True si se envió correctamente, false en caso contrario</returns>
        Task<bool> EnviarNotificacionGrupoAsync(string grupo, string titulo, string mensaje, object datos = null);

        /// <summary>
        /// Envía una notificación a todos los usuarios
        /// </summary>
        /// <param name="titulo">Título de la notificación</param>
        /// <param name="mensaje">Contenido de la notificación</param>
        /// <param name="datos">Datos adicionales (opcional)</param>
        /// <returns>True si se envió correctamente, false en caso contrario</returns>
        Task<bool> EnviarNotificacionGlobalAsync(string titulo, string mensaje, object datos = null);

        /// <summary>
        /// Envía un correo electrónico a un destinatario
        /// </summary>
        /// <param name="destinatario">Email del destinatario</param>
        /// <param name="asunto">Asunto del correo</param>
        /// <param name="cuerpo">Contenido del correo</param>
        /// <param name="esHtml">Indica si el contenido es HTML</param>
        /// <returns>True si se envió correctamente, false en caso contrario</returns>
        Task<bool> EnviarEmailAsync(string destinatario, string asunto, string cuerpo, bool esHtml = true);
    }
} 