using Microsoft.Extensions.Logging;
using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.Interfaces.Services;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly SignalRService _signalRService;

        public NotificationService(ILogger<NotificationService> logger, SignalRService signalRService)
        {
            _logger = logger;
            _signalRService = signalRService;
        }

        public async Task NotifyComandaCreatedAsync(ComandaDto comanda)
        {
            await _signalRService.SendToGroupAsync("Cocineros", $"Nueva comanda creada: {comanda.Id}");
            _logger.LogInformation($"Notificación de comanda creada enviada: {comanda.Id}");
        }

        public async Task NotifyComandaCreatedAsync(int comandaId)
        {
            await _signalRService.SendToGroupAsync("Cocineros", $"Nueva comanda creada: {comandaId}");
            _logger.LogInformation($"Notificación de comanda creada enviada: {comandaId}");
        }

        public async Task NotifyComandaStatusChangedAsync(int comandaId, EstadoComanda newStatus)
        {
            await _signalRService.SendToAllAsync($"Cambio de estado en comanda {comandaId}: {newStatus}");
            _logger.LogInformation($"Notificación de cambio de estado enviada: {comandaId} -> {newStatus}");
        }

        public async Task NotifyComandaUpdatedAsync(int comandaId)
        {
            await _signalRService.SendToAllAsync($"Comanda actualizada: {comandaId}");
            _logger.LogInformation($"Notificación de comanda actualizada enviada: {comandaId}");
        }

        public async Task NotifyAsync(string message)
        {
            await _signalRService.SendToAllAsync(message);
            _logger.LogInformation($"Notificación general enviada: {message}");
        }

        public async Task NotifyRoleAsync(string role, string message)
        {
            await _signalRService.SendToGroupAsync(role, message);
            _logger.LogInformation($"Notificación enviada al rol {role}: {message}");
        }

        public async Task NotifyUserAsync(string userId, string message)
        {
            await _signalRService.SendToUserAsync(userId, message);
            _logger.LogInformation($"Notificación enviada al usuario {userId}: {message}");
        }

        public async Task SendNotificationAsync(string message)
        {
            await NotifyAsync(message);
        }
    }
} 