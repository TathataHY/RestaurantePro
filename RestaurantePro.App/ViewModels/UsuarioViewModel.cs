using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.App.Models;
using RestaurantePro.App.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using RestaurantePro.App.Views;

namespace RestaurantePro.App.ViewModels
{
    public partial class UsuarioViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<Usuario> _usuarios;

        [ObservableProperty]
        private Usuario _selectedUsuario;

        public UsuarioViewModel()
        {
            Usuarios = new ObservableCollection<Usuario>();
        }

        [RelayCommand]
        private async Task LoadUsuarios()
        {
            var usuarios = await _databaseService.GetUsuariosAsync();
            Usuarios.Clear();
            foreach (var usuario in usuarios)
            {
                Usuarios.Add(usuario);
            }
        }

        [RelayCommand]
        private async Task AddUsuario()
        {
            if (!await _authorizationService.IsUserAuthorizedAsync(RolUsuario.Administrador))
            {
                await Application.Current.MainPage.DisplayAlert("Acceso Denegado", "No tienes permiso para agregar usuarios.", "OK");
                return;
            }

            await Shell.Current.GoToAsync(nameof(UsuarioDetailPage));
        }

        [RelayCommand]
        private async Task EditUsuario(Usuario usuario)
        {
            if (usuario != null)
            {
                if (!await _authorizationService.IsUserAuthorizedAsync(RolUsuario.Administrador))
                {
                    await Application.Current.MainPage.DisplayAlert("Acceso Denegado", "No tienes permiso para editar usuarios.", "OK");
                    return;
                }

                await Shell.Current.GoToAsync($"{nameof(UsuarioDetailPage)}?usuarioId={usuario.Id}");
            }
        }

        [RelayCommand]
        private async Task DeleteUsuario(Usuario usuario)
        {
            if (usuario != null)
            {
                if (!await _authorizationService.IsUserAuthorizedAsync(RolUsuario.Administrador))
                {
                    await Application.Current.MainPage.DisplayAlert("Acceso Denegado", "No tienes permiso para eliminar usuarios.", "OK");
                    return;
                }

                bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmar", "¿Estás seguro de que deseas eliminar este usuario?", "Sí", "No");
                if (confirm)
                {
                    await _databaseService.DeleteUsuarioAsync(usuario);
                    Usuarios.Remove(usuario);
                }
            }
        }
    }
}