using MediatR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Notifications;
using RestaurantePro.Application.Operaciones.Comandas.Events;

namespace RestaurantePro.Application.Operaciones.Comandas.EventHandlers;

/// <summary>
/// Event handler para notificar cuando se crea una nueva comanda
/// Envía notificación en tiempo real a la cocina
/// </summary>
public class ComandaCreadaEventHandler : INotificationHandler<ComandaCreadaNotificationEvent>
{
    private readonly ISignalRService _signalRService;
    private readonly ILogger<ComandaCreadaEventHandler> _logger;

    public ComandaCreadaEventHandler(
        ISignalRService signalRService,
        ILogger<ComandaCreadaEventHandler> logger)
    {
        _signalRService = signalRService;
        _logger = logger;
    }

    public async Task Handle(ComandaCreadaNotificationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Procesando evento ComandaCreadaNotificationEvent para comanda {ComandaId}", notification.ComandaId);

            // Crear DTO para la notificación
            var comandaDto = new NuevaComandaNotificationDto
            {
                ComandaId = notification.ComandaId,
                MesaId = notification.MesaId,
                Estado = "Pendiente", // Estado inicial por defecto
                FechaCreacion = DateTime.UtcNow,
                Items = new List<ComandaItemNotificationDto>() // Los items se agregarán cuando se procesen
            };

            // Notificar a la cocina en tiempo real
            await _signalRService.NotificarNuevaComandaAsync(comandaDto);

            _logger.LogInformation("Notificación de nueva comanda {ComandaId} enviada exitosamente", notification.ComandaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar evento ComandaCreadaNotificationEvent para comanda {ComandaId}", notification.ComandaId);
            throw;
        }
    }
} 