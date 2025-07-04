using RestaurantePro.Application.Common.Notifications;

namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Interfaz para el servicio de notificaciones en tiempo real con SignalR
/// Sigue Clean Architecture - solo define contratos sin dependencias externas
/// </summary>
public interface ISignalRService
{
    /// <summary>
    /// Notifica a la cocina sobre una nueva comanda
    /// </summary>
    /// <param name="comanda">Datos de la comanda</param>
    Task NotificarNuevaComandaAsync(NuevaComandaNotificationDto comanda);

    /// <summary>
    /// Notifica actualización de estado de comanda
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <param name="nuevoEstado">Nuevo estado</param>
    /// <param name="comentario">Comentario opcional</param>
    Task NotificarActualizacionComandaAsync(Guid comandaId, string nuevoEstado, string? comentario = null);

    /// <summary>
    /// Notifica a todos los clientes sobre un evento del sistema
    /// </summary>
    /// <param name="tipoEvento">Tipo de evento</param>
    /// <param name="datos">Datos del evento</param>
    Task NotificarEventoSistemaAsync(string tipoEvento, object datos);

    /// <summary>
    /// Notifica a un usuario específico
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="tipoNotificacion">Tipo de notificación</param>
    /// <param name="datos">Datos de la notificación</param>
    Task NotificarUsuarioAsync(string userId, string tipoNotificacion, object datos);

    /// <summary>
    /// Notifica a un grupo específico
    /// </summary>
    /// <param name="nombreGrupo">Nombre del grupo</param>
    /// <param name="tipoNotificacion">Tipo de notificación</param>
    /// <param name="datos">Datos de la notificación</param>
    Task NotificarGrupoAsync(string nombreGrupo, string tipoNotificacion, object datos);

    // Métodos adicionales para compatibilidad con tests de Infrastructure

    /// <summary>
    /// Envía una notificación a un usuario específico
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="titulo">Título de la notificación</param>
    /// <param name="mensaje">Mensaje de la notificación</param>
    /// <param name="tipo">Tipo de notificación</param>
    Task EnviarNotificacionAUsuarioAsync(Guid usuarioId, string titulo, string mensaje, string tipo = "Info");

    /// <summary>
    /// Envía una notificación a múltiples usuarios
    /// </summary>
    /// <param name="usuariosIds">Lista de IDs de usuarios</param>
    /// <param name="titulo">Título de la notificación</param>
    /// <param name="mensaje">Mensaje de la notificación</param>
    /// <param name="tipo">Tipo de notificación</param>
    Task EnviarNotificacionAUsuariosAsync(List<Guid> usuariosIds, string titulo, string mensaje, string tipo = "Info");

    /// <summary>
    /// Envía una notificación a un rol específico
    /// </summary>
    /// <param name="rol">Rol de los usuarios</param>
    /// <param name="titulo">Título de la notificación</param>
    /// <param name="mensaje">Mensaje de la notificación</param>
    /// <param name="tipo">Tipo de notificación</param>
    Task EnviarNotificacionARolAsync(string rol, string titulo, string mensaje, string tipo = "Info");

    /// <summary>
    /// Envía una notificación global a todos los usuarios
    /// </summary>
    /// <param name="titulo">Título de la notificación</param>
    /// <param name="mensaje">Mensaje de la notificación</param>
    /// <param name="tipo">Tipo de notificación</param>
    Task EnviarNotificacionGlobalAsync(string titulo, string mensaje, string tipo = "Info");

    /// <summary>
    /// Actualiza el estado de una mesa
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <param name="estado">Nuevo estado</param>
    /// <param name="detalles">Detalles adicionales</param>
    Task ActualizarEstadoMesaAsync(Guid mesaId, string estado, object detalles);

    /// <summary>
    /// Actualiza el estado de una comanda
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <param name="estado">Nuevo estado</param>
    /// <param name="detalles">Detalles adicionales</param>
    Task ActualizarEstadoComandaAsync(Guid comandaId, string estado, object detalles);

    /// <summary>
    /// Envía una alerta de inventario
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="nombreIngrediente">Nombre del ingrediente</param>
    /// <param name="stockActual">Stock actual</param>
    /// <param name="stockMinimo">Stock mínimo</param>
    Task EnviarAlertaInventarioAsync(Guid ingredienteId, string nombreIngrediente, decimal stockActual, decimal stockMinimo);

    /// <summary>
    /// Obtiene la lista de usuarios conectados
    /// </summary>
    /// <returns>Lista de IDs de usuarios conectados</returns>
    Task<List<Guid>> ObtenerUsuariosConectadosAsync();

    /// <summary>
    /// Verifica si un usuario está conectado
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <returns>True si el usuario está conectado</returns>
    Task<bool> UsuarioEstaConectadoAsync(Guid usuarioId);
} 