using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.App.Models;
using RestaurantePro.App.Services;
using Microsoft.Maui.Controls;
using RestaurantePro.App.Views;

namespace RestaurantePro.App.ViewModels
{
    public partial class ComandaViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<Comanda> _comandas;

        [ObservableProperty]
        private ObservableCollection<ComandaDetalle> _comandaDetalles;

        [ObservableProperty]
        private Comanda _selectedComanda;

        public ComandaViewModel()
        {
            Comandas = new ObservableCollection<Comanda>();
            ComandaDetalles = new ObservableCollection<ComandaDetalle>();
        }

        [RelayCommand]
        private async Task LoadComandas()
        {
            var comandas = await _apiService.GetComandasAsync();
            Comandas.Clear();
            foreach (var comanda in comandas)
            {
                if (comanda.Estado == EstadoComanda.Pendiente)
                {
                    Comandas.Add(comanda);
                }
            }
        }

        [RelayCommand]
        private async Task AddComanda()
        {
            if (!await _authorizationService.IsUserAuthorizedAsync(RolUsuario.Administrador, RolUsuario.Mesero))
            {
                await Application.Current.MainPage.DisplayAlert("Acceso Denegado", "No tienes permiso para agregar comandas.", "OK");
                return;
            }

            await Shell.Current.GoToAsync(nameof(ComandaDetailPage));
        }

        [RelayCommand]
        private async Task EditComanda(Comanda comanda)
        {
            if (comanda != null)
            {
                if (!await _authorizationService.IsUserAuthorizedAsync(RolUsuario.Administrador, RolUsuario.Mesero))
                {
                    await Application.Current.MainPage.DisplayAlert("Acceso Denegado", "No tienes permiso para editar comandas.", "OK");
                    return;
                }

                await Shell.Current.GoToAsync($"{nameof(ComandaDetailPage)}?comandaId={comanda.Id}");
            }
        }

        [RelayCommand]
        private async Task DeleteComanda(Comanda comanda)
        {
            if (comanda != null)
            {
                if (!await _authorizationService.IsUserAuthorizedAsync(RolUsuario.Administrador))
                {
                    await Application.Current.MainPage.DisplayAlert("Acceso Denegado", "No tienes permiso para eliminar comandas.", "OK");
                    return;
                }

                bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmar", "¿Estás seguro de que deseas eliminar esta comanda?", "Sí", "No");
                if (confirm)
                {
                    await _apiService.DeleteComandaAsync(comanda.Id);
                    Comandas.Remove(comanda);
                }
            }
        }

        [RelayCommand]
        private async Task NavigateToComandaDetail(Comanda comanda)
        {
            if (comanda != null)
            {
                await Shell.Current.GoToAsync($"{nameof(ComandaDetallesPage)}?comandaId={comanda.Id}");
            }
        }
    }
}