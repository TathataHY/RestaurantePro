using RestaurantePro.App.ViewModels;
using Microsoft.Maui.Controls;

namespace RestaurantePro.App.Views
{
    public partial class ComandaDetailPage : ContentPage
    {
        private ComandaDetailViewModel _viewModel;

        public ComandaDetailPage()
        {
            InitializeComponent();
            _viewModel = BindingContext as ComandaDetailViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel != null)
            {
                await _viewModel.LoadMesasCommand.ExecuteAsync(null);
                await _viewModel.LoadComanda(_viewModel.ComandaId);
            }
        }
    }
}