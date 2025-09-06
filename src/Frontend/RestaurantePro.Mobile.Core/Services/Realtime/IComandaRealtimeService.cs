namespace RestaurantePro.Mobile.Core.Services.Realtime;

public interface IComandaRealtimeService
{
    event Action? OnNuevaComanda;
    event Action? OnComandaActualizada;

    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}


