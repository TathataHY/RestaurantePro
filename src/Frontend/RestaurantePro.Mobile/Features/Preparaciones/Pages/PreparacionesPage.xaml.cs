using RestaurantePro.Mobile.Core.Features.Preparaciones.ViewModels;

namespace RestaurantePro.Mobile.Features.Preparaciones.Pages;

/// <summary>
/// Página para gestión de preparaciones de cocina
/// </summary>
public partial class PreparacionesPage : ContentPage
{
    private readonly PreparacionesViewModel _viewModel;

    public PreparacionesPage(PreparacionesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.OnAppearingAsync();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _viewModel.OnDisappearingAsync();
    }
} 