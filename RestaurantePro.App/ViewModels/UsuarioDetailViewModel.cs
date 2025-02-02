using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.App.Models;
using RestaurantePro.App.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RestaurantePro.App.ViewModels
{
    [QueryProperty(nameof(UsuarioId), "usuarioId")]
    public partial class UsuarioDetailViewModel : BaseViewModel
    {
        [ObservableProperty]
        private int _usuarioId;

        [ObservableProperty]
        private string _nombre;

        [ObservableProperty]
        private string _nombreUsuario;

        [ObservableProperty]
        private string _contraseña;

        [ObservableProperty]
        private RolUsuario _rol;

        [ObservableProperty]
        private bool _activo;

        public ObservableCollection<RolUsuario> Roles { get; } = new ObservableCollection<RolUsuario>(Enum.GetValues(typeof(RolUsuario)).Cast<RolUsuario>());

        [RelayCommand]
        private async Task Save()
        {
            var usuario = new Usuario
            {
                Id = UsuarioId,
                Nombre = Nombre,
                NombreUsuario = NombreUsuario,
                Contraseña = Contraseña,
                Rol = Rol,
                Activo = Activo
            };

            await _apiService.SaveUsuarioAsync(usuario);
            await Shell.Current.GoToAsync("..");
        }

        public async void LoadUsuario(int usuarioId)
        {
            var usuario = await _apiService.GetUsuarioByIdAsync(usuarioId);
            if (usuario != null)
            {
                UsuarioId = usuario.Id;
                Nombre = usuario.Nombre;
                NombreUsuario = usuario.NombreUsuario;
                Contraseña = usuario.Contraseña;
                Rol = usuario.Rol;
                Activo = usuario.Activo;
            }
        }
    }
}