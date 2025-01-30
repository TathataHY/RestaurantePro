using RestaurantePro.App.ViewModels;
using Microsoft.Maui.Controls;

namespace RestaurantePro.App.Views
{
    public partial class MenuPage : ContentPage
    {
        private MenuViewModel _viewModel;

        public MenuPage()
        {
            InitializeComponent();
            _viewModel = BindingContext as MenuViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel != null)
            {
                await _viewModel.LoadPlatosCommand.ExecuteAsync(null);
            }
        }
    }
}