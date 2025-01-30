using RestaurantePro.App.ViewModels;
using Microsoft.Maui.Controls;

namespace RestaurantePro.App.Views
{
    public partial class ComandaDetallesPage : ContentPage
    {
        private ComandaDetallesViewModel _viewModel;

        public ComandaDetallesPage()
        {
            InitializeComponent();
            _viewModel = BindingContext as ComandaDetallesViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel != null)
            {
                await _viewModel.LoadDetallesCommand.ExecuteAsync(null);
            }
        }
    }
}