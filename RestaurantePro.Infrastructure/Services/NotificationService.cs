using Microsoft.Extensions.Logging;
using RestaurantePro.Core.Interfaces.Services;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public async Task SendNotificationAsync(string message)
        {
            _logger.LogInformation($"Notificación enviada: {message}");
            await Task.CompletedTask;
        }

        public async Task NotifyComandaCreatedAsync(int comandaId)
        {
            await SendNotificationAsync($"Nueva comanda creada: {comandaId}");
        }

        public async Task NotifyComandaUpdatedAsync(int comandaId)
        {
            await SendNotificationAsync($"Comanda actualizada: {comandaId}");
        }

        public async Task NotifyAsync(string message)
        {
            await SendNotificationAsync(message);
        }

        public async Task NotifyRoleAsync(string role, string message)
        {
            _logger.LogInformation($"Notificación para rol {role}: {message}");
            await Task.CompletedTask;
        }

        public async Task NotifyUserAsync(string username, string message)
        {
            _logger.LogInformation($"Notificación para usuario {username}: {message}");
            await Task.CompletedTask;
        }
    }
} 