using RestaurantePro.Mobile.Features.Authentication.ViewModels;

namespace RestaurantePro.Mobile.Features.Authentication.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Verificar estado de autenticación al aparecer la página
        if (BindingContext is LoginViewModel viewModel)
        {
            await viewModel.CheckAuthStatusCommand.ExecuteAsync(null);
        }
    }
} 