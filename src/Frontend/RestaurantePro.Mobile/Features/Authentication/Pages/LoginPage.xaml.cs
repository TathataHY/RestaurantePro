using RestaurantePro.Mobile.Core.Features.Authentication.ViewModels;

namespace RestaurantePro.Mobile.Features.Authentication.Pages;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage(LoginViewModel viewModel)
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