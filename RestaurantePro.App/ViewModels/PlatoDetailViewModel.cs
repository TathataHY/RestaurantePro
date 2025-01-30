using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.App.Models;
using RestaurantePro.App.Services;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace RestaurantePro.App.ViewModels
{
    [QueryProperty(nameof(PlatoId), "platoId")]
    public partial class PlatoDetailViewModel : BaseViewModel
    {
        [ObservableProperty]
        private int _platoId;

        [ObservableProperty]
        private string _nombre;

        [ObservableProperty]
        private string _descripcion;

        [ObservableProperty]
        private decimal _precio;

        [ObservableProperty]
        private bool _disponible;

        [ObservableProperty]
        private CategoriaPlato _categoria; // Nueva propiedad para la categoría

        public ObservableCollection<CategoriaPlato> Categorias { get; } = new ObservableCollection<CategoriaPlato>(Enum.GetValues(typeof(CategoriaPlato)).Cast<CategoriaPlato>());

        public PlatoDetailViewModel()
        {
        }

        [RelayCommand]
        private async Task Save()
        {
            var plato = new Plato
            {
                Id = PlatoId,
                Nombre = Nombre,
                Descripcion = Descripcion,
                Precio = Precio,
                Disponible = Disponible,
                Categoria = Categoria
            };

            await _databaseService.SavePlatoAsync(plato);
            await Shell.Current.GoToAsync("..");
        }

        public async void LoadPlato(int platoId)
        {
            var plato = await _databaseService.GetPlatoByIdAsync(platoId);
            if (plato != null)
            {
                PlatoId = plato.Id;
                Nombre = plato.Nombre;
                Descripcion = plato.Descripcion;
                Precio = plato.Precio;
                Disponible = plato.Disponible;
                Categoria = plato.Categoria;
            }
        }
    }
}