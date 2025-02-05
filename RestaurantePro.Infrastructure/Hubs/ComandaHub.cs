using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.Interfaces.Hubs;

namespace RestaurantePro.Infrastructure.Hubs
{
    public class ComandaHub : Hub, IComandaHub
    {
        private readonly ILogger<ComandaHub> _logger;

        public ComandaHub(ILogger<ComandaHub> logger)
        {
            _logger = logger;
        }

        public async Task NotifyComandaCreatedWithDetails(ComandaDto comanda)
        {
            await Clients.All.SendAsync("ComandaCreatedWithDetails", comanda);
            _logger.LogInformation($"Notificación de comanda creada con detalles: {comanda.Id}");
        }

        public async Task NotifyComandaCreatedById(int comandaId)
        {
            await Clients.All.SendAsync("ComandaCreatedById", comandaId);
            _logger.LogInformation($"Notificación de comanda creada por ID: {comandaId}");
        }

        public async Task NotifyComandaStatusChanged(int comandaId, EstadoComanda newStatus)
        {
            await Clients.All.SendAsync("ComandaStatusChanged", comandaId, newStatus);
            _logger.LogInformation($"Notificación de cambio de estado: {comandaId} -> {newStatus}");
        }

        public async Task NotifyComandaUpdated(int comandaId)
        {
            await Clients.All.SendAsync("ComandaUpdated", comandaId);
            _logger.LogInformation($"Notificación de comanda actualizada: {comandaId}");
        }

        public async Task NotifyRole(string role, string message)
        {
            await Clients.Group(role).SendAsync("ReceiveNotification", message);
            _logger.LogInformation($"Notificación a rol {role}: {message}");
        }

        public async Task JoinGroup(string role)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, role);
            _logger.LogInformation($"Usuario {Context.ConnectionId} se unió al grupo {role}");
        }

        public async Task LeaveGroup(string role)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, role);
            _logger.LogInformation($"Usuario {Context.ConnectionId} dejó el grupo {role}");
        }

        public async Task NotifyAll(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
            _logger.LogInformation($"Notificación general: {message}");
        }

        public async Task NotifyUser(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
            _logger.LogInformation($"Notificación a usuario {userId}: {message}");
        }
    }
}