using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Navigation;

namespace RestaurantePro.Mobile.Core.Features.Authentication.ViewModels;

/// <summary>
/// ViewModel para la página de login - V1 Fundamental
/// </summary>
public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    public LoginViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "Iniciar Sesión";
    }

    /// <summary>
    /// Comando para realizar el login
    /// </summary>
    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsLoading) return;

        try
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(Email))
            {
                await ShowErrorAsync("Por favor ingrese su email");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                await ShowErrorAsync("Por favor ingrese su contraseña");
                return;
            }

            IsLoading = true;
            ClearError();

            // Realizar login
            var result = await _authService.LoginAsync(Email, Password);

            if (result.Success)
            {
                // Navegar al dashboard principal (corregido)
                await _navigationService.NavigateToAsync("//main/dashboard");
            }
            else
            {
                var errorMessage = result.Errors?.FirstOrDefault() ?? "Error de autenticación";
                await ShowErrorAsync(errorMessage);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"Error inesperado: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Comando para limpiar los campos
    /// </summary>
    [RelayCommand]
    private void ClearFields()
    {
        Email = string.Empty;
        Password = string.Empty;
        ClearError();
    }

    [RelayCommand]
    private async Task CheckAuthStatusAsync()
    {
        // Verificar si ya está autenticado al cargar la página
        var isAuthenticated = await _authService.IsAuthenticatedAsync();
        if (isAuthenticated)
        {
            await _navigationService.NavigateToAsync("//main/dashboard");
        }
    }
} 