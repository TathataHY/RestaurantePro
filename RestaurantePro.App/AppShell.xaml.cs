using RestaurantePro.App.Views;
using RestaurantePro.App.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Linq;
using RestaurantePro.App.Models;
using System.Windows.Input;

namespace RestaurantePro.App
{
    public partial class AppShell : Shell
    {
        private readonly AuthorizationService _authorizationService;

        public ICommand LogoutCommand { get; }

        public AppShell()
        {
            InitializeComponent();

            _authorizationService = ServiceLocator.GetService<AuthorizationService>();

            // Registrar rutas
            Routing.RegisterRoute(nameof(AddDetallePage), typeof(AddDetallePage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(PlatoDetailPage), typeof(PlatoDetailPage));
            Routing.RegisterRoute(nameof(MesaDetailPage), typeof(MesaDetailPage));
            Routing.RegisterRoute(nameof(ComandaDetailPage), typeof(ComandaDetailPage));
            Routing.RegisterRoute(nameof(UsuarioDetailPage), typeof(UsuarioDetailPage));
            Routing.RegisterRoute(nameof(ComandaDetallesPage), typeof(ComandaDetallesPage));

            // Inicializar el comando de logout
            LogoutCommand = new Command(OnLogout);

            // Establecer el BindingContext
            BindingContext = this;

            // Ajustar la visibilidad de los elementos según el rol del usuario
            AdjustVisibility();
        }

        private async void AdjustVisibility()
        {
            var user = await _authorizationService.GetCurrentUserAsync();
            if (user == null)
                return;

            // Obtener el FlyoutItem principal
            var principalFlyoutItem = this.Items.OfType<FlyoutItem>().FirstOrDefault(item => item.Title == "Principal");
            if (principalFlyoutItem == null)
                return;

            // Ocultar pestañas según el rol del usuario
            if (user.Rol != RolUsuario.Administrador && user.Rol != RolUsuario.Mesero)
            {
                var comandasTab = principalFlyoutItem.Items.OfType<Tab>().FirstOrDefault(item => item.Title == "Comandas");
                if (comandasTab != null)
                {
                    principalFlyoutItem.Items.Remove(comandasTab);
                }
            }

            // Ocultar FlyoutItems según el rol del usuario
            if (user.Rol != RolUsuario.Administrador)
            {
                var usuariosItem = this.Items.FirstOrDefault(item => item.Title == "Usuarios");
                if (usuariosItem != null)
                {
                    this.Items.Remove(usuariosItem);
                }

                var mesasItem = this.Items.FirstOrDefault(item => item.Title == "Mesas");
                if (mesasItem != null)
                {
                    this.Items.Remove(mesasItem);
                }

                var reportesItem = this.Items.FirstOrDefault(item => item.Title == "Reportes");
                if (reportesItem != null)
                {
                    this.Items.Remove(reportesItem);
                }
            }

            if (user.Rol != RolUsuario.Administrador && user.Rol != RolUsuario.Cocinero)
            {
                var inventarioItem = this.Items.FirstOrDefault(item => item.Title == "Inventario");
                if (inventarioItem != null)
                {
                    this.Items.Remove(inventarioItem);
                }

                var platosItem = this.Items.FirstOrDefault(item => item.Title == "Platos");
                if (platosItem != null)
                {
                    this.Items.Remove(platosItem);
                }
            }
        }

        private async void OnLogout()
        {
            Preferences.Set("IsLoggedIn", false);
            Preferences.Remove("UserId");
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}
