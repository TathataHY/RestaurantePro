using RestaurantePro.App.ViewModels;

namespace RestaurantePro.App.Views
{
    public partial class UsuarioDetailPage : ContentPage
    {
        public UsuarioDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is UsuarioDetailViewModel viewModel && viewModel.UsuarioId > 0)
            {
                viewModel.LoadUsuario(viewModel.UsuarioId);
            }
        }
    }
}