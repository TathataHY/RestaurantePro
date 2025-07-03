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
} 