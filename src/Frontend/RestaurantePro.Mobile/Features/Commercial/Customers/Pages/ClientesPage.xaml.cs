using RestaurantePro.Mobile.Core.Features.Commercial.Customers.ViewModels;

namespace RestaurantePro.Mobile.Features.Commercial.Customers.Pages;

/// <summary>
/// Página para gestión de clientes
/// </summary>
public partial class ClientesPage : ContentPage
{
    private readonly ClientesViewModel _viewModel;

    public ClientesPage(ClientesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarClientesCommand.ExecuteAsync(null);
    }
} 