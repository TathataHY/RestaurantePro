using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.Interfaces.Services;
using RestaurantePro.Core.Interfaces.Hubs;
using System;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Services
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IComandaHub _comandaHub;
        private readonly ILogger<SignalRNotificationService> _logger;

        public SignalRNotificationService(
            IComandaHub comandaHub,
            ILogger<SignalRNotificationService> logger)
        {
            _comandaHub = comandaHub;
            _logger = logger;
        }

        public async Task NotifyComandaCreatedAsync(ComandaDto comanda)
        {
            await _comandaHub.NotifyComandaCreatedWithDetails(comanda);
            _logger.LogInformation($"Notificación de comanda creada enviada: {comanda.Id}");
        }

        public async Task NotifyComandaCreatedAsync(int comandaId)
        {
            await _comandaHub.NotifyComandaCreatedById(comandaId);
            _logger.LogInformation($"Notificación de comanda creada enviada: {comandaId}");
        }

        public async Task NotifyComandaStatusChangedAsync(int comandaId, EstadoComanda newStatus)
        {
            await _comandaHub.NotifyComandaStatusChanged(comandaId, newStatus);
            _logger.LogInformation($"Notificación de cambio de estado enviada: {comandaId} -> {newStatus}");
        }

        public async Task NotifyComandaUpdatedAsync(int comandaId)
        {
            await _comandaHub.NotifyComandaUpdated(comandaId);
            _logger.LogInformation($"Notificación de comanda actualizada enviada: {comandaId}");
        }

        public async Task NotifyAsync(string message)
        {
            await _comandaHub.NotifyAll(message);
            _logger.LogInformation($"Notificación general enviada: {message}");
        }

        public async Task NotifyRoleAsync(string role, string message)
        {
            await _comandaHub.NotifyRole(role, message);
            _logger.LogInformation($"Notificación enviada al rol {role}: {message}");
        }

        public async Task NotifyUserAsync(string userId, string message)
        {
            await _comandaHub.NotifyUser(userId, message);
            _logger.LogInformation($"Notificación enviada al usuario {userId}: {message}");
        }

        public async Task SendNotificationAsync(string message)
        {
            await NotifyAsync(message);
        }
    }
} 