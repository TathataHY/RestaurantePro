using Plugin.LocalNotification;
using System;
using System.Threading.Tasks;
using RestaurantePro.Core.Interfaces.Services;
using Microsoft.Maui.ApplicationModel.Communication;

namespace RestaurantePro.App.Services
{
    public class PushNotificationService
    {
        private readonly INotificationService _notificationService;

        public PushNotificationService(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task Initialize()
        {
            if (await Permissions.CheckStatusAsync<Permissions.Notification>() != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.Notification>();
            }
        }

        public async Task ShowNotification(string title, string message)
        {
            try
            {
                var notification = new NotificationRequest
                {
                    NotificationId = 100,
                    Title = title,
                    Description = message,
                    ReturningData = "Datos custom aquí"
                };

                await LocalNotificationCenter.Current.Show(notification);
            }
            catch (Exception ex)
            {
                await _notificationService.NotifyAsync($"Error al mostrar notificación: {ex.Message}");
            }
        }
    }
}