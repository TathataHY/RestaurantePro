using RestaurantePro.Mobile.Core.Features.Commercial.Billing.ViewModels;

namespace RestaurantePro.Mobile.Features.Commercial.Billing.Pages;

/// <summary>
/// Página para gestión de facturas
/// </summary>
public partial class FacturasPage : ContentPage
{
    private readonly FacturasViewModel _viewModel;

    public FacturasPage(FacturasViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarFacturasCommand.ExecuteAsync(null);
    }

    private void OnFechaChanged(object sender, DateChangedEventArgs e)
    {
        // El ViewModel maneja la lógica del cambio de fecha
        // Este método solo existe para satisfacer el binding del XAML
    }
} 