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
} 