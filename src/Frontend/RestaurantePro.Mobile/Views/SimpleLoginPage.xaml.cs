using RestaurantePro.Mobile.Core.Features.Authentication.ViewModels;

namespace RestaurantePro.Mobile.Views;

/// <summary>
/// Página de Login simplificada para pruebas UI
/// </summary>
public partial class SimpleLoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public SimpleLoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Verificar estado de autenticación al aparecer
        if (_viewModel.CheckAuthStatusCommand.CanExecute(null))
        {
            await _viewModel.CheckAuthStatusCommand.ExecuteAsync(null);
        }
    }
} 