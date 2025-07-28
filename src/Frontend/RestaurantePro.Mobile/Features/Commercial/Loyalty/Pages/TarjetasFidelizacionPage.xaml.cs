using RestaurantePro.Mobile.Core.Features.Commercial.Loyalty.ViewModels;

namespace RestaurantePro.Mobile.Features.Commercial.Loyalty.Pages;

/// <summary>
/// Página para gestión de tarjetas de fidelización
/// </summary>
public partial class TarjetasFidelizacionPage : ContentPage
{
    private readonly TarjetasFidelizacionViewModel _viewModel;

    public TarjetasFidelizacionPage(TarjetasFidelizacionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
} 