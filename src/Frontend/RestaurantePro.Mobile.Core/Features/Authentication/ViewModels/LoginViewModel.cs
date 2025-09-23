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

    [ObservableProperty]
    private bool recordarme;

    public LoginViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "Iniciar Sesión";
        
        // Cargar credenciales guardadas al inicializar
        _ = LoadSavedCredentialsAsync();
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
            var result = await _authService.LoginAsync(Email, Password, Recordarme);

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
        Recordarme = false;
        ClearError();
    }

    [RelayCommand]
    private async Task CheckAuthStatusAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔍 [CheckAuthStatus] Iniciando verificación de autenticación...");
            
            // Verificar si ya está autenticado al cargar la página
            var isAuthenticated = await _authService.IsAuthenticatedAsync();
            
            System.Diagnostics.Debug.WriteLine($"🔍 [CheckAuthStatus] Usuario autenticado: {isAuthenticated}");
            
            if (isAuthenticated)
            {
                System.Diagnostics.Debug.WriteLine("✅ [CheckAuthStatus] Navegando al dashboard...");
                await _navigationService.NavigateToAsync("//main/dashboard");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ [CheckAuthStatus] Usuario no autenticado, permaneciendo en login");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [CheckAuthStatus] Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Comando para alternar el estado de "Recordarme"
    /// </summary>
    [RelayCommand]
    private void ToggleRecordarme()
    {
        Recordarme = !Recordarme;
    }

    /// <summary>
    /// Carga las credenciales guardadas si "Recordarme" está activado
    /// </summary>
    private async Task LoadSavedCredentialsAsync()
    {
        try
        {
            var (savedEmail, savedPassword, recordarme) = await _authService.GetSavedCredentialsAsync();
            
            if (recordarme && !string.IsNullOrWhiteSpace(savedEmail))
            {
                Email = savedEmail;
                Recordarme = recordarme;
                
                // Log para debug
                System.Diagnostics.Debug.WriteLine($"🔐 [LoginViewModel] Credenciales cargadas: Email={savedEmail}, Recordarme={recordarme}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [LoginViewModel] Error cargando credenciales: {ex.Message}");
        }
    }

    /// <summary>
    /// Comando público para recargar credenciales guardadas
    /// </summary>
    [RelayCommand]
    private async Task LoadSavedCredentialsCommand()
    {
        await LoadSavedCredentialsAsync();
    }
} 