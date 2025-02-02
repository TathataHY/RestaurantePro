using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.App.Models;
using RestaurantePro.App.Services;
using Microsoft.Maui.Controls;

namespace RestaurantePro.App.ViewModels
{
    public partial class MenuViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<Plato> _platos;

        [ObservableProperty]
        private Plato _selectedPlato;

        public MenuViewModel() 
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
                if (plato.Disponible)
                {
                    Platos.Add(plato);
                }
            }
        }

        [RelayCommand]
        private async Task UpdateStock(Plato plato)
        {
            if (plato != null)
            {
                if (!await _authorizationService.IsUserAuthorizedAsync(RolUsuario.Administrador, RolUsuario.Cocinero))
                {
                    await Application.Current.MainPage.DisplayAlert("Acceso Denegado", "No tienes permiso para actualizar el stock.", "OK");
                    return;
                }

                string result = await Application.Current.MainPage.DisplayPromptAsync("Actualizar Stock", $"Ingrese la nueva cantidad de stock para {plato.Nombre}:", initialValue: plato.Stock.ToString(), keyboard: Keyboard.Numeric);

                if (!string.IsNullOrEmpty(result) && int.TryParse(result, out int newStock))
                {
                    plato.Stock = newStock;
                    await _apiService.SavePlatoAsync(plato);
                    await LoadPlatos();
                }
                else if (!string.IsNullOrEmpty(result))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Cantidad de stock inválida.", "OK");
                }
            }
        }
    }
}