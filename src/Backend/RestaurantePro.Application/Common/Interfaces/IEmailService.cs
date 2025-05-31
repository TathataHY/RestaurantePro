namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Interfaz para el servicio de envío de correos electrónicos
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envía un correo electrónico simple
    /// </summary>
    /// <param name="to">Destinatario</param>
    /// <param name="subject">Asunto</param>
    /// <param name="body">Cuerpo del mensaje</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> SendEmailAsync(string to, string subject, string body);
    
    /// <summary>
    /// Envía un correo electrónico con archivo adjunto
    /// </summary>
    /// <param name="to">Destinatario</param>
    /// <param name="subject">Asunto</param>
    /// <param name="body">Cuerpo del mensaje</param>
    /// <param name="attachmentPath">Ruta del archivo adjunto</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath);
    
    /// <summary>
    /// Envía un correo electrónico a múltiples destinatarios
    /// </summary>
    /// <param name="toList">Lista de destinatarios</param>
    /// <param name="subject">Asunto</param>
    /// <param name="body">Cuerpo del mensaje</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> SendBulkEmailAsync(List<string> toList, string subject, string body);
    
    /// <summary>
    /// Envía un correo electrónico HTML
    /// </summary>
    /// <param name="to">Destinatario</param>
    /// <param name="subject">Asunto</param>
    /// <param name="htmlBody">Cuerpo HTML del mensaje</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> SendHtmlEmailAsync(string to, string subject, string htmlBody);
} 