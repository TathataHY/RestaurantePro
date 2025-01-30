using RestaurantePro.App.ViewModels;

namespace RestaurantePro.App.Views
{
    public partial class MesaDetailPage : ContentPage
    {
        public MesaDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is MesaDetailViewModel viewModel && viewModel.MesaId > 0)
            {
                viewModel.LoadMesa(viewModel.MesaId);
            }
        }
    }
}