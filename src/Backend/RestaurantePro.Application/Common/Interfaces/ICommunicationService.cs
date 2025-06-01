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

    /// <summary>
    /// Envía notificación general
    /// </summary>
    /// <param name="destinatarios">Array de destinatarios</param>
    /// <param name="titulo">Título de la notificación</param>
    /// <param name="mensaje">Contenido del mensaje</param>
    /// <param name="tipo">Tipo de comunicación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> EnviarNotificacionAsync(string[] destinatarios, string titulo, string mensaje, TipoComunicacion tipo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía confirmación de reservación
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="reservacion">Datos de la reservación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> EnviarConfirmacionReservacionAsync(Guid clienteId, ReservacionDto reservacion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía notificación específica sobre modificación de reservación
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="reservacionId">ID de la reservación</param>
    /// <param name="motivoModificacion">Motivo de la modificación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Task</returns>
    Task EnviarNotificacionModificacionReservacionAsync(Guid clienteId, Guid reservacionId, string motivoModificacion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía notificación interna sobre modificación de reservación
    /// </summary>
    /// <param name="reservacionId">ID de la reservación</param>
    /// <param name="motivoModificacion">Motivo de la modificación</param>
    /// <param name="usuarioId">ID del usuario que realizó la modificación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Task</returns>
    Task EnviarNotificacionInternaModificacionAsync(Guid reservacionId, string motivoModificacion, string usuarioId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Tipos de comunicación para mensajes externos (Email, SMS, Push)
/// </summary>
public enum TipoComunicacion
{
    Informacion,
    Advertencia,
    Error,
    Exito,
    TransferenciaMesa,
    ReservacionCreada,
    ReservacionModificada,
    ReservacionCancelada,
    ComandaCreada,
    ComandaFinalizada,
    FacturaGenerada,
    PagoRecibido,
    StockBajo,
    PromocionAplicada
} 