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
    public partial class PlatoViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<Plato> _platos;

        [ObservableProperty]
        private Plato _selectedPlato;

        public PlatoViewModel()
        {
            Platos = new ObservableCollection<Plato>();
        }

        [RelayCommand]
        private async Task LoadPlatos()
        {
            var platos = await _apiService.GetPlatosAsync();
            Platos.Clear();
            foreach (var plato in platos)
            {
                Platos.Add(plato);
            }
        }

        [RelayCommand]
        private async Task AddPlato()
        {
            if (!await _authorizationService.IsUserAuthorizedAsync(RolUsuario.Administrador, RolUsuario.Cocinero))
            {
                await Application.Current.MainPage.DisplayAlert("Acceso Denegado", "No tienes permiso para agregar platos.", "OK");
                return;
            }

            await Shell.Current.GoToAsync(nameof(PlatoDetailPage));
        }

        [RelayCommand]
        private async Task EditPlato(Plato plato)
        {
            if (plato != null)
            {
                if (!await _authorizationService.IsUserAuthorizedAsync(RolUsuario.Administrador, RolUsuario.Cocinero))
                {
                    await Application.Current.MainPage.DisplayAlert("Acceso Denegado", "No tienes permiso para editar platos.", "OK");
                    return;
                }

                await Shell.Current.GoToAsync($"{nameof(PlatoDetailPage)}?platoId={plato.Id}");
            }
        }

        [RelayCommand]
        private async Task DeletePlato(Plato plato)
        {
            if (plato != null)
            {
                if (!await _authorizationService.IsUserAuthorizedAsync(RolUsuario.Administrador))
                {
                    await Application.Current.MainPage.DisplayAlert("Acceso Denegado", "No tienes permiso para eliminar platos.", "OK");
                    return;
                }

                bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmar", "¿Estás seguro de que deseas eliminar este plato?", "Sí", "No");
                if (confirm)
                {
                    await _apiService.DeletePlatoAsync(plato.Id);
                    Platos.Remove(plato);
                }
            }
        }
    }
}