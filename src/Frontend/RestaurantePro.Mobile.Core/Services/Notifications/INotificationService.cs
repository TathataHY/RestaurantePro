namespace RestaurantePro.Mobile.Core.Services.Notifications;

public interface INotificationService
{
    Task ShowToastAsync(string message, int durationMs = 2000);
    Task VibrateAsync(int milliseconds = 100);
}


