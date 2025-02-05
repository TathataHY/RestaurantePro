using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Xamarin.Forms;

public class SignalRService
{
    private readonly HubConnection _hubConnection;
    private readonly INotificationService _notificationService;
    private readonly PushNotificationService _pushNotificationService;

    public SignalRService(
        HubConnection hubConnection, 
        INotificationService notificationService,
        PushNotificationService pushNotificationService)
    {
        _hubConnection = hubConnection;
        _notificationService = notificationService;
        _pushNotificationService = pushNotificationService;
        
        ConfigureHubEvents();
    }

    private void ConfigureHubEvents()
    {
        _hubConnection.On<ComandaDto>("ComandaCreated", async (comanda) =>
        {
            await _notificationService.NotifyAsync($"Nueva comanda #{comanda.Id} creada");
            await _pushNotificationService.ShowNotification(
                "Nueva Comanda", 
                $"Se ha creado la comanda #{comanda.Id}");
            MessagingCenter.Send(this, "ComandaCreated", comanda);
        });

        _hubConnection.On<int, EstadoComanda>("ComandaStatusChanged", async (comandaId, newStatus) =>
        {
            var mensaje = $"Comanda #{comandaId} cambió a {newStatus}";
            await _notificationService.NotifyAsync(mensaje);
            await _pushNotificationService.ShowNotification(
                "Cambio de Estado", 
                mensaje);
            MessagingCenter.Send(this, "ComandaStatusChanged", (comandaId, newStatus));
        });
    }

    public async Task StartAsync()
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            await _hubConnection.StartAsync();
        }
    }

    public async Task StopAsync()
    {
        if (_hubConnection.State != HubConnectionState.Disconnected)
        {
            await _hubConnection.StopAsync();
        }
    }

    public async Task JoinGroupAsync(string role)
    {
        await _hubConnection.InvokeAsync("JoinGroup", role);
    }
} 