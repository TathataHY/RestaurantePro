using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using RestaurantePro.Core.Events;
using RestaurantePro.Core.Services;
using RestaurantePro.Core.Interfaces.Services;

namespace RestaurantePro.Core.EventHandlers
{
    public class ComandaEventHandlers : 
        INotificationHandler<ComandaEstadoCambiadoEvent>,
        INotificationHandler<ComandaDetalleAgregadoEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<ComandaEventHandlers> _logger;

        public async Task Handle(ComandaEstadoCambiadoEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Comanda {ComandaId} cambió de estado {EstadoAnterior} a {NuevoEstado} por {Usuario}",
                notification.ComandaId,
                notification.EstadoAnterior,
                notification.NuevoEstado,
                notification.Usuario);

            await _notificationService.NotifyComandaStatusChangedAsync(
                notification.ComandaId,
                notification.NuevoEstado);
        }

        public async Task Handle(ComandaDetalleAgregadoEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Nuevo detalle agregado a comanda {ComandaId}: {PlatoId} x {Cantidad}",
                notification.ComandaId,
                notification.Detalle.PlatoId,
                notification.Detalle.Cantidad);

            await _notificationService.NotifyRoleAsync(
                "Cocinero",
                $"Nuevo plato agregado a comanda #{notification.ComandaId}");
        }
    }
} 