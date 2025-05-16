namespace RestaurantePro.Domain.Core.SharedKernel.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de correo electrónico
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envía un correo electrónico
        /// </summary>
        /// <param name="to">Dirección de correo del destinatario</param>
        /// <param name="subject">Asunto del correo</param>
        /// <param name="body">Cuerpo del mensaje</param>
        /// <param name="isHtml">Indica si el cuerpo está en formato HTML</param>
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
        
        /// <summary>
        /// Envía un correo con archivos adjuntos
        /// </summary>
        /// <param name="to">Dirección de correo del destinatario</param>
        /// <param name="subject">Asunto del correo</param>
        /// <param name="body">Cuerpo del mensaje</param>
        /// <param name="attachments">Lista de archivos adjuntos</param>
        /// <param name="isHtml">Indica si el cuerpo está en formato HTML</param>
        Task SendEmailWithAttachmentsAsync(string to, string subject, string body, IEnumerable<string> attachments, bool isHtml = false);
        
        /// <summary>
        /// Envía un correo a múltiples destinatarios
        /// </summary>
        /// <param name="to">Lista de direcciones de correo de los destinatarios</param>
        /// <param name="subject">Asunto del correo</param>
        /// <param name="body">Cuerpo del mensaje</param>
        /// <param name="isHtml">Indica si el cuerpo está en formato HTML</param>
        Task SendBulkEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = false);
    }
} 