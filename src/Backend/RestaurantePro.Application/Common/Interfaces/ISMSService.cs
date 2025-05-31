namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Interfaz para el servicio de envío de mensajes SMS
/// </summary>
public interface ISMSService
{
    /// <summary>
    /// Envía un mensaje SMS simple
    /// </summary>
    /// <param name="phoneNumber">Número de teléfono del destinatario</param>
    /// <param name="message">Mensaje a enviar</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> SendSMSAsync(string phoneNumber, string message);
    
    /// <summary>
    /// Envía un mensaje SMS con datos adicionales para tracking
    /// </summary>
    /// <param name="phoneNumber">Número de teléfono del destinatario</param>
    /// <param name="message">Mensaje a enviar</param>
    /// <param name="clienteId">ID del cliente para tracking</param>
    /// <param name="tipoNotificacion">Tipo de notificación (Confirmacion, Recordatorio, etc.)</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> SendSMSWithTrackingAsync(string phoneNumber, string message, Guid? clienteId = null, string? tipoNotificacion = null);
    
    /// <summary>
    /// Envía mensajes SMS a múltiples destinatarios
    /// </summary>
    /// <param name="phoneNumbers">Lista de números de teléfono</param>
    /// <param name="message">Mensaje a enviar</param>
    /// <returns>True si se enviaron correctamente</returns>
    Task<bool> SendBulkSMSAsync(List<string> phoneNumbers, string message);
    
    /// <summary>
    /// Valida si un número de teléfono es válido para SMS
    /// </summary>
    /// <param name="phoneNumber">Número de teléfono a validar</param>
    /// <returns>True si el número es válido</returns>
    bool IsValidPhoneNumber(string phoneNumber);
    
    /// <summary>
    /// Obtiene el estado de entrega de un SMS
    /// </summary>
    /// <param name="messageId">ID del mensaje</param>
    /// <returns>Estado de entrega</returns>
    Task<string> GetDeliveryStatusAsync(string messageId);
} 