using RestaurantePro.Mobile.Core.Features.Inventory.Reservaciones.ViewModels;

namespace RestaurantePro.Mobile.Features.Reservaciones.Pages;

/// <summary>
/// Página para la gestión de reservaciones
/// </summary>
public partial class ReservacionesPage : ContentPage
{
    private readonly ReservacionesViewModel _viewModel;

    public ReservacionesPage(ReservacionesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarReservacionesCommand.ExecuteAsync(null);
    }
} 