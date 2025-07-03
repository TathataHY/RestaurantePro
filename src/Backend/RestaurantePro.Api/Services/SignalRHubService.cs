using Microsoft.AspNetCore.SignalR;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Api.Hubs;

namespace RestaurantePro.Api.Services;

/// <summary>
/// Implementación del ISignalRHub que usa un IHubContext genérico
/// Conecta la infraestructura con el hub de SignalR
/// </summary>
public class SignalRHubService : ISignalRHub
{
    private readonly IHubContext<ComandaHub> _hubContext;

    public SignalRHubService(IHubContext<ComandaHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToGroupAsync(string groupName, string method, params object[] args)
    {
        await _hubContext.Clients.Group(groupName).SendAsync(method, args);
    }

    public async Task SendToAllAsync(string method, params object[] args)
    {
        await _hubContext.Clients.All.SendAsync(method, args);
    }

    public async Task SendToUserAsync(string userId, string method, params object[] args)
    {
        await _hubContext.Clients.User(userId).SendAsync(method, args);
    }
} 