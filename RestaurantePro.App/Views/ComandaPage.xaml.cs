using RestaurantePro.App.Models;
using RestaurantePro.App.Services;
using RestaurantePro.App.ViewModels;
using Microsoft.Maui.Controls;

namespace RestaurantePro.App.Views
{
    public partial class ComandaPage : ContentPage
    {
        private ComandaViewModel _viewModel;
        private readonly AuthorizationService _authorizationService;

        public ComandaPage(AuthorizationService authorizationService)
        {
            InitializeComponent();
            _viewModel = BindingContext as ComandaViewModel;
            _authorizationService = authorizationService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!await _authorizationService.IsUserAuthorizedAsync(RolUsuario.Administrador, RolUsuario.Mesero))
            {
                await DisplayAlert("Acceso Denegado", "No tienes permiso para acceder a esta página.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            if (_viewModel != null)
            {
                await _viewModel.LoadComandasCommand.ExecuteAsync(null);
            }
        }

        private async void OnComandaTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is Comanda comanda)
            {
                await Shell.Current.GoToAsync($"{nameof(ComandaDetallesPage)}?comandaId={comanda.Id}");
            }
        }
    }
}