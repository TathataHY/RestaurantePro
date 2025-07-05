using RestaurantePro.Mobile.Features.Operations.Mesas.ViewModels;

namespace RestaurantePro.Mobile.Features.Operations.Mesas.Pages;

/// <summary>
/// Página de detalle para una mesa específica
/// </summary>
public partial class MesaDetallePage : ContentPage
{
    private readonly MesaDetalleViewModel _viewModel;

    public MesaDetallePage(MesaDetalleViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Se ejecuta cuando la página aparece
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    /// <summary>
    /// Se ejecuta cuando la página desaparece
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.Cleanup();
    }
} 