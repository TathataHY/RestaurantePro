namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Interfaz para el servicio de notificaciones en tiempo real con SignalR
/// </summary>
public interface ISignalRService
{
    /// <summary>
    /// Envía una notificación en tiempo real a un usuario específico
    /// </summary>
    /// <param name="usuarioId">ID del usuario destinatario</param>
    /// <param name="titulo">Título de la notificación</param>
    /// <param name="mensaje">Mensaje de la notificación</param>
    /// <param name="tipo">Tipo de notificación (info, success, warning, error)</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> EnviarNotificacionAUsuarioAsync(Guid usuarioId, string titulo, string mensaje, string tipo = "info");
    
    /// <summary>
    /// Envía una notificación en tiempo real a múltiples usuarios
    /// </summary>
    /// <param name="usuariosIds">Lista de IDs de usuarios destinatarios</param>
    /// <param name="titulo">Título de la notificación</param>
    /// <param name="mensaje">Mensaje de la notificación</param>
    /// <param name="tipo">Tipo de notificación</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> EnviarNotificacionAUsuariosAsync(List<Guid> usuariosIds, string titulo, string mensaje, string tipo = "info");
    
    /// <summary>
    /// Envía una notificación a todos los usuarios de un rol específico
    /// </summary>
    /// <param name="rol">Rol de los usuarios (Administrador, Gerente, Mesero, etc.)</param>
    /// <param name="titulo">Título de la notificación</param>
    /// <param name="mensaje">Mensaje de la notificación</param>
    /// <param name="tipo">Tipo de notificación</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> EnviarNotificacionARolAsync(string rol, string titulo, string mensaje, string tipo = "info");
    
    /// <summary>
    /// Envía una notificación a todos los usuarios conectados
    /// </summary>
    /// <param name="titulo">Título de la notificación</param>
    /// <param name="mensaje">Mensaje de la notificación</param>
    /// <param name="tipo">Tipo de notificación</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> EnviarNotificacionGlobalAsync(string titulo, string mensaje, string tipo = "info");
    
    /// <summary>
    /// Envía actualización de estado de mesa en tiempo real
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <param name="estado">Nuevo estado de la mesa</param>
    /// <param name="detalles">Detalles adicionales</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> ActualizarEstadoMesaAsync(Guid mesaId, string estado, object? detalles = null);
    
    /// <summary>
    /// Envía actualización de comanda en tiempo real
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <param name="estado">Nuevo estado de la comanda</param>
    /// <param name="detalles">Detalles adicionales</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> ActualizarEstadoComandaAsync(Guid comandaId, string estado, object? detalles = null);
    
    /// <summary>
    /// Envía alerta de inventario bajo en tiempo real
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="nombreIngrediente">Nombre del ingrediente</param>
    /// <param name="stockActual">Stock actual</param>
    /// <param name="stockMinimo">Stock mínimo</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> EnviarAlertaInventarioAsync(Guid ingredienteId, string nombreIngrediente, decimal stockActual, decimal stockMinimo);
    
    /// <summary>
    /// Obtiene los usuarios conectados actualmente
    /// </summary>
    /// <returns>Lista de IDs de usuarios conectados</returns>
    Task<List<Guid>> ObtenerUsuariosConectadosAsync();
    
    /// <summary>
    /// Verifica si un usuario específico está conectado
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <returns>True si está conectado</returns>
    Task<bool> UsuarioEstaConectadoAsync(Guid usuarioId);
} 