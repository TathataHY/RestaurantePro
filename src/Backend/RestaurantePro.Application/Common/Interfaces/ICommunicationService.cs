namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Servicio para comunicaciones externas (Email, SMS, Push)
/// Diferente de INotificationService que maneja notificaciones internas del sistema
/// </summary>
public interface ICommunicationService
{
    /// <summary>
    /// Envía una comunicación por email
    /// </summary>
    /// <param name="destinatario">Email del destinatario</param>
    /// <param name="asunto">Asunto del email</param>
    /// <param name="mensaje">Contenido del mensaje</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> EnviarEmailAsync(string destinatario, string asunto, string mensaje, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía una comunicación SMS
    /// </summary>
    /// <param name="telefono">Número de teléfono</param>
    /// <param name="mensaje">Contenido del mensaje</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> EnviarSmsAsync(string telefono, string mensaje, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía una comunicación push
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="titulo">Título de la notificación</param>
    /// <param name="mensaje">Contenido del mensaje</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> EnviarPushAsync(string usuarioId, string titulo, string mensaje, CancellationToken cancellationToken = default);
} 