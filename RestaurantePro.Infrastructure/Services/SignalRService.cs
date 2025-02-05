using Microsoft.AspNetCore.SignalR;
using RestaurantePro.Core.Interfaces.Services;
using RestaurantePro.Infrastructure.Hubs;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Services
{
    public class SignalRService
    {
        private readonly IHubContext<ComandaHub> _hubContext;

        public SignalRService(IHubContext<ComandaHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToAllAsync(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task SendToGroupAsync(string groupName, string message)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", message);
        }

        public async Task SendToUserAsync(string userId, string message)
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveMessage", message);
        }
    }
} 