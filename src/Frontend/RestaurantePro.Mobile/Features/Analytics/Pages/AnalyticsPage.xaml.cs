using RestaurantePro.Mobile.Core.Features.Analytics.ViewModels;

namespace RestaurantePro.Mobile.Features.Analytics.Pages;

/// <summary>
/// Página para mostrar analytics y métricas
/// </summary>
public partial class AnalyticsPage : ContentPage
{
    private readonly AnalyticsViewModel _viewModel;

    public AnalyticsPage(AnalyticsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private void OnPeriodoChanged(object sender, EventArgs e)
    {
        // El ViewModel maneja la lógica del cambio de período
        // Este método solo existe para satisfacer el binding del XAML
    }
} 