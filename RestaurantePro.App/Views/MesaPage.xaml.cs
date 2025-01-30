using RestaurantePro.App.Models;
using RestaurantePro.App.Services;
using RestaurantePro.App.ViewModels;

namespace RestaurantePro.App.Views
{
    public partial class MesaPage : ContentPage
    {
        private MesaViewModel _viewModel;
        private readonly AuthorizationService _authorizationService;
        
        public MesaPage(AuthorizationService authorizationService)
        {
            InitializeComponent();
            _viewModel = BindingContext as MesaViewModel;
            _authorizationService = authorizationService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!await _authorizationService.IsUserAuthorizedAsync(RolUsuario.Administrador))
            {
                await DisplayAlert("Acceso Denegado", "No tienes permiso para acceder a esta página.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            if (_viewModel != null)
            {
                _viewModel.LoadMesasCommand.Execute(null);
            }
        }
    }
}