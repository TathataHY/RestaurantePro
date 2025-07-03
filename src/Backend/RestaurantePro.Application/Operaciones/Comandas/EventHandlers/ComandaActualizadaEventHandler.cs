using MediatR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Comandas.Events;

namespace RestaurantePro.Application.Operaciones.Comandas.EventHandlers;

/// <summary>
/// Event handler para notificar cuando se actualiza el estado de una comanda
/// Envía notificación en tiempo real a todos los clientes conectados
/// </summary>
public class ComandaActualizadaEventHandler : INotificationHandler<ComandaActualizadaNotificationEvent>
{
    private readonly ISignalRService _signalRService;
    private readonly ILogger<ComandaActualizadaEventHandler> _logger;

    public ComandaActualizadaEventHandler(
        ISignalRService signalRService,
        ILogger<ComandaActualizadaEventHandler> logger)
    {
        _signalRService = signalRService;
        _logger = logger;
    }

    public async Task Handle(ComandaActualizadaNotificationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Procesando evento ComandaActualizadaNotificationEvent para comanda {ComandaId} - Estado: {Estado}", 
                notification.ComandaId, notification.NuevoEstado);

            // Notificar actualización en tiempo real
            await _signalRService.NotificarActualizacionComandaAsync(
                notification.ComandaId, 
                notification.NuevoEstado.ToString(), 
                null); // No hay comentario en este evento

            _logger.LogInformation("Notificación de actualización de comanda {ComandaId} enviada exitosamente", notification.ComandaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar evento ComandaActualizadaNotificationEvent para comanda {ComandaId}", notification.ComandaId);
            throw;
        }
    }
} 