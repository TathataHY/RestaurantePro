using RestaurantePro.Mobile.Core.Services.Notifications;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Devices;

namespace RestaurantePro.Mobile.Services;

public class NotificationService : INotificationService
{
    public async Task ShowToastAsync(string message, int durationMs = 2000)
    {
        var duration = durationMs > 2500 ? ToastDuration.Long : ToastDuration.Short;
        var toast = Toast.Make(message, duration, textSize: 14);
        await toast.Show();
    }

    public Task VibrateAsync(int milliseconds = 100)
    {
        try
        {
            Vibration.Vibrate(TimeSpan.FromMilliseconds(milliseconds));
        }
        catch
        {
            // Ignorar dispositivos sin vibración
        }
        return Task.CompletedTask;
    }
}


