using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.App.Services;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace RestaurantePro.App.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string _nombreUsuario;

        [ObservableProperty]
        private string _contraseña;

        public LoginViewModel()
        {
        }

        [RelayCommand]
        private async Task Login()
        {
            try
            {
                var (usuario, token) = await _apiService.LoginAsync(NombreUsuario, Contraseña);
                if (usuario != null)
                {
                    // Guardar el estado de autenticación y el token
                    Preferences.Set("IsLoggedIn", true);
                    Preferences.Set("UserId", usuario.Id);
                    Preferences.Set("AuthToken", token);

                    // Establecer el token en el ApiService
                    _apiService.SetAuthToken(token);

                    // Establecer AppShell como la MainPage
                    Application.Current.MainPage = new AppShell();
                }
            }
            catch
            {
                // Mostrar mensaje de error
                await Application.Current.MainPage.DisplayAlert("Error", "Nombre de usuario o contraseña incorrectos", "OK");
            }
        }
    }
}