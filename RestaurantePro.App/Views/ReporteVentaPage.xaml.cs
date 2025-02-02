using RestaurantePro.App.ViewModels;

namespace RestaurantePro.App.Views
{
    public partial class ReporteVentaPage : ContentPage
    {
        public ReporteVentaPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is ReporteVentaViewModel viewModel)
            {
                viewModel.GenerarReporteCommand.Execute(null);
            }
        }
    }
}