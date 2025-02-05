using CommunityToolkit.Mvvm.ComponentModel;
using RestaurantePro.App.Services;

namespace RestaurantePro.App.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        protected readonly IApiService _apiService;
        protected readonly SignalRService _signalRService;
        protected readonly INotificationService _notificationService;
        protected readonly IAuthorizationService _authorizationService;

        public BaseViewModel()
        {
            _apiService = ServiceLocator.Current.GetService<IApiService>();
            _signalRService = ServiceLocator.Current.GetService<SignalRService>();
            _notificationService = ServiceLocator.Current.GetService<INotificationService>();
            _authorizationService = ServiceLocator.Current.GetService<IAuthorizationService>();
        }

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _title;

        public virtual async Task InitializeAsync()
        {
            // Los ViewModels derivados pueden sobrescribir este método
            await Task.CompletedTask;
        }
    }
}