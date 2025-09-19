using RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;

namespace RestaurantePro.Mobile.Views
{
    /// <summary>
    /// Página para la gestión del menú del día
    /// </summary>
    public partial class DailyPreparationsPage : ContentPage
    {
        private readonly DailyPreparationsViewModel _viewModel;

        public DailyPreparationsPage(DailyPreparationsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Cargar datos cuando la página aparece
            if (_viewModel != null)
            {
                await _viewModel.LoadPreparacionesDiariasCommand.ExecuteAsync(null);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // Limpiar recursos si es necesario
            if (_viewModel != null)
            {
                _viewModel.SelectedPreparacion = null;
            }
        }
    }
} 