using Microsoft.AspNetCore.SignalR.Client;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Realtime;

namespace RestaurantePro.Mobile.Services;

public class ComandaRealtimeService : IComandaRealtimeService
{
    private readonly IAuthService _authService;
    private HubConnection? _connection;

    public event Action? OnNuevaComanda;
    public event Action? OnComandaActualizada;

    public ComandaRealtimeService(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_connection != null && _connection.State == HubConnectionState.Connected)
            return;

        var baseUrl = RestaurantePro.Mobile.Config.ApiConfig.GetBaseUrl().TrimEnd('/');
        var hubUrl = $"{baseUrl}/hubs/comandas";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = async () => await _authService.GetTokenAsync();
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.Reconnecting += error => Task.CompletedTask;
        _connection.Reconnected += connectionId => Task.CompletedTask;

        _connection.On("RecibirNuevaComanda", () => OnNuevaComanda?.Invoke());
        _connection.On("ComandaActualizada", () => OnComandaActualizada?.Invoke());

        await _connection.StartAsync(cancellationToken);

        // Intentar unirse al grupo de cocina (servidor agrega por rol igualmente)
        try { await _connection.InvokeAsync("JoinGroup", "Cocina", cancellationToken); } catch { }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_connection != null)
        {
            await _connection.StopAsync(cancellationToken);
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}


