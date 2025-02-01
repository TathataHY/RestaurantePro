using RestaurantePro.App.ViewModels;
using Microsoft.Maui.Controls;

namespace RestaurantePro.App.Views
{
    public partial class CocinaPage : ContentPage
    {
        private CocinaViewModel _viewModel;

        public CocinaPage()
        {
            InitializeComponent();
            _viewModel = BindingContext as CocinaViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel != null)
            {
                await _viewModel.LoadComandasCommand.ExecuteAsync(null);
            }
        }
    }
}