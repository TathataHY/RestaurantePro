using RestaurantePro.App.Models;
using RestaurantePro.App.Services;
using RestaurantePro.App.ViewModels;

namespace RestaurantePro.App.Views
{
    public partial class PlatoPage : ContentPage
    {
        private PlatoViewModel _viewModel;
        private readonly AuthorizationService _authorizationService;

        public PlatoPage(AuthorizationService authorizationService)
        {
            InitializeComponent();
            _viewModel = BindingContext as PlatoViewModel;
            _authorizationService = authorizationService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!await _authorizationService.IsUserAuthorizedAsync(RolUsuario.Administrador, RolUsuario.Cocinero))
            {
                await DisplayAlert("Acceso Denegado", "No tienes permiso para acceder a esta página.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            if (_viewModel != null)
            {
                _viewModel.LoadPlatosCommand.Execute(null);
            }
        }
    }
}