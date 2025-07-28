using RestaurantePro.Mobile.Core.Features.Inventory.Preparaciones.ViewModels;

namespace RestaurantePro.Mobile.Features.Inventory.Preparaciones.Pages;

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
        await _viewModel.CargarPreparacionesCommand.ExecuteAsync(null);
        await _viewModel.CargarEstadisticasCommand.ExecuteAsync(null);
    }
} 