using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Navigation;

namespace RestaurantePro.Mobile.Features.Authentication.ViewModels;

/// <summary>
/// ViewModel para la página de login - V1 Fundamental
/// </summary>
public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    public LoginViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "Iniciar Sesión";
    }

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await ShowErrorAsync("Por favor complete todos los campos");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var result = await _authService.LoginAsync(Email, Password);
            
            if (result.Success)
            {
                // Navegar al dashboard principal
                await _navigationService.NavigateToAsync("//dashboard");
            }
            else
            {
                var errorMessage = result.Errors?.FirstOrDefault() ?? "Error de autenticación";
                await ShowErrorAsync(errorMessage);
            }
        });
    }

    [RelayCommand]
    private async Task CheckAuthStatusAsync()
    {
        // Verificar si ya está autenticado al cargar la página
        var isAuthenticated = await _authService.IsAuthenticatedAsync();
        if (isAuthenticated)
        {
            await _navigationService.NavigateToAsync("//dashboard");
        }
    }
} 