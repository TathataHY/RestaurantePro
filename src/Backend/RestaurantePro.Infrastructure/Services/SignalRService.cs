using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Notifications;

namespace RestaurantePro.Infrastructure.Services;

/// <summary>
/// Servicio de SignalR para notificaciones en tiempo real
/// Implementa la interfaz ISignalRService para comunicación bidireccional
/// </summary>
public class SignalRService : ISignalRService
{
    private readonly ISignalRHub _signalRHub;
    private readonly ILogger<SignalRService> _logger;

    public SignalRService(
        ISignalRHub signalRHub,
        ILogger<SignalRService> logger)
    {
        _signalRHub = signalRHub;
        _logger = logger;
    }

    /// <summary>
    /// Notifica a la cocina sobre una nueva comanda
    /// </summary>
    public async Task NotificarNuevaComandaAsync(NuevaComandaNotificationDto comanda)
    {
        try
        {
            _logger.LogInformation("Notificando nueva comanda {ComandaId} a la cocina", comanda.ComandaId);
            
            await _signalRHub.SendToGroupAsync("Cocina", "NuevaComanda", comanda);
                
            _logger.LogInformation("Notificación de nueva comanda {ComandaId} enviada exitosamente", comanda.ComandaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar nueva comanda {ComandaId}", comanda.ComandaId);
            throw;
        }
    }

    /// <summary>
    /// Notifica actualización de estado de comanda
    /// </summary>
    public async Task NotificarActualizacionComandaAsync(Guid comandaId, string nuevoEstado, string? comentario = null)
    {
        try
        {
            _logger.LogInformation("Notificando actualización de comanda {ComandaId} a estado {Estado}", comandaId, nuevoEstado);
            
            var notificacion = new
            {
                ComandaId = comandaId,
                NuevoEstado = nuevoEstado,
                Comentario = comentario,
                Timestamp = DateTime.UtcNow
            };

            await _signalRHub.SendToAllAsync("ComandaActualizada", notificacion);
                
            _logger.LogInformation("Notificación de actualización de comanda {ComandaId} enviada exitosamente", comandaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar actualización de comanda {ComandaId}", comandaId);
            throw;
        }
    }

    /// <summary>
    /// Notifica a todos los clientes sobre un evento del sistema
    /// </summary>
    public async Task NotificarEventoSistemaAsync(string tipoEvento, object datos)
    {
        try
        {
            _logger.LogInformation("Notificando evento del sistema: {TipoEvento}", tipoEvento);
            
            var notificacion = new
            {
                TipoEvento = tipoEvento,
                Datos = datos,
                Timestamp = DateTime.UtcNow
            };

            await _signalRHub.SendToAllAsync("EventoSistema", notificacion);
                
            _logger.LogInformation("Notificación de evento del sistema {TipoEvento} enviada exitosamente", tipoEvento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar evento del sistema {TipoEvento}", tipoEvento);
            throw;
        }
    }

    /// <summary>
    /// Notifica a un usuario específico
    /// </summary>
    public async Task NotificarUsuarioAsync(string userId, string tipoNotificacion, object datos)
    {
        try
        {
            _logger.LogInformation("Notificando usuario {UserId} con tipo {TipoNotificacion}", userId, tipoNotificacion);
            
            var notificacion = new
            {
                TipoNotificacion = tipoNotificacion,
                Datos = datos,
                Timestamp = DateTime.UtcNow
            };

            await _signalRHub.SendToUserAsync(userId, "NotificacionPersonal", notificacion);
                
            _logger.LogInformation("Notificación personal enviada exitosamente al usuario {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar usuario {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Notifica a un grupo específico
    /// </summary>
    public async Task NotificarGrupoAsync(string nombreGrupo, string tipoNotificacion, object datos)
    {
        try
        {
            _logger.LogInformation("Notificando grupo {Grupo} con tipo {TipoNotificacion}", nombreGrupo, tipoNotificacion);
            
            var notificacion = new
            {
                TipoNotificacion = tipoNotificacion,
                Datos = datos,
                Timestamp = DateTime.UtcNow
            };

            await _signalRHub.SendToGroupAsync(nombreGrupo, "NotificacionGrupo", notificacion);
                
            _logger.LogInformation("Notificación de grupo enviada exitosamente al grupo {Grupo}", nombreGrupo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar grupo {Grupo}", nombreGrupo);
            throw;
        }
    }
} 