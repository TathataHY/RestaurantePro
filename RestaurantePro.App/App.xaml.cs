using Microsoft.Maui.Controls;
using RestaurantePro.App.Views;
using Microsoft.Maui.Storage;

namespace RestaurantePro.App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Verificar si el usuario está autenticado
            if (Preferences.Get("IsLoggedIn", false))
            {
                MainPage = new AppShell();
            }
            else
            {
                MainPage = new NavigationPage(new LoginPage());
            }
        }
    }
}
