using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Interfaz para el servicio de notificaciones del sistema
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Envía una notificación a un usuario específico
    /// </summary>
    /// <param name="usuarioId">ID del usuario destinatario</param>
    /// <param name="titulo">Título de la notificación</param>
    /// <param name="mensaje">Mensaje de la notificación</param>
    /// <param name="tipo">Tipo de notificación</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> EnviarNotificacionAsync(Guid usuarioId, string titulo, string mensaje, string tipo = "Info");
    
    /// <summary>
    /// Envía una notificación a múltiples usuarios
    /// </summary>
    /// <param name="usuariosIds">Lista de IDs de usuarios destinatarios</param>
    /// <param name="titulo">Título de la notificación</param>
    /// <param name="mensaje">Mensaje de la notificación</param>
    /// <param name="tipo">Tipo de notificación</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> EnviarNotificacionMasivaAsync(List<Guid> usuariosIds, string titulo, string mensaje, string tipo = "Info");
    
    /// <summary>
    /// Envía una notificación push
    /// </summary>
    /// <param name="usuarioId">ID del usuario destinatario</param>
    /// <param name="titulo">Título de la notificación</param>
    /// <param name="mensaje">Mensaje de la notificación</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> EnviarNotificacionPushAsync(Guid usuarioId, string titulo, string mensaje);
    
    /// <summary>
    /// Marca una notificación como leída
    /// </summary>
    /// <param name="notificacionId">ID de la notificación</param>
    /// <param name="usuarioId">ID del usuario</param>
    /// <returns>True si se marcó correctamente</returns>
    Task<bool> MarcarComoLeidaAsync(Guid notificacionId, Guid usuarioId);
    
    /// <summary>
    /// Obtiene las notificaciones no leídas de un usuario
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <returns>Número de notificaciones no leídas</returns>
    Task<int> ObtenerNotificacionesNoLeidasAsync(Guid usuarioId);
    
    /// <summary>
    /// Envía una notificación utilizando un objeto Notification
    /// </summary>
    /// <param name="notification">Objeto Notification con todos los datos</param>
    /// <returns>True si se envió correctamente</returns>
    Task<bool> SendNotificationAsync(Notification notification);
} 