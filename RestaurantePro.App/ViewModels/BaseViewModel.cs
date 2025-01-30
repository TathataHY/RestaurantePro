using CommunityToolkit.Mvvm.ComponentModel;
using RestaurantePro.App.Services;

namespace RestaurantePro.App.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        protected readonly DatabaseService _databaseService;
        protected readonly AuthorizationService _authorizationService;

        public BaseViewModel()
        {
            _databaseService = ServiceLocator.GetService<DatabaseService>();
            _authorizationService = ServiceLocator.GetService<AuthorizationService>();
        }
    }
}