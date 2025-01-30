using RestaurantePro.App.ViewModels;

namespace RestaurantePro.App.Views
{
    public partial class PlatoDetailPage : ContentPage
    {
        public PlatoDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is PlatoDetailViewModel viewModel && viewModel.PlatoId > 0)
            {
                viewModel.LoadPlato(viewModel.PlatoId);
            }
        }
    }
}