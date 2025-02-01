using RestaurantePro.App.ViewModels;
using Microsoft.Maui.Controls;

namespace RestaurantePro.App.Views
{
    public partial class MeseroPage : ContentPage
    {
        private MeseroViewModel _viewModel;

        public MeseroPage()
        {
            InitializeComponent();
            _viewModel = BindingContext as MeseroViewModel;
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