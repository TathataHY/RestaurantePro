using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RestaurantePro.Core.Services;
using RestaurantePro.Core.Events;
using RestaurantePro.Core.Interfaces.Services;

namespace RestaurantePro.Core.EventHandlers
{
    public class ComandaCreatedEventHandler : INotificationHandler<ComandaCreatedEvent>
    {
        private readonly ILogger<ComandaCreatedEventHandler> _logger;
        private readonly INotificationService _notificationService;

        public ComandaCreatedEventHandler(
            ILogger<ComandaCreatedEventHandler> logger,
            INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task Handle(ComandaCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Comanda {notification.ComandaId} creada");
            await _notificationService.NotifyRoleAsync(
                "Cocinero", 
                $"Nueva comanda #{notification.ComandaId} creada para la mesa {notification.MesaId}"
            );
        }
    }
} 