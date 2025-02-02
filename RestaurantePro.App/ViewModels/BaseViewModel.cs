using CommunityToolkit.Mvvm.ComponentModel;
using RestaurantePro.App.Services;

namespace RestaurantePro.App.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        protected readonly ApiService _apiService;
        protected readonly AuthorizationService _authorizationService;

        public BaseViewModel()
        {
            _apiService = ServiceLocator.GetService<ApiService>();
            _authorizationService = ServiceLocator.GetService<AuthorizationService>();
        }


        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _title;
    }
}