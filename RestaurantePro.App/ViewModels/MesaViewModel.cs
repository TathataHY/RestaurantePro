using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.App.Models;
using RestaurantePro.App.Services;
using RestaurantePro.App.Views;
using Microsoft.Maui.Controls;

namespace RestaurantePro.App.ViewModels
{
    public partial class MesaViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<Mesa> _mesas;

        [ObservableProperty]
        private Mesa _selectedMesa;

        public MesaViewModel()
        {
            Mesas = new ObservableCollection<Mesa>();
        }

        [RelayCommand]
        private async Task LoadMesas()
        {
            var mesas = await _apiService.GetMesasAsync();
            Mesas.Clear();
            foreach (var mesa in mesas)
            {
                Mesas.Add(mesa);
            }
        }

        [RelayCommand]
        private async Task AddMesa()
        {
            if (!await _authorizationService.IsUserAuthorizedAsync(RolUsuario.Administrador))
            {
                await Application.Current.MainPage.DisplayAlert("Acceso Denegado", "No tienes permiso para agregar mesas.", "OK");
                return;
            }

            await Shell.Current.GoToAsync(nameof(MesaDetailPage));
        }

        [RelayCommand]
        private async Task EditMesa(Mesa mesa)
        {
            if (mesa != null)
            {
                if (!await _authorizationService.IsUserAuthorizedAsync(RolUsuario.Administrador))
                {
                    await Application.Current.MainPage.DisplayAlert("Acceso Denegado", "No tienes permiso para editar mesas.", "OK");
                    return;
                }

                await Shell.Current.GoToAsync($"{nameof(MesaDetailPage)}?mesaId={mesa.Id}");
            }
        }

        [RelayCommand]
        private async Task DeleteMesa(Mesa mesa)
        {
            if (mesa != null)
            {
                if (!await _authorizationService.IsUserAuthorizedAsync(RolUsuario.Administrador))
                {
                    await Application.Current.MainPage.DisplayAlert("Acceso Denegado", "No tienes permiso para eliminar mesas.", "OK");
                    return;
                }

                bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmar", "¿Estás seguro de que deseas eliminar esta mesa?", "Sí", "No");
                if (confirm)
                {
                    await _apiService.DeleteMesaAsync(mesa.Id);
                    Mesas.Remove(mesa);
                }
            }
        }
    }
}